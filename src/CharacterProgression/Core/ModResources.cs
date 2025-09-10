using CharacterProgressionMod.Loaders;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace CharacterProgressionMod.Core
{
    internal class ModResources
    {
        private const string AssetsPath = "Assets/_Leveling System/";
        private readonly PluginConfig _config;

        public ModResources(PluginConfig config)
        {
            _config = config;
            AssetBundle = AssetUtils.LoadAssetBundleFromResources("leveling_system");
            PrefabManager.OnVanillaPrefabsAvailable += LoadAssets;

            if (_config.UseCustomExperienceTable.Value) {
                // TODO: Implement loading for custom experience table.
                /*const string levelTablePath = "default_configs.player.xp_tables.default.json";
                var loadedJson = EmbeddedResourceLoader.Load(levelTablePath);
                ExperienceTable = new ExperienceTable(loadedJson);*/
            }
            else {
                var experienceTableGenerator = new LevelExperienceTableGenerator(_config.MaxLevel.Value, _config.MaxLevelTotalExperience.Value, _config.ExperienceFormula.Value);
                LevelExperienceTable = experienceTableGenerator.Generate();
            }
        }

        public AssetBundle AssetBundle { get; }
        public LevelExperienceTable LevelExperienceTable { get; private set; }

        private void LoadAssets()
        {
            // Canvas UI with the XP Bar
            var levelSystem = AssetBundle.LoadAsset<GameObject>(AssetsPath + "Prefabs/LevelingSystemUI.prefab");
            PrefabManager.Instance.AddPrefab(levelSystem);

            var levelUpEffect = AssetBundle.LoadAsset<GameObject>(AssetsPath + "Prefabs/LevelUpEffectNew.prefab");
            PrefabManager.Instance.AddPrefab(levelUpEffect);

            var criticalHitEffect = AssetBundle.LoadAsset<GameObject>(AssetsPath + "Prefabs/CriticalHitEffect.prefab");
            PrefabManager.Instance.AddPrefab(criticalHitEffect);

            var skillUI = AssetBundle.LoadAsset<GameObject>(AssetsPath + "Prefabs/SkillUI.prefab");
            PrefabManager.Instance.AddPrefab(skillUI);

            var trainingDummy = AssetBundle.LoadAsset<GameObject>(AssetsPath + "Prefabs/LevelingDummy.prefab");
            PieceManager.Instance.AddPiece(new CustomPiece(trainingDummy, PieceTables.Hammer, false));

            var trainingDummyStrawman =
                AssetBundle.LoadAsset<GameObject>(AssetsPath + "Prefabs/LevelingDummyStrawman.prefab");
            PieceManager.Instance.AddPiece(new CustomPiece(trainingDummyStrawman, PieceTables.Hammer, false));

            PrefabManager.OnVanillaPrefabsAvailable -= LoadAssets;
        }
    }
}