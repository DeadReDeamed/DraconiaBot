using Azure.Identity;
using DiscordBot.Database.Profile;
using DiscordBot.Database.Queries;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading.Channels;

namespace DiscordBot.PlayerFunctionality.Creation
{
    public class CharacterCreator
    {
        public Player player { get; private set; }
        private InteractivityExtension interactivity;
        private Dictionary<string, short> emojiNumberPair = new Dictionary<string, short>()
        {
            {":one:", 1 },
            {":two:", 2 },
            {":three:", 3 },
            {":four:", 4 },
            {":five:", 5 },
            {":six:", 6 },
            {":seven:", 7 }
        };

        int attributePoints = 25;

        public async Task CreateCharacter(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            interactivity = ctx.Client.GetInteractivity();
            var category = ctx.Guild.Channels.Values.ToList().Find(x => x.Name == "Character creation");
            var permissionList = new System.Collections.Generic.List<DiscordOverwriteBuilder>() { new DiscordOverwriteBuilder(ctx.Guild.EveryoneRole).Deny(Permissions.AccessChannels) };

            if (category == null)
            {
                category = await ctx.Guild.CreateChannelCategoryAsync("Character creation", permissionList, 100);
            } else
            {
                await category.AddOverwriteAsync(ctx.Member, Permissions.AccessChannels);
            }

            var channel = category.Children.ToList().Find(x => x.Name == ctx.Member.DisplayName.ToLower() + "-character-creator");
            if(channel != null) 
            {
                return;
            }
            channel = await ctx.Guild.CreateChannelAsync(ctx.Member.DisplayName + " character creator", DSharpPlus.ChannelType.Text, category, null, null, null, permissionList);
            await channel.AddOverwriteAsync(ctx.Member, Permissions.AccessChannels);

            var tempMessage = await channel.SendMessageAsync(ctx.Member.Mention);
            await tempMessage.DeleteAsync();

            player = await PlayerQuerries.GetPlayer(ctx.Member.Id, ctx.Guild.Id);

            if(player != null) 
            {
                var deleteCharacterMessage = await channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Title = "Character already exists!",
                    Color = DiscordColor.Yellow,
                    Description = $"Character with the name {player.Name} already exists, do you want to create a new character and delete this one?"
                });
                await deleteCharacterMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":white_check_mark:"));
                await deleteCharacterMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":negative_squared_cross_mark:"));

                var deleteReaction = await interactivity.WaitForReactionAsync(deleteCharacterMessage, ctx.User, TimeSpan.FromMinutes(15));
                string deleteReactionStr = deleteReaction.Result.Emoji.GetDiscordName();

                if (deleteReactionStr.Equals(":white_check_mark:"))
                {
                    await PlayerQuerries.DeleteCharacter(player.Id, player.DiscordId, player.GuildId);
                    await deleteCharacterMessage.DeleteAsync();
                } 
                else if (deleteReactionStr.Equals(":negative_squared_cross_mark:"))
                {
                    await channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "Canceling character creation",
                        Color = DiscordColor.Yellow
                    });

                    await Task.Delay(1000);

                    await channel.DeleteAsync();
                    if (category.Children.Count == 0 )
                    {
                        await category.DeleteAsync();
                    }

                }
            }

            player = new Player(0);
            player.Name = "empty";
            player.attributes = new Attributes
            {
                Strength = 8,
                Dexterity = 8,
                Constitution = 8,
                Intelligence = 8,
                Wisdom = 8,
                Charisma = 8
            };

            var message = await CreateCharacterPopUp(ctx, channel);

            var reaction = await interactivity.WaitForReactionAsync(message, ctx.User, TimeSpan.FromMinutes(15));
            if(reaction.Result == null)
            {
                await channel.DeleteAsync();
                if (category.Children.Count == 0)
                {
                    await category.DeleteAsync();
                }
                return;
            }
            string reactionStr = reaction.Result.Emoji.GetDiscordName();

            while (!reactionStr.Equals(":white_check_mark:"))
            {
                await message.DeleteAsync();
                if (reactionStr.Equals(":scroll:"))
                {
                    bool succeded = await ChangeAttributes(ctx, channel);
                    if(!succeded) 
                    {
                        await channel.DeleteAsync();
                        if (category.Children.Count == 0)
                        {
                            await category.DeleteAsync();
                        }
                        return;
                    }
                }
                else if (reactionStr.Equals(":writing_hand:"))
                {
                    await ChangeName(ctx, channel);
                }
                message = await CreateCharacterPopUp(ctx, channel);
                reaction = await interactivity.WaitForReactionAsync(message, ctx.User, TimeSpan.FromMinutes(15));
                if (reaction.Result != null)
                {
                    reactionStr = reaction.Result.Emoji.GetDiscordName();
                }
                else
                {
                    break;
                }
            }

            await message.DeleteAsync();
            await channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Character created",
                Color = DiscordColor.Green
            });

            player.DiscordId = ctx.User.Id;
            player.GuildId = ctx.Guild.Id;

            bool succes = await PlayerQuerries.AddPlayer(player.Name, player.DiscordId, player.GuildId);
            if (succes)
            {
                long id = await PlayerQuerries.GetPlayerId(player.DiscordId, player.GuildId);
                succes = await PlayerQuerries.AddAttributes(id, player.attributes);
                if(succes) 
                {
                    await Task.Delay(1000);

                    await channel.DeleteAsync();
                    if (category.Children.Count == 0)
                    {
                        await category.DeleteAsync();
                    }
                } else
                {
                    var errorEmbed = new DiscordEmbedBuilder
                    {
                        Title = "Something went wrong",
                        Description = "Please try again later!",
                        Color = DiscordColor.Red,
                    };
                    await ctx.Channel.SendMessageAsync(embed: errorEmbed);
                }
            }

            
        }

        private async Task<DiscordMessage> CreateCharacterPopUp(CommandContext ctx, DiscordChannel channel)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Character creator",
                Color = DiscordColor.Green,
                Description = "Click on :writing_hand: to change the name or on :scroll: to change attributes",
                Timestamp = DateTime.Now,
            };

            embed.AddField("Name:", player.Name);
            embed.AddField("Attributes", $":muscle: Strength: {player.attributes.Strength}" +
                $"\n:man_running: Dexterity: {player.attributes.Dexterity}" +
                $"\n:heart: Constitution:{player.attributes.Constitution} " +
                $"\n:brain: Intelligence: {player.attributes.Intelligence}" +
                $"\n:church: Wisdom: {player.attributes.Wisdom}" +
                $"\n:speech_balloon: Charisma: {player.attributes.Charisma}");
            embed.WithFooter("Click on the checkmark to finish your character");

            var message = await channel.SendMessageAsync(embed: embed);
            String[] emojies = { ":writing_hand:", ":scroll:", ":white_check_mark:" };
            foreach (var emoji in emojies)
            {
                await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, emoji));
            }

            return message;
        }

        private async Task<DiscordMessage> CreateAttributesMessage(CommandContext ctx, DiscordChannel channel, int abilityPoints)
        {
            String[] abilityEmojies = { ":muscle:", ":man_running:", ":heart:", ":brain:", ":church:", ":speech_balloon:", ":white_check_mark:" };
            var embed = new DiscordEmbedBuilder
            {
                Title = "Character creator",
                Color = DiscordColor.Green,
                Description = "Click on a attribute icon to change values",
                Timestamp = DateTime.Now,
            };

            embed.AddField("Ability points left:", abilityPoints.ToString());
            embed.AddField("Attributes", $":muscle: Strength: {player.attributes.Strength}" +
                $"\n:man_running: Dexterity: {player.attributes.Dexterity}" +
                $"\n:heart: Constitution:{player.attributes.Constitution} " +
                $"\n:brain: Intelligence: {player.attributes.Intelligence}" +
                $"\n:church: Wisdom: {player.attributes.Wisdom}" +
                $"\n:speech_balloon: Charisma: {player.attributes.Charisma}");
            embed.WithFooter("Click on the checkmark to finish changing attributes");

            var message = await channel.SendMessageAsync(embed: embed);
            String[] emojies = { ":writing_hand:", ":scroll:", ":white_check_mark:" };
            foreach (var emoji in abilityEmojies)
            {
                await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, emoji));
            }
            return message;
        }

        private async Task<bool> ChangeAttributes(CommandContext ctx, DiscordChannel channel)
        {
            var message = await CreateAttributesMessage(ctx, channel, attributePoints);
            var reaction = await interactivity.WaitForReactionAsync(message, ctx.User, TimeSpan.FromMinutes(15));
            if(reaction.Result == null) 
            {
                await message.DeleteAsync();
                return false;
            }
            string reactionStr = reaction.Result.Emoji.GetDiscordName();

            while (!reactionStr.Equals(":white_check_mark:"))
            {
                await message.DeleteAsync();
                var embed = new DiscordEmbedBuilder
                {
                    Title = "Character creator",
                    Description = "Firstly select if you want to add or subtract points and then how many. You can't go higher than 15 or lower than 8.",
                    Color = DiscordColor.Green
                };
                string attributeText = "";
                string category = reactionStr;
                short attributeValue = 0;
                switch (reactionStr)
                {
                    case ":muscle:":
                        attributeText = $":muscle: Strength: {player.attributes.Strength}";
                        attributeValue = player.attributes.Strength;
                        break;
                    case ":man_running:":
                        attributeText = $":man_running: Dexterity: {player.attributes.Dexterity}";
                        attributeValue = player.attributes.Dexterity;
                        break;
                    case ":heart:":
                        attributeText = $":heart: Constitution: {player.attributes.Constitution}";
                        attributeValue = player.attributes.Constitution;
                        break;
                    case ":brain:":
                        attributeText = $":brain: Intelligence: {player.attributes.Intelligence}";
                        attributeValue = player.attributes.Intelligence;
                        break;
                    case ":church:":
                        attributeText = $":church: Wisdom: {player.attributes.Wisdom}";
                        attributeValue = player.attributes.Wisdom;
                        break;
                    case ":speech_balloon:":
                        attributeText = $":speech_balloon: Charisma: {player.attributes.Charisma}";
                        attributeValue = player.attributes.Charisma;
                        break;
                }
                embed.AddField("Attribute:", attributeText);
                message = await channel.SendMessageAsync(embed: embed);
                if (attributeValue != 15)
                {
                    await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":arrow_up:"));
                }

                if (attributeValue != 8)
                {
                    await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":arrow_down:"));
                }

                reaction = await interactivity.WaitForReactionAsync(message, ctx.User, TimeSpan.FromMinutes(15));
                if(reaction.Result == null)
                {
                    await message.DeleteAsync();
                    return false;
                }
                reactionStr = reaction.Result.Emoji.GetDiscordName();

                bool addBool = false;

                if (reactionStr.Equals(":arrow_up:"))
                {
                    addBool = true;
                }

                await message.DeleteAllReactionsAsync();

                foreach (var key in emojiNumberPair)
                {
                    if ((addBool && key.Value <= attributePoints && attributeValue + key.Value <= 15) || (!addBool && attributeValue - key.Value >= 8))
                    {
                        await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, key.Key));
                    }
                }

                reaction = await interactivity.WaitForReactionAsync(message, ctx.User, TimeSpan.FromMinutes(15));
                if(reaction.Result == null) {
                    await message.DeleteAsync();
                    return false; 
                }
                reactionStr = reaction.Result.Emoji.GetDiscordName();
                
                short numberAmount = emojiNumberPair[reactionStr];

                if (addBool)
                {
                    switch (category)
                    {
                        case ":muscle:":
                            player.attributes.Strength += numberAmount;
                            break;
                        case ":man_running:":
                            player.attributes.Dexterity += numberAmount;
                            break;
                        case ":heart:":
                            player.attributes.Constitution += numberAmount;
                            break;
                        case ":brain:":
                            player.attributes.Intelligence += numberAmount;
                            break;
                        case ":church:":
                            player.attributes.Wisdom += numberAmount;
                            break;
                        case ":speech_balloon:":
                            player.attributes.Charisma += numberAmount;
                            break;
                    }
                    attributePoints -= numberAmount;
                }
                else
                {
                    switch (category)
                    {
                        case ":muscle:":
                            player.attributes.Strength -= numberAmount;
                            break;
                        case ":man_running:":
                            player.attributes.Dexterity -= numberAmount;
                            break;
                        case ":heart:":
                            player.attributes.Constitution -= numberAmount;
                            break;
                        case ":brain:":
                            player.attributes.Intelligence -= numberAmount;
                            break;
                        case ":church:":
                            player.attributes.Wisdom -= numberAmount;
                            break;
                        case ":speech_balloon:":
                            player.attributes.Charisma -= numberAmount;
                            break;
                    }
                    attributePoints += numberAmount;
                }

                await message.DeleteAsync();
                message = await CreateAttributesMessage(ctx, channel, attributePoints);
                reaction = await interactivity.WaitForReactionAsync(message, ctx.User, TimeSpan.FromMinutes(15));
                if(reaction.Result == null) 
                {
                    await message.DeleteAsync();
                    return false; 
                }
                reactionStr = reaction.Result.Emoji.GetDiscordName();
            }

            await message.DeleteAsync();
            return true;
        }

        private async Task ChangeName(CommandContext ctx, DiscordChannel channel)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Character creator",
                Color = DiscordColor.Green,
                Description = ":writing_hand: Type a new name for your character",
                Timestamp = DateTime.Now,
            };

            var message = await channel.SendMessageAsync(embed: embed);
            var nameMessage = await interactivity.WaitForMessageAsync(x => x.ChannelId == channel.Id && x.Author.Id == ctx.Message.Author.Id, TimeSpan.FromMinutes(5));
            if(nameMessage.Result != null)
            {
                player.Name = nameMessage.Result.Content;
                await nameMessage.Result.DeleteAsync();
            }
            await message.DeleteAsync();
        }
    }
}
