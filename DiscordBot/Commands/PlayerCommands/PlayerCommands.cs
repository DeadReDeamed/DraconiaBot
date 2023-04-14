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
        [Command("myCharacter")]
        public async Task GetPlayerProfile(CommandContext ctx)
        {
            var DiscordId = ctx.User.Id;
            var GuildId = ctx.Guild.Id;
            var player = await PlayerQuerries.GetPlayer(DiscordId);
            if (player == null)
            {
                var errorEmbed = new DiscordEmbedBuilder
                {
                    Title = "Character not found",
                    Description = "You don't have a character yet or something went wrong, please create one with the .createCharacter command!",
                    Color = DiscordColor.Red,
                };
                await ctx.Channel.SendMessageAsync(embed: errorEmbed);
                return;
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{ctx.Guild.Members[player.DiscordId].DisplayName}'s player",
                Color = DiscordColor.Green
            };

            embed.AddField("Name:", player.Name);
            embed.AddField("Xp:", ":sparkle: " + player.Xp.ToString(), true);
            embed.AddField("Gold: ", ":coin: " + player.Gold, true);
            embed.AddField("Attributes", $":muscle: Strength: {player.attributes.Strength}" +
                $"\n:man_running: Dexterity: {player.attributes.Dexterity}" +
                $"\n:heart: Constitution:{player.attributes.Constitution} " +
                $"\n:brain: Intelligence: {player.attributes.Intelligence}") ;
            embed.AddField("Inventory: ", "empty");
            embed.WithThumbnail(ctx.Guild.Members[player.DiscordId].AvatarUrl);
            await ctx.Channel.SendMessageAsync(embed: embed);
        }

        [Command("addXp")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task AppXpToPlayer(CommandContext ctx, int amount)
        {
            var DiscordId = ctx.User.Id;
            var GuildId = ctx.Guild.Id;

            await PlayerQuerries.AddXp(DiscordId, amount);
            await ctx.Channel.SendMessageAsync($"{amount} xp added");
        }

        [Command("addXp")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task AppXpToPlayer(CommandContext ctx, DiscordMember member, int amount)
        {
            var DiscordId = member.Id;
            var GuildId = ctx.Guild.Id;

            await PlayerQuerries.AddXp(DiscordId, amount);
            await ctx.Channel.SendMessageAsync($"{amount} xp added");
        }

        [Command("createCharacter")]
        public async Task CreateCharacter(CommandContext ctx)
        {
            CharacterCreator creator = new CharacterCreator();
            await creator.CreateCharacter(ctx);
        }

        [Command("createRandomCharacter")]
        public async Task CreateRandomCharacter(CommandContext ctx, string name)
        {
            CharacterCreator creator = new CharacterCreator();
            await creator.CreateRandomCharacter(ctx, name);
        }
    }
}
