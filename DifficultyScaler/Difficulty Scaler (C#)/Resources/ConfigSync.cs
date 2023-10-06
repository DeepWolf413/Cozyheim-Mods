using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace ServerSync;

[PublicAPI]
public abstract class OwnConfigEntryBase
{
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
	public object? LocalBaseValue;
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.

	public bool SynchronizedConfig = true;
	public abstract ConfigEntryBase BaseConfig { get; }
}

[PublicAPI]
public class SyncedConfigEntry<T> : OwnConfigEntryBase
{
	public readonly ConfigEntry<T> SourceConfig;

	public SyncedConfigEntry(ConfigEntry<T> sourceConfig) { SourceConfig = sourceConfig; }

	public override ConfigEntryBase BaseConfig => SourceConfig;

	public T Value
	{
		get => SourceConfig.Value;
		set => SourceConfig.Value = value;
	}

	public void AssignLocalValue(T value)
	{
		if (LocalBaseValue == null)
			Value = value;
		else
			LocalBaseValue = value;
	}
}

public abstract class CustomSyncedValueBase
{
	public readonly string Identifier;
	public readonly Type Type;

	private object? boxedValue;

	public object? LocalBaseValue;

	protected bool localIsOwner;

	protected CustomSyncedValueBase(ConfigSync configSync, string identifier, Type type)
	{
		Identifier = identifier;
		Type = type;
		configSync.AddCustomValue(this);
		localIsOwner = configSync.IsSourceOfTruth;
		configSync.SourceOfTruthChanged += truth => localIsOwner = truth;
	}

	public object? BoxedValue
	{
		get => boxedValue;
		set
		{
			boxedValue = value;
			ValueChanged?.Invoke();
		}
	}

	public event Action? ValueChanged;
}

[PublicAPI]
public sealed class CustomSyncedValue<T> : CustomSyncedValueBase
{
	public CustomSyncedValue(ConfigSync configSync, string identifier, T value = default!) : base(configSync, identifier, typeof(T)) { Value = value; }

	public T Value
	{
		get => (T)BoxedValue!;
		set => BoxedValue = value;
	}

	public void AssignLocalValue(T value)
	{
		if (localIsOwner)
			Value = value;
		else
			LocalBaseValue = value;
	}
}

internal class ConfigurationManagerAttributes
{
	[UsedImplicitly]
	public bool? ReadOnly = false;
}

[PublicAPI]
public class ConfigSync
{
	private const byte PARTIAL_CONFIGS = 1;
	private const byte FRAGMENTED_CONFIG = 2;
	private const byte COMPRESSED_CONFIG = 4;
	public static bool ProcessingServerUpdate;

	private static readonly HashSet<ConfigSync> configSyncs = new();

	private static bool isServer;

	private static bool lockExempt;

	private static long packageCounter;

	private readonly HashSet<OwnConfigEntryBase> allConfigs = new();
	private readonly HashSet<CustomSyncedValueBase> allCustomValues = new();
	private readonly List<KeyValuePair<long, string>> cacheExpirations = new(); // avoid leaking memory

	private readonly Dictionary<string, SortedDictionary<int, byte[]>> configValueCache = new();

	public readonly string Name;
	public string? CurrentVersion;
	public string? DisplayName;

	private bool? forceConfigLocking;

	private bool isSourceOfTruth = true;

	private OwnConfigEntryBase? lockedConfig;
	public string? MinimumRequiredVersion;
	public bool ModRequired;

	static ConfigSync() { RuntimeHelpers.RunClassConstructor(typeof(VersionCheck).TypeHandle); }

	public ConfigSync(string name)
	{
		Name = name;
		configSyncs.Add(this);
		_ = new VersionCheck(this);
	}

	public bool IsLocked
	{
		get => (forceConfigLocking ?? (lockedConfig != null && ((IConvertible)lockedConfig.BaseConfig.BoxedValue).ToInt32(CultureInfo.InvariantCulture) != 0)) && !lockExempt;
		set => forceConfigLocking = value;
	}

	public bool IsAdmin => lockExempt;

	public bool IsSourceOfTruth
	{
		get => isSourceOfTruth;
		private set
		{
			if (value != isSourceOfTruth) {
				isSourceOfTruth = value;
				SourceOfTruthChanged?.Invoke(value);
			}
		}
	}

	public event Action<bool>? SourceOfTruthChanged;
	private event Action? lockedConfigChanged;

	public SyncedConfigEntry<T> AddConfigEntry<T>(ConfigEntry<T> configEntry)
	{
		if (configData(configEntry) is not SyncedConfigEntry<T> syncedEntry) {
			syncedEntry = new SyncedConfigEntry<T>(configEntry);
			AccessTools.DeclaredField(typeof(ConfigDescription), "<Tags>k__BackingField").SetValue(configEntry.Description, new object[]
			{ new ConfigurationManagerAttributes() }.Concat(configEntry.Description.Tags ?? Array.Empty<object>()).Concat(new[]
			{ syncedEntry }).ToArray());
			configEntry.SettingChanged += (_, _) =>
			{
				if (!ProcessingServerUpdate && syncedEntry.SynchronizedConfig) Broadcast(ZRoutedRpc.Everybody, configEntry);
			};
			allConfigs.Add(syncedEntry);
		}

		return syncedEntry;
	}

	public SyncedConfigEntry<T> AddLockingConfigEntry<T>(ConfigEntry<T> lockingConfig) where T : IConvertible
	{
		if (lockedConfig != null) throw new Exception("Cannot initialize locking ConfigEntry twice");

		lockedConfig = AddConfigEntry(lockingConfig);
		lockingConfig.SettingChanged += (_, _) => lockedConfigChanged?.Invoke();

		return (SyncedConfigEntry<T>)lockedConfig;
	}

	internal void AddCustomValue(CustomSyncedValueBase customValue)
	{
		if (allCustomValues.Select(v => v.Identifier).Concat(new[]
		    { "serverversion" }).Contains(customValue.Identifier)) throw new Exception("Cannot have multiple settings with the same name or with a reserved name (serverversion)");

		allCustomValues.Add(customValue);
		customValue.ValueChanged += () =>
		{
			if (!ProcessingServerUpdate) Broadcast(ZRoutedRpc.Everybody, customValue);
		};
	}

	private void RPC_InitialConfigSync(ZRpc rpc, ZPackage package) { RPC_ConfigSync(0, package); }

	private void RPC_ConfigSync(long sender, ZPackage package)
	{
		try {
			if (isServer && IsLocked) {
				var exempt = ((SyncedList?)AccessTools.DeclaredField(typeof(ZNet), "m_adminList").GetValue(ZNet.instance))?.Contains(SnatchCurrentlyHandlingRPC.currentRpc?.GetSocket()?.GetHostName());
				if (exempt == false) return;
			}

			cacheExpirations.RemoveAll(kv =>
			{
				if (kv.Key < DateTimeOffset.Now.Ticks) {
					configValueCache.Remove(kv.Value);
					return true;
				}

				return false;
			});

			var packageFlags = package.ReadByte();

			if ((packageFlags & FRAGMENTED_CONFIG) != 0) {
				var uniqueIdentifier = package.ReadLong();
				var cacheKey = sender.ToString() + uniqueIdentifier;
				if (!configValueCache.TryGetValue(cacheKey, out var dataFragments)) {
					dataFragments = new SortedDictionary<int, byte[]>();
					configValueCache[cacheKey] = dataFragments;
					cacheExpirations.Add(new KeyValuePair<long, string>(DateTimeOffset.Now.AddSeconds(60).Ticks, cacheKey));
				}

				var fragment = package.ReadInt();
				var fragments = package.ReadInt();

				dataFragments.Add(fragment, package.ReadByteArray());

				if (dataFragments.Count < fragments) return;

				configValueCache.Remove(cacheKey);

				package = new ZPackage(dataFragments.Values.SelectMany(a => a).ToArray());
				packageFlags = package.ReadByte();
			}

			ProcessingServerUpdate = true;

			if ((packageFlags & COMPRESSED_CONFIG) != 0) {
				var data = package.ReadByteArray();

				MemoryStream input = new(data);
				MemoryStream output = new();
				using (DeflateStream deflateStream = new(input, CompressionMode.Decompress)) {
					deflateStream.CopyTo(output);
				}

				package = new ZPackage(output.ToArray());
				packageFlags = package.ReadByte();
			}

			if ((packageFlags & PARTIAL_CONFIGS) == 0) resetConfigsFromServer();

			if (!isServer) {
				if (IsSourceOfTruth) lockedConfigChanged += serverLockedSettingChanged;
				IsSourceOfTruth = false;
			}

			var configs = ReadConfigsFromPackage(package);

			foreach (var configKv in configs.configValues) {
				if (!isServer && configKv.Key.LocalBaseValue == null) configKv.Key.LocalBaseValue = configKv.Key.BaseConfig.BoxedValue;

				configKv.Key.BaseConfig.BoxedValue = configKv.Value;
			}

			foreach (var configKv in configs.customValues) {
				if (!isServer) configKv.Key.LocalBaseValue ??= configKv.Key.BoxedValue;

				configKv.Key.BoxedValue = configKv.Value;
			}

			if (!isServer) {
				Debug.Log($"Received {configs.configValues.Count} configs and {configs.customValues.Count} custom values from the server for mod {DisplayName ?? Name}");

				serverLockedSettingChanged(); // Re-evaluate for intial locking
			}
		}
		finally {
			ProcessingServerUpdate = false;
		}
	}

	private ParsedConfigs ReadConfigsFromPackage(ZPackage package)
	{
		ParsedConfigs configs = new();
		var configMap = allConfigs.Where(c => c.SynchronizedConfig).ToDictionary(c => c.BaseConfig.Definition.Section + "_" + c.BaseConfig.Definition.Key, c => c);

		var customValueMap = allCustomValues.ToDictionary(c => c.Identifier, c => c);

		var valueCount = package.ReadInt();
		for (var i = 0; i < valueCount; ++i) {
			var groupName = package.ReadString();
			var configName = package.ReadString();
			var typeName = package.ReadString();

			var type = Type.GetType(typeName);
			if (typeName == "" || type != null) {
				object? value;
				try {
					value = typeName == "" ? null : ReadValueWithTypeFromZPackage(package, type!);
				}
				catch (InvalidDeserializationTypeException e) {
					Debug.LogWarning($"Got unexpected struct internal type {e.received} for field {e.field} struct {typeName} for {configName} in section {groupName} for mod {DisplayName ?? Name}, expecting {e.expected}");
					continue;
				}

				if (groupName == "Internal") {
					if (configName == "serverversion") {
						if (value?.ToString() != CurrentVersion) Debug.LogWarning($"Received server version is not equal: server version = {value?.ToString() ?? "null"}; local version = {CurrentVersion ?? "unknown"}");
					}
					else if (configName == "lockexempt") {
						if (value is bool exempt) lockExempt = exempt;
					}
					else if (customValueMap.TryGetValue(configName, out var config)) {
						if ((typeName == "" && (!config.Type.IsValueType || Nullable.GetUnderlyingType(config.Type) != null)) || GetZPackageTypeString(config.Type) == typeName)
							configs.customValues[config] = value;
						else
							Debug.LogWarning($"Got unexpected type {typeName} for internal value {configName} for mod {DisplayName ?? Name}, expecting {config.Type.AssemblyQualifiedName}");
					}
				}
				else if (configMap.TryGetValue(groupName + "_" + configName, out var config)) {
					var expectedType = configType(config.BaseConfig);
					if ((typeName == "" && (!expectedType.IsValueType || Nullable.GetUnderlyingType(expectedType) != null)) || GetZPackageTypeString(expectedType) == typeName)
						configs.configValues[config] = value;
					else
						Debug.LogWarning($"Got unexpected type {typeName} for {configName} in section {groupName} for mod {DisplayName ?? Name}, expecting {expectedType.AssemblyQualifiedName}");
				}
				else {
					Debug.LogWarning($"Received unknown config entry {configName} in section {groupName} for mod {DisplayName ?? Name}. This may happen if client and server versions of the mod do not match.");
				}
			}
			else {
				Debug.LogWarning($"Got invalid type {typeName}, abort reading of received configs");
				return new ParsedConfigs();
			}
		}

		return configs;
	}

	private static bool isWritableConfig(OwnConfigEntryBase config)
	{
		if (configSyncs.FirstOrDefault(cs => cs.allConfigs.Contains(config)) is not
		    { } configSync) return true;

		return configSync.IsSourceOfTruth || !config.SynchronizedConfig || config.LocalBaseValue == null || (!configSync.IsLocked && (config != configSync.lockedConfig || lockExempt));
	}

	private void serverLockedSettingChanged()
	{
		foreach (var configEntryBase in allConfigs) configAttribute<ConfigurationManagerAttributes>(configEntryBase.BaseConfig).ReadOnly = !isWritableConfig(configEntryBase);
	}

	private void resetConfigsFromServer()
	{
		foreach (var config in allConfigs.Where(config => config.LocalBaseValue != null)) {
			config.BaseConfig.BoxedValue = config.LocalBaseValue;
			config.LocalBaseValue = null;
		}

		foreach (var config in allCustomValues.Where(config => config.LocalBaseValue != null)) {
			config.BoxedValue = config.LocalBaseValue;
			config.LocalBaseValue = null;
		}

		lockedConfigChanged -= serverLockedSettingChanged;
		IsSourceOfTruth = true;
		serverLockedSettingChanged();
	}

	private IEnumerator<bool> distributeConfigToPeers(ZNetPeer peer, ZPackage package)
	{
		if (ZRoutedRpc.instance is not
		    { } rpc) yield break;

		const int packageSliceSize = 250000;
		const int maximumSendQueueSize = 20000;

		IEnumerable<bool> waitForQueue()
		{
			var timeout = Time.time + 30;
			while (peer.m_socket.GetSendQueueSize() > maximumSendQueueSize) {
				if (Time.time > timeout) {
					Debug.Log($"Disconnecting {peer.m_uid} after 30 seconds config sending timeout");
					peer.m_rpc.Invoke("Error", ZNet.ConnectionStatus.ErrorConnectFailed);
					ZNet.instance.Disconnect(peer);
					yield break;
				}

				yield return false;
			}
		}

		void SendPackage(ZPackage pkg)
		{
			var method = Name + " ConfigSync";
			if (isServer)
				peer.m_rpc.Invoke(method, pkg);
			else
				rpc.InvokeRoutedRPC(peer.m_server ? 0 : peer.m_uid, method, pkg);
		}

		if (package.GetArray() is
		    { LongLength: > packageSliceSize } data) {
			var fragments = (int)(1 + (data.LongLength - 1) / packageSliceSize);
			var packageIdentifier = ++packageCounter;
			for (var fragment = 0; fragment < fragments; ++fragment) {
				foreach (var wait in waitForQueue()) yield return wait;

				if (!peer.m_socket.IsConnected()) yield break;

				ZPackage fragmentedPackage = new();
				fragmentedPackage.Write(FRAGMENTED_CONFIG);
				fragmentedPackage.Write(packageIdentifier);
				fragmentedPackage.Write(fragment);
				fragmentedPackage.Write(fragments);
				fragmentedPackage.Write(data.Skip(packageSliceSize * fragment).Take(packageSliceSize).ToArray());
				SendPackage(fragmentedPackage);

				if (fragment != fragments - 1) yield return true;
			}
		}
		else {
			foreach (var wait in waitForQueue()) yield return wait;

			SendPackage(package);
		}
	}

	private IEnumerator sendZPackage(long target, ZPackage package)
	{
		if (!ZNet.instance) return Enumerable.Empty<object>().GetEnumerator();

		var peers = (List<ZNetPeer>)AccessTools.DeclaredField(typeof(ZRoutedRpc), "m_peers").GetValue(ZRoutedRpc.instance);
		if (target != ZRoutedRpc.Everybody) peers = peers.Where(p => p.m_uid == target).ToList();

		return sendZPackage(peers, package);
	}

	private IEnumerator sendZPackage(List<ZNetPeer> peers, ZPackage package)
	{
		if (!ZNet.instance) yield break;

		const int compressMinSize = 10000;

		if (package.GetArray() is
		    { LongLength: > compressMinSize } rawData) {
			ZPackage compressedPackage = new();
			compressedPackage.Write(COMPRESSED_CONFIG);
			MemoryStream output = new();
			using (DeflateStream deflateStream = new(output, CompressionLevel.Optimal)) {
				deflateStream.Write(rawData, 0, rawData.Length);
			}

			compressedPackage.Write(output.ToArray());
			package = compressedPackage;
		}

		var writers = peers.Where(peer => peer.IsReady()).Select(p => distributeConfigToPeers(p, package)).ToList();
		writers.RemoveAll(writer => !writer.MoveNext());
		while (writers.Count > 0) {
			yield return null;
			writers.RemoveAll(writer => !writer.MoveNext());
		}
	}

	private void Broadcast(long target, params ConfigEntryBase[] configs)
	{
		if (!IsLocked || isServer) {
			var package = ConfigsToPackage(configs);
			ZNet.instance?.StartCoroutine(sendZPackage(target, package));
		}
	}

	private void Broadcast(long target, params CustomSyncedValueBase[] customValues)
	{
		if (!IsLocked || isServer) {
			var package = ConfigsToPackage(customValues: customValues);
			ZNet.instance?.StartCoroutine(sendZPackage(target, package));
		}
	}

	private static OwnConfigEntryBase? configData(ConfigEntryBase config) { return config.Description.Tags?.OfType<OwnConfigEntryBase>().SingleOrDefault(); }

	public static SyncedConfigEntry<T>? ConfigData<T>(ConfigEntry<T> config) { return config.Description.Tags?.OfType<SyncedConfigEntry<T>>().SingleOrDefault(); }

	private static T configAttribute<T>(ConfigEntryBase config) { return config.Description.Tags.OfType<T>().First(); }

	private static Type configType(ConfigEntryBase config) { return configType(config.SettingType); }

	private static Type configType(Type type) { return type.IsEnum ? Enum.GetUnderlyingType(type) : type; }

	private static ZPackage ConfigsToPackage(IEnumerable<ConfigEntryBase>? configs = null, IEnumerable<CustomSyncedValueBase>? customValues = null, IEnumerable<PackageEntry>? packageEntries = null, bool partial = true)
	{
		var configList = configs?.Where(config => configData(config)!.SynchronizedConfig).ToList() ?? new List<ConfigEntryBase>();
		var customValueList = customValues?.ToList() ?? new List<CustomSyncedValueBase>();
		ZPackage package = new();
		package.Write(partial ? PARTIAL_CONFIGS : (byte)0);
		package.Write(configList.Count + customValueList.Count + (packageEntries?.Count() ?? 0));
		foreach (var packageEntry in packageEntries ?? Array.Empty<PackageEntry>()) AddEntryToPackage(package, packageEntry);
		foreach (var customValue in customValueList)
			AddEntryToPackage(package, new PackageEntry
			{ section = "Internal", key = customValue.Identifier, type = customValue.Type, value = customValue.BoxedValue });
		foreach (var config in configList)
			AddEntryToPackage(package, new PackageEntry
			{ section = config.Definition.Section, key = config.Definition.Key, type = configType(config), value = config.BoxedValue });

		return package;
	}

	private static void AddEntryToPackage(ZPackage package, PackageEntry entry)
	{
		package.Write(entry.section);
		package.Write(entry.key);
		package.Write(entry.value == null ? "" : GetZPackageTypeString(entry.type));
		AddValueToZPackage(package, entry.value);
	}

	private static string GetZPackageTypeString(Type type) { return type.AssemblyQualifiedName!; }

	private static void AddValueToZPackage(ZPackage package, object? value)
	{
		var type = value?.GetType();
		if (value is Enum) {
			value = ((IConvertible)value).ToType(Enum.GetUnderlyingType(value.GetType()), CultureInfo.InvariantCulture);
		}
		else if (value is ICollection collection) {
			package.Write(collection.Count);
			foreach (var item in collection) AddValueToZPackage(package, item);
			return;
		}
		else if (type is
		         { IsValueType: true, IsPrimitive: false }) {
			var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			package.Write(fields.Length);
			foreach (var field in fields) {
				package.Write(GetZPackageTypeString(field.FieldType));
				AddValueToZPackage(package, field.GetValue(value));
			}

			return;
		}

		ZRpc.Serialize(new[]
		{ value }, ref package);
	}

	private static object ReadValueWithTypeFromZPackage(ZPackage package, Type type)
	{
		if (type is
		    { IsValueType: true, IsPrimitive: false, IsEnum: false }) {
			var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var fieldCount = package.ReadInt();
			if (fieldCount != fields.Length)
				throw new InvalidDeserializationTypeException
				{ received = $"(field count: {fieldCount})", expected = $"(field count: {fields.Length})" };

			var value = FormatterServices.GetUninitializedObject(type);
			foreach (var field in fields) {
				var typeName = package.ReadString();
				if (typeName != GetZPackageTypeString(field.FieldType))
					throw new InvalidDeserializationTypeException
					{ received = typeName, expected = GetZPackageTypeString(field.FieldType), field = field.Name };
				field.SetValue(value, ReadValueWithTypeFromZPackage(package, field.FieldType));
			}

			return value;
		}

		if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)) {
			var entriesCount = package.ReadInt();
			var dict = (IDictionary)Activator.CreateInstance(type);
			var kvType = typeof(KeyValuePair<,>).MakeGenericType(type.GenericTypeArguments);
			var keyField = kvType.GetField("key", BindingFlags.NonPublic | BindingFlags.Instance)!;
			var valueField = kvType.GetField("value", BindingFlags.NonPublic | BindingFlags.Instance)!;
			for (var i = 0; i < entriesCount; ++i) {
				var kv = ReadValueWithTypeFromZPackage(package, kvType);
				dict.Add(keyField.GetValue(kv), valueField.GetValue(kv));
			}

			return dict;
		}

		if (type != typeof(List<string>) && type.IsGenericType && typeof(ICollection<>).MakeGenericType(type.GenericTypeArguments[0]) is
		    { } collectionType && collectionType.IsAssignableFrom(type.GetGenericTypeDefinition())) {
			var entriesCount = package.ReadInt();
			var list = Activator.CreateInstance(type);
			var adder = collectionType.GetMethod("Add")!;
			for (var i = 0; i < entriesCount; ++i)
				adder.Invoke(list, new[]
				{ ReadValueWithTypeFromZPackage(package, type.GenericTypeArguments[0]) });
			return list;
		}

		var param = (ParameterInfo)FormatterServices.GetUninitializedObject(typeof(ParameterInfo));
		AccessTools.DeclaredField(typeof(ParameterInfo), "ClassImpl").SetValue(param, type);
		List<object> data = new();
		ZRpc.Deserialize(new[]
		{ null, param }, package, ref data);
		return data.First();
	}

	[HarmonyPatch(typeof(ZRpc), "HandlePackage")]
	private static class SnatchCurrentlyHandlingRPC
	{
		public static ZRpc? currentRpc;

		[HarmonyPrefix]
		private static void Prefix(ZRpc __instance) { currentRpc = __instance; }
	}

	[HarmonyPatch(typeof(ZNet), "Awake")]
	internal static class RegisterRPCPatch
	{
		[HarmonyPostfix]
		private static void Postfix(ZNet __instance)
		{
			isServer = __instance.IsServer();
			foreach (var configSync in configSyncs) {
				configSync.IsSourceOfTruth = __instance.IsDedicated() || __instance.IsServer();
				ZRoutedRpc.instance.Register<ZPackage>(configSync.Name + " ConfigSync", configSync.RPC_ConfigSync);
				if (isServer) Debug.Log($"Registered '{configSync.Name} ConfigSync' RPC - waiting for incoming connections");
			}

			IEnumerator WatchAdminListChanges()
			{
				var adminList = (SyncedList)AccessTools.DeclaredField(typeof(ZNet), "m_adminList").GetValue(ZNet.instance);
				List<string> CurrentList = new(adminList.GetList());
				for (;;) {
					yield return new WaitForSeconds(30);
					if (!adminList.GetList().SequenceEqual(CurrentList)) {
						CurrentList = new List<string>(adminList.GetList());

						void SendAdmin(List<ZNetPeer> peers, bool isAdmin)
						{
							var package = ConfigsToPackage(packageEntries: new[]
							{ new PackageEntry
							  { section = "Internal", key = "lockexempt", type = typeof(bool), value = isAdmin } });

							if (configSyncs.First() is
							    { } configSync) ZNet.instance.StartCoroutine(configSync.sendZPackage(peers, package));
						}

						var adminPeer = ZNet.instance.GetPeers().Where(p => adminList.Contains(p.m_rpc.GetSocket().GetHostName())).ToList();
						var nonAdminPeer = ZNet.instance.GetPeers().Except(adminPeer).ToList();
						SendAdmin(nonAdminPeer, false);
						SendAdmin(adminPeer, true);
					}
				}
				// ReSharper disable once IteratorNeverReturns
			}

			if (isServer) __instance.StartCoroutine(WatchAdminListChanges());
		}
	}

	[HarmonyPatch(typeof(ZNet), "OnNewConnection")]
	private static class RegisterClientRPCPatch
	{
		[HarmonyPostfix]
		private static void Postfix(ZNet __instance, ZNetPeer peer)
		{
			if (!__instance.IsServer())
				foreach (var configSync in configSyncs)
					peer.m_rpc.Register<ZPackage>(configSync.Name + " ConfigSync", configSync.RPC_InitialConfigSync);
		}
	}

	private class ParsedConfigs
	{
		public readonly Dictionary<OwnConfigEntryBase, object?> configValues = new();
		public readonly Dictionary<CustomSyncedValueBase, object?> customValues = new();
	}

	[HarmonyPatch(typeof(ZNet), "Shutdown")]
	private class ResetConfigsOnShutdown
	{
		[HarmonyPostfix]
		private static void Postfix()
		{
			ProcessingServerUpdate = true;
			foreach (var serverSync in configSyncs) serverSync.resetConfigsFromServer();
			ProcessingServerUpdate = false;
		}
	}

	[HarmonyPatch(typeof(ZNet), "RPC_PeerInfo")]
	private class SendConfigsAfterLogin
	{
		[HarmonyPriority(Priority.First)]
		[HarmonyPrefix]
		private static void Prefix(ref Dictionary<Assembly, BufferingSocket>? __state, ZNet __instance, ZRpc rpc)
		{
			if (__instance.IsServer()) {
				BufferingSocket bufferingSocket = new(rpc.GetSocket());
				AccessTools.DeclaredField(typeof(ZRpc), "m_socket").SetValue(rpc, bufferingSocket);
				// Don't replace on steam sockets, RPC_PeerInfo does peer.m_socket as ZSteamSocket - which will cause a nullref when replaced
				if (AccessTools.DeclaredMethod(typeof(ZNet), "GetPeer", new[]
				    { typeof(ZRpc) }).Invoke(__instance, new object[]
				    { rpc }) is ZNetPeer peer && ZNet.m_onlineBackend != OnlineBackendType.Steamworks) AccessTools.DeclaredField(typeof(ZNetPeer), "m_socket").SetValue(peer, bufferingSocket);

				__state ??= new Dictionary<Assembly, BufferingSocket>();
				__state[Assembly.GetExecutingAssembly()] = bufferingSocket;
			}
		}

		[HarmonyPostfix]
		private static void Postfix(Dictionary<Assembly, BufferingSocket> __state, ZNet __instance, ZRpc rpc)
		{
			if (!__instance.IsServer()) return;

			void SendBufferedData()
			{
				if (rpc.GetSocket() is BufferingSocket bufferingSocket) {
					AccessTools.DeclaredField(typeof(ZRpc), "m_socket").SetValue(rpc, bufferingSocket.Original);
					if (AccessTools.DeclaredMethod(typeof(ZNet), "GetPeer", new[]
					    { typeof(ZRpc) }).Invoke(__instance, new object[]
					    { rpc }) is ZNetPeer peer) AccessTools.DeclaredField(typeof(ZNetPeer), "m_socket").SetValue(peer, bufferingSocket.Original);
				}

				bufferingSocket = __state[Assembly.GetExecutingAssembly()];
				bufferingSocket.finished = true;

				for (var i = 0; i < bufferingSocket.Package.Count; ++i) {
					if (i == bufferingSocket.versionMatchQueued) bufferingSocket.Original.VersionMatch();
					bufferingSocket.Original.Send(bufferingSocket.Package[i]);
				}

				if (bufferingSocket.Package.Count == bufferingSocket.versionMatchQueued) bufferingSocket.Original.VersionMatch();
			}

			if (AccessTools.DeclaredMethod(typeof(ZNet), "GetPeer", new[]
			    { typeof(ZRpc) }).Invoke(__instance, new object[]
			    { rpc }) is not ZNetPeer peer) {
				SendBufferedData();
				return;
			}

			IEnumerator sendAsync()
			{
				foreach (var configSync in configSyncs) {
					List<PackageEntry> entries = new();
					if (configSync.CurrentVersion != null)
						entries.Add(new PackageEntry
						{ section = "Internal", key = "serverversion", type = typeof(string), value = configSync.CurrentVersion });

					var listContainsId = AccessTools.DeclaredMethod(typeof(ZNet), "ListContainsId");
					var adminList = (SyncedList)AccessTools.DeclaredField(typeof(ZNet), "m_adminList").GetValue(ZNet.instance);
					entries.Add(new PackageEntry
					{ section = "Internal", key = "lockexempt", type = typeof(bool), value = listContainsId is null
						  ? adminList.Contains(rpc.GetSocket().GetHostName())
						  : listContainsId.Invoke(ZNet.instance, new object[]
						  { adminList, rpc.GetSocket().GetHostName() }) });

					var package = ConfigsToPackage(configSync.allConfigs.Select(c => c.BaseConfig), configSync.allCustomValues, entries, false);

					yield return __instance.StartCoroutine(configSync.sendZPackage(new List<ZNetPeer>
					{ peer }, package));
				}

				SendBufferedData();
			}

			__instance.StartCoroutine(sendAsync());
		}

		private class BufferingSocket : ISocket
		{
			public readonly ISocket Original;
			public readonly List<ZPackage> Package = new();
			public volatile bool finished;
			public volatile int versionMatchQueued = -1;

			public BufferingSocket(ISocket original) { Original = original; }

			public bool IsConnected() { return Original.IsConnected(); }

			public ZPackage Recv() { return Original.Recv(); }

			public int GetSendQueueSize() { return Original.GetSendQueueSize(); }

			public int GetCurrentSendRate() { return Original.GetCurrentSendRate(); }

			public bool IsHost() { return Original.IsHost(); }

			public void Dispose() { Original.Dispose(); }

			public bool GotNewData() { return Original.GotNewData(); }

			public void Close() { Original.Close(); }

			public string GetEndPointString() { return Original.GetEndPointString(); }

			public void GetAndResetStats(out int totalSent, out int totalRecv) { Original.GetAndResetStats(out totalSent, out totalRecv); }

			public void GetConnectionQuality(out float localQuality, out float remoteQuality, out int ping, out float outByteSec, out float inByteSec) { Original.GetConnectionQuality(out localQuality, out remoteQuality, out ping, out outByteSec, out inByteSec); }

			public ISocket Accept() { return Original.Accept(); }

			public int GetHostPort() { return Original.GetHostPort(); }

			public bool Flush() { return Original.Flush(); }

			public string GetHostName() { return Original.GetHostName(); }

			public void VersionMatch()
			{
				if (finished)
					Original.VersionMatch();
				else
					versionMatchQueued = Package.Count;
			}

			public void Send(ZPackage pkg)
			{
				var oldPos = pkg.GetPos();
				pkg.SetPos(0);
				var methodHash = pkg.ReadInt();
				if ((methodHash == "PeerInfo".GetStableHashCode() || methodHash == "RoutedRPC".GetStableHashCode() || methodHash == "ZDOData".GetStableHashCode()) && !finished) {
					ZPackage newPkg = new(pkg.GetArray());
					newPkg.SetPos(oldPos);
					Package.Add(newPkg); // the original ZPackage gets reused, create a new one
				}
				else {
					pkg.SetPos(oldPos);
					Original.Send(pkg);
				}
			}
		}
	}

	private class PackageEntry
	{
		public string key = null!;
		public string section = null!;
		public Type type = null!;
		public object? value;
	}

	[HarmonyPatch(typeof(ConfigEntryBase), nameof(ConfigEntryBase.GetSerializedValue))]
	private static class PreventSavingServerInfo
	{
		[HarmonyPrefix]
		private static bool Prefix(ConfigEntryBase __instance, ref string __result)
		{
			if (configData(__instance) is not
			    { } data || isWritableConfig(data)) return true;

			__result = TomlTypeConverter.ConvertToString(data.LocalBaseValue, __instance.SettingType);
			return false;
		}
	}

	[HarmonyPatch(typeof(ConfigEntryBase), nameof(ConfigEntryBase.SetSerializedValue))]
	private static class PreventConfigRereadChangingValues
	{
		[HarmonyPrefix]
		private static bool Prefix(ConfigEntryBase __instance, string value)
		{
			if (configData(__instance) is not
			    { } data || data.LocalBaseValue == null) return true;

			try {
				data.LocalBaseValue = TomlTypeConverter.ConvertToValue(value, __instance.SettingType);
			}
			catch (Exception e) {
				Debug.LogWarning($"Config value of setting \"{__instance.Definition}\" could not be parsed and will be ignored. Reason: {e.Message}; Value: {value}");
			}

			return false;
		}
	}

	private class InvalidDeserializationTypeException : Exception
	{
		public string expected = null!;
		public string field = "";
		public string received = null!;
	}
}

