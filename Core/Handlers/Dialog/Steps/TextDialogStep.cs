using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TroyBot.Core.Handlers.Dialog.Steps
{
    class TextDialogStep : BaseDialogStep
    {
        private readonly int? _MinLength;
        private readonly int? _MaxLength;

        private IDialogStep _NextStep;

        public TextDialogStep(string content, IDialogStep nextStep, int? minLength = null, int? maxLength = null) : base(content)
        {
            _NextStep = nextStep;
            _MinLength = minLength;
            _MaxLength = maxLength;
        }

        public Action<string> OnValidResult { get; set; } = delegate { };

        public override IDialogStep NextStep => _NextStep;

        public void SetNextStep(IDialogStep step)
        {
            _NextStep = step;
        }

        public override async Task<bool> ProcessStep(DiscordClient client, DiscordChannel channel, DiscordUser user)
        {
            var embed = new DiscordEmbedBuilder
            {
                Title = $"Please Respond Below",
                Description = $"{user.Mention}\n{Content}"
            };
            embed.AddField("To stop the dialog", $"use the `cancel` command");

            if (_MinLength.HasValue)
            {
                embed.AddField("Minimum Length:", $"{_MinLength.Value} characters");
            }

            if (_MaxLength.HasValue)
            {
                embed.AddField("Maximum Length:", $"{_MaxLength.Value} characters");
            }

            var interactivity = client.GetInteractivity();
            while (true)
            {
                var message = await channel.SendMessageAsync(embed: embed);
                OnMessageAdded(message);

                var messageResult = await interactivity.WaitForMessageAsync(msg =>
                    msg.ChannelId == channel.Id && msg.Author.Id == user.Id);

                if (messageResult.Result.Content.Contains("cancel", StringComparison.OrdinalIgnoreCase)) return true;

                if (_MinLength.HasValue)
                {
                    if (messageResult.Result.Content.Length < _MinLength.Value)
                    {
                        await TryAgain(channel, $"Your input is {_MinLength.Value - messageResult.Result.Content.Length} characters too short");
                        continue;
                    }
                }

                if (_MaxLength.HasValue)
                {
                    if (messageResult.Result.Content.Length > _MaxLength.Value)
                    {
                        await TryAgain(channel, $"Your input is {messageResult.Result.Content.Length - _MaxLength.Value} characters too long");
                        continue;
                    }
                }
            }
        }
    }
}
