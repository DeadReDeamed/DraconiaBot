using Data;
using DiscordBot.Database.Profile;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting.Server;
using MySqlConnector;
using System.Collections.Specialized;

namespace DiscordBot.Database.Queries
{
    public class PlayerQuerries
    {
        public static async Task<Player> GetPlayer(ulong DiscordId, ulong GuildId)
        {
            Player player = null;
            var connection = DBConnection.Instance().getConnection();
            if (connection != null) {
                var command = new MySqlCommand($"SELECT * FROM players WHERE DiscordId={DiscordId} AND GuildId={GuildId}", connection);
                var reader = await command.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    await reader.ReadAsync();
                    player = new Player(reader.GetInt32(0), reader.GetUInt64(1), reader.GetUInt64(2), reader.GetInt32(3));
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

        public static async Task AddXp(ulong DiscordId, ulong GuildId, int xp)
        {
            var connection = DBConnection.Instance().getConnection();
            if (connection != null)
            {
                var command = new MySqlCommand($"UPDATE players SET Xp=(Xp + {xp}) WHERE DiscordId={DiscordId} AND GuildId={GuildId}", connection);
                await command.ExecuteScalarAsync();
                connection.Close();
            }
            return;
        }
    }
}