[PublicAPI]
[HarmonyPatch]
public class VersionCheck
{
	private static readonly HashSet<VersionCheck> versionChecks = new();
	private static readonly Dictionary<string, string> notProcessedNames = new();

	// Tracks which clients have passed the version check (only for servers).
	private readonly List<ZRpc> ValidatedClients = new();

	// Optional backing field to use ConfigSync values (will override other fields).
	private ConfigSync? ConfigSync;

	private string? currentVersion;

	private string? displayName;

	private string? minimumRequiredVersion;

	public bool ModRequired = true;

	public string Name;

	private string? ReceivedCurrentVersion;

	private string? ReceivedMinimumRequiredVersion;

	static VersionCheck()
	{
		typeof(ThreadingHelper).GetMethod("StartSyncInvoke")!.Invoke(ThreadingHelper.Instance, new object[]
		{ (Action)PatchServerSync });
	}

	public VersionCheck(string name)
	{
		Name = name;
		ModRequired = true;
		versionChecks.Add(this);
	}

	public VersionCheck(ConfigSync configSync)
	{
		ConfigSync = configSync;
		Name = ConfigSync.Name;
		versionChecks.Add(this);
	}

	public string DisplayName
	{
		get => displayName ?? Name;
		set => displayName = value;
	}

	public string CurrentVersion
	{
		get => currentVersion ?? "0.0.0";
		set => currentVersion = value;
	}

