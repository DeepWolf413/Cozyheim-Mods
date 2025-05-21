using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jotunn.Utils;

namespace Cozyheim.LevelingSystem;

public sealed class LevelXpTable
{
    private int[] _levels;
    private readonly string _configFolderPath;
    private readonly string _fallbackConfigFilePath;

    public int MaxLevel => _levels.Length;
    
    public LevelXpTable(string configFolderPath, string fallbackConfigFilePath)
    {
        _configFolderPath = configFolderPath;
        _fallbackConfigFilePath = fallbackConfigFilePath;
        LoadConfig();
    }

    private void LoadConfig()
    {
        if (!Directory.Exists(_configFolderPath))
        {
            LoadFallbackConfig();
            return;
        }
        
        var jsonFiles = Directory.GetFiles(_configFolderPath, "*.json", SearchOption.TopDirectoryOnly);
        foreach (var jsonFile in jsonFiles)
        {
            string json = File.ReadAllText(jsonFile);
            var xpTable = SimpleJson.SimpleJson.DeserializeObject<IReadOnlyDictionary<string, int>>(json);
            if (xpTable.Count == 0)
            {
                continue;
            }

            _levels = xpTable.Values.ToArray();
            Jotunn.Logger.LogDebug($"Successfully loaded the config for level xp table: '{jsonFile}'");
            break;
        }
    }

    /// <summary>
    /// Loads the fallback configuration for the level experience table.
    /// This method is called when the primary configuration files cannot be located or parsed.
    /// It reads and deserializes a fallback JSON resource to ensure the level XP table is populated
    /// with default values, avoiding runtime issues.
    /// </summary>
    /// <remarks>
    /// If the fallback configuration cannot be loaded or is invalid, an error is logged and the XP table remains uninitialized.
    /// </remarks>
    private void LoadFallbackConfig()
    {
        string json = AssetUtils.LoadTextFromResources(_fallbackConfigFilePath);
        var xpTable = SimpleJson.SimpleJson.DeserializeObject<Dictionary<string, int>>(json);
        if (xpTable.Count == 0)
        {
            Jotunn.Logger.LogError("Failed to load the fallback config for level xp table.");
            return;
        }

        _levels = xpTable.Values.ToArray();
    }

    /// <summary>
    /// Retrieves the maximum experience points required to reach a specified level.
    /// This method returns the corresponding XP value from the level XP table for the given level,
    /// ensuring level constraints are respected.
    /// </summary>
    /// <param name="level">The level for which the maximum XP is requested. Must be a positive integer within the valid level range.</param>
    /// <returns>
    /// The maximum XP required to reach the specified level. If the level is out of range, returns 1 as a default value.
    /// Logs an error if the level is invalid.
    /// </returns>
    public int GetMaxXpAtLevel(int level)
    {
        int levelIndex = level - 1;
        if (levelIndex < 0 || levelIndex >= _levels.Length)
        {
            Jotunn.Logger.LogError($"Level {level} is out of range. Max level is {_levels.Length}.");
            return 1;
        }
        
        return _levels[levelIndex];
    }
}