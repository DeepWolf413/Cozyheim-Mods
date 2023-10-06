using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Cozyheim.DifficultyScaler.Patches;

[HarmonyPatch(typeof(WearNTear))]
public static class WearNTearPatch
{
	[HarmonyPatch("RPC_Damage")]
	[HarmonyPrefix]
	private static void WearNTear_RPC_Damage_Prefix(ref HitData hit)
	{
		if (hit == null) {
			ConsoleLog.Print("Hit is null", LogType.Warning);
			return;
		}

		var attacker = hit.GetAttacker();
		if (attacker == null) {
			ConsoleLog.Print("Hit attacker is null", LogType.Warning);
			return;
		}

		if (!attacker.IsMonsterFaction(Time.time) || !hit.GetAttacker().IsBoss()) return;

		var multiplier = 1f;
		if (Main.TryGetMonsterDamage(attacker.name, out var value)) {
			ConsoleLog.Print("(RPC_Damage) Found: " + attacker.name + " = " + value);
			multiplier += value;
		}

		if (attacker.TryGetComponent(out DifficultyScalerBase difficultyScalerBase)) {
			var enabledMultipliers = new List<DifficultyScalerMultiplier>();
			var multiplierTypes = Enum.GetValues(typeof(DifficultyScalerMultiplier));
			foreach (DifficultyScalerMultiplier multiplierType in multiplierTypes) {
				if (multiplierType == DifficultyScalerMultiplier.HealthMultiplier || !Main.IsMultiplierEnabled(multiplierType)) continue;

				enabledMultipliers.Add(multiplierType);
				ConsoleLog.Print($"{attacker.name} -> {multiplierType.ToString()}: {difficultyScalerBase.GetMultiplier(multiplierType)}");
			}

			multiplier += difficultyScalerBase.GetSumOfMultipliers(enabledMultipliers);
			hit.ApplyModifier(1f / multiplier);
		}
	}
}