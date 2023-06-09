﻿using Azure.Identity;
using DiscordBot.Database.Profile;
using DiscordBot.Database.Queries;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
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

        int attributePoints = 12;

        public async Task CreateRandomCharacter(CommandContext ctx, string name)
        {
            await ctx.Message.DeleteAsync();
            var channel = await ctx.Member.CreateDmChannelAsync();
            interactivity = ctx.Client.GetInteractivity();

            player = await PlayerQuerries.GetPlayer(ctx.Member.Id);

            if (player != null)
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
                    await PlayerQuerries.DeleteCharacter(player.Id, player.DiscordId);
                    await deleteCharacterMessage.DeleteAsync();
                }
                else if (deleteReactionStr.Equals(":negative_squared_cross_mark:"))
                {
                    var cancelMessage = await channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "Canceling character creation",
                        Color = DiscordColor.Yellow
                    });
                    await Task.Delay(1000);

                    await cancelMessage.DeleteAsync();
                    return;
                }
            }
            player = new Player(name);

            CreateRandomAttributes();

            player.DiscordId = ctx.User.Id;
            bool succes = await PlayerQuerries.AddPlayer(player.Name, player.DiscordId);
            if (succes)
            {
                long id = await PlayerQuerries.GetPlayerId(player.DiscordId);
                succes = await PlayerQuerries.AddAttributes(id, player.attributes);
                if (succes)
                {
                    await Task.Delay(1000);
                    await channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "Character created",
                        Color = DiscordColor.Green
                    });
                }
                else
                {
                    var errorEmbed = new DiscordEmbedBuilder
                    {
                        Title = "Something went wrong",
                        Description = "Please try again later!",
                        Color = DiscordColor.Red,
                    };
                    await channel.SendMessageAsync(embed: errorEmbed);
                }
            }
        }

        public void CreateRandomAttributes()
        {
            Random random = new Random();
            short[] randomAttributes = new short[]{ 8, 8, 8, 8 };

            while(attributePoints > 0)
            {
                for(int i = 0; i < randomAttributes.Length; i++) 
                {
                    int randomPlus = 0;
                    if (attributePoints == 0) break;
                    if(attributePoints >= 7) 
                    {
                        randomPlus = random.Next(1, 7);
                    } else
                    {
                        randomPlus = random.Next(1, attributePoints);
                    }
                    randomAttributes[i] += (short)randomPlus;
                    if (randomAttributes[i] > 15)
                    {
                        int overflow = randomAttributes[i] % 15;
                        randomAttributes[i] = 15;
                        randomPlus -= overflow;
                    }
                    attributePoints -= randomPlus;
                }
            }

            player.attributes = new Attributes
            {
                Strength = randomAttributes[0],
                Constitution = randomAttributes[1],
                Intelligence = randomAttributes[2],
                Dexterity = randomAttributes[3],
            };

        }

        public async Task CreateCharacter(CommandContext ctx)
        {
            await ctx.Message.DeleteAsync();
            interactivity = ctx.Client.GetInteractivity();

            var channel = await ctx.Member.CreateDmChannelAsync();

            player = await PlayerQuerries.GetPlayer(ctx.Member.Id);

            // There is a character so checking if the player wants to create a new one
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
                    await PlayerQuerries.DeleteCharacter(player.Id, player.DiscordId);
                    await deleteCharacterMessage.DeleteAsync();
                } 
                else if (deleteReactionStr.Equals(":negative_squared_cross_mark:"))
                {
                    var cancelMessage = await channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "Canceling character creation",
                        Color = DiscordColor.Yellow
                    });
                    await Task.Delay(1000);

                    await cancelMessage.DeleteAsync();
                    return;
                }
            }

            // Creating new player
            player = new Player(0);
            player.Name = "empty";
            player.attributes = new Attributes
            {
                Strength = 8,
                Dexterity = 8,
                Constitution = 8,
                Intelligence = 8
            };

            var message = await CreateCharacterPopUp(ctx, channel);

            // Check reaction to the create character message, if its a checkmark the creation is complete
            // If its a scroll, the player can change the attributes
            // If its a writing hand then the player can change the name
            var reaction = await interactivity.WaitForReactionAsync(message, ctx.User, TimeSpan.FromMinutes(15));
            if(reaction.Result == null)
            {
                await message.DeleteAsync();
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
                        message = await CreateCharacterPopUp(ctx, channel);
                        reaction = await interactivity.WaitForReactionAsync(message, ctx.User, TimeSpan.FromMinutes(15));
                        if (reaction.Result != null)
                        {
                            reactionStr = reaction.Result.Emoji.GetDiscordName();
                        }
                        continue;
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
            player.DiscordId = ctx.User.Id;

            bool succes = await PlayerQuerries.AddPlayer(player.Name, player.DiscordId);
            if (succes)
            {
                long id = await PlayerQuerries.GetPlayerId(player.DiscordId);
                succes = await PlayerQuerries.AddAttributes(id, player.attributes);
                if(succes) 
                {
                    await Task.Delay(1000);
                    await channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "Character created",
                        Color = DiscordColor.Green
                    });
                } else
                {
                    var errorEmbed = new DiscordEmbedBuilder
                    {
                        Title = "Something went wrong",
                        Description = "Please try again later!",
                        Color = DiscordColor.Red,
                    };
                    await channel.SendMessageAsync(embed: errorEmbed);
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
                $"\n:brain: Intelligence: {player.attributes.Intelligence}");
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
            String[] abilityEmojies = { ":muscle:", ":man_running:", ":heart:", ":brain:", ":white_check_mark:"};
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
                $"\n:brain: Intelligence: {player.attributes.Intelligence}");
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
                }
                embed.AddField("Attribute:", attributeText);
                message = await channel.SendMessageAsync(embed: embed);
                if (attributeValue != 15 && attributePoints != 0)
                {
                    await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":arrow_up:"));
                }

                if (attributeValue != 8)
                {
                    await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":arrow_down:"));
                }
                await message.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":negative_squared_cross_mark:"));


                reaction = await interactivity.WaitForReactionAsync(message, ctx.User, TimeSpan.FromMinutes(15));
                if(reaction.Result == null || reaction.Result.Emoji.GetDiscordName() == ":negative_squared_cross_mark:")
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

                await message.DeleteAsync();
                message = await channel.SendMessageAsync(embed: embed);

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
            }
            await message.DeleteAsync();
        }
    }
}
