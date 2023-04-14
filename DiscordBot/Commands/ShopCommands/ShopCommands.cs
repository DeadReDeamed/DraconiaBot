using DiscordBot.Database.Items;
using DiscordBot.Database.Queries;
using DiscordBot.Enums;
using DiscordBot.ShopFunctionality.ShopMessages;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace DiscordBot.Commands.ShopCommands
{
    public class ShopCommands : BaseCommandModule
    {
        [Command("openShop")]
        public async Task openShop(CommandContext ctx)
        {
            var shopMessages = new ShopMessages();
            await shopMessages.startShop(ctx);
        }
        
    }
}
