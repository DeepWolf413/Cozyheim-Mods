using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using HarmonyLib;
using Jotunn;
using Jotunn.Utils;
using MonoMod.Utils;
using ReflectionHelper = Jotunn.Utils.ReflectionHelper;

namespace Cozyheim.LevelingSystem;

public sealed class XpTable
{
    private const string EmbeddedConfigPath = "LevelingSystem.Resources.default_configs";
    private readonly Dictionary<string, int> _entries = new();
    private readonly Dictionary<string, string> _groups = new();
    
    public string NameId { get; }
    
    public bool AllowGroups { get; }
    
    public string CustomConfigFolderPath { get; }
    
    public string CustomGroupsFolderPath { get; }
    
    public XpTable(Assembly resourceAssembly, string customFolderPath, bool allowGroups)
    {
        if (string.IsNullOrEmpty(customFolderPath))
        {
            Jotunn.Logger.LogError("A critical error occurred during the initialization of the leveling system. Please report this to the mod author.");
            throw new ArgumentException($"The {nameof(customFolderPath)} parameter cannot be null or empty.");
        }
        
        NameId = Path.GetFileName(customFolderPath).ToLowerInvariant();
        AllowGroups = allowGroups;
        CustomConfigFolderPath = customFolderPath;
        CustomGroupsFolderPath = Path.Combine(customFolderPath, "groups");
        
        VerifyAndSetupConfigDirectory();

        if (resourceAssembly is null)
        {
            Jotunn.Logger.LogError("A critical error occurred during the initialization of the leveling system. Please report this to the mod author.");
            throw new ArgumentNullException($"The {nameof(resourceAssembly)} parameter cannot be null.");
        }
        
        LoadEmbeddedResources(resourceAssembly);
        LoadCustomResources();
        
        Jotunn.Logger.LogDebug($"Loaded {NameId} xp table with {_entries.Count} entries.");
        if (AllowGroups)
        {
            Jotunn.Logger.LogDebug($"Loaded {NameId} groups with {_groups.Count} entries.");
        }
    }
    
    private void VerifyAndSetupConfigDirectory()
    {
        var doesCustomConfigFolderExist = Directory.Exists(CustomConfigFolderPath);
        var doesGroupsFolderExist = Directory.Exists(CustomGroupsFolderPath);

        if (doesCustomConfigFolderExist && doesGroupsFolderExist)
        {
            return;
        }
        
        Jotunn.Logger.LogDebug($"Creating directories for custom {NameId} configs.");
        Directory.CreateDirectory(CustomConfigFolderPath);
        Directory.CreateDirectory(CustomGroupsFolderPath);
    }

    private void LoadEmbeddedResources(Assembly resourceAssembly)
    {
        const string xpTableSubPath = "xp_tables";
        const string groupsSubPath = "groups";
        
        var embeddedResourceNames = resourceAssembly.GetManifestResourceNames();
        foreach (var embeddedResourceName in embeddedResourceNames)
        {
            string resourcePathMatchPattern = $@"^{EmbeddedConfigPath}\.{NameId}\.{xpTableSubPath}\.(?:[\w-]+).json";
            bool isFileXpTable = Regex.IsMatch(embeddedResourceName, resourcePathMatchPattern);
            if (isFileXpTable)
            {
                string json = AssetUtils.LoadTextFromResources(embeddedResourceName, resourceAssembly);
                var entriesFromJson = SimpleJson.SimpleJson.DeserializeObject<Dictionary<string, int>>(json);
                if (entriesFromJson.Count == 0)
                {
                    Jotunn.Logger.LogError($"Skipped loading embedded {NameId} xp table file at '{embeddedResourceName}' - no entries found. Please report this to the mod author.");
                    continue;
                }
                
                _entries.AddRange(entriesFromJson);
                continue;
            }
            
            if (!AllowGroups)
            {
                continue;
            }
            
            string groupPathMatchPattern = $@"^{EmbeddedConfigPath}\.{NameId}\.{groupsSubPath}\.(?:[\w-]+).json";
            bool isFileGroup = Regex.IsMatch(embeddedResourceName, groupPathMatchPattern);
            if (isFileGroup)
            {
                string json = AssetUtils.LoadTextFromResources(embeddedResourceName, resourceAssembly);
                var groupsFromJson = SimpleJson.SimpleJson.DeserializeObject<Dictionary<string, string[]>>(json);
                if (groupsFromJson.Count == 0)
                {
                    Jotunn.Logger.LogError($"Skipped loading embedded {NameId} group file at '{embeddedResourceName}' - no entries found. Please report this to the mod author.");
                    continue;
                }
                
                groupsFromJson.Do(pair =>
                {
                    pair.Value.Do(groupEntry =>
                    {
                        _groups[groupEntry] = pair.Key;
                    });
                });
            }
        }
    }

