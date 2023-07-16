using System;
using System.Collections.Generic;
using Cozyheim.API;
using HarmonyLib;

namespace Cozyheim.DifficultyScaler.Patches;

[HarmonyPatch(typeof(Character))]
internal static class CharacterPatch
{
    [HarmonyPatch("Awake"), HarmonyPostfix]
    private static void Setup(Character __instance)
    {
        if (__instance == null) {
            return;
        }

        if (__instance.m_faction == Character.Faction.Players) {
            return;
        }

        if (__instance.IsTamed()) {
            return;
        }

        if (!__instance.TryGetComponent<ZNetView>(out var nview) || !nview.IsOwner()) {
            return;
        }

        if (nview.GetZDO() == null) {
            return;
        }

        if (Main.TryGetMonsterBaseHealthOverride(__instance.name, out float baseHealthOverrideValue)) {
            __instance.SetMaxHealth(baseHealthOverrideValue);
        }

        if (!__instance.TryGetComponent(out DifficultyScalerBase difficultyScalerComponent)) {
            difficultyScalerComponent = __instance.gameObject.AddComponent<DifficultyScalerBase>();
        }

        if (difficultyScalerComponent == null) {
            ConsoleLog.Print($"{nameof(difficultyScalerComponent)} is null!", LogType.Error);
            return;
        }

        // Setup multipliers for the difficulty scaler component.
        var multipliers = new Dictionary<DifficultyScalerMultiplier, float>
        { { DifficultyScalerMultiplier.BiomeMultiplier, Main.GetBiomeMultiplier(Heightmap.FindBiome(__instance.transform.position)) },
          { DifficultyScalerMultiplier.BossKillMultiplier, Main.GetCalculatedBossKillMultiplier() },
          { DifficultyScalerMultiplier.NightMultiplier, EnvMan.instance.IsNight() ? Main.nightMultiplier.Value : 0f },
          { DifficultyScalerMultiplier.HealthMultiplier, Main.overallHealthMultipler.Value },
          { DifficultyScalerMultiplier.DamageMultiplier, Main.overallDamageMultipler.Value },
          { DifficultyScalerMultiplier.StarMultiplier, Main.starMultiplier.Value } };

        foreach (var multiplier in multipliers) {
            if (!Main.IsMultiplierEnabled(multiplier.Key)) {
                continue;
            }

            difficultyScalerComponent.SetMultiplier(multiplier.Key, multiplier.Value);
        }
    }
    
    [HarmonyPatch("ApplyDamage"), HarmonyPrefix]
    private static void Character_ApplyDamage_Prefix(Character __instance, ref HitData hit)
    {
        if (hit.HaveAttacker()) {
            if (__instance.m_faction == Character.Faction.Players) {
                float startDamage = hit.GetTotalDamage();
                float multiplier = 1f;

                if (Main.TryGetMonsterDamage(hit.GetAttacker().name, out float value)) {
                    float oldBaseDamage = hit.GetTotalDamage();
                    hit.ApplyModifier(value);
                    ConsoleLog.Print("(ApplyDamage) " + hit.GetAttacker().name + " base damage adjusted to " + (value * 100f).ToString("N0") + "% of normal damage.");
                    ConsoleLog.Print($"-> Base damage: {oldBaseDamage} -> {hit.GetTotalDamage()}", LogType.Info);
                }
                
                if (!hit.GetAttacker().TryGetComponent(out DifficultyScalerBase difficultyScalerComponent)) {
                    ConsoleLog.Print($"{__instance.name} -> (ApplyDamage) Didn't find attacker's difficultyScalarBase. {hit.GetAttacker().name}", LogType.Error);
                    return;
                }
                
                var enabledMultipliers = new List<DifficultyScalerMultiplier>();
                var multiplierTypes = Enum.GetValues(typeof(DifficultyScalerMultiplier));
                foreach (DifficultyScalerMultiplier multiplierType in multiplierTypes) {
                    if (multiplierType == DifficultyScalerMultiplier.HealthMultiplier || !Main.IsMultiplierEnabled(multiplierType)) {
                        continue;
                    }

                    enabledMultipliers.Add(multiplierType);
                    ConsoleLog.Print($"{__instance.name} -> (ApplyDamage) {multiplierType.ToString()} Bonus: +{difficultyScalerComponent.GetMultiplier(multiplierType) * 100f:N0}%", LogType.Info);
                }

                multiplier += difficultyScalerComponent.GetSumOfMultipliers(enabledMultipliers);
                hit.ApplyModifier(multiplier);

                ConsoleLog.Print(__instance.name + " -> (ApplyDamage) Total bonus: +" + ((multiplier - 1f) * 100f).ToString("N0") + "%", LogType.Info);
                ConsoleLog.Print(__instance.name + " -> (ApplyDamage) Damage: " + startDamage + " -> " + hit.GetTotalDamage(), LogType.Info);
            }
        }
    }
}