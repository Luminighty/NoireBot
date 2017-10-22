using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.IO;

namespace NoireBot
{
	public class UtilityCommands : ModuleBase
	{

		[Command("prune")]
		public async Task Prune(int size = 0, IGuildUser user = null, [Remainder]string text = "")
		{
			if (!((IGuildUser)Context.User).GuildPermissions.Administrator)
				return;
			if (size < 1)
			{
				await ReplyAsync("Usage: `>prune [Message Count] {@User} {Message}` or `>prune [Message Count] {Message}`");
				return;
			}
			try
			{
				int count = 0;
				var messages = Context.Channel.GetMessagesAsync(1000);
				var list = await messages.ToList<IReadOnlyCollection<IMessage>>();
				List<IMessage> ids = new List<IMessage>(); ;
				foreach (IReadOnlyCollection<IMessage> msg in list)
				{
					foreach (IMessage imsg in msg.ToList<IMessage>())
					{
						if (count > size)
							break;
						if ((user == null || imsg.Author == user) && (text == "" || imsg.Content.Contains(text)))
						{
							ids.Add(imsg);
							count++;
						}

					}
				}
				await Context.Channel.DeleteMessagesAsync(ids.AsEnumerable<IMessage>());
				string s = (ids.Count > 1) ? "s" : "";

				var deletedMessage = await ReplyAsync("**" + (ids.Count - 1) + "** message" + s + " deleted! :wastebasket:");
				await Task.Delay(3000);
				await deletedMessage.DeleteAsync();
			}
			catch (Exception exc)
			{
				await Program.Log(new LogMessage(LogSeverity.Info, "Prune", "Couldn't delete the messages. (Exception: " + exc.StackTrace + "\n" + exc.TargetSite + "\n" + exc.HelpLink + "\n" + exc.Message +")"));
				await ReplyAsync("I don't have permission to delete the messages.");
			}
		}

		[Command("prune")]
		public async Task Prune(int size = 0, [Remainder]string text = "")
		{
			if (!((IGuildUser)Context.User).GuildPermissions.Administrator)
				return;
			await Prune(size, null, text);

		}

		[Command("kick")]
		public async Task Kick(IGuildUser user = null, [Remainder]string reason = "")
		{
			if(user == null)
			{
				await ReplyAsync("Usage: `>kick @User`");

			} else
			{
				await user.KickAsync(reason);
				await ReplyAsync(user.Nickname + " was kicked from the server.");
			}
		}
		
		[Command("ban")]
		public async Task Ban(IGuildUser user = null, [Remainder]string reason = "")
		{
			if (user == null)
			{
				await ReplyAsync("Usage: `>ban @User`");

			}
			else
			{
				await Context.Guild.AddBanAsync(user, 0, reason);
				await ReplyAsync(user.Nickname + " was banned from the server.");
			}
		}
	}
}
