using HarmonyLib;
using static ItemDrop;
using System.Collections.Generic;
using UnityEngine;

namespace Cozyheim.GearWithPersonality {
    internal class RegisterKills {

        [HarmonyPatch]
        private class Patch {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Character), "Damage")]
            private static void Character_Damage_Prefix(Character __instance, ref HitData hit, ZNetView ___m_nview) {
                if(!___m_nview.IsValid()) {
                    ConsoleLog.Print("Damage: ZNetView not valid!", LogType.Error);
                    return;
                }

                if(hit == null) {
                    ConsoleLog.Print("Damage: No HitData found!", LogType.Error);
                    return;
                }

                if(__instance == null) {
                    ConsoleLog.Print("Damage: No Character found!", LogType.Error);
                    return;
                }

                Character target = __instance;
                Character attacker = hit.GetAttacker();
                float totalDamage = hit.GetTotalDamage();

                if(target == null) {
                    ConsoleLog.Print("Damage: No target found!", LogType.Error);
                    return;
                }

                if(attacker == null) {
                    ConsoleLog.Print("Damage: No attacker found!", LogType.Error);
                    return;
                }

                if(!attacker.IsPlayer()) {
                    ConsoleLog.Print("Damage: Attacker not a Player!", LogType.Error);
                    return;
                }

                if(Player.m_localPlayer == null) {
                    ConsoleLog.Print("Damage: No local player found!", LogType.Error);
                    return;
                }

                if(totalDamage <= 0f) {
                    ConsoleLog.Print("Damage: Total damage is less than 0!", LogType.Error);
                    return;
                }

                Player player = attacker.GetComponent<Player>();
                if(player != Player.m_localPlayer) {
                    ConsoleLog.Print("Damage: The attacker is not you!", LogType.Error);
                    return;
                }

                // Register the kill
                if(target.IsMonsterFaction(Time.time) || target.IsBoss()) {
                    for(int i = 0; i < player.GetInventory().GetEquippedItems().Count; i++) {
                        ItemDrop.ItemData item = player.GetInventory().GetEquippedItems()[i];
                        if(IsItemAWeapon(item)) {
                            MaxDamagePersonality(item, totalDamage);

                            if(totalDamage >= target.GetHealth()) {
                                AddKillPersonality(item, target.name);
                            }

                            PersonalityTraits.UpdateWeaponTraits(item);
                        }
                    }
                }
            }

            private static bool IsItemAWeapon(ItemData itemData) {
                List<int> allowedTypes = new List<int>() {
                    3, 4, 14, 15, 22
                };

                if(allowedTypes.Contains((int)itemData.m_shared.m_itemType)) {
                    return true;
                }

                return false;
            }

            private static void AddKillPersonality(ItemDrop.ItemData item, string monsterName) {
                string key = ItemPersonality.KILL_KEY + monsterName.Replace("(Clone)", "");

                int keyValue = ItemPersonality.GetPersonalityValueAsInt(item, key);

                if(keyValue > 0) {
                    keyValue++;
                } else {
                    keyValue = 1;
                }

                ItemPersonality.SetPersonalityValue(item, key, keyValue);
                ConsoleLog.Print(item.m_shared.m_name + ": " + monsterName + " killed " + keyValue + " times");
            }

            private static void MaxDamagePersonality(ItemDrop.ItemData item, float damage) {
                string key = ItemPersonality.MAX_DMG_KEY;

                float keyValue = ItemPersonality.GetPersonalityValueAsFloat(item, key);

                if(damage > keyValue) {
                    ItemPersonality.SetPersonalityValue(item, key, damage);
                    ConsoleLog.Print(item.m_shared.m_name + ": New max damage: " + damage);
                }
            }
        }
    }
}
