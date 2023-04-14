using DiscordBot.Database.Items;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Util.Embed
{
    public class EmbedUtil
    {
        public static void AddTableToEmbed(ref DiscordEmbedBuilder embed, List<string> columns, params List<string>[] values)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                string valuesString = "";
                var valueList = values[i];
                foreach (var value in valueList)
                {
                    valuesString += value + "\n";
                }
                embed.AddField(columns[i], valuesString, true);
            }
        }

        public static void AddItemTableToEmbed(ref DiscordEmbedBuilder embed, List<Item> items)
        {
            Dictionary<string, string> collumnValueString = new Dictionary<string, string>()
            {
                { "Name", "" },
                { "Type", "" },
                { "Price", "" }
            };
            foreach (var item in items)
            {
                collumnValueString["Name"] += item.Name + "\n";
                collumnValueString["Price"] += ":coin: " + item.Price + "\n";
                collumnValueString["Type"] += ItemCategories.itemSubTypeToIcon[item.SubType] + " " + item.SubType.ToString().ToLower() + "\n";
            }

            foreach(var keyPair in  collumnValueString) 
            {
                embed.AddField(keyPair.Key, keyPair.Value, true);
            }
        }
    }
}
