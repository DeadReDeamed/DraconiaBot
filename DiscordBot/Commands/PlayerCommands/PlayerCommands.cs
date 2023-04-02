using DiscordBot.Database.Profile;
using DiscordBot.Database.Queries;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus;
using DiscordBot.PlayerFunctionality.Creation;

namespace DiscordBot.Commands.PlayerCommands
{
    
    public class PlayerCommands : BaseCommandModule
    {
        [Command("myPlayer")]
        public async Task GetPlayerProfile(CommandContext ctx)
        {
            var DiscordId = ctx.User.Id;
            var GuildId = ctx.Guild.Id;
            var player = await PlayerQuerries.GetPlayer(DiscordId, GuildId);
            if (player == null)
            {
                var errorEmbed = new DiscordEmbedBuilder
                {
                    Title = "Character not found",
                    Description = "You don't have a character yet, please create one with the .createCharacter command!",
                    Color = DiscordColor.Red,
                };
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{ctx.Guild.Members[player.DiscordId].DisplayName}'s player"
            };
            embed.WithThumbnail(ctx.Guild.Members[player.DiscordId].AvatarUrl);
            embed.AddField("Xp", player.Xp.ToString());
            await ctx.Channel.SendMessageAsync(embed: embed);
        }

        [Command("addXp")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task AppXpToPlayer(CommandContext ctx, int amount)
        {
            var DiscordId = ctx.User.Id;
            var GuildId = ctx.Guild.Id;

            await PlayerQuerries.AddXp(DiscordId, GuildId, amount);
            await ctx.Channel.SendMessageAsync($"{amount} xp added");
        }

        [Command("addXp")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task AppXpToPlayer(CommandContext ctx, DiscordMember member, int amount)
        {
            var DiscordId = member.Id;
            var GuildId = ctx.Guild.Id;

            await PlayerQuerries.AddXp(DiscordId, GuildId, amount);
            await ctx.Channel.SendMessageAsync($"{amount} xp added");
        }

        [Command("createCharacter")]
        public async Task CreateCharacter(CommandContext ctx)
        {
            CharacterCreator creator = new CharacterCreator();
            await creator.CreateCharacter(ctx);
        }
    }
}
