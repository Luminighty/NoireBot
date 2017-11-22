using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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


        [Command("reminder")]
        [Alias("remindme")]
        public async Task Reminder([Remainder]string reminder = "")
        {
            if(string.IsNullOrEmpty(reminder))
            {
                await ReplyAsync("Usage: `>remindme {Reminder} in {D} days {H} hours {M} Minutes {S} Seconds`.\nExample:`>remindme to watch anime in 1 day`");
                return;
            }
            string[] input = reminder.Split(' ');
            string text = "";
            DateTime time = DateTime.UtcNow;
            bool hasIn = false;
            for (int i = 0; i < input.Length; i++)
            {
                if(!hasIn && input[i].ToLower() != "in")
                {
                    text += " " + input[i];
                } else
                {
                    if(!hasIn)
                    {
                        hasIn = true;
                        continue;
                    }
                    switch(input[i].ToLower())
                    {
                        case "days":
                        case "day":
                        case "d":
                            if (Int32.TryParse(input[i - 1], out int d))
                                time = time.AddDays(d);
                            break;
                        case "hours":
                        case "hour":
                        case "h":
                            if (Int32.TryParse(input[i - 1], out int h))
                                time = time.AddHours(h);
                            break;
                        case "minutes":
                        case "minute":
                        case "min":
                        case "m":
                            if (Int32.TryParse(input[i - 1], out int m))
                                time = time.AddMinutes(m);
                            break;
                        case "seconds":
                        case "second":
                        case "sec":
                        case "s":
                            if (Int32.TryParse(input[i - 1], out int s))
                                time = time.AddSeconds(s);
                            break;
                        default:
                            break;
                    }
                }
            }
            if(!hasIn)
            { 
                await ReplyAsync("Usage: `>remindme {Reminder} in {D} days {H} hours {M} Minutes {S} Seconds`.\nExample:`>remindme to watch anime in 1 day`");
                return;
            }

            Remind remindme = new Remind(text, time, Context.User.Id, Context.Message.Id);
            Program.reminders.Add(remindme);
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream file = File.Create(Program.sourcePath + "reminders/" + Context.Message.Id + ".rem");
            formatter.Serialize(file, remindme);
            file.Close();
            await ReplyAsync("**Got it**! I'll make sure to remind you as soon as possible when the time comes!");
        }
        
		[Command("save")]
		public async Task Save()
		{
			if (Context.User.Id != Program.LumiID)
				return;
			await Program.Log(LogSeverity.Info, "Save", "Saving on call!");
			Program.SaveData(true);
		}

        [System.Serializable]
        public class Remind
        {
            public string text;
            public ulong user;
            public DateTime time;
            public ulong id;

            public Remind(string _text, DateTime _time, ulong _user, ulong _id)
            {
                text = _text;
                time = _time;
                user = _user;
                id = _id;
            }
        }

    }
}
