using DiscordBot.Database.Items;
using DiscordBot.Database.Queries;
using DiscordBot.Enums;
using DiscordBot.Util.Embed;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.ShopFunctionality.ShopMessages
{
    public class ShopMessages
    {
        string currentTown = "Yleria";
        public async Task startShop(CommandContext ctx)
        {
            await showCategories(ctx);
        }

        public async Task showCategories(CommandContext ctx)
        {
            var message = await showWelcomeScreen(ctx);
            foreach (var category in ItemCategories.buttonMapping.Keys)
            {
                await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, category));
            }
            await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":negative_squared_cross_mark:"));

            var interActivity = ctx.Client.GetInteractivity();
            var reaction = await interActivity.WaitForReactionAsync(message, ctx.User, TimeSpan.FromMinutes(1));
            if (reaction.Result == null) { await message.DeleteAsync(); return; }

            string resultEmoji = reaction.Result.Emoji.GetDiscordName();
            await message.DeleteAsync();
            await showSelectedCategory(ctx, ItemCategories.buttonMapping[resultEmoji]);
        }

        public async Task showSelectedCategory(CommandContext ctx, ItemCategory category)
        {
            var items = await ShopQueries.getShopItems(category);
            var embed = new DiscordEmbedBuilder
            {
                Title = ":scales:Shop",
                ImageUrl = "https://www.tribality.com/wp-content/uploads/2021/04/robin-olausson-shopkeeper71-750x375.jpg",
                Description = $"This is the collection of {category.ToString().ToLower()} I have laying around.",
                Color = new DiscordColor("#a441bf")
            };

            EmbedUtil.AddItemTableToEmbed(ref embed, items);
            await ctx.Channel.SendMessageAsync(embed: embed);
        }

        public async Task<DiscordMessage> showWelcomeScreen(CommandContext ctx)
        {
            var shopText = await ShopQueries.getRandomShopMessage(ShopType.GENERAL, ShopMessageType.WELCOME, currentTown);
            var embed = new DiscordEmbedBuilder
            {
                Title = ":scales:Shop",
                ImageUrl = "https://www.tribality.com/wp-content/uploads/2021/04/robin-olausson-shopkeeper71-750x375.jpg",
                Description = shopText,
                Color = new DiscordColor("#a441bf")
            };
            var itemCategoryString = "";
            foreach (var category in ItemCategories.buttonMapping)
            {
                itemCategoryString += $"{category.Key} {category.Value.ToString().ToLower()}\n";
            }
            embed.AddField("Categories:", itemCategoryString);

            var message = await ctx.Channel.SendMessageAsync(embed: embed);

            return message;
        }
    }
}
