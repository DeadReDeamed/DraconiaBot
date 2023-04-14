using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Database.Items
{
    public class Item : Entity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ItemCategory Type { get; set; }
        public ItemSubType SubType { get; set; } 
        public int Price { get; set; }
        public int modifier { get; set; }
        public Rarity Rarity { get; set; }

        public Item(ulong id, string name, string description) : base(id)
        {
            Name = name;
            Description = description;
        }

        public Item() : base(0)
        {
            Name = string.Empty;
            Description = string.Empty;
            
        }
    }
}
