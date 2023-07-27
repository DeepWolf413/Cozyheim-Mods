using HarmonyLib;

namespace Cozyheim.LevelingSystem;

internal class SkillHP : SkillBase
{
	public static SkillHP Instance;

	public SkillHP(int maxLevel, float bonusPerLevel, string iconName, string displayName, string unit = "", float baseBonus = 0f) : base(maxLevel, bonusPerLevel, iconName, displayName, unit, baseBonus)
	{
		skillType = SkillType.HP;
		Instance = this;
	}


	[HarmonyPatch]
	private class PatchClass
	{
		[HarmonyPostfix]
		[HarmonyPatch(typeof(Player), "GetBaseFoodHP")]
		public static void Player_GetBaseFoodHP_Postfix(ref float __result)
		{
			if (Instance == null) return;

			__result += Instance.level * Instance.bonusPerLevel;
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(Player), "GetTotalFoodValue")]
		public static void Player_GetTotalFoodValue_Postfix(ref float hp)
		{
			if (Instance == null || Main.modJewelcraftingLoaded) return;

			hp += Instance.level * Instance.bonusPerLevel;
		}
	}
}