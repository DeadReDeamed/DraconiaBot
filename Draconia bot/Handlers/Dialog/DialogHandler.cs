using Draconia_bot.Handlers.Dialog.Steps;
using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Draconia_bot.Handlers.Dialog
{
    public class DialogHandler
    {
        private readonly DiscordClient _client;
        private readonly DiscordChannel _channel;
        private readonly DiscordUser _user;
        private IDialogueStep _currentStep;

        public DialogHandler(
            DiscordClient client, 
            DiscordChannel channel, 
            DiscordUser user, 
            IDialogueStep startingStep) 
        { 
            _client = client;
            _channel = channel;
            _user = user;
            _currentStep = startingStep;
        }

        private readonly List<DiscordMessage> messages = new List<DiscordMessage>();

        public async Task<bool> ProcessDialog()
        {
            while(_currentStep != null)
            {
                _currentStep.OnMessageAdded += (message) => messages.Add(message);

                bool cancled = await _currentStep.ProcessStep(_client, _channel, _user);

                if (cancled)
                {
                    await DeleteMessages();

                    var cancelEmbed = new DiscordEmbedBuilder
                    {
                        Title = "Dialogue has succesfully been cancled",
                        Description = _user.Mention,
                        Color = DiscordColor.Green
                    };

                    await _channel.SendMessageAsync(embed: cancelEmbed);
                }

                _currentStep = _currentStep.NextStep;
            }
            await DeleteMessages();
            return true;
        }

        private async Task DeleteMessages()
        {
            if (_channel.IsPrivate)
            {
                return;
            }

            foreach (var message in messages)
            {
                await message.DeleteAsync();
            }
        }
    }
}
