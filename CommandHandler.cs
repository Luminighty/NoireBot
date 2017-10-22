using System.Threading.Tasks;
using System;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Audio;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace NoireBot
{
    public class CommandHandler
    {
		

		private readonly CommandService commands;
        private readonly DiscordSocketClient client;
        private IServiceProvider provider;

		public CommandHandler(IServiceProvider _provider, DiscordSocketClient _discord, CommandService _commands)
		{
			commands = _commands;
			provider = _provider;
			client = _discord;

			client.MessageReceived += HandleCommand;

		}

        public async Task Install(IServiceProvider _provider)
        {
			provider = _provider;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task HandleCommand(SocketMessage rawMessage)
		{
			// Ignore system messages and messages from bots
			if (!(rawMessage is SocketUserMessage message)) return;
			if (message.Source != MessageSource.User) return;

			int argPos = 0;
			if (!message.HasMentionPrefix(client.CurrentUser, ref argPos) && !message.HasCharPrefix('>', ref argPos)) return;

			var context = new SocketCommandContext(client, message);
			var result = await commands.ExecuteAsync(context, argPos, provider, MultiMatchHandling.Best);
			
		}
    }
}