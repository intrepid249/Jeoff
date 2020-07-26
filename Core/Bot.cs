using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TroyBot.Core
{
    class Bot
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }

        public async Task RunAsync()
        {
            var json = string.Empty;

            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);

            var configJson = JsonConvert.DeserializeObject<ConfigJson>(json);

            var config = new DiscordConfiguration
            {
                Token = configJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true
            };

            Client = new DiscordClient(config);

            // Setup event handling
            Client.Ready += OnClientReady;

            // Setup Interactivity
            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromSeconds(40)
            });

            var commandsConfig = new CommandsNextConfiguration
            {
                CaseSensitive = false,
                EnableDms = false,
                EnableDefaultHelp = true,
                EnableMentionPrefix = true
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            // Register Commands classes

        }

        public async Task OnClientReady(ReadyEventArgs args)
        {
            await Client.UpdateStatusAsync(new DiscordActivity
            {
                ActivityType = ActivityType.Watching,
                Name = "everything you do"
            });
        }
    }
}
