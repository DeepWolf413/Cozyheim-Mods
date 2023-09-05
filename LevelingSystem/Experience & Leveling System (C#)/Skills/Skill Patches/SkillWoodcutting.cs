using HarmonyLib;

namespace Cozyheim.LevelingSystem;

internal class SkillWoodcutting : SkillBase
{
	public static SkillWoodcutting Instance;

	public SkillWoodcutting(int maxLevel, float bonusPerLevel, string iconName, string displayName, string unit = "", float baseBonus = 0f) : base(maxLevel, bonusPerLevel, iconName, displayName, unit, baseBonus)
	{
		skillType = SkillType.Woodcutting;
		Instance = this;
	}


	[HarmonyPatch]
	private class PatchClass
	{
		[HarmonyPostfix]
		[HarmonyPatch(typeof(ItemDrop.ItemData), "GetDamage", typeof(int), typeof(float))]
		private static void ItemData_GetDamage_Woodcutting_Postfix(ItemDrop.ItemData __instance, ref HitData.DamageTypes __result)
		{
			if (Instance == null) return;

			if (__instance.m_shared.m_skillType == Skills.SkillType.Axes) {
				var multiplier = 1 + Instance.level * Instance.bonusPerLevel / 100;
				__result.m_chop *= multiplier;
			}
		}
	}
}