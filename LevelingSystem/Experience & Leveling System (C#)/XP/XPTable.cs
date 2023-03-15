﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cozyheim.LevelingSystem
{
    internal class XPTable
    {
        public static Dictionary<string, string> miningXPMappingTable = new Dictionary<string, string>()
        {
            // Other rocks that are not awarding XP
//            {"Ice_floor_fractured", 3},
//            {"ice_rock1_frac", 3},
//            {"stoneblock_fracture", 3},
//            {"Rock_destructible_test", 3},

            // Tier 1: Stone
            {"rock4_coast", "Stone"},
            {"rock4_coast_frac", "Stone"},
            {"HeathRockPillar", "Stone"},
            {"HeathRockPillar_frac", "Stone"},
            {"highstone", "Stone"},
            {"highstone_frac", "Stone"},
            {"Rock_3", "Stone"},
            {"Rock_3_frac", "Stone"},
            {"rock2_heath", "Stone"},
            {"rock2_heath_frac", "Stone"},
            {"rock4_forest_", "Stone"},
            {"rock4_forest_frac", "Stone"},
            {"rock4_heath", "Stone"},
            {"rock4_heath_frac", "Stone"},
            {"RockFinger", "Stone"},
            {"RockFinger_frac", "Stone"},
            {"RockFingerBroken", "Stone"},
            {"RockFingerBroken_frac", "Stone"},
            {"RockThumb", "Stone"},
            {"RockThumb_frac", "Stone"},
            {"widestone", "Stone"},
            {"widestone_frac", "Stone"},
            {"rock1_mountain", "Stone"},
            {"rock1_mountain_frac", "Stone"},
            {"rock2_mountain", "Stone"},
            {"rock2_mountain_frac", "Stone"},
            {"rock3_mountain_", "Stone"},
            {"rock3_mountain_frac", "Stone"},
            {"tarlump1", "Stone"},
            {"tarlump1_frac", "Stone"},
            {"rock_mistlands1", "Stone"},
            {"rock_mistlands1_frac", "Stone"},
            {"cliff_mistlands1_creep", "Stone"},
            {"cliff_mistlands1_creep_frac", "Stone"},
            {"cliff_mistlands1", "Stone"},
            {"cliff_mistlands1_frac", "Stone"},
            {"MineRock_Stone", "Stone"},

            // Tier 2: Copper & Tin
            {"MineRock_Tin", "Tin"},

            {"MineRock_Copper", "Copper"},
            {"rock4_copper", "Copper"},
            {"rock4_copper_frac", "Copper"},

            // Tier 3: Iron
            {"MineRock_Iron", "Iron"},
            {"mudpile_old", "Iron"},
            {"mudpile", "Iron"},
            {"mudpile_frac", "Iron"},
            {"mudpile2", "Iron"},
            {"mudpile2_frac", "Iron"},
            {"giant_helmet1", "Iron"},
            {"giant_helmet1_destruction", "Iron"},
            {"giant_helmet2", "Iron"},
            {"giant_helmet2_destruction", "Iron"},
            {"giant_sword1", "Iron"},
            {"giant_sword1_destruction", "Iron"},
            {"giant_sword2", "Iron"},
            {"giant_sword2_destruction", "Iron"},

            // Tier 4: Obsidian & Chitin
            {"MineRock_Obsidian", "Obsidian"},
            {"Leviathan", "Chitin"},

            // Tier 5: Silver & Flametal
            {"rock3_silver", "Silver"},
            {"rock3_silver_frac", "Silver"},
            {"silvervein", "Silver"},
            {"silvervein_frac", "Silver"},

            {"MineRock_Meteorite", "Flametal"},

            // Tier 6: Black Marble & Soft Tissue 
            {"giant_brain", "SoftTissue"},
            {"giant_brain_frac", "SoftTissue"},
            {"giant_ribs", "BlackMarble"},
            {"giant_ribs_frac", "BlackMarble"},
            {"giant_skull", "BlackMarble"},
            {"giant_skull_frac", "BlackMarble"},
        };

        public static Dictionary<string, string> woodcuttingXPMappingTable = new Dictionary<string, string>()
        {
            // Beech Tree
            {"beech_log", "Beech"},
            {"beech_log_half", "Beech"},
            {"Beech_Stub", "Beech"},
            {"Beech1", "Beech"},

            // Fir Tree
            {"FirTree_log", "Fir"},
            {"FirTree_log_half", "Fir"},
            {"FirTree", "Fir"},
            {"FirTree_Stub", "Fir"},

            // Pine Tree
            {"PineTree_log", "Pine"},
            {"PineTree_log_half", "Pine"},
            {"PineTree_log_halfOLD", "Pine"},
            {"PineTree_logOLD", "Pine"},
            {"PineTree", "Pine"},
            {"Pinetree_01", "Pine"},
            {"Pinetree_01_Stub", "Pine"},

            // Birch Tree
            {"Birch_log", "Birch"},
            {"Birch_log_half", "Birch"},
            {"Birch1", "Birch"},
            {"Birch1_aut", "Birch"},
            {"Birch2", "Birch"},
            {"Birch2_aut", "Birch"},
            {"BirchStub", "Birch"},

            // Oak Tree
            {"Oak_log", "Oak"},
            {"Oak_log_half", "Oak"},
            {"Oak1", "Oak"},
            {"OakStub", "Oak"},

            // Ancient Tree
            {"SwampTree1_log", "Ancient"},
            {"SwampTree1", "Ancient"},
            {"SwampTree1_Stub", "Ancient"},

            // Yggdrasil Tree
            {"yggashoot_log", "Yggdrasil"},
            {"yggashoot_log_half", "Yggdrasil"},
            {"YggaShoot1", "Yggdrasil"},
            {"YggaShoot2", "Yggdrasil"},
            {"YggaShoot3", "Yggdrasil"},
        };

        public static Dictionary<string, int> miningXPTable = new Dictionary<string, int>()
        {
            {"Stone", 3},
            {"Copper", 6},
            {"Tin", 6},
            {"Iron", 9},
            {"Obsidian", 12},
            {"Chitin", 12},
            {"Silver", 15},
            {"Flametal", 15},
            {"BlackMarble", 18},
            {"SoftTissue", 18}
        };

        public static Dictionary<string, int> woodcuttingXPTable = new Dictionary<string, int>()
        {
            {"Beech", 6},
            {"Fir", 12},
            {"Pine", 18},
            {"Birch", 24},
            {"Ancient", 30},
            {"Oak", 40},
            {"Yggdrasil", 100}
        };

        public static Dictionary<string, int> pickableXPTable = new Dictionary<string, int>()
        {
            // Tier 1 Pickable: Meadows
            {"Pickable_Branch", 2},
            {"Pickable_Stone", 2},
            {"Pickable_Dandelion", 2},
            {"Pickable_Mushroom", 2},
            {"RaspberryBush", 3},
            

            // Tier 2 Pickable: Black Forest
            {"BlueberryBush", 6},
            {"Pickable_Carrot", 4},
            {"Pickable_SeedCarrot", 4},
            {"Pickable_Flint", 4},
            {"Pickable_ForestCryptRemains01", 8},
            {"Pickable_ForestCryptRemains02", 8},
            {"Pickable_ForestCryptRemains03", 8},
            {"Pickable_ForestCryptRemains04", 8},
            {"Pickable_ForestCryptRandom", 8},
            {"Pickable_Mushroom_yellow", 4},
            {"Pickable_Thistle", 4},
            {"Pickable_Tin", 4},
            {"Pickable_SurtlingCoreStand", 30},


            // Tier 3 Pickable: Swamps
            {"Pickable_BogIronOre", 6},
            {"Pickable_Turnip", 6},
            {"Pickable_SeedTurnip", 6},
            {"Pickable_SunkenCryptRandom", 12},


            // Tier 4 Pickable: Mountains
            {"Pickable_Obsidian", 8},
            {"Pickable_Onion", 8},
            {"Pickable_SeedOnion", 8},
            {"hanging_hairstrands", 16},
            {"Pickable_Hairstrands01", 16},
            {"Pickable_Hairstrands02", 16},
            {"Pickable_MeatPile", 16},
            {"Pickable_MountainCaveCrystal", 16},
            {"Pickable_MountainCaveObsidian", 16},
            {"Pickable_MountainCaveRandom", 16},
            {"Pickable_MountainRemains01_buried", 16},
            {"Pickable_DragonEgg", 1200},


            // Tier 5 Pickable: Plains
            {"CloudberryBush", 10},
            {"Pickable_Barley", 10},
            {"Pickable_Barley_Wild", 10},
            {"Pickable_Flax", 10},
            {"Pickable_Flax_Wild", 10},
            {"Pickable_Tar", 10},
            {"Pickable_TarBig", 20},
            {"goblin_totempole", 3000},


            // Tier 6 Pickable: Mistlands
            {"Pickable_Mushroom_JotunPuffs", 12},
            {"Pickable_Mushroom_Magecap", 12},
            {"Pickable_RoyalJelly", 12},
            {"Pickable_DvergrMineTreasure", 24},
            {"Pickable_BlackCoreStand", 6000},


            // Tier X Pickable: Valueables
            {"Pickable_Item", 10},
            {"Pickable_DolmenTreasure", 10},


            // Tier X Pickable: Ashlands, Deep North, Other
            {"Pickable_Meteorite", 10},
            {"LuredWisp", 100},
            {"piece_beehive", 10}


            // Other pickables that are not awarding XP
//            {"Pickable_DvergrLantern", 10},
//            {"Pickable_DvergerThing", 10},
//            {"Pickable_DvergrStein", 10},
//            {"Pickable_Fishingrod", 10},
//            {"Pickable_Mushroom_blue", 10},
//            {"Pickable_RandomFood", 10},
        };

        public static Dictionary<string, int> monsterXPTable = new Dictionary<string, int>()
        {
            // Tier 1 Mobs: Meadows (Lv. 1-10)
            // (80xp -> 895xp,  Total: 4.817xp)
            {"Deer", 5},                  // Difficulty: 0   
            {"Neck", 5},                  // Difficulty: 1   
            {"Boar", 8},                  // Difficulty: 1  
            {"Greyling", 8},              // Difficulty: 1   
            {"Eikthyr", 96},              // Eikthyr        


            // Tier 2 Mobs: BlackForest (Lv. 11-30)
            // (991xp -> 3.420xp,  Total: 41.654xp)
            {"Greydwarf", 22},            // Difficulty: 2   
            {"Skeleton", 30},             // Difficulty: 2  
            {"Ghost", 36},                // Difficulty: 2   
            {"Greydwarf_Shaman", 40},     // Difficulty: 2  
            {"Skeleton_Poison", 53},      // Difficulty: 3   
            {"Greydwarf_Elite", 69},      // Difficulty: 4   
            {"Troll", 243},               // Difficulty: 7   
            {"gd_king", 833},             // The Elder      


            // Tier 3 Mobs: Swamp (Lv. 31-50)
            // (3.610xp -> 11.697xp,  Total: 134.247xp)
            {"Surtling", 85},             // Difficulty: 2   
            {"Blob", 99},                 // Difficulty: 3   
            {"Leech", 120},               // Difficulty: 3   
            {"Draugr", 139},              // Difficulty: 3   
            {"Wraith", 156},              // Difficulty: 4  
            {"BlobElite", 187},           // Difficulty: 4  
            {"Draugr_Elite", 224},        // Difficulty: 5   
            {"Abomination", 783},         // Difficulty: 8   
            {"Bonemass", 2685},           // Bonemass       


            // Tier 4 Mobs: Mountain + Ocean (Lv. 51-70)
            // (12.603xp -> 67.445xp,  Total: 639.932xp)
            {"Bat", 109},                 // Difficulty: 1   
            {"Ulv", 457},                 // Difficulty: 4      
            {"Wolf", 479},                // Difficulty: 4 
            {"Hatchling", 609},           // Difficulty: 4  
            {"Cultist", 762},             // Difficulty: 5 
            {"Fenring", 1067},            // Difficulty: 5 
            {"Serpent", 3200},            // Difficulty: 6 
            {"StoneGolem", 1600},         // Difficulty: 9  
            {"Dragon", 12800},            // Moder          


            // Tier 5 Mobs: Plains (Lv. 71-80)
            // (74.508xp -> 189.009xp,  Total: 1.234.664xp)
            {"Deathsquito", 620},         // Difficulty: 3  
            {"BlobTar", 782},             // Difficulty: 4  
            {"GoblinShaman", 980},        // Difficulty: 4
            {"Goblin", 1126},              // Difficulty: 5   
            {"GoblinBrute", 1904},        // Difficulty: 9   
            {"Lox", 2264},                // Difficulty: 10   
            {"GoblinKing", 24693},        // Yagluth        


            // Tier 6 Mobs: Mistlands (Lv. 81-90)
            // (210.294xp -> 562.259xp,  Total: 3.600.378xp)
            {"Hare", 34},                 // Difficulty: 1  - Faction: AnimalVeg
            {"SeekerBrood", 483},         // Difficulty: 3  
            {"Tick", 504},                // Difficulty: 3 
            {"DvergerMageSupport", 1207}, // Difficulty: 5  - Faction: Dverger
            {"Seeker", 1517},             // Difficulty: 5 
            {"DvergerMageFire", 1897},    // Difficulty: 6  - Faction: Dverger
            {"Dverger", 1966 },           // Difficulty: 6  - Faction: Dverger
            {"DvergerMage", 1966},        // Difficulty: 6  - Faction: Dverger
            {"DvergerMageIce", 2586},     // Difficulty: 7  - Faction: Dverger
            {"SeekerBrute", 6001},        // Difficulty: 11
            {"Gjall", 18002},             // Difficulty: 11
            {"SeekerQueen", 72008}        // The Queen      
        };

        public static int[] playerXPTable;
        /*
        Lv1:80,       Lv2:167,      Lv3:255,      Lv4:343,      Lv5:433,      Lv6:523,      Lv7:614,      Lv8:707,      Lv9:800,      Lv10:895,         (Total: 4.817xp       Diff: 4.817xp)
        Lv11:991,     Lv12:1089,    Lv13:1189,    Lv14:1291,    Lv15:1394,    Lv16:1500,    Lv17:1609,    Lv18:1720,    Lv19:1835,    Lv20:1953,        (Total: 19.388xp      Diff: 14.571xp)
        Lv21:2074,    Lv22:2200,    Lv23:2331,    Lv24:2466,    Lv25:2607,    Lv26:2755,    Lv27:2909,    Lv28:3071,    Lv29:3241,    Lv30:3420,        (Total: 46.462xp      Diff: 27.074xp)
        Lv31:3610,    Lv32:3811,    Lv33:4024,    Lv34:4251,    Lv35:4494,    Lv36:4754,    Lv37:5032,    Lv38:5332,    Lv39:5654,    Lv40:6002,        (Total: 93.426xp      Diff: 46.964xp)
        Lv41:6379,    Lv42:6787,    Lv43:7230,    Lv44:7712,    Lv45:8237,    Lv46:8810,    Lv47:9436,    Lv48:10122,   Lv49:10873,   Lv50:11697,       (Total: 180.709xp     Diff: 87.283xp)
        Lv51:12603,   Lv52:13599,   Lv53:14696,   Lv54:15906,   Lv55:17240,   Lv56:18713,   Lv57:20341,   Lv58:22141,   Lv59:24134,   Lv60:26342,       (Total: 366.424xp     Diff: 185.715xp)
        Lv61:28789,   Lv62:31503,   Lv63:34515,   Lv64:37860,   Lv65:41576,   Lv66:45707,   Lv67:50302,   Lv68:55414,   Lv69:61106,   Lv70:67445,       (Total: 820.641xp     Diff: 454.217xp)
        Lv71:74508,   Lv72:82382,   Lv73:91162,   Lv74:100958,  Lv75:111890,  Lv76:124095,  Lv77:137726,  Lv78:152956,  Lv79:169978,  Lv80:189009,      (Total: 2.055.305xp   Diff: 1.234.664xp)
        Lv81:210294,  Lv82:234108,  Lv83:260759,  Lv84:290595,  Lv85:324009,  Lv86:361440,  Lv87:403385,  Lv88:450404,  Lv89:503125,  Lv90:562259       (Total: 5.655.683xp   Diff: 3.600.378xp)
        */

        public static void GenerateDefaultPlayerXPTable()
        {
            int totalLevels = 90;
            float baseXP = 80;
            float fixedXPAddPrLv = 80f;
            float lvMultiplier = 0.09f;
            float lvMultiplierAdd = 0.00033f;

            int totalXpNeeded = 0;
            int lastTotal = 0;

            List<int> levels = new List<int>();

            for(int i = 1; i <= totalLevels; i++)
            {
                int xpForThisLevel = (int)(baseXP + fixedXPAddPrLv * (i - 1));
                baseXP *= 1f + lvMultiplier;
                lvMultiplier += lvMultiplierAdd;

                levels.Add(xpForThisLevel);
                totalXpNeeded += xpForThisLevel;

                if(i % 10 == 0)
                {
                    ConsoleLog.Print("Lv. " + i + ": " + totalXpNeeded + "xp total, (diff: " + (totalXpNeeded - lastTotal) + "xp)");
                    lastTotal = totalXpNeeded;
                }
            }
            ConsoleLog.Print("Lv. " + totalLevels + " (max): " + totalXpNeeded + "xp total, (diff: " + (totalXpNeeded - lastTotal) + "xp)");

            playerXPTable = levels.ToArray(); 
        }

        public static void UpdatePickableXPTable()
        {
            if (Main.pickableXpTable.Value == "")
            {
                return;
            }

            ConsoleLog.Print("Level System: Setting pickable base XP", LogType.Message);
            pickableXPTable.Clear();

            string[] pickableXpString = Main.pickableXpTable.Value.Split(',');
            foreach (string s in pickableXpString)
            {
                int value;
                string[] data = s.Split(':');
                string key = data[0].Replace("\n", "").Replace(" ", "");
                int.TryParse(data[1], out value);
                pickableXPTable[key] = value;
                ConsoleLog.Print("-> " + key + ": " + value + "xp");
            }
        }

        public static void UpdateWoodcuttingXPTable()
        {
            if (Main.woodcuttingXpTable.Value == "")
            {
                return;
            }

            ConsoleLog.Print("Level System: Setting woodcutting base XP", LogType.Message);
            woodcuttingXPTable.Clear();
            string[] woodcuttingXpString = Main.woodcuttingXpTable.Value.Split(',');
            foreach (string s in woodcuttingXpString)
            {
                int value;
                string[] data = s.Split(':');
                string key = data[0].Replace("\n", "").Replace(" ", "");
                int.TryParse(data[1], out value);
                woodcuttingXPTable[key] = value;
                ConsoleLog.Print("-> " + key + ": " + value + "xp");
            }
        }

        public static void UpdateMiningXPTable()
        {
            if (Main.miningXpTable.Value == "")
            {
                return;
            }

            ConsoleLog.Print("Level System: Setting mining base XP", LogType.Message);
            miningXPTable.Clear();
            string[] miningXpString = Main.miningXpTable.Value.Split(',');
            foreach (string s in miningXpString)
            {
                int value;
                string[] data = s.Split(':');
                string key = data[0].Replace("\n", "").Replace(" ", "");
                int.TryParse(data[1], out value);
                miningXPTable[key] = value;
                ConsoleLog.Print("-> " + key + ": " + value + "xp");
            }
        }

        public static void UpdateMonsterXPTable()
        {
            ConsoleLog.Print("Level System: Setting monster base XP", LogType.Message);
            monsterXPTable.Clear();
            string[] monsterXPstring = Main.monsterXpTable.Value.Split(',');
            foreach(string s in monsterXPstring)
            {
                int value;
                string[] data = s.Split(':');
                string key = data[0].Replace("\n", "").Replace(" ", "");
                int.TryParse(data[1], out value);
                monsterXPTable[key] = value;
                ConsoleLog.Print("-> " + key + ": " + value + "xp");
            }
        }

        public static void UpdatePlayerXPTable()
        {
            ConsoleLog.Print("Level System: Setting player level XP requirements", LogType.Message);
            List<int> playerLevels = new List<int>();

            string[] playerXPstring = Main.playerXpTable.Value.Split(',');
            int counter = 1;
            foreach (string s in playerXPstring)
            {
                int value;
                string[] data = s.Split(':');
                int.TryParse(data[1], out value);
                playerLevels.Add(value);
                ConsoleLog.Print("-> Level " + counter + ": " + value + "xp");
                counter++;
            }
            playerXPTable = playerLevels.ToArray();
        }

        public static int GetMonsterXP(string name)
        {
            name = name.Replace("(Clone)", "");
            return monsterXPTable.ContainsKey(name) ? monsterXPTable[name] : 0;
        }

        public static int GetPickableXP(string name)
        {
            name = name.Replace("(Clone)", "");
            return pickableXPTable.ContainsKey(name) ? pickableXPTable[name] : 0;
        }

        public static int GetMiningXP(string name)
        {
            name = name.Replace("(Clone)", "");
            string material = miningXPMappingTable.ContainsKey(name) ? miningXPMappingTable[name] : "";
            return miningXPTable.ContainsKey(material) ? miningXPTable[material] : 0;
        }

        public static int GetWoodcuttingXP(string name)
        {
            name = name.Replace("(Clone)", "");
            string material = woodcuttingXPMappingTable.ContainsKey(name) ? woodcuttingXPMappingTable[name] : "";
            return woodcuttingXPTable.ContainsKey(material) ? woodcuttingXPTable[material] : 0;
        }
    }
}
