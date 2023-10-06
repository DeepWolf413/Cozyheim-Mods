using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using Log = Jotunn.Logger;

namespace Cozyheim.LevelingSystem;

internal class ConsoleLog : ConsoleCommand
{
	private static List<string> commandArgs;

	public static CustomRPC rpc_ReloadConfigServer;
	public static CustomRPC rpc_ReloadConfigClient;
	public static CustomRPC rpc_SetLevel;

	private readonly Dictionary<string, Action> _commands = new();

	private readonly CommandList[] commandList =
	{ new("ReloadConfig", ReloadConfig),
	  new("ResetOwnLevel", ResetOwnLevel),
	  new("SetLevel", SetLevel),
	  new("LevelUp", LevelUp) };

	public override string Name => Main.modName;
	public override string Help => "Commands for '" + Name + "'";
	public override bool IsCheat => true;
	public override bool IsSecret => false;
	public override bool IsNetwork => true;
	public override bool OnlyServer => false;

	public static void Init()
	{
		if (rpc_ReloadConfigServer == null) rpc_ReloadConfigServer = NetworkManager.Instance.AddRPC("ReloadConfigServer", RPC_ReloadConfigServer, RPC_ReloadConfigServer);
		if (rpc_ReloadConfigClient == null) rpc_ReloadConfigClient = NetworkManager.Instance.AddRPC("ReloadConfigClient", RPC_ReloadConfigClient, RPC_ReloadConfigClient);
		if (rpc_SetLevel == null) rpc_SetLevel = NetworkManager.Instance.AddRPC("SetLevel", RPC_SetLevel, RPC_SetLevel);
	}

	public static void LevelUp()
	{
		Print("Console LevelUp Command");
		NetworkHandler.LevelUpVFX();
	}

	public static void ReloadConfig()
	{
		// Only admins may use this command
		if (!IsUserAdmin()) {
			Print("Reloading Configs: Client side");
			ReloadAndUpdateAll();
		}
		else {
			Print("Reloading Configs: Server side");
			rpc_ReloadConfigServer.SendPackage(ZRoutedRpc.Everybody, new ZPackage());
		}
	}

	private static IEnumerator RPC_ReloadConfigClient(long sender, ZPackage package)
	{
		ReloadAndUpdateAll();
		yield return null;
	}

	private static IEnumerator RPC_ReloadConfigServer(long sender, ZPackage package)
	{
		if (ZNet.instance.IsServer()) {
			Main.configFile.Reload();
			rpc_ReloadConfigClient.SendPackage(ZRoutedRpc.Everybody, new ZPackage());
		}

		yield return null;
	}

	private static void ReloadAndUpdateAll()
	{
		XPTable.UpdatePlayerXPTable();
		XPTable.UpdateMonsterXPTable();
		XPTable.UpdatePickableXPTable();
		XPTable.UpdateMiningXPTable();
		XPTable.UpdateWoodcuttingXPTable();

		if (SkillManager.Instance != null) SkillManager.Instance.ReloadAllSkills();
	}

	private static void GetAll()
	{
		// Only admins may use this command
		if (!IsUserAdmin()) return;

		// Command code here
		var stringToPrint = XPManager.Instance.GetAllMonsterXpString();
		Print(stringToPrint);
	}

	private static void ResetOwnLevel() { SetPlayerLevel(1); }

	private static void SetPlayerLevel(int level)
	{
		if (XPManager.Instance != null && UIManager.Instance != null && SkillManager.Instance != null) {
			XPManager.Instance.SetPlayerLevel(level);
			XPManager.Instance.SetPlayerXP(0);
			UIManager.Instance.playerLevel = level;
			UIManager.Instance.playerXP = 0;
			SkillManager.Instance.SkillResetAll();
			UIManager.Instance.UpdateUI(true);
		}
	}

