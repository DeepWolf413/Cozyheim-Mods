using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using BepInEx.Configuration;
using UnityEngine;

namespace CharacterProgressionMod.Core
{
    public sealed class PluginConfig
    {
        private const string CustomFolder = "custom";
        private const string CategoriesFolderName = "categories";

        private readonly ConfigFile _configFile;

        /// <summary>
        /// Binds config values to the specified <see cref="ConfigFile"/>.
        /// </summary>
        /// <param name="configFile">The <see cref="ConfigFile"/> that will used for binding.</param>
        public PluginConfig(ConfigFile configFile)
        {
            _configFile = configFile;
            _configFile.SaveOnConfigSet = true;

            var configBuilder = new ConfigEntryBuilder(_configFile);
            ShowLevel = configBuilder
                        .SetSection("HUD")
                        .SetKey()
                        .Build(true);

            ShowXp = configBuilder
                     .SetKey()
                     .Build(true);

            XpBarScale = configBuilder
                         .SetKey()
                         .SetAcceptableValues(new AcceptableValueRange<float>(5.0f, 100.0f))
                         .Build(100.0f);

            XpBarPosition = configBuilder
                            .SetKey()
                            .Build(Vector2.zero);

            ShowScrollbar = configBuilder
                            .SetSection("Menus")
                            .SetKey()
                            .SetDescription(
                                "Unchecking this only disables the graphics, you will still be able to scroll.")
                            .Build(true);


            MaxLevel = configBuilder
                       .SetSection("Progression")
                       .SetKey()
                       .RequireAdmin()
                       .SetAcceptableValues(new AcceptableValueRange<int>(1, 999))
                       .Build(50);

            InitialMaxExperience = configBuilder
                                   .SetKey()
                                   .RequireAdmin()
                                   .Build(75);

            MaxExperienceModifierFormula = configBuilder
                                           .SetKey()
                                           .RequireAdmin()
                                           .Build("0=[10,15%]");

            SkillPointsPerLevel = configBuilder
                                  .SetKey()
                                  .RequireAdmin()
                                  .Build(1.0f);
            
            LevelUpVFX = configBuilder
                         .SetSection("VFX")
                         .SetKey()
                         .Build(true);

        }

        public static string CustomMiningDirectory => Path.Combine(CustomFolder, "mining");

        public static string CustomMiningCategoriesDirectory =>
            Path.Combine(CustomFolder, "mining", CategoriesFolderName);

        public static string CustomWoodcuttingDirectory => Path.Combine(CustomFolder, "woodcutting");

        public static string CustomWoodcuttingCategoriesDirectory =>
            Path.Combine(CustomFolder, "woodcutting", CategoriesFolderName);

        public static string CustomCreaturesDirectory => Path.Combine(CustomFolder, "creatures");
        public static string CustomPickablesDirectory => Path.Combine(CustomFolder, "pickables");

        public static string CustomPickablesCategoriesDirectory =>
            Path.Combine(CustomFolder, "pickables", CategoriesFolderName);

        public static string CustomPlayerDirectory => Path.Combine(CustomFolder, "player");
        
        // VFX
        public ConfigEntry<bool> LevelUpVFX { get; }

        // Skills Menu
        public ConfigEntry<bool> ShowScrollbar { get; }
        public ConfigEntry<float> SkillPointsPerLevel { get; }

        // Player Experience
        public ConfigEntry<int> MaxLevel { get; }
        public ConfigEntry<int> InitialMaxExperience { get; }
        public ConfigEntry<string> MaxExperienceModifierFormula { get; }

        // Heads-up Display
        public ConfigEntry<bool> ShowLevel { get; }
        public ConfigEntry<bool> ShowXp { get; }
        public ConfigEntry<float> XpBarScale { get; }
        public ConfigEntry<Vector2> XpBarPosition { get; }

        private class ConfigEntryBuilder
        {
            private readonly ConfigFile _configFile;
            private readonly EntrySettings _entrySettings = new();
            private string _description = string.Empty;
            private AcceptableValueBase _acceptableValues = null;
            private ConfigurationManagerAttributes _attributes = new();
            
            public ConfigEntryBuilder(ConfigFile configFile)
            {
                _configFile = configFile;
            }

            public ConfigEntryBuilder SetSection(string section)
            {
                _entrySettings.Section = section;
                return this;
            }
            
            public ConfigEntryBuilder SetKey([CallerMemberName] string key = "")
            {
                _entrySettings.Key = key;
                return this;
            }
            
            public ConfigEntryBuilder SetDescription(string description)
            {
                _description = description;
                return this;
            }

            public ConfigEntryBuilder SetAcceptableValues(AcceptableValueBase acceptableValues)
            {
                _acceptableValues = acceptableValues;
                return this;
            }

            public ConfigEntryBuilder RequireAdmin()
            {
                _attributes.IsAdminOnly = true;
                return this;
            }
            
            public ConfigEntry<TValue> Build<TValue>(TValue defaultValue = default)
            {
                _entrySettings.Description = new ConfigDescription(_description, _acceptableValues, _attributes);
                var configEntry = _configFile.Bind(_entrySettings.Section, _entrySettings.Key, defaultValue, _entrySettings.Description);
                Reset();
                return configEntry;
            }

            private void Reset()
            {
                _description = string.Empty;
                _acceptableValues = null;
                _attributes = new ConfigurationManagerAttributes();
            }

            private class EntrySettings
            {
                public string Section { get; set; }
                public string Key { get; set; }
                public ConfigDescription Description { get; set; }
            }
        }
    }
}