using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using TroyBot.Core.Handlers.Dialog;
using TroyBot.Core.Handlers.Dialog.Steps;

namespace TroyBot.Core.Commands
{
    class AdminModule : BaseCommandModule
    {
        [Command("purge"), RequirePermissions(DSharpPlus.Permissions.ManageMessages)]
        [Description("Bulk delete a specified number of messages - can't be used to delete messages older than 1 week")]
        public async Task Purge(CommandContext context, [Description("Number of messages to delete (max 100)")]int numMsg = 5)
        {
            var interactivity = context.Client.GetInteractivity();

            var msgList = await context.Channel.GetMessagesAsync(numMsg + 1); //include the command message

            var confirmationMsg = await context.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Are you sure?",
                Description = $"Delete the last {numMsg} messages in {context.Channel.Mention}?",
                Color = DiscordColor.DarkRed,
                Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                {
                    Url = context.Client.CurrentUser.AvatarUrl
                }
            });

            var thumbsup = DiscordEmoji.FromName(context.Client, ":+1:");
            var thumbsdown = DiscordEmoji.FromName(context.Client, ":-1:");

            await confirmationMsg.CreateReactionAsync(thumbsup);
            await confirmationMsg.CreateReactionAsync(thumbsdown);

            var reactionResult = await interactivity.WaitForReactionAsync(x =>
                x.Message == confirmationMsg &&
                x.User == context.User &&
                (x.Emoji == thumbsup || x.Emoji == thumbsdown));

            if (reactionResult.Result.Emoji == thumbsup)
            {
                await confirmationMsg.DeleteAsync();

                await context.Channel.DeleteMessagesAsync(msgList);

                var response = await context.Channel.SendFileAsync("Assets/trashcan.png", embed: new DiscordEmbedBuilder
                {
                    Title = "Messages Deleted",
                    Description = $"Successfully deleted {(msgList.Count - 1)} messages from {context.Channel.Mention}",
                    Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
                    {
                        Url = "attachment://trashcan.png"
                    }
                }.Build());

                await Task.Delay(5000);
                await response.DeleteAsync();

            } 
            else if (reactionResult.Result.Emoji == thumbsdown)
            {
                await confirmationMsg.DeleteAsync();

                var response = await context.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Title = "Messages Deleted",
                    Description = $"No messages have been deleted",
                });

                await Task.Delay(5000);
                await response.DeleteAsync();
            }
            else
            {
                // Something went wrong
            }

            await context.Message.DeleteAsync();
        }


        [Command("Poll")]
        [Description("Create a poll for a topic")]
        public async Task Poll(CommandContext context,
            [Description("The title of the poll - must be one word")] string pollTitle,
            [Description("Number followed by s/m/h/d describing the duration of the poll")]TimeSpan duration, 
            [Description("List of emojis to describe the voting options")]params DiscordEmoji[] emojiOptions)
        {
            var interactivity = context.Client.GetInteractivity();
            string[] options = emojiOptions.Select(x => x.ToString()).ToArray();
            Dictionary<DiscordEmoji, string> descriptions = new Dictionary<DiscordEmoji, string>();

            // Prompt for descriptions for each option
            for (int i = 0; i < emojiOptions.Length; i++)
            {
                var descriptionPrompt = await context.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
                {
                    Title = "Option Description",
                    Description = $"Please type a description for the response {emojiOptions[i]}"
                });

                var reactionResult = await interactivity.WaitForMessageAsync(msg =>
                    msg.Author == context.User &&
                    msg.Channel == context.Channel);


                descriptions[emojiOptions[i]] = reactionResult.Result.Content;

                await Task.Delay(1000);
                await descriptionPrompt.DeleteAsync();
                await reactionResult.Result.DeleteAsync();
            }

            // Format a display string for the voting options
            string optionDisplay = string.Empty;
            for (int i = 0; i < emojiOptions.Length; i++)
            {
                optionDisplay += $"{options[i]}: {descriptions[emojiOptions[i]]}\n";
            }

            // Send the message that will collect the poll results
            var pollMsg = await context.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = pollTitle,
                Description = optionDisplay
            });
            foreach (var option in emojiOptions)
            {
                await pollMsg.CreateReactionAsync(option);
            }

            // Collect the results
            var result = await interactivity.CollectReactionsAsync(pollMsg, duration);
            var distinctResult = result.Distinct();

            var resultDisplay = distinctResult.Select(reaction => $"{reaction.Emoji}({descriptions[reaction.Emoji]}): {reaction.Total}");

            await context.Channel.SendMessageAsync(embed: new DiscordEmbedBuilder
            {
                Title = "Poll Results",
                Description = string.Join('\n', resultDisplay)
            });
        }



    }
}
