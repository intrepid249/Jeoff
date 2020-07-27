using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;

namespace TroyBot.Core.Handlers.Dialog.Steps
{
    public abstract class BaseDialogStep : IDialogStep
    {
        protected readonly string Content;

        public BaseDialogStep(string content)
        {
            Content = content;
        }

        public Action<DiscordMessage> OnMessageAdded { get; set; } = delegate { };

        public abstract IDialogStep NextStep { get; }

        public abstract Task<bool> ProcessStep(DiscordClient client, DiscordChannel channel, DiscordUser user);

        protected async Task TryAgain(DiscordChannel channel, string problem)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = "Please Try Again",
                Color = DiscordColor.Red
            };
            embed.AddField("There was a problem with your previous input", problem);

            var msg = await channel.SendMessageAsync(embed: embed);
            OnMessageAdded(msg);
        }
    }
}
