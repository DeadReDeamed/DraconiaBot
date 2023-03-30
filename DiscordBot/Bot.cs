using Draconia_bot.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Draconia_bot
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        public CommandsNextExtension Commands { get; private set; }

        public async Task RunAsync()
        {
            
            string token = Environment.GetEnvironmentVariable("DISCORD_TOKEN");

            if(token == null)
            {
                Console.WriteLine("Token was not found!");
                Environment.Exit(-1);
            }

            var config = new DiscordConfiguration()
            {
                Token = token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents,
            };

            Client = new DiscordClient(config);

            Client.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(1)
            });

            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { "." },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = true,
                DmHelp = true,
            };

            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.RegisterCommands<FunCommands>();

            await Client.ConnectAsync();

            await Task.Delay(-1);
        }

        private Task OnClientReady(ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
    }
}
