using HarmonyLib;

namespace Cozyheim.LevelingSystem
{
    [HarmonyPatch]
    internal class LevelingDummy_Patch
    {
        // ref bool __result = returned value of the original method
        // ref <T> __instance = reference to original class (this)
        // object[] __args = original parameters
        // -> Example: Player player = (Player) __args[0];

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CraftingStation), "Interact")]
        private static bool CraftingStation_Interact_Prefix(CraftingStation __instance)
        {
            var parent = __instance.transform.parent;
            if (parent != null) {
                if (__instance.transform.parent.name.StartsWith("LevelingDummy")) {
                    if (!UIManager.Instance.skillsUIVisible) {
                        UIManager.Instance.ToggleSkillsUI(true);
                        return false;
                    }
                }
            }

            return true;
        }
    }
}