    private void LoadCustomResources()
    {
        if (!Directory.Exists(CustomConfigFolderPath))
        {
            VerifyAndSetupConfigDirectory();
			return;
        }
        
        var jsonFilePaths = Directory.GetFiles(CustomConfigFolderPath, "*.json", SearchOption.TopDirectoryOnly);
        if (jsonFilePaths.Length == 0)
        {
            Jotunn.Logger.LogDebug($"Skipping loading custom {NameId} configs - no files found in the custom config folder.");
            return;
        }
        
        // Load xp tables
        foreach (var jsonFilePath in jsonFilePaths)
        {
            var json = File.ReadAllText(jsonFilePath);
            var loadedXpTable = SimpleJson.SimpleJson.DeserializeObject<Dictionary<string, int>>(json);
            if (loadedXpTable.Count == 0)
            {
                Logger.LogWarning($"Skipped loading custom {NameId} xp table file at '{jsonFilePath}' - no entries found.");
                continue;
            }

            loadedXpTable.Do(pair =>
            {
                _entries[pair.Key] = pair.Value;
            });
        }
        
        if (!AllowGroups)
        {
            return;
        }
        
        // Load groups
        if (!Directory.Exists(CustomGroupsFolderPath))
        {
            return;
        }
        
        jsonFilePaths = Directory.GetFiles(CustomGroupsFolderPath, "*.json", SearchOption.TopDirectoryOnly);
        foreach (var jsonFilePath in jsonFilePaths)
        {
            var json = File.ReadAllText(jsonFilePath);
            var groups = SimpleJson.SimpleJson.DeserializeObject<Dictionary<string, string[]>>(json);
            if (groups.Count == 0)
            {
                Logger.LogWarning($"Skipped loading custom {NameId} group file at '{jsonFilePath}' - no entries found.");
                continue;
            }

            groups.Do(pair =>
            {
                pair.Value.Do(groupEntry =>
                {
                    _groups[groupEntry] = pair.Key;
                });
            });
        }
    }

    public void ReloadResources()
    {
        _entries.Clear();
        var resourceAssembly = ReflectionHelper.GetCallingAssembly();
        LoadEmbeddedResources(resourceAssembly);
        LoadCustomResources();
    }

    public int GetXp(string key)
    {
        key = key.Replace("(Clone)", "");
        if (_entries.TryGetValue(key, out var xp))
        {
            Jotunn.Logger.LogDebug($"Found xp for '{key}': {xp} xp");
            return xp;
        }

        if (!AllowGroups || _groups.Count == 0)
        {
            Jotunn.Logger.LogDebug($"Skipping group check - groups are not allowed for the {NameId} xp table.");
            return 0;
        }

        // Check if the key exists within a group
        if (!_groups.TryGetValue(key, out var groupId))
        {
            Jotunn.Logger.LogDebug($"Failed to find a group for '{key}'.");
            return 0;
        }
        
        Jotunn.Logger.LogDebug($"Found group '{groupId}' for '{key}'.");
        return _entries.TryGetValue(groupId, out xp) ? xp : 0;
    }
}