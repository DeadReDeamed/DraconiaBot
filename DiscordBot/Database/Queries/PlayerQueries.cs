using Data;
using DiscordBot.Database.Profile;
using MySqlConnector;
using System.Numerics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace DiscordBot.Database.Queries
{
    public class PlayerQuerries
    {
        public static async Task<Player> GetPlayer(ulong DiscordId)
        {
            Player player = null;
            var connection = DBConnection.Instance().getConnection();
            if (connection != null)
            {
                try
                {
                    var command = new MySqlCommand($"SELECT * FROM player_attributes a LEFT JOIN players p ON p.Id = a.Id WHERE p.DiscordId={DiscordId}", connection);
                    var reader = await command.ExecuteReaderAsync();
                    if (reader.HasRows)
                    {
                        await reader.ReadAsync();
                        player = new Player(reader.GetUInt32(reader.GetOrdinal("Id")),
                            reader.GetString(reader.GetOrdinal("Name")),
                            reader.GetUInt64(reader.GetOrdinal("DiscordId")),
                            reader.GetInt32(reader.GetOrdinal("Xp")),
                            reader.GetUInt32(reader.GetOrdinal("Gold")));

                        player.attributes = new Attributes(player.Id,
                            reader.GetInt16(reader.GetOrdinal("Strength")),
                            reader.GetInt16(reader.GetOrdinal("Dexterity")),
                            reader.GetInt16(reader.GetOrdinal("Constitution")),
                            reader.GetInt16(reader.GetOrdinal("Intelligence")));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[DBERROR] " + ex.Message);
                }
                connection.Close();
            }
            return player;
        }

        public static async Task<long> GetPlayerId(ulong DiscordId)
        {
            long id = 0;
            var connection = DBConnection.Instance().getConnection();
            if (connection != null)
            {
                var command = new MySqlCommand($"SELECT Id FROM players WHERE players.DiscordId={DiscordId}", connection);

                var reader = await command.ExecuteReaderAsync();
                if (reader != null)
                {
                    await reader.ReadAsync();
                    id = reader.GetInt64(0);
                }
                connection.Close();
            }
            return id;
        }

        public static async Task<bool> AddPlayer(string Name, ulong DiscordId)
        {
            try
            {
                var connection = DBConnection.Instance().getConnection();
                if (connection != null)
                {
                    var command = new MySqlCommand($"INSERT INTO players (Name, DiscordId, Xp, Gold) VALUES" +
                                                $"(\"{Name}\", {DiscordId}, 0, 0)", connection);

                    await command.ExecuteScalarAsync();
                    connection.Close();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        public static async Task<bool> AddAttributes(long id, Attributes attributes)
        {
            try
            {
                var connection = DBConnection.Instance().getConnection();
                if (connection != null)
                {
                    var command = new MySqlCommand($"INSERT INTO player_attributes VALUES ({id}, " +
                    $"{attributes.Strength}, " +
                    $"{attributes.Dexterity}, " +
                    $"{attributes.Constitution}, " +
                    $"{attributes.Intelligence})", connection);
                    await command.ExecuteScalarAsync();

                    connection.Close();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        public static async Task<Attributes> GetAttributes(long DiscordId)
        {
            var connection = DBConnection.Instance().getConnection();
            Attributes attributes = null;
            if (connection != null)
            {
                var command = new MySqlCommand($"SELECT * FROM player_attributes a LEFT JOIN players p ON p.Id = a.Id WHERE p.DiscordId={DiscordId}", connection);
                var reader = await command.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    attributes = new Attributes(
                        reader.GetUInt64(reader.GetOrdinal("Id")),
                    reader.GetInt16(reader.GetOrdinal("Strength")),
                    reader.GetInt16(reader.GetOrdinal("Dexterity")),
                    reader.GetInt16(reader.GetOrdinal("Constitution")),
                    reader.GetInt16(reader.GetOrdinal("Intelligence")));
                }
                connection.Close();
            }
            return attributes;
        }

        public static async Task UpdateAttributes(long DiscordId, Attributes attributes)
        {
            var connection = DBConnection.Instance().getConnection();
            if (connection != null)
            {
                var command = new MySqlCommand($"UPDATE player_attributes a INNER JOIN players p ON a.Id=p.Id SET " +
                $"Strength={attributes.Strength}, " +
                $"Dexterity={attributes.Dexterity}, " +
                $"Constitution={attributes.Constitution}, " +
                $"Intelligence={attributes.Intelligence}, " +
                $"WHERE p.DiscordId={DiscordId})", connection);
                await command.ExecuteScalarAsync();

                connection.Close();
            }
        }

        public static async Task AddXp(ulong DiscordId, int xp)
        {
            var connection = DBConnection.Instance().getConnection();
            if (connection != null)
            {
                var command = new MySqlCommand($"UPDATE players SET Xp=(Xp + {xp}) WHERE DiscordId={DiscordId}", connection);
                await command.ExecuteScalarAsync();
                connection.Close();
            }
        }

        public static async Task SetGold(ulong DiscordId, uint gold)
        {
            var connection = DBConnection.Instance().getConnection();
            if (connection != null)
            {
                var command = new MySqlCommand($"UPDATE players SET Gold={gold} WHERE DiscordId={DiscordId}", connection);
                await command.ExecuteScalarAsync();
                connection.Close();
            }
        }

        public static async Task DeleteCharacter(ulong id, ulong DiscordId)
        {
            try
            {
                var connection = DBConnection.Instance().getConnection();
                if (connection != null)
                {
                    var command = new MySqlCommand($"DELETE FROM players WHERE DiscordId={DiscordId}", connection);
                    await command.ExecuteScalarAsync();
                    command = new MySqlCommand($"DELETE FROM player_attributes WHERE Id={id}", connection);
                    await command.ExecuteScalarAsync();
                    command = new MySqlCommand($"DELETE FROM inventory_items WHERE Id={id}", connection);
                    await command.ExecuteScalarAsync();
                    connection.Close();
                }
            } catch(Exception ex) 
            {
                Console.WriteLine(ex.Message);
            } 

        }
    }
}
