using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace TroyBot.Core.Commands
{
    class Admin : BaseCommandModule
    {
        [Command("purge")]
        public async Task Purge(CommandContext context, [Description("Number of messages to delete")]int numMsg)
        {

        }
    }
}