	public string MinimumRequiredVersion
	{
		get => minimumRequiredVersion ?? (ModRequired ? CurrentVersion : "0.0.0");
		set => minimumRequiredVersion = value;
	}

	private static void PatchServerSync()
	{
		if (PatchProcessor.GetPatchInfo(AccessTools.DeclaredMethod(typeof(ZNet), "Awake"))?.Postfixes.Count(p => p.PatchMethod.DeclaringType == typeof(ConfigSync.RegisterRPCPatch)) > 0) return;

		Harmony harmony = new("org.bepinex.helpers.ServerSync");
		foreach (var type in typeof(ConfigSync).GetNestedTypes(BindingFlags.NonPublic).Concat(new[]
		         { typeof(VersionCheck) }).Where(t => t.IsClass)) harmony.PatchAll(type);
	}

	public void Initialize()
	{
		ReceivedCurrentVersion = null;
		ReceivedMinimumRequiredVersion = null;
		if (ConfigSync == null) return;
		Name = ConfigSync.Name;
		DisplayName = ConfigSync.DisplayName!;
		CurrentVersion = ConfigSync.CurrentVersion!;
		MinimumRequiredVersion = ConfigSync.MinimumRequiredVersion!;
		ModRequired = ConfigSync.ModRequired;
	}

