using DSharpPlus;
using DSharpPlus.Entities;
using TroyBot.Core.Handlers.Dialog.Steps;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TroyBot.Core.Handlers.Dialog
{
    class DialogHandler
    {
        private readonly DiscordClient Client;
        private readonly DiscordChannel Channel;
        private readonly DiscordUser User;
        private IDialogStep CurrentStep;

        public DialogHandler(DiscordClient client, DiscordChannel channel, DiscordUser user, IDialogStep startingStep)
        {
            Client = client;
            Channel = channel;
            User = user;
            CurrentStep = startingStep;
        }

        private readonly List<DiscordMessage> messages = new List<DiscordMessage>();

        public async Task<bool> ProcessDialog()
        {
            while (CurrentStep != null)
            {
                CurrentStep.OnMessageAdded += (message) => messages.Add(message);
                bool cancelled = await CurrentStep.ProcessStep(Client, Channel, User);

                if (cancelled)
                {
                    await DeleteMessages();

                    await Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                    {
                        Title = "Cancelled",
                        Description = $"{User.Mention} The Dialog has been cancelled",
                        Color = DiscordColor.Chartreuse
                    });
                    return false;
                }

                CurrentStep = CurrentStep.NextStep;
            }

            await DeleteMessages();
            return true;
        }

        private async Task DeleteMessages()
        {
            if (Channel.IsPrivate) return;

            foreach (var message in messages) await message.DeleteAsync();
        }
    }
}
