using System;
using System.Collections.Generic;
using HarmonyLib;
using Jotunn;
using Jotunn.Utils;

namespace Cozyheim.LevelingSystem
{
    public sealed class ModRpcRegistry
    {
        public struct RegistryEntry
        {
            public enum RpcType
            {
                ServerOnly = 0,
                ClientOnly,
                Both
            }
            
            public string RPCId { get; }
            public RpcType RPCType { get; }
            public Action<long, ZPackage> TargetFunction { get; }

            public RegistryEntry(string rpcId, RpcType rpcType, Action<long, ZPackage> receiveFunction) : this(rpcId, rpcType, receiveFunction, receiveFunction)
            { }
        
            public RegistryEntry(string rpcId, RpcType rpcType, Action<long, ZPackage> clientReceiveFunction, Action<long, ZPackage> serverReceiveFunction)
            {
                RPCId = rpcId;
                RPCType = rpcType;
                TargetFunction = (senderId, package) =>
                {
                    bool isServer = ZNet.instance.IsServer();
                    if (isServer && rpcType is RpcType.Both or RpcType.ServerOnly)
                    {
                        serverReceiveFunction?.Invoke(senderId, package);
                        return;
                    }
                    
                    bool isClient = ZNet.instance.IsClientInstance() || ZNet.instance.IsLocalInstance();
                    if (isClient && rpcType is RpcType.Both or RpcType.ClientOnly)
                    {
                        clientReceiveFunction?.Invoke(senderId, package);
                    }
                };
            }
        }
        
        private static ModRpcRegistry instance;

        public static ModRpcRegistry Instance => instance ??= new ModRpcRegistry();
        
        private List<ModRpcRegistry> Registries { get; } = new();
        
        private Dictionary<string, RegistryEntry> Entries { get; } = new();

        private ModRpcRegistry()
        {
            Registries.Add(this);
        }
        
        public IReadOnlyList<ModRpcRegistry> GetAllRegistries() => Registries;

        /// <summary>
        /// Registers all <see cref="Entries"/> to the game's rpc registry.
        /// </summary>
        public void PushToGame()
        {
            Entries.Do(rpcEntry =>
            {
                Jotunn.Logger.LogDebug($"Registering rpc '{rpcEntry.Value.RPCId}' with game");
                ZRoutedRpc.instance.Register(rpcEntry.Value.RPCId, rpcEntry.Value.TargetFunction);
            });
        }

        public void SendServerRpc(string name, ZPackage package)
        {
            if (!Entries.TryGetValue(name, out var rpcEntry))
            {
                return;
            }
            
            ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), rpcEntry.RPCId, package);
        }
        
        public void SendTargetRpc(string name, ZPackage package, long targetPeerId)
        {
            if (!Entries.TryGetValue(name, out var rpcEntry))
            {
                return;
            }
            
            if (targetPeerId == ZRoutedRpc.Everybody)
            {
                Jotunn.Logger.LogDebug($"Sending rpc '{name}' to everybody");
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, rpcEntry.RPCId, package);
                return;
            }
            
            Jotunn.Logger.LogDebug($"Sending rpc '{name}' to '{targetPeerId}'");
            ZRoutedRpc.instance.InvokeRoutedRPC(targetPeerId, rpcEntry.RPCId, package);
        }
        
        public void AddRpc(string name, RegistryEntry.RpcType rpcType, Action<long, ZPackage> function)
        {
            var sourceMod = BepInExUtils.GetSourceModMetadata();
            var entry = new RegistryEntry($"{sourceMod.GUID}!{name}", rpcType, function);
            Entries.Add(name, entry);
        }
        
        public void AddRpc(string name, RegistryEntry.RpcType rpcType, Action<long, ZPackage> clientFunction, Action<long, ZPackage> serverFunction)
        {
            var sourceMod = BepInExUtils.GetSourceModMetadata();
            var entry = new RegistryEntry($"{sourceMod.GUID}!{name}", rpcType, clientFunction, serverFunction);
            Entries.Add(name, entry);
        }
    }
}