	private bool IsVersionOk()
	{
		if (ReceivedMinimumRequiredVersion == null || ReceivedCurrentVersion == null) return !ModRequired;
		var myVersionOk = new System.Version(CurrentVersion) >= new System.Version(ReceivedMinimumRequiredVersion);
		var otherVersionOk = new System.Version(ReceivedCurrentVersion) >= new System.Version(MinimumRequiredVersion);
		return myVersionOk && otherVersionOk;
	}

	private string ErrorClient()
	{
		if (ReceivedMinimumRequiredVersion == null) return $"Mod {DisplayName} must not be installed.";
		var myVersionOk = new System.Version(CurrentVersion) >= new System.Version(ReceivedMinimumRequiredVersion);
		return myVersionOk ? $"Mod {DisplayName} requires maximum {ReceivedCurrentVersion}. Installed is version {CurrentVersion}." : $"Mod {DisplayName} requires minimum {ReceivedMinimumRequiredVersion}. Installed is version {CurrentVersion}.";
	}

	private string ErrorServer(ZRpc rpc) { return $"Disconnect: The client ({rpc.GetSocket().GetHostName()}) doesn't have the correct {DisplayName} version {MinimumRequiredVersion}"; }

	private string Error(ZRpc? rpc = null) { return rpc == null ? ErrorClient() : ErrorServer(rpc); }

