using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;

namespace CharacterProgressionMod.Core
{
    public class ModResources
    {
        private const string AssetsPath = "Assets/_Leveling System/";
        private readonly PluginConfig _config;

        public ModResources(PluginConfig config)
        {
            _config = config;
            AssetBundle = AssetUtils.LoadAssetBundleFromResources("leveling_system");
            PrefabManager.OnVanillaPrefabsAvailable += LoadAssets;

            var maxLevel = _config.MaxLevel.Value;
            var initialMaxExperience = _config.InitialMaxExperience.Value;
            var maxExperienceIncreaseCurve = _config.MaxExperienceModifierFormula.Value;

            LevelExperienceTable = new LevelExperienceTable(new LevelTableGenerationSettings(maxLevel, initialMaxExperience, maxExperienceIncreaseCurve));
        }

        public AssetBundle AssetBundle { get; }
        public LevelExperienceTable LevelExperienceTable { get; private set; }

        public void SetLevelExperienceTable(LevelExperienceTable experienceTable)
        {
            LevelExperienceTable = experienceTable;
        }

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