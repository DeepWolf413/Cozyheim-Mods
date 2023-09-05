using HarmonyLib;

namespace Cozyheim.LevelingSystem;

internal class SkillMining : SkillBase
{
	public static SkillMining Instance;

	public SkillMining(int maxLevel, float bonusPerLevel, string iconName, string displayName, string unit = "", float baseBonus = 0f) : base(maxLevel, bonusPerLevel, iconName, displayName, unit, baseBonus)
	{
		skillType = SkillType.Mining;
		Instance = this;
	}


	[HarmonyPatch]
	private class PatchClass
	{
		[HarmonyPostfix]
		[HarmonyPatch(typeof(ItemDrop.ItemData), "GetDamage", typeof(int), typeof(float))]
		private static void ItemData_GetDamage_Mining_Postfix(ItemDrop.ItemData __instance, ref HitData.DamageTypes __result)
		{
			if (Instance == null) return;

			if (__instance.m_shared.m_skillType == Skills.SkillType.Pickaxes) {
				var multiplier = 1 + Instance.level * Instance.bonusPerLevel / 100;
				__result.m_pickaxe *= multiplier;
			}
		}
	}
}