	private static VersionCheck[] GetFailedClient() { return versionChecks.Where(check => !check.IsVersionOk()).ToArray(); }

	private static VersionCheck[] GetFailedServer(ZRpc rpc) { return versionChecks.Where(check => check.ModRequired && !check.ValidatedClients.Contains(rpc)).ToArray(); }

	private static void Logout()
	{
		Game.instance.Logout();
		AccessTools.DeclaredField(typeof(ZNet), "m_connectionStatus").SetValue(null, ZNet.ConnectionStatus.ErrorVersion);
	}

	private static void DisconnectClient(ZRpc rpc) { rpc.Invoke("Error", (int)ZNet.ConnectionStatus.ErrorVersion); }

	private static void CheckVersion(ZRpc rpc, ZPackage pkg) { CheckVersion(rpc, pkg, null); }

	private static void CheckVersion(ZRpc rpc, ZPackage pkg, Action<ZRpc, ZPackage>? original)
	{
		var guid = pkg.ReadString();
		var minimumRequiredVersion = pkg.ReadString();
		var currentVersion = pkg.ReadString();

		var matched = false;

		foreach (var check in versionChecks) {
			if (guid != check.Name) continue;

			Debug.Log($"Received {check.DisplayName} version {currentVersion} and minimum version {minimumRequiredVersion} from the {(ZNet.instance.IsServer() ? "client" : "server")}.");

			check.ReceivedMinimumRequiredVersion = minimumRequiredVersion;
			check.ReceivedCurrentVersion = currentVersion;
			if (ZNet.instance.IsServer() && check.IsVersionOk()) check.ValidatedClients.Add(rpc);

			matched = true;
		}

		if (!matched) {
			pkg.SetPos(0);
			if (original is not null) {
				original(rpc, pkg);
				if (pkg.GetPos() == 0) notProcessedNames.Add(guid, currentVersion);
			}
		}
	}

