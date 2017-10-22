using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace NoireBot
{
	public class LogService
	{
		public static LogService service;
		private readonly DiscordSocketClient _discord;
		private readonly CommandService _commands;
		private readonly ILoggerFactory _loggerFactory;
		private readonly ILogger _discordLogger;
		private readonly ILogger _commandsLogger;
		private readonly ILogger _debugLogger;

		public LogService(DiscordSocketClient discord, CommandService commands, ILoggerFactory loggerFactory)
		{
			_discord = discord;
			_commands = commands;

			_loggerFactory = ConfigureLogging(loggerFactory);
			_discordLogger = _loggerFactory.CreateLogger("discord");
			_commandsLogger = _loggerFactory.CreateLogger("commands");
			_debugLogger = _loggerFactory.CreateLogger("debug");

			_discord.Log += LogDiscord;
			_commands.Log += LogCommand;
			service = this;
		}

		private ILoggerFactory ConfigureLogging(ILoggerFactory factory)
		{
			factory.AddConsole();
			return factory;
		}

		public static Task Log(LogMessage message)
		{

			service._debugLogger.Log(
				LogLevelFromSeverity(message.Severity),
				0,
				message,
				message.Exception,
				(_1, _2) => message.ToString(prependTimestamp: false));
			return Task.CompletedTask;
		}

		private Task LogDiscord(LogMessage message)
		{
			_discordLogger.Log(
				LogLevelFromSeverity(message.Severity),
				0,
				message,
				message.Exception,
				(_1, _2) => message.ToString(prependTimestamp: false));
			return Task.CompletedTask;
		}

		private Task LogCommand(LogMessage message)
		{
			_commandsLogger.Log(
				LogLevelFromSeverity(message.Severity),
				0,
				message,
				message.Exception,
				(_1, _2) => message.ToString(prependTimestamp: false));
			return Task.CompletedTask;
		}

		private static LogLevel LogLevelFromSeverity(LogSeverity severity)
			=> (LogLevel)(Math.Abs((int)severity - 5));

	}
}
