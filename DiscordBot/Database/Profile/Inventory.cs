using DiscordBot.Database.Items;

namespace DiscordBot.Database.Profile
{
    public class Inventory
    {
        public List<Item> Items { get; set; }

        public Inventory(List<Item> items)
        {
            Items = items;
        }
    }
}