	[HarmonyPatch(typeof(ZNet), "RPC_PeerInfo")]
	[HarmonyPrefix]
	private static bool RPC_PeerInfo(ZRpc rpc, ZNet __instance)
	{
		var failedChecks = __instance.IsServer() ? GetFailedServer(rpc) : GetFailedClient();
		if (failedChecks.Length == 0) return true;

		foreach (var check in failedChecks) Debug.LogWarning(check.Error(rpc));

		if (__instance.IsServer())
			DisconnectClient(rpc);
		else
			Logout();
		return false;
	}

	[HarmonyPatch(typeof(ZNet), "OnNewConnection")]
	[HarmonyPrefix]
	private static void RegisterAndCheckVersion(ZNetPeer peer, ZNet __instance)
	{
		notProcessedNames.Clear();

		var rpcFunctions = (IDictionary)typeof(ZRpc).GetField("m_functions", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(peer.m_rpc);
		if (rpcFunctions.Contains("ServerSync VersionCheck".GetStableHashCode())) {
			var function = rpcFunctions["ServerSync VersionCheck".GetStableHashCode()];
			var action = (Action<ZRpc, ZPackage>)function.GetType().GetField("m_action", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(function);
			peer.m_rpc.Register<ZPackage>("ServerSync VersionCheck", (rpc, pkg) => CheckVersion(rpc, pkg, action));
		}
		else {
			peer.m_rpc.Register<ZPackage>("ServerSync VersionCheck", CheckVersion);
		}

		foreach (var check in versionChecks) {
			check.Initialize();
			// If the mod is not required, then it's enough for only one side to do the check.
			if (!check.ModRequired && !__instance.IsServer()) continue;

			Debug.Log($"Sending {check.DisplayName} version {check.CurrentVersion} and minimum version {check.MinimumRequiredVersion} to the {(__instance.IsServer() ? "client" : "server")}.");

			ZPackage zpackage = new();
			zpackage.Write(check.Name);
			zpackage.Write(check.MinimumRequiredVersion);
			zpackage.Write(check.CurrentVersion);
			peer.m_rpc.Invoke("ServerSync VersionCheck", zpackage);
		}
	}

	[HarmonyPatch(typeof(ZNet), nameof(ZNet.Disconnect))]
	[HarmonyPrefix]
	private static void RemoveDisconnected(ZNetPeer peer, ZNet __instance)
	{
		if (!__instance.IsServer()) return;
		foreach (var check in versionChecks) check.ValidatedClients.Remove(peer.m_rpc);
	}

	[HarmonyPatch(typeof(FejdStartup), "ShowConnectError")]
	[HarmonyPostfix]
	private static void ShowConnectionError(FejdStartup __instance)
	{
		if (!__instance.m_connectionFailedPanel.activeSelf || ZNet.GetConnectionStatus() != ZNet.ConnectionStatus.ErrorVersion) return;
		var failedChecks = GetFailedClient();
		if (failedChecks.Length > 0) {
			var error = string.Join("\n", failedChecks.Select(check => check.Error()));
			__instance.m_connectionFailedError.text += "\n" + error;
		}

		foreach (var kv in notProcessedNames.OrderBy(kv => kv.Key))
			if (!__instance.m_connectionFailedError.text.Contains(kv.Key))
				__instance.m_connectionFailedError.text += $"\n{kv.Key} (Version: {kv.Value})";
	}
}