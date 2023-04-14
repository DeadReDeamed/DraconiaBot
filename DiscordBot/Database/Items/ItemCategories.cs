using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Database.Items
{
    public class ItemCategories
    {
        public static Dictionary<string, ItemCategory> buttonMapping = new Dictionary<string, ItemCategory>()
        {
            {":crossed_swords:",  ItemCategory.WEAPONS},
            {":crystal_ball:", ItemCategory.MAGIC},
            {":shield:", ItemCategory.PROTECTION},
            {":tropical_drink:", ItemCategory.POTIONS},
            { ":poultry_leg:", ItemCategory.FOOD}
        };

        public static Dictionary<ItemCategory, string> itemTypeToIcon = new Dictionary<ItemCategory, string>()
        {
            {ItemCategory.WEAPONS, ":crossed_swords:" },
            {ItemCategory.MAGIC , ":crystal_ball:"},
            {ItemCategory.PROTECTION , ":shield:"},
            {ItemCategory.POTIONS , ":tropical_drink:"},
            { ItemCategory.FOOD , ":poultry_leg:"}
        };

        public static Dictionary<string, ItemSubType> itemSubTypeMapping = new Dictionary<string, ItemSubType>()
        {
            { ":archery:", ItemSubType.BOW },
            { ":crossed_swords:", ItemSubType.SWORD }
        };

        public static Dictionary<ItemSubType, string> itemSubTypeToIcon = new Dictionary<ItemSubType, string>()
        {
            { ItemSubType.BOW , ":archery:" },
            { ItemSubType.SWORD , ":crossed_swords:" },
            { ItemSubType.NONE, "" }
        };
    }

    public enum ItemCategory
    {
        WEAPONS,
        PROTECTION,
        POTIONS,
        MAGIC,
        FOOD
    }

    public enum ItemSubType
    {
        BOW,
        SWORD,
        NONE
    }

    public enum Rarity
    {
        COMMON,
        UNCOMMON,
        RARE,
        LEGENDARY
    }
}
