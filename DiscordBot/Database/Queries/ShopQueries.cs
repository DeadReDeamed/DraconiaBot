using Data;
using DiscordBot.Database.Items;
using DiscordBot.Enums;
using DiscordBot.ShopFunctionality.ShopMessages;
using DSharpPlus;
using Microsoft.Identity.Client;
using MySqlConnector;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace DiscordBot.Database.Queries
{
    public static class ShopQueries
    {
        public static async Task<string> getRandomShopMessage(ShopType shopType, ShopMessageType messageType, string townName)
        {
            string shopMessage = string.Empty;

            var connection = DBConnection.Instance().getConnection();
            if (connection != null)
            {
                try
                {
                    var command = new MySqlCommand($"SELECT * FROM shop_messages WHERE townName=\"{townName}\" AND messageType=\"{messageType}\" AND shopType=\"{shopType}\"", connection);
                    var reader = await command.ExecuteReaderAsync();
                    DataTable table = new DataTable();
                    table.Load(reader);
                    int numberOfResults = table.Rows.Count;
                    Random rnd = new Random();
                    int index = rnd.Next(0, numberOfResults);
                    if (table.Rows != null)
                    {
                        shopMessage = table.Rows[index]["message"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[DBERROR] " + ex.Message);
                }
                connection.Close();
            }

            return shopMessage;
        }

        public static async Task<List<Item>> getShopItems(ItemCategory type)
        {
            List<Item> items = new List<Item>();
            
            var connection = DBConnection.Instance().getConnection();
            if (connection != null)
            {
                try
                {
                    var command = new MySqlCommand($"SELECT * FROM items WHERE Type=\"{type}\"", connection);
                    var reader = await command.ExecuteReaderAsync();
                    if(reader.HasRows)
                    {
                        while (await reader.ReadAsync())
                        {
                            var item = new Item
                            {
                                Id = reader.GetUInt64("Id"),
                                Name = reader.GetString("Name"),
                                Description = reader.GetString("Description"),
                                Type = type,
                                SubType = Enum.Parse<ItemSubType>(reader.GetString("Subtype")),
                                Rarity = Enum.Parse<Rarity>(reader.GetString("Rarity")),
                                Price = reader.GetInt32("Price"),
                                modifier = reader.GetInt32("Modifier")
                            };
                            items.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[DBERROR] " + ex.Message);
                }
                connection.Close();
            }

            return items;
        }
    }
}
