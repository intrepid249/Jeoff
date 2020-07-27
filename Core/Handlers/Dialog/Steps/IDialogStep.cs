using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;

namespace TroyBot.Core.Handlers.Dialog.Steps
{
    public interface IDialogStep
    {
        Action<DiscordMessage> OnMessageAdded { get; set; }
        IDialogStep NextStep { get; }

        Task<bool> ProcessStep(DiscordClient client, DiscordChannel channel, DiscordUser user);
    }
}
