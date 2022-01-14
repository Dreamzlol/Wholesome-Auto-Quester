﻿using System.Collections.Generic;

namespace Wholesome_Auto_Quester.Database.Models
{
    public class ModelItemTemplate
    {
        public int Entry { get; set; }
        public string Name { get; set; }

        public ModelSpell Spell1 { get; set; }
        public ModelSpell Spell2 { get; set; }
        public ModelSpell Spell3 { get; set; }
        public ModelSpell Spell4 { get; set; }
        public List<ModelCreatureLootTemplate> CreatureLootTemplates { get; set; } = new List<ModelCreatureLootTemplate>();
        public List<ModelGameObjectLootTemplate> GameObjectLootTemplates { get; set; } = new List<ModelGameObjectLootTemplate>();
        public List<ModelItemLootTemplate> ItemLootTemplates { get; set; } = new List<ModelItemLootTemplate>();

        public int AllowableClass { get; set; }
        public int AllowableRace { get; set; }
        public int BuyCount { get; set; }
        public int BuyPrice { get; set; }
        public int Class { get; set; }
        public int DisplayId { get; set; }
        public int Flags { get; set; }
        public int FlagsExtra { get; set; }
        public int InventoryType { get; set; }
        public int ItemLevel { get; set; }
        public int Quality { get; set; }
        public int RequiredLevel { get; set; }
        public int RequiredSkill { get; set; }
        public int RequiredSkillRank { get; set; }
        public int RequiredSpell { get; set; }
        public int RequiredHonorRank { get; set; }
        public int RequiredCityRank { get; set; }
        public int RequiredReputationFaction { get; set; }
        public int RequiredReputationRank { get; set; }
        public int maxcount { get; set; }
        public int stackable { get; set; }
        public int ContainerSlots { get; set; }
        /*
        public int StatsCount { get; set; }
        public int stat_type1 { get; set; }
        public int stat_value1 { get; set; }
        public int stat_type2 { get; set; }
        public int stat_value2 { get; set; }
        public int stat_type3 { get; set; }
        public int stat_value3 { get; set; }
        public int stat_type4 { get; set; }
        public int stat_value4 { get; set; }
        public int stat_type5 { get; set; }
        public int stat_value5 { get; set; }
        public int stat_type6 { get; set; }
        public int stat_value6 { get; set; }
        public int stat_type7 { get; set; }
        public int stat_value7 { get; set; }
        public int stat_type8 { get; set; }
        public int stat_value8 { get; set; }
        public int stat_type9 { get; set; }
        public int stat_value9 { get; set; }
        public int stat_type10 { get; set; }
        public int stat_value10 { get; set; }
        public int ScalingStatDistribution { get; set; }
        public int ScalingStatValue { get; set; }
        public int dmg_min1 { get; set; }
        public int dmg_max1 { get; set; }
        public int dmg_type1 { get; set; }
        public int dmg_min2 { get; set; }
        public int dmg_max2 { get; set; }
        public int dmg_type2 { get; set; }
        public int armor { get; set; }
        public int holy_res { get; set; }
        public int fire_res { get; set; }
        public int nature_res { get; set; }
        public int frost_res { get; set; }
        public int shadow_res { get; set; }
        public int arcane_res { get; set; }*/
        public int delay { get; set; }
        
        public int ammo_type { get; set; }
        public int RangedModRange { get; set; }
        public int spellid_1 { get; set; }
        public int spelltrigger_1 { get; set; }
        public int spellcharges_1 { get; set; }
        public int spellppmRate_1 { get; set; }
        public int spellcooldown_1 { get; set; }
        public int spellcategory_1 { get; set; }
        public int spellcategorycooldown_1 { get; set; }
        public int spellid_2 { get; set; }
        public int spelltrigger_2 { get; set; }
        public int spellcharges_2 { get; set; }
        public int spellppmRate_2 { get; set; }
        public int spellcooldown_2 { get; set; }
        public int spellcategory_2 { get; set; }
        public int spellcategorycooldown_2 { get; set; }
        public int spellid_3 { get; set; }
        public int spelltrigger_3 { get; set; }
        public int spellcharges_3 { get; set; }
        public int spellppmRate_3 { get; set; }
        public int spellcooldown_3 { get; set; }
        public int spellcategory_3 { get; set; }
        public int spellcategorycooldown_3 { get; set; }
        public int spellid_4 { get; set; }
        public int spelltrigger_4 { get; set; }
        public int spellcharges_4 { get; set; }
        public int spellppmRate_4 { get; set; }
        public int spellcooldown_4 { get; set; }
        public int spellcategory_4 { get; set; }
        public int spellcategorycooldown_4 { get; set; }
        public int spellid_5 { get; set; }
        public int spelltrigger_5 { get; set; }
        public int spellcharges_5 { get; set; }
        public int spellppmRate_5 { get; set; }
        public int spellcooldown_5 { get; set; }
        public int spellcategory_5 { get; set; }
        public int spellcategorycooldown_5 { get; set; }
        public int bonding { get; set; }
        public string Description { get; set; }
        public int PageText { get; set; }
        public int LanguageID { get; set; }
        public int PageMaterial { get; set; }
        public int startquest { get; set; }
        public int lockid { get; set; }
        public int Material { get; set; }
        public int sheath { get; set; }
        public int RandomProperty { get; set; }
        public int RandomSuffix { get; set; }
        public int block { get; set; }
        public int itemset { get; set; }
        public int MaxDurability { get; set; }
        public int area { get; set; }
        public int Map { get; set; }
        public int BagFamily { get; set; }
        /*
        public int TotemCategory { get; set; }
        public int socketColor_1 { get; set; }
        public int socketContent_1 { get; set; }
        public int socketColor_2 { get; set; }
        public int socketContent_2 { get; set; }
        public int socketColor_3 { get; set; }
        public int socketContent_3 { get; set; }
        public int socketBonus { get; set; }
        public int GemProperties { get; set; }
        public int RequiredDisenchantSkill { get; set; }
        public int ArmorDamageModifier { get; set; }
        */
        public int duration { get; set; }
        public int ItemLimitCategory { get; set; }
        public int HolidayId { get; set; }
        public string ScriptName { get; set; }
        public int DisenchantID { get; set; }
        public int FoodType { get; set; }
        public int minMoneyLoot { get; set; }
        public int maxMoneyLoot { get; set; }
        public int flagsCustom { get; set; }
        public int SellPrice { get; set; }
        public int Subclass { get; set; }
    }
}
