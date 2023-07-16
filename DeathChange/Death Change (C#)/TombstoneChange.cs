using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

namespace Cozyheim.DeathChange
{
    internal class TombstoneChange
    {
        [HarmonyPatch]
        private class Patch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Player), "CreateTombStone")]
            private static bool Player_CreateTombStone_Prefix(ref Player __instance, ref GameObject ___m_tombstone, ref Inventory ___m_inventory)
            {
                if(___m_inventory.NrOfItems() != 0)
                {
                    List<ItemDrop.ItemData> allItems = ___m_inventory.GetAllItems();
                    foreach (ItemDrop.ItemData item in allItems)
                    {
                        if (!IsNonDropItem(item))
                        {
                            ConsoleLog.Print("Dropped: " + item.m_shared.m_name);
                            Vector3 position = __instance.transform.position + Vector3.up * 0.5f + Random.insideUnitSphere * 0.3f;
                            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);

                            ItemDrop.DropItem(item, 0, position, rotation);

                            ___m_inventory.RemoveItem(item);
                        }
                    }

                    ChangedFix(___m_inventory);
                   

                    /*
                    ConsoleLog.Print("Custom tombstone", LogType.Message);
                    GameObject obj = Object.Instantiate(___m_tombstone, __instance.GetCenterPoint(), __instance.transform.rotation);
                    obj.GetComponent<Container>().GetInventory().MoveInventoryToGrave(___m_inventory);

                    Floating objFloat = obj.GetComponent<Floating>();
                    objFloat.m_waterLevelOffset = 0.5f;

                    TombStone component = obj.GetComponent<TombStone>();
                    PlayerProfile playerProfile = Game.instance.GetPlayerProfile();
                    component.Setup(playerProfile.GetName(), playerProfile.GetPlayerID());
                    */
                }

                return false;
            }

            [HarmonyReversePatch]
            [HarmonyPatch(typeof(Inventory), "Changed")]
            private static void ChangedFix(object instance)
            {
            }


            [HarmonyPrefix]
            [HarmonyPatch(typeof(Inventory), "MoveInventoryToGrave")]
            private static bool Inventory_MoveInventoryToGrave_Prefix(ref Inventory __instance, ref List<ItemDrop.ItemData> ___m_inventory, ref int ___m_width, ref int ___m_height, ref Inventory original)
            {
                ___m_inventory.Clear();

                ___m_width = original.GetWidth();
                ___m_height = original.GetHeight();

                ConsoleLog.Print("Total weigth BEFORE: " + original.GetTotalWeight() + " - Items: " + original.NrOfItems());

                foreach (ItemDrop.ItemData item in original.GetAllItems())
                {
                    if (!IsNonDropItem(item))
                    {
                        ConsoleLog.Print("Moved: " + item.m_shared.m_name);
                        __instance.MoveItemToThis(original, item);
                    }
                }


                ConsoleLog.Print("Total weigth AFTER: " + original.GetTotalWeight() + " - Items: " + original.NrOfItems());

                return false;
            }

            private static bool IsNonDropItem(ItemDrop.ItemData item)
            {
                if(item.m_shared.m_questItem || item.m_equipped || item.m_gridPos.y == 0)
                {
                    return true;
                }
                return false;
            }
        }
    }
}
