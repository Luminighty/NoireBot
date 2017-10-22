using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.IO;
using System.Drawing;

namespace NoireBot
{
	public class FunCommads : ModuleBase
	{
		[Command("Ping")]
		public async Task Ping()
		{
			await ReplyAsync("Pong.");
		}

		[Command("choose")]
		public async Task Choose([Remainder] string input = "")
		{
			string[] choose = input.Split('|');


			if (input == "" || choose.Length < 2)
			{
				await ReplyAsync("Usage: `>choose [Option 1] | [Option 2] |...`");
				return;
			}
			for (int i = 0; i < choose.Length; i++)
			{
				if (choose[i][0] == ' ')
					choose[i] = choose[i].Remove(0, 1);
				if (choose[i][choose[i].Length - 1] == ' ')
					choose[i] = choose[i].Remove(choose[i].Length - 1);
			}
			string[] replies = new string[] {
				"Definitely **#&**!", "I'd go with **#&**.",
				"Choose **#&**!", "**#&** sounds better.",
				"Obviously **#&**.", "What else than **#&**?", "**#&** is the choice.",
				"**#&**, and I'm pretty sure you would've choosen it too.", "**#&**, if you ask me.",
				"I think **#&** would be wiser."};

			Random rnd = new Random();
			string option = choose[rnd.Next(0, choose.Length)];
			string reply = replies[rnd.Next(0, replies.Length)];

			await ReplyAsync(reply.Replace("#&", option));

		}

		[Command("coinflip")]
		[Alias("coin")]
		public async Task CoinFlip()
		{
			Random rnd = new Random();
			string result = (rnd.Next(0, 2) == 0) ? "Tails" : "Head";
			await ReplyAsync(Context.User.Username + " flipped a coin and got **" + result + "**!");
		}

		[Command("roll")]
		public async Task Roll(string roll = "")
		{
			string Usage = "Usages: `>roll [Max]` or `>roll [Min]-[Max]`";
			string[] minMax = roll.Split('-');
			Random rnd = new Random();
			if (roll == "")
			{
				await ReplyAsync(Context.User.Username + " rolled and got **" + rnd.Next(1, 7) + "**!");
				return;
			}
			switch (minMax.Length)
			{
				case 1:
					if (Int32.TryParse(roll, out int max))
					{
						await ReplyAsync(Context.User.Username + " rolled and got **" + rnd.Next(1, max + 1) + "**!");
						return;
					}
					await ReplyAsync(Usage);
					return;
				case 2:
					if (Int32.TryParse(minMax[1], out max) && Int32.TryParse(minMax[0], out int min))
					{
						await ReplyAsync(Context.User.Username + " rolled and got **" + rnd.Next(min, max + 1) + "**!");
						return;
					}
					await ReplyAsync(Usage);
					return;
				default:
					await ReplyAsync(Usage);
					return;
			}

		}

		[Command("poke")]
		public async Task Poke(IUser user = null)
		{
			if (user == null)
			{
				await ReplyAsync("Usage: `>poke @User`");
				return;
			}

			await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync(Context.User.Username + " poked you in **" + Context.Channel.Name + "**! (" + Context.Guild.Name + ")");
			await ReplyAsync(Context.User.Username + " poked " + user.Mention + "!");
		}

		[Command("rockpaperscissors")]
		[Alias(new string[] { "rockpaperscissor", "rps", "ropasci" })]
		public async Task Ropasci(string input = "")
		{
			if (input == null)
			{
				await ReplyAsync("Usage: `>rps {**rock** | **paper** | **scissors**}`");
				return;
			}
			string win = "You win!";
			string tie = "Tie!";
			string lose = "You Lost!";

			Random rnd = new Random();

			int comp = rnd.Next(1, 4);



			switch (input.ToLower())
			{
				case "rock":
				case "r":
				case "1":
					if (comp == 1)
						await ReplyAsync(":fist: - :fist: " + tie);
					if (comp == 2)
						await ReplyAsync(":fist: - :raised_hand: " + lose);
					if (comp == 3)
						await ReplyAsync(":fist: - :v: " + win);
					return;
				case "paper":
				case "p":
				case "2":

					if (comp == 1)
						await ReplyAsync(":raised_hand: - :fist: " + win);
					if (comp == 2)
						await ReplyAsync(":raised_hand: - :raised_hand: " + tie);
					if (comp == 3)
						await ReplyAsync(":raised_hand: - :v: " + lose);
					return;
				case "scissors":
				case "scissor":
				case "sci":
				case "s":
				case "3":
					if (comp == 1)
						await ReplyAsync(":v: - :fist: " + lose);
					if (comp == 2)
						await ReplyAsync(":v: - :raised_hand: " + win);
					if (comp == 3)
						await ReplyAsync(":v: - :v: " + tie);
					return;
			}

		}

