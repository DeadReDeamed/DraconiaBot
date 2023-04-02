using Data;
using DiscordBot.Database.Profile;
using MySqlConnector;
using System.Numerics;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace DiscordBot.Database.Queries
{
    public class PlayerQuerries
    {
        public static async Task<Player> GetPlayer(ulong DiscordId, ulong GuildId)
        {
            Player player = null;
            var connection = DBConnection.Instance().getConnection();
            if (connection != null) {
                var command = new MySqlCommand($"SELECT * FROM players p, player_attributes a INNER JOIN players ON a.Id=p.Id WHERE p.DiscordId={DiscordId} AND p.GuildId={GuildId}", connection);
                var reader = await command.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    await reader.ReadAsync();
                    player = new Player(reader.GetInt32(reader.GetOrdinal("Id")), 
                        reader.GetString(reader.GetOrdinal("Name")), 
                        reader.GetUInt64(reader.GetOrdinal("DiscordId")), 
                        reader.GetUInt64(reader.GetOrdinal("GuildId")), 
                        reader.GetInt32(reader.GetOrdinal("Xp")));

                    player.attributes = new Attributes(player.Id,
                        reader.GetInt16(reader.GetOrdinal("Strength")),
                        reader.GetInt16(reader.GetOrdinal("Dexterity")),
                        reader.GetInt16(reader.GetOrdinal("Constitution")),
                        reader.GetInt16(reader.GetOrdinal("Intelligence")),
                        reader.GetInt16(reader.GetOrdinal("Wisdom")),
                        reader.GetInt16(reader.GetOrdinal("Charisma")));
                }
                connection.Close();
            } 
            return player;
        }

        public static async Task AddPlayer(ulong DiscordId, ulong GuildId)
        {
            var connection = DBConnection.Instance().getConnection();
            if (connection != null)
            {
                var command = new MySqlCommand($"INSERT INTO players (DiscordId, GuildId, Xp) VALUES" +
                                            $"({DiscordId}, {GuildId}, 0)", connection);

                await command.ExecuteScalarAsync();
                connection.Close();
            }
            return;
        }

        public static async Task AddAttributes(long id, Attributes attributes)
        {
            var connection = DBConnection.Instance().getConnection();
            if (connection != null)
            {
                var command = new MySqlCommand($"INSERT INTO player_attributes VALUES ({id}, " +
                $"{attributes.Strength}, " +
                $"{attributes.Dexterity}, " +
                $"{attributes.Constitution}, " +
                $"{attributes.Intelligence}, " +
                $"{attributes.Wisdom}, " +
                $"{attributes.Charisma})", connection);
                await command.ExecuteScalarAsync();

                connection.Close();
            }
        }

        public static async Task<Attributes> GetAttributes(long DiscordId, long GuildId)
        {
            var connection = DBConnection.Instance().getConnection();
            Attributes attributes = null;
            if (connection != null)
            {
                var command = new MySqlCommand($"SELECT * FROM players p, player_attributes a INNER JOIN players ON a.Id=p.Id WHERE p.DiscordId={DiscordId} AND p.GuildId={GuildId}", connection);
                var reader = await command.ExecuteReaderAsync();
               
                if (reader.HasRows)
                {
                        attributes = new Attributes(
                            reader.GetInt64(reader.GetOrdinal("Id")),
                        reader.GetInt16(reader.GetOrdinal("Strength")),
                        reader.GetInt16(reader.GetOrdinal("Dexterity")),
                        reader.GetInt16(reader.GetOrdinal("Constitution")),
                        reader.GetInt16(reader.GetOrdinal("Intelligence")),
                        reader.GetInt16(reader.GetOrdinal("Wisdom")),
                        reader.GetInt16(reader.GetOrdinal("Charisma")));
                }
                connection.Close();
            }
            return attributes;
        }

        public static async Task UpdateAttributes(long DiscordId, long GuildId, Attributes attributes)
        {
            var connection = DBConnection.Instance().getConnection();
            if (connection != null)
            {
                var command = new MySqlCommand($"UPDATE player_attributes a INNER JOIN players p ON a.Id=p.Id SET " +
                $"Strength={attributes.Strength}, " +
                $"Dexterity={attributes.Dexterity}, " +
                $"Constitution={attributes.Constitution}, " +
                $"Intelligence={attributes.Intelligence}, " +
                $"Wisdom={attributes.Wisdom}, " +
                $"Charisma={attributes.Charisma}" +
                $"WHERE p.DiscordId={DiscordId} AND p.GuildId={GuildId}", connection);
                await command.ExecuteScalarAsync();

                connection.Close();
            }
        }

        public static async Task AddXp(ulong DiscordId, ulong GuildId, int xp)
        {
            var connection = DBConnection.Instance().getConnection();
            if (connection != null)
            {
                var command = new MySqlCommand($"UPDATE players SET Xp=(Xp + {xp}) WHERE DiscordId={DiscordId} AND GuildId={GuildId}", connection);
                await command.ExecuteScalarAsync();
                connection.Close();
            }
        }
    }
}
