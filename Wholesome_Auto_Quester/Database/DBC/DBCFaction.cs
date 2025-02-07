﻿using System.Collections.Generic;
using wManager.Wow.Helpers;

namespace Wholesome_Auto_Quester.Database.DBC
{
    class DBCFaction
    {
        private static List<Reputation> _myReputations = new List<Reputation>();
        private static object _repLock = new object();

        public static Reputation GetReputationById(int repId)
        {
            lock (_repLock)
            {
                return _myReputations.Find(rep => rep.Id == repId); // can be null
            }
        }

        public static void RecordReputations()
        {
            string luaReps = Lua.LuaDoString<string>($@"
                local result  = """";
                for i=1, 40 do 
                    local name, description, standingID, _, _, barValue, _, _, isHeader, _, _, _, _ = GetFactionInfo(i);
                    if (not isHeader) and name ~= nil then
                        result = result .. '$' .. name .. '|' .. standingID .. '|' .. barValue
                    end
                end
                return result
            ");
            string[] reps = luaReps.Split('$');
            lock (_repLock)
            {
                _myReputations.Clear();
                foreach (string rep in reps)
                {
                    string[] repProperties = rep.Split('|');
                    if (repProperties.Length < 3)
                    {
                        continue;
                    }
                    _myReputations.Add(new Reputation(repProperties[0], repProperties[1], repProperties[2]));
                }
            }
        }
    }
}

public class Reputation
{
    public int Id { get; }
    public string Name { get; }
    public int Amount { get; }
    FactionStanding StandingId { get; }

    private Dictionary<FactionStanding, int> _reputationMasks = new Dictionary<FactionStanding, int>()
    {
        {  FactionStanding.Hated, 1 },
        {  FactionStanding.Hostile, 2 },
        {  FactionStanding.Unfriendly, 4 },
        {  FactionStanding.Neutral, 8 },
        {  FactionStanding.Friendly, 16 },
        {  FactionStanding.Honored, 32 },
        {  FactionStanding.Revered, 64 },
        {  FactionStanding.Exalted, 128 }
    };

    public int GetFactionMask => _reputationMasks[StandingId];

    public Reputation(string name, string standingId, string amount)
    {
        Name = name;

        if (int.TryParse(standingId, out int id))
        {
            StandingId = (FactionStanding)id;
        }
        else
        {
            throw new System.Exception($"Couldn't parse {name} reputation standing Id");
        }

        if (int.TryParse(amount, out int am))
        {
            Amount = am;
        }
        else
        {
            throw new System.Exception($"Couldn't parse {name} reputation amount");
        }

        if (reputationIds.TryGetValue(name, out int repId))
        {
            Id = repId;
        }
        else
        {
            throw new System.Exception($"Couldn't get {name} from reputation ID dictionary");
        }
    }

    private Dictionary<string, int> reputationIds = new Dictionary<string, int>()
    {
        { "PLAYER, Human", 1 },
        { "PLAYER, Orc", 2 },
        { "PLAYER, Dwarf", 3 },
        { "PLAYER, Night Elf", 4 },
        { "PLAYER, Undead", 5 },
        { "PLAYER, Tauren", 6 },
        { "Creature", 7 },
        { "PLAYER, Gnome", 8 },
        { "PLAYER, Troll", 9 },
        { "Monster", 14 },
        { "Defias Brotherhood", 15 },
        { "Gnoll - Riverpaw", 16 },
        { "Gnoll - Redridge", 17 },
        { "Gnoll - Shadowhide", 18 },
        { "Murloc", 19 },
        { "Undead, Scourge", 20 },
        { "Booty Bay", 21 },
        { "Beast - Spider", 22 },
        { "Beast - Boar", 23 },
        { "Worgen", 24 },
        { "Kobold", 25 },
        { "Troll, Bloodscalp", 26 },
        { "Troll, Skullsplitter", 27 },
        { "Prey", 28 },
        { "Beast - Wolf", 29 },
        { "Defias Brotherhood Traitor", 30 },
        { "Friendly", 31 },
        { "Trogg", 32 },
        { "Troll, Frostmane", 33 },
        { "Orc, Blackrock", 34 },
        { "Villian", 35 },
        { "Victim", 36 },
        { "Beast - Bear", 37 },
        { "Ogre", 38 },
        { "Kurzen's Mercenaries", 39 },
        { "Escortee", 40 },
        { "Venture Company", 41 },
        { "Beast - Raptor", 42 },
        { "Basilisk", 43 },
        { "Dragonflight, Green", 44 },
        { "Lost Ones", 45 },
        { "Blacksmithing - Armorsmithing", 46 },
        { "Ironforge", 47 },
        { "Dark Iron Dwarves", 48 },
        { "Human, Night Watch", 49 },
        { "Dragonflight, Red", 50 },
        { "Gnoll - Mosshide", 51 },
        { "Orc, Dragonmaw", 52 },
        { "Gnome - Leper", 53 },
        { "Gnomeregan Exiles", 54 },
        { "Leopard", 55 },
        { "Scarlet Crusade", 56 },
        { "Gnoll - Rothide", 57 },
        { "Beast - Gorilla", 58 },
        { "Thorium Brotherhood", 59 },
        { "Naga", 60 },
        { "Dalaran", 61 },
        { "Forlorn Spirit", 62 },
        { "Darkhowl", 63 },
        { "Grell", 64 },
        { "Furbolg", 65 },
        { "Horde Generic", 66 },
        { "Horde", 67 },
        { "Undercity", 68 },
        { "Darnassus", 69 },
        { "Syndicate", 70 },
        { "Hillsbrad Militia", 71 },
        { "Stormwind", 72 },
        { "Demon", 73 },
        { "Elemental", 74 },
        { "Spirit", 75 },
        { "Orgrimmar", 76 },
        { "Treasure", 77 },
        { "Gnoll - Mudsnout", 78 },
        { "HIllsbrad, Southshore Mayor", 79 },
        { "Dragonflight, Black", 80 },
        { "Thunder Bluff", 81 },
        { "Troll, Witherbark", 82 },
        { "Leatherworking - Elemental", 83 },
        { "Quilboar, Razormane", 84 },
        { "Quilboar, Bristleback", 85 },
        { "Leatherworking - Dragonscale", 86 },
        { "Bloodsail Buccaneers", 87 },
        { "Blackfathom", 88 },
        { "Makrura", 89 },
        { "Centaur, Kolkar", 90 },
        { "Centaur, Galak", 91 },
        { "Gelkis Clan Centaur", 92 },
        { "Magram Clan Centaur", 93 },
        { "Maraudine", 94 },
        { "Theramore", 108 },
        { "Quilboar, Razorfen", 109 },
        { "Quilboar, Razormane 2", 110 },
        { "Quilboar, Deathshead", 111 },
        { "Enemy", 128 },
        { "Ambient", 148 },
        { "Nethergarde Caravan", 168 },
        { "Steamwheedle Cartel", 169 },
        { "Alliance Generic", 189 },
        { "Nethergarde", 209 },
        { "Wailing Caverns", 229 },
        { "Silithid", 249 },
        { "Silvermoon Remnant", 269 },
        { "Zandalar Tribe", 270 },
        { "Blacksmithing - Weaponsmithing", 289 },
        { "Scorpid", 309 },
        { "Beast - Bat", 310 },
        { "Titan", 311 },
        { "Taskmaster Fizzule", 329 },
        { "Ravenholdt", 349 },
        { "Gadgetzan", 369 },
        { "Gnomeregan Bug", 389 },
        { "Harpy", 409 },
        { "Burning Blade", 429 },
        { "Shadowsilk Poacher", 449 },
        { "Searing Spider", 450 },
        { "Alliance", 469 },
        { "Ratchet", 470 },
        { "Wildhammer Clan", 471 },
        { "Goblin, Dark Iron Bar Patron", 489 },
        { "The League of Arathor", 509 },
        { "The Defilers", 510 },
        { "Giant", 511 },
        { "Argent Dawn", 529 },
        { "Darkspear Trolls", 530 },
        { "Dragonflight, Bronze", 531 },
        { "Dragonflight, Blue", 532 },
        { "Leatherworking - Tribal", 549 },
        { "Engineering - Goblin", 550 },
        { "Engineering - Gnome", 551 },
        { "Blacksmithing - Hammersmithing", 569 },
        { "Blacksmithing - Axesmithing", 570 },
        { "Blacksmithing - Swordsmithing", 571 },
        { "Troll, Vilebranch", 572 },
        { "Southsea Freebooters", 573 },
        { "Caer Darrow", 574 },
        { "Furbolg, Uncorrupted", 575 },
        { "Timbermaw Hold", 576 },
        { "Everlook", 577 },
        { "Wintersaber Trainers", 589 },
        { "Cenarion Circle", 609 },
        { "Shatterspear Trolls", 629 },
        { "Ravasaur Trainers", 630 },
        { "Majordomo Executus", 649 },
        { "Beast - Carrion Bird", 669 },
        { "Beast - Cat", 670 },
        { "Beast - Crab", 671 },
        { "Beast - Crocilisk", 672 },
        { "Beast - Hyena", 673 },
        { "Beast - Owl", 674 },
        { "Beast - Scorpid", 675 },
        { "Beast - Tallstrider", 676 },
        { "Beast - Turtle", 677 },
        { "Beast - Wind Serpent", 678 },
        { "Training Dummy", 679 },
        { "Dragonflight, Black - Bait", 689 },
        { "Battleground Neutral", 709 },
        { "Frostwolf Clan", 729 },
        { "Stormpike Guard", 730 },
        { "Hydraxian Waterlords", 749 },
        { "Sulfuron Firelords", 750 },
        { "Gizlock's Dummy", 769 },
        { "Gizlock's Charm", 770 },
        { "Gizlock", 771 },
        { "Moro'gai", 789 },
        { "Spirit Guide - Alliance", 790 },
        { "Shen'dralar", 809 },
        { "Ogre (Captain Kromcrush)", 829 },
        { "Spirit Guide - Horde", 849 },
        { "Jaedenar", 869 },
        { "Warsong Outriders", 889 },
        { "Silverwing Sentinels", 890 },
        { "Alliance Forces", 891 },
        { "Horde Forces", 892 },
        { "Revantusk Trolls", 893 },
        { "Darkmoon Faire", 909 },
        { "Brood of Nozdormu", 910 },
        { "Silvermoon City", 911 },
        { "Might of Kalimdor", 912 },
        { "PLAYER, Blood Elf", 914 },
        { "Armies of C'Thun", 915 },
        { "Silithid Attackers", 916 },
        { "The Ironforge Brigade", 917 },
        { "RC Enemies", 918 },
        { "RC Objects", 919 },
        { "Red", 920 },
        { "Blue", 921 },
        { "Tranquillien", 922 },
        { "Farstriders", 923 },
        { "Sunstriders", 925 },
        { "Magister's Guild", 926 },
        { "PLAYER, Draenei", 927 },
        { "Scourge Invaders", 928 },
        { "Bloodmaul Clan", 929 },
        { "Exodar", 930 },
        { "Test Faction (not a real faction)", 931 },
        { "The Aldor", 932 },
        { "The Consortium", 933 },
        { "The Scryers", 934 },
        { "The Sha'tar", 935 },
        { "Shattrath City", 936 },
        { "Troll, Forest", 937 },
        { "The Omenai", 938 },
        { "The Sons of Lothar", 940 },
        { "The Mag'har", 941 },
        { "Cenarion Expedition", 942 },
        { "Fel Orc", 943 },
        { "Fel Orc Ghost", 944 },
        { "Sons of Lothar Ghosts", 945 },
        { "Honor Hold", 946 },
        { "Thrallmar", 947 },
        { "Test Faction 2", 948 },
        { "Test Faction 1", 949 },
        { "ToWoW - Flag", 950 },
        { "ToWoW - Flag Trigger Alliance (DND)", 951 },
        { "Test Faction 3", 952 },
        { "Test Faction 4", 953 },
        { "ToWoW - Flag Trigger Horde (DND)", 954 },
        { "Broken", 955 },
        { "Ethereum", 956 },
        { "Earth Elemental", 957 },
        { "Fighting Robots", 958 },
        { "Actor Good", 959 },
        { "Actor Evil", 960 },
        { "Stillpine Furbolg", 961 },
        { "Crazed Owlkin", 962 },
        { "Chess Alliance", 963 },
        { "Chess Horde", 964 },
        { "Monster Spar", 965 },
        { "Monster Spar Buddy", 966 },
        { "The Violet Eye", 967 },
        { "Sunhawks", 968 },
        { "Hand of Argus", 969 },
        { "Sporeggar", 970 },
        { "Fungal Giant", 971 },
        { "Spore Bat", 972 },
        { "Monster, Predator", 973 },
        { "Monster, Prey", 974 },
        { "Void Anomaly", 975 },
        { "Hyjal Defenders", 976 },
        { "Hyjal Invaders", 977 },
        { "Kurenai", 978 },
        { "Earthen Ring", 979 },
        { "The Burning Crusade", 980 },
        { "Arakkoa", 981 },
        { "Zangarmarsh Banner (Alliance)", 982 },
        { "Zangarmarsh Banner (Horde)", 983 },
        { "Zangarmarsh Banner (Neutral)", 984 },
        { "Caverns of Time - Thrall", 985 },
        { "Caverns of Time - Durnholde", 986 },
        { "Caverns of Time - Southshore Guards", 987 },
        { "Shadow Council Covert", 988 },
        { "Keepers of Time", 989 },
        { "The Scale of the Sands", 990 },
        { "Dark Portal Defender, Alliance", 991 },
        { "Dark Portal Defender, Horde", 992 },
        { "Dark Portal Attacker, Legion", 993 },
        { "Inciter Trigger", 994 },
        { "Inciter Trigger 2", 995 },
        { "Inciter Trigger 3", 996 },
        { "Inciter Trigger 4", 997 },
        { "Inciter Trigger 5", 998 },
        { "Mana Creature", 999 },
        { "Khadgar's Servant", 1000 },
        { "Bladespire Clan", 1001 },
        { "Ethereum Sparbuddy", 1002 },
        { "Protectorate", 1003 },
        { "Arcane Annihilator (DNR)", 1004 },
        { "Friendly, Hidden", 1005 },
        { "Kirin'Var - Dathric", 1006 },
        { "Kirin'Var - Belmara", 1007 },
        { "Kirin'Var - Luminrath", 1008 },
        { "Kirin'Var - Cohlien", 1009 },
        { "Servant of Illidan", 1010 },
        { "Lower City", 1011 },
        { "Ashtongue Deathsworn", 1012 },
        { "Spirits of Shadowmoon 1", 1013 },
        { "Spirits of Shadowmoon 2", 1014 },
        { "Netherwing", 1015 },
        { "Wyrmcult", 1016 },
        { "Treant", 1017 },
        { "Leotheras Demon I", 1018 },
        { "Leotheras Demon II", 1019 },
        { "Leotheras Demon III", 1020 },
        { "Leotheras Demon IV", 1021 },
        { "Leotheras Demon V", 1022 },
        { "Azaloth", 1023 },
        { "Rock Flayer", 1024 },
        { "Flayer Hunter", 1025 },
        { "Shadowmoon Shade", 1026 },
        { "Legion Communicator", 1027 },
        { "Ravenswood Ancients", 1028 },
        { "Chess, Friendly to All Chess", 1029 },
        { "Black Temple Gates - Illidari", 1030 },
        { "Sha'tari Skyguard", 1031 },
        { "Area 52", 1032 },
        { "Maiev", 1033 },
        { "Skettis Shadowy Arakkoa", 1034 },
        { "Skettis Arakkoa", 1035 },
        { "Dragonmaw Enemy", 1036 },
        { "Alliance Vanguard", 1037 },
        { "Ogri'la", 1038 },
        { "Ravager", 1039 },
        { "Frenzy", 1041 },
        { "Skyguard Enemy", 1042 },
        { "Skunk, Petunia", 1043 },
        { "Theramore Deserter", 1044 },
        { "Northsea Pirates", 1046 },
        { "Tuskarr", 1047 },
        { "Troll, Amani", 1049 },
        { "Valiance Expedition", 1050 },
        { "Horde Expedition", 1052 },
        { "Westguard", 1053 },
        { "Spotted Gryphon", 1054 },
        { "Tamed Plaguehound", 1055 },
        { "Vrykul (Ancient Spirit 1)", 1056 },
        { "Vrykul (Ancient Siprit 2)", 1057 },
        { "Vrykul (Ancient Siprit 3)", 1058 },
        { "CTF - Flag - Alliance", 1059 },
        { "Test", 1060 },
        { "Vrykul Gladiator", 1062 },
        { "Valgarde Combatant", 1063 },
        { "The Taunka", 1064 },
        { "Monster, Zone Force Reaction 1", 1065 },
        { "Monster, Zone Force Reaction 2", 1066 },
        { "The Hand of Vengeance", 1067 },
        { "Explorers' League", 1068 },
        { "Ram Racing Powerup DND", 1069 },
        { "Ram Racing Trap DND", 1070 },
        { "Craig's Squirrels", 1071 },
        { "The Kalu'ak", 1073 },
        { "Holiday - Water Barrel", 1074 },
        { "Holiday - Generic", 1075 },
        { "Iron Dwarves", 1076 },
        { "Shattered Sun Offensive", 1077 },
        { "Fighting Vanity Pet", 1078 },
        { "Murloc, Winterfin", 1079 },
        { "Friendly, Force Reaction", 1080 },
        { "Object, Force Reaction", 1081 },
        { "Vrykul, Sea", 1084 },
        { "Warsong Offensive", 1085 },
        { "Poacher", 1086 },
        { "Holiday Monster", 1087 },
        { "Furbolg, Redfang", 1088 },
        { "Furbolg, Frostpaw", 1089 },
        { "Kirin Tor", 1090 },
        { "The Wyrmrest Accord", 1091 },
        { "Azjol-Nerub", 1092 },
        { "The Silver Covenant", 1094 },
        { "Grizzly Hills Trapper", 1095 },
        { "Wrath of the Lich King", 1097 },
        { "Knights of the Ebon Blade", 1098 },
        { "Wrathgate Scourge", 1099 },
        { "Wrathgate Alliance", 1100 },
        { "Wrathgate Horde", 1101 },
        { "CTF - Flag - Horde", 1102 },
        { "CTF - Flag - Neutral", 1103 },
        { "Frenzyheart Tribe", 1104 },
        { "The Oracles", 1105 },
        { "Argent Crusade", 1106 },
        { "Troll, Drakkari", 1107 },
        { "CoT Arthas", 1108 },
        { "CoT Stratholme Citizen", 1109 },
        { "CoT Scourge", 1110 },
        { "Freya", 1111 },
        { "Mount - Taxi - Alliance", 1112 },
        { "Mount - Taxi - Horde", 1113 },
        { "Mount - Taxi - Neutral", 1114 },
        { "Elemental, Water", 1115 },
        { "Elemental, Air", 1116 },
        { "Sholazar Basin", 1117 },
        { "Classic", 1118 },
        { "The Sons of Hodir", 1119 },
        { "Iron Giants", 1120 },
        { "Frost Vrykul", 1121 },
        { "Earthen", 1122 },
        { "Monster Referee", 1123 },
        { "The Sunreavers", 1124 },
        { "Hyldsmeet", 1125 },
        { "The Frostborn", 1126 },
        { "Orgrimmar (Alex Test)", 1127 },
        { "Tranquillien Conversion", 1136 },
        { "Wintersaber Conversion", 1137 },
        { "Hates Everything", 1145 },
        { "Silver Covenant Conversion", 1154 },
        { "Sunreavers Conversion", 1155 },
        { "The Ashen Verdict", 1156 },
        { "CTF - Flag - Alliance 2", 1159 },
        { "CTF - Flag - Horde 2", 1160 },
    };
}

enum FactionStanding
{
    Hated = 1,
    Hostile = 2,
    Unfriendly = 3,
    Neutral = 4,
    Friendly = 5,
    Honored = 6,
    Revered = 7,
    Exalted = 8
}