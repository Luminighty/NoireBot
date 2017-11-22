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
	public class Default : ModuleBase
	{

		ulong VofiServerID = 206475498153443329;

		Random rand = new Random();

		[Command("invite")]
		[Alias(new string[] {"inv" })]
		public async Task Invite()
		{
			await ReplyAsync("**Website: <https://noirebot.neocities.org> \nInvite me: <https://discordapp.com/oauth2/authorize?client_id=246933734010519552&scope=bot&permissions=15989775> \nFan Server: <https://discordapp.com/invite/CTsQfvq>**");
		}

		[Command("details")]
		[Alias(new string[] { "version", "v", "info", "about" })]
		public async Task Details()
		{
			ProfileCommands.SortProfiles();
			var em = new EmbedBuilder();
			em.Title = "NoireBot#8339";
			em.Description = "v. 0.8";
			em.AddInlineField("Servers", Program.client.Guilds.Count);
			em.AddInlineField("Users", ProfileCommands.profiles.Count);
			em.ThumbnailUrl = "https://noirebot.neocities.org/images/noire_3.jpg";
			em.AddField("Website", "https://noirebot.neocities.org");
			em.AddField("Invite", "https://noirebot.neocities.org/go.html?to=invite");
			em.AddField("Fan Server", "https://noirebot.neocities.org/go.html?to=fan");
			var footer = new EmbedFooterBuilder();
			footer.Text = "Created by Luminight#3109";
			em.Footer = footer;
			em.Color = new Color(136, 78, 252);
			await ReplyAsync("", false, em.Build());
			/*
			Program.LoadZippies();
			int points = 0;
			foreach (Profile prof in ProfileCommands.profiles)
				points += prof.credit;
			string zippyLine = "";
			string autism = "";
			string richest = "richest";
			if ((await Context.User.GetOrCreateDMChannelAsync()) != Context.Channel)
				if (Context.Guild.Id == VofiServerID)
				{
					zippyLine = Program.Zippies.Count + " Zippy quotes, ";
					autism = " autism";
					richest = "most autistic";
				}
			string text = "Noirebot is currently on version 1.3.3.8" + Environment.NewLine +
				"There are " + zippyLine + ProfileCommands.profiles.Count + " profiles and the " + richest + " has " + ProfileCommands.profiles[0].credit + autism + " point." + Environment.NewLine +
				"Altogether we have " + points + autism + " points.";
			await ReplyAsync(text);*/
		}

		[Command("help")]
		public async Task Help()
		{
			string plusText = "";
			if (Context.Guild.Id == VofiServerID)
				plusText = "\n**Zippy:**\n`>zippy` - Sends a * **>zippy * **message    {tts}\n            -Sends the message as a Text - To - Speech message.\n`> sendzippy [**Message**]` - Send a message to the '>zippy' command. \nNote: type '#&' for New Line! (Only admins can use this)`";
			await ReplyAsync(Program.help + plusText);
		}

		[Command("Noire")]
		public async Task Noire()
		{
			string[] files = Directory.GetFiles("../../noire/");
			int i = rand.Next(files.Length);
			await Context.Channel.SendFileAsync(files[i]);
		}

		[Command("Meme")]
		public async Task Meme()
		{
			string[] files = Directory.GetFiles("../../Meme/");

			int i = rand.Next(files.Length);
			int maxCount = 0;
			while (Program.lastMemes.Contains<int>(i) && maxCount < 1000)
			{
				maxCount++;
				i = rand.Next(files.Length);
			}

			await Context.Channel.SendFileAsync(files[i]);
		}
		
		[Command("Zippy")]
		public async Task Zippy([Remainder]string args = "")
		{
			if (Context.Guild.Id != VofiServerID && Context.User.Id != Program.LumiID && Context.Guild.Id != 319450270759124992)
				return;
			if (Context.User.IsBot)
				return;

			int lineCount = Program.Zippies.Count;
			int i = rand.Next(lineCount);
			bool alreadySent = false;
			bool skipRandomizer = false;
			bool isTTS = false;
			string[] splittedArgs = args.Split(' ');
			if (splittedArgs[0].ToLower() == "tts")
				isTTS = true;
			if (isTTS)
				args = args.Remove(0, Math.Min(4, args.Length));

			int x = 0;
			if (args != "")
			{
				if (Int32.TryParse(args, out x))
				{
					var user = await Context.Guild.GetUserAsync(Context.User.Id);
					if (user.GuildPermissions.Administrator)
					{
						i = Int32.Parse(args) - 1;
						i = i % lineCount;
						skipRandomizer = true;

					}
				}
				else if (args.ToLower() == "last")
				{
					var user = await Context.Guild.GetUserAsync(Context.User.Id);
					if (user.GuildPermissions.Administrator)
					{
						skipRandomizer = true;
						i = lineCount - 1;
					}
				}
				else if (args != "")
				{
					List<int> possibleZippies = new List<int>();
					for (int j = 0; j < Program.Zippies.Count; j++)
					{
						if (Program.Zippies[j].ToLower().Contains(args.ToLower()))
						{
							possibleZippies.Add(j);
							skipRandomizer = true;
						}
					}
					if(skipRandomizer)
						i = possibleZippies[rand.Next(possibleZippies.Count)];
				}
			}

			if (!skipRandomizer)
				do
				{
					alreadySent = false;
					i = rand.Next(lineCount);
					foreach (int k in Program.lastZippies)
						if (k == i)
							alreadySent = true;
				} while (alreadySent);

			int save = i;
			for (int j = 0; j < Program.lastZippies.Length; j++)
			{

				int save2 = Program.lastZippies[j];
				Program.lastZippies[j] = save;
				save = save2;
			}
			string text = "";
			text = Program.Zippies[i];
			text = text.Replace("#&", Environment.NewLine);
			text = text.Replace("_", " ");
			if (isTTS)
			{
				await Context.Channel.SendMessageAsync(text, true);
			}
			else
			{
				await ReplyAsync(text);
			}
		}

		[Command("sendZippy")]
		public async Task SendZippy([Remainder]string text)
		{
			if (Context.Guild.Id != VofiServerID && Context.User.Id != Program.LumiID && Context.Guild.Id != 319450270759124992)
				return;
			var user = await Context.Guild.GetUserAsync(Context.User.Id);
			if (!user.GuildPermissions.Administrator && Context.User.Id != Program.LumiID)
			{

				await ReplyAsync("Rip, You have no permission. ;-;");
			}
			else
			{
				StreamWriter writer = new StreamWriter("../../zippyInsults", true);
				writer.WriteLine(text);
				writer.Close();

				await Program.Log(new LogMessage(LogSeverity.Debug, "Zippy", "SendInsult used by " + Context.User.Username));
				await Program.Log(new LogMessage(LogSeverity.Debug, "Zippy", text));
				Program.LoadZippies();
				await ReplyAsync("Zippy Insult sent!");
			}
		}

		[Command("autism")]
		[Alias(new string[] { "aut", "ap", "autist" })]
		public async Task Autism(IGuildUser autist = null, [Remainder]string number = "")
		{
			var channel = await Context.User.GetOrCreateDMChannelAsync();
			if (Context.User.Id != Program.LumiID && Context.Channel == channel)
			{
				IGuildUser user = Context.User as IGuildUser;
				if (!user.GuildPermissions.Administrator)
					return;
			}
			try
			{
				await Context.Message.DeleteAsync();
			}
			catch (Exception exc)
			{
				await Program.Log(new LogMessage(LogSeverity.Debug, "aut", "The bot couldn't delete the message!"));
			}
			if (autist == null || number == "")
			{
				await channel.SendMessageAsync("Usage: >autism @User point");
				return;
			}

			int sender = ProfileCommands.CheckUser(Context.User);
			
			int pointUser = ProfileCommands.CheckUser(autist as IUser);

			try
			{
				int newPoint = Convert.ToInt32(number);
				ProfileCommands.profiles[pointUser].credit += newPoint;
				ProfileCommands.profiles[pointUser].WriteFile();
				if (newPoint > 0)
					await ReplyAsync(ProfileCommands.profiles[pointUser].Name + " is more autistic now. Huzzah!");
				if (newPoint < 0)
					await ReplyAsync(ProfileCommands.profiles[pointUser].Name + " is less autistic now... Somehow.");
				await Program.Log(new LogMessage(LogSeverity.Debug, "Autism", Context.User.Username + " used Autism."));

			}
			catch (Exception exc)
			{
				await channel.SendMessageAsync("Usage: >autism @User point");
				return;
			}


		}

		[Command("rep")]
		[Alias(new string[] { "repu", "r" })]
		public async Task Rep(IGuildUser user = null, [Remainder]string text = "")
		{
			if (user == null)
			{
				await ReplyAsync("Usage: > >rep @User { **+ / -** }");
				return;
			}
			if(user.Id == Context.User.Id)
			{
				await ReplyAsync("You can't give rep to yourself!");
				return;
			}

			int index = ProfileCommands.CheckUser(Context.User);
			Console.WriteLine(ProfileCommands.profiles[index].LastRep.AddDays(1).CompareTo(DateTime.UtcNow));
			Console.WriteLine(ProfileCommands.profiles[index].LastRep.ToShortDateString() + "; UTCNOW: " + DateTime.UtcNow.ToShortDateString());
			if (0 < ProfileCommands.profiles[index].LastRep.AddDays(1).CompareTo(DateTime.UtcNow))
			{
				TimeSpan time = ProfileCommands.profiles[index].LastRep.AddDays(1).Subtract(DateTime.UtcNow);
				string m = (time.Minutes > 1) ? "s" : "";
				string h = (time.Hours > 1) ? "s" : "";
				await ReplyAsync("**Hold on!** You can't give reputation for another **" + time.Hours + "** hour" + h + " and **" + time.Minutes + "** minute" + m + "!");
				return;
			}

			int Rep = ProfileCommands.CheckUser(user);

			if (text == "-")
			{
				ProfileCommands.profiles[Rep].reputation--;
				await ReplyAsync(":thumbsdown: | **" + Context.User.Username + "** taken **reputation point** from " + user.Mention + "!");
				ProfileCommands.profiles[Rep].WriteFile();
				ProfileCommands.profiles[index].LastRep = DateTime.UtcNow;
				return;
			}
			if (text == "+" || string.IsNullOrEmpty(text))
			{
				ProfileCommands.profiles[Rep].reputation++;
				await ReplyAsync(":thumbsup: | **" + Context.User.Username + "** given **reputation point** to " + user.Mention + "!");
				ProfileCommands.profiles[Rep].WriteFile();
				ProfileCommands.profiles[index].LastRep = DateTime.UtcNow;
				Console.WriteLine("NewRep: " + ProfileCommands.profiles[index].LastRep.ToShortDateString() + "; UTCNOW: " + DateTime.UtcNow.ToShortDateString());
				return;
			}

		}

		[Command("daily")]
		[Alias(new string[] { "dailies" })]
		public async Task DailyPoint(IGuildUser user = null)
		{

			Profile s = ProfileCommands.profiles[ProfileCommands.CheckUser(Context.User)];

			if (0 < s.dailyCd.AddDays(1).CompareTo(DateTime.UtcNow))
			{
				TimeSpan time = s.dailyCd.AddDays(1).Subtract(DateTime.UtcNow);
				string m = (time.Minutes > 1) ? "s" : "";
				string h = (time.Hours > 1) ? "s" : "";
				await ReplyAsync("**Hold on!** You can't get your dailies for another **" + time.Hours + "** hour" + h + " and **" + time.Minutes + "** minute" + m + "!");
				return;
			}
			Random rnd = new Random();
			int amount = (user == null || user.Id == Context.User.Id) ? 100 : rnd.Next(100, 200);
			Profile p = (user == null || user.Id == Context.User.Id) ? s : ProfileCommands.profiles[ProfileCommands.GetProfile(Context.User.Id)];
			p.credit += amount;
			s.dailyCd = DateTime.UtcNow;
			if (user != null && user.Id != Context.User.Id)
			{
				await ReplyAsync(":gift: | **" + Context.User.Username + "** given his **" + amount + "** of daily credits to " + user.Mention + "!");
			}
			else
			{
				await ReplyAsync(":moneybag: | You've got your **" + amount + "** of daily credits!");
			}
		}

		[Command("support")]
		[Alias(new string[] { "donate", "don", "sup" })]
		public async Task Donate([Remainder]string text = "")
		{
			string extratext = "";
			if (text == "")
				extratext = "If you want to leave a message aswell just use this form: `> donate [message]`";
			await ReplyAsync("Infinitely appreciated, but although NoireBot as a hobby project, and the creator is running it on it's own, currently there's no way to support me.\n Don't worry though, your kindness is saved.\n " + extratext);
			FileStream file = File.Open("../../donates.txt", FileMode.Append);
			StreamWriter writer = new StreamWriter(file);
			writer.WriteLine(Context.User.Id + " " + Context.User.Username + " - " + text);
			writer.Close();
			file.Close();

		}

		[Command("secretspam")]
		public async Task Spam([Remainder]string spamtext = "")
		{
			try
			{
				await Context.Message.DeleteAsync();
			}
			catch (Exception e)
			{
				await Program.Log(new LogMessage(LogSeverity.Info, "Spam", "Couldn't delete the message! Exception: " + e.Message));
			}
			if (Program.spamChannels.Contains(Context.Channel.Id))
			{
				Program.spamChannels.Remove(Context.Channel.Id);
				return;
			}
			spamtext = (string.IsNullOrEmpty(spamtext)) ? "spam" : spamtext;

			Program.spamChannels.Add(Context.Channel.Id);

			while (Program.spamChannels.Contains(Context.Channel.Id))
			{
				await Context.Channel.SendMessageAsync(spamtext);
				await Task.Delay(1000);
			}

		}

	}
}
