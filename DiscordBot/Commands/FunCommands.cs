using Draconia_bot.Handlers.Dialog;
using Draconia_bot.Handlers.Dialog.Steps;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

namespace Draconia_bot.Commands
{
    public class FunCommands : BaseCommandModule
    {
        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("pong");
        }

        [Command("response")]
        public async Task Response(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();

            await interactivity.WaitForMessageAsync(x => true);
        }

        [Command("battle")]
        public async Task Battle(CommandContext ctx, DiscordMember member)
        {
            var channels = ctx.Guild.Channels.Values.ToList();

            var deniedEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Red,
            };

            if(ctx.User == member)
            {
                deniedEmbed.Title = "You can't start a battle against yourself";
                var message = await ctx.Channel.SendMessageAsync(embed: deniedEmbed);
                await Task.Delay(2000);
                await ctx.Channel.DeleteMessageAsync(message);
                await ctx.Channel.DeleteMessageAsync(ctx.Message);
                return;
            } 

            var category = channels.Find(x => x.Name == "Battles");
            var permissionList = new System.Collections.Generic.List<DiscordOverwriteBuilder>() { new DiscordOverwriteBuilder(ctx.Member).Allow(Permissions.AccessChannels), new DiscordOverwriteBuilder(member).Allow(Permissions.AccessChannels), new DiscordOverwriteBuilder(ctx.Guild.EveryoneRole).Deny(Permissions.AccessChannels) };

            if (category == null)
            {
                category = await ctx.Guild.CreateChannelCategoryAsync("Battles", permissionList, 100);
            }
            
            var userChannel = await ctx.Guild.CreateChannelAsync(ctx.Member.DisplayName + " v " + member.DisplayName, DSharpPlus.ChannelType.Text, category, "test", null, null, permissionList);
            
            var embed = new DiscordEmbedBuilder()
            {
                Title = ctx.Member.DisplayName + " versus " + member.DisplayName,
                Description = ctx.Member.DisplayName + " started a battle against " + member.DisplayName,
                Color = DiscordColor.Red
            };

            embed.AddField("Reward", "100g", false);

            await userChannel.SendMessageAsync(embed: embed);
        }

        [Command("clear")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ClearChannel(CommandContext ctx, int amount)
        {
            var messages = await ctx.Channel.GetMessagesAsync(amount);
            await ctx.Channel.DeleteMessagesAsync(messages);
        }
    }
}
