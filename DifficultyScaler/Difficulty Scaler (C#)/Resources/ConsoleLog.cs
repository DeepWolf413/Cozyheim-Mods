using Jotunn.Entities;
using Jotunn.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Log = Jotunn.Logger;

namespace Cozyheim.DifficultyScaler
{
    internal class ConsoleLog : ConsoleCommand
    {
        public override string Name => Main.modName;
        public override string Help => "Commands for 'Cozyheim: " + Name + "'";
        public override bool IsCheat => false;
        public override bool IsSecret => false;
        public override bool IsNetwork => true;
        public override bool OnlyServer => false;

        private Dictionary<string, Action> _commands = new Dictionary<string, Action>();
        private List<string> commandArgs;

        CommandList[] commandList = new CommandList[]
        {
            new CommandList("ReloadConfig", ReloadConfig)
        };

        private static void ReloadConfig()
        {
            // Only admins may use this command
            if (!IsUserAdmin()) return;

            // Command code here
            Main.configFile.Reload();
            Main.CreateMonsterDamageDictionary();
            Main.CreateMonsterHealthDictionary();
            Print("Config reloaded!", LogType.Message);
        }

        #region Console Setup
        private static bool IsUserAdmin()
        {
            if (!SynchronizationManager.Instance.PlayerIsAdmin)
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Only admins are allowed to use this command");
                return false;
            }

            return true;
        }

        // Setup the list of available commands
        public override List<string> CommandOptionList()
        {
            List<string> commands = new List<string>();
            foreach (CommandList com in commandList)
            {
                if (!_commands.ContainsKey(com.name))
                {
                    _commands.Add(com.name, com.action);
                }
                commands.Add(com.name);
            }
            return commands;
        }

        // Check if the command exists and execute the associated method
        public override void Run(string[] args)
        {
            string command = args[0];

            commandArgs = args.ToList();
            commandArgs.RemoveAt(0);

            foreach (KeyValuePair<string, Action> com in _commands)
            {
                if (com.Key.ToLower() == command.ToLower())
                {
                    com.Value();
                    return;
                }
            }

            Debug.Log("The command doesn't exist: '" + command + "'");
        }

        //    -----------------------
        //   ----- PRINT METHODS -----
        //    -----------------------

        internal static void Print(object printMsg, LogType type = LogType.Info, bool debugMode = true)
        {
            if (Main.debugEnabled.Value && debugMode)
            {
                string textToPrint = printMsg.ToString();
                switch (type)
                {
                    case LogType.Info:
                        if (Main.debugLevel.Value == LogType.Info) {
                            Log.LogInfo(textToPrint);
                        }
                        break;
                    case LogType.Message:
                        if (Main.debugLevel.Value <= LogType.Message) {
                            Log.LogMessage(textToPrint);
                        }
                        break;
                    case LogType.Warning:
                        if (Main.debugLevel.Value <= LogType.Warning) {
                            Log.LogWarning(textToPrint);
                        }
                        break;
                    case LogType.Error:
                        if (Main.debugLevel.Value <= LogType.Error) {
                            Log.LogError(textToPrint);
                        }
                        break;
                    case LogType.Fatal:
                        if (Main.debugLevel.Value <= LogType.Fatal) {
                            Log.LogFatal(textToPrint);
                        }
                        break;
                    default: Log.LogInfo(textToPrint); break;
                }
            }
        }
        internal static void Print(object printMsg, bool debugMode) => Print(printMsg, LogType.Info, debugMode);
        #endregion
    }

    internal class CommandList
    {
        public string name;
        public Action action;

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
}