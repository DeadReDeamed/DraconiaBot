using Data;
using DiscordBot.Database;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Draconia_bot
{
    public static class Program
    {
        static void Main(string[] args)
        {
            DBConnection.Instance().Server = "host.docker.internal";
            DBConnection.Instance().DatabaseName = "discordbotdatabase";
            DBConnection.Instance().UserName = "bot";
            DBConnection.Instance().Password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");

            var bot = new Bot();
            bot.RunAsync().GetAwaiter().GetResult();
        }

    }
}