		[Command("colorpicker")]
		[Alias(new string[] { "color", "picker", "colorpick" })]
		public async Task ColorPick(string input = "")
		{
			if (!(input[0] == '#' && input.Length == 7))
			{
				await ReplyAsync("Usage: `>colorpicker {HEX CODE}`");
				return;
			}
			System.Drawing.Color c = System.Drawing.ColorTranslator.FromHtml(input);

			Bitmap pic = new Bitmap(50, 50);
			for (int i = 0; i < 50; i++)
				for (int j = 0; j < 50; j++)
					pic.SetPixel(i, j, c);

			pic.Save(Program.sourcePath + Context.User.Id + "Color.png");
			await Context.Channel.SendFileAsync(Program.sourcePath + Context.User.Id + "Color.png");
			await Task.Delay(100);
			File.Delete(Program.sourcePath + Context.User.Id + "Color.png");
		}

		[Command("ratewaifu")]
		[Alias("waifu")]
		public async Task rateWaifu([Remainder]string input = "")
		{
			if (string.IsNullOrWhiteSpace(input))
			{
				await ReplyAsync("Usage: `>ratewaifu {Character}`");
				return;
			}

			if (input.ToLower().Contains("noire"))
			{
				await Context.Channel.SendMessageAsync("I'd rate " + input + " a **100.00/100.00**");
				return;
			}
			Random r = new Random();
			int i = r.Next(0, 10000);
			double rate = Convert.ToDouble(i) / 100.00F;
			await Context.Channel.SendMessageAsync("I'd rate " + input + " a **" + rate.ToString() + "/100.00**");
		}

		[Command("Embed")]
		public async Task EmbedTest()
		{
			var em = new EmbedBuilder();
			EmbedAuthorBuilder author = new EmbedAuthorBuilder();
			author.Name = Context.User.Username + " #" + Context.User.Discriminator;
			author.IconUrl = Context.User.GetAvatarUrl();
			em.Author = author;
			em.Color = new Discord.Color(58, 61, 255);
			em.AddField("Credits", 5000);
			em.AddInlineField("Rep", 15069);
			em.ImageUrl = "https://noirebot.neocities.org/images/ShockedNoire.gif";
			em.ThumbnailUrl = "https://cdn.discordapp.com/attachments/206475498153443329/356395432751923200/Zoombear_gif.gif";
			em.Footer = new EmbedFooterBuilder();
			em.Footer.Text = "Make Zippy Great Again";
			em.Footer.IconUrl = "https://cdn.discordapp.com/avatars/133871176199045120/ead8ae0d00062b1fadfddc6d716bc7f9.jpg";
			em.Title = "Weeb Asian scrub";
			em.Description = "REEEEEEEEE";
			em.Author.Url = "http://tehurn.com/gayweed";
			em.Timestamp = new DateTimeOffset(2018, 3, 15, 16, 20, 00, TimeSpan.Zero);

			await Context.Channel.SendMessageAsync("", false, em.Build());

		}

		[Command("gn")]
		public async Task Gn()
		{
			string[] gns = new string[] { "Good night", "gn", "night", "Oyasumi", "Gn", "Sleep well"};
			Random r = new Random();
			await ReplyAsync(gns[r.Next(gns.Length - 1)]);
		}

		[Command("nick")]
		[Alias("nickname")]
		public async Task Nick(IGuildUser user, [Remainder] string newName = "")
		{
			Action<GuildUserProperties> a = new Action<GuildUserProperties>( (GuildUserProperties u) =>
			{
					u.Nickname = newName;
			});
			try
			{
				if((newName != "" && user.Nickname != newName) || (newName == "" && user.Nickname != user.Username))
					await user.ModifyAsync(a);
			} catch(Exception exc)
			{
				await Program.Log(LogSeverity.Info, "Nick", "Couldn't rename " + user.Username);
			}
		}

		[Command("nick")]
		[Alias("nickname")]
		public async Task Nick([Remainder]string input = "")
		{

			if (string.IsNullOrEmpty(input))
			{
				await ReplyAsync("Usage: `>nick {UserName}` or `>nick @User {UserName}`");
				return;
			}
			var u = await Context.Guild.GetUsersAsync();
			if (input == "reset")
			{
				for (int i = 0; i < u.Count; i++)
					await Nick(u.ElementAt<IGuildUser>(i), "");
				await ReplyAsync("Nickname reset finished!");
				return;
			}
			for (int i = 0; i < u.Count; i++)
				await Nick(u.ElementAt<IGuildUser>(i), input);
			await ReplyAsync("Nickname set to `" + input + "` to everyone!");
		}


		[Command("zippyemote")]
		[Alias("zippyreaction", "zippyreact")]
		public async Task ZippyEmote()
		{
			Program.ZippyMessageReactions = !Program.ZippyMessageReactions;
		}

	}
}