	private static void SetLevel()
	{
		// Only admins may use this command
		if (!IsUserAdmin()) return;

		if (commandArgs.Count >= 2)
			if (int.TryParse(commandArgs[0], out var level)) {
				var newPackage = new ZPackage();
				newPackage.Write(level);

				var playerName = "";
				for (var i = 1; i < commandArgs.Count; i++) {
					if (i > 1) playerName += " ";
					playerName += commandArgs[i];
				}

				newPackage.Write(playerName);

				rpc_SetLevel.SendPackage(ZRoutedRpc.Everybody, newPackage);
				return;
			}

		Debug.Log("* Incorrect Arguments * (Example: 'LevelingSystem SetLevel 10 Name of the player')");
	}

	private static IEnumerator RPC_SetLevel(long sender, ZPackage package)
	{
		var level = package.ReadInt();
		var playerName = package.ReadString();

		if (Player.m_localPlayer != null) {
			Print($"Trying to set \"{playerName}\"'s level to {level}");
			if (string.Equals(Player.m_localPlayer.GetPlayerName(), playerName, StringComparison.InvariantCultureIgnoreCase)) {
				if (level < 1) level = 1;

				if (level > XPTable.playerXPTable.Length + 1) level = XPTable.playerXPTable.Length + 1;

				SetPlayerLevel(level);
				Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Admin set your level to " + level);
				Print($"Admin has set your level to {level}");
			}
		}
		else {
			Print($"Invalid local player reference. IsServer?: {ZNet.instance.IsServer()}", LogType.Warning);
		}

		yield return null;
	}

	#region Console Setup

	private static bool IsUserAdmin()
	{
		if (!SynchronizationManager.Instance.PlayerIsAdmin && !ZNet.IsSinglePlayer) {
			Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Only admins are allowed to use this command");
			return false;
		}

		return true;
	}

	// Setup the list of available commands
	public override List<string> CommandOptionList()
	{
		var commands = new List<string>();
		foreach (var com in commandList) {
			if (!_commands.ContainsKey(com.name)) _commands.Add(com.name, com.action);
			commands.Add(com.name);
		}

		return commands;
	}

	// Check if the command exists and execute the associated method
	public override void Run(string[] args)
	{
		var command = args[0];

		commandArgs = args.ToList();
		commandArgs.RemoveAt(0);

		foreach (var com in _commands)
			if (com.Key.ToLower() == command.ToLower()) {
				com.Value();
				return;
			}

		Debug.Log("The command doesn't exist: '" + command + "'");
	}

	//    -----------------------
	//   ----- PRINT METHODS -----
	//    -----------------------

	internal static void Print(object printMsg, LogType type = LogType.Info, bool debugMode = true)
	{
		if (Main.debugEnabled.Value && debugMode) {
			var textToPrint = "[Time: " + Time.time.ToString("N0") + "] " + printMsg;
			switch (type) {
				case LogType.Info:
					Log.LogInfo(textToPrint);
					break;
				case LogType.Message:
					Log.LogMessage(textToPrint);
					break;
				case LogType.Warning:
					Log.LogWarning(textToPrint);
					break;
				case LogType.Error:
					Log.LogError(textToPrint);
					break;
				case LogType.Fatal:
					Log.LogFatal(textToPrint);
					break;
				default:
					Log.LogInfo(textToPrint);
					break;
			}
		}
	}

	internal static void Print(object printMsg, bool debugMode) { Print(printMsg, LogType.Info, debugMode); }

	internal static void PrintOverrideDebugMode(object printMsg, LogType type = LogType.Info)
	{
		var textToPrint = "[Time: " + Time.time.ToString("N0") + "] " + printMsg;
		switch (type) {
			case LogType.Info:
				Log.LogInfo(textToPrint);
				break;
			case LogType.Message:
				Log.LogMessage(textToPrint);
				break;
			case LogType.Warning:
				Log.LogWarning(textToPrint);
				break;
			case LogType.Error:
				Log.LogError(textToPrint);
				break;
			case LogType.Fatal:
				Log.LogFatal(textToPrint);
				break;
			default:
				Log.LogInfo(textToPrint);
				break;
		}
	}

	#endregion
}

internal class CommandList
{
	public Action action;
	public string name;

	public CommandList(string name, Action action)
	{
		this.name = name;
		this.action = action;
	}
}

internal enum LogType
{
	Info,
	Message,
	Error,
	Warning,
	Fatal
}