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


			if(input == "" || choose.Length < 2)
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
				"Definietly **#&**!", "I'd go with **#&**.",
				"Choose **#&**!", "**#&** sounds better.",
				"Obviously **#&**.", "What else than **#&**?", "**#&** is the choice.",
				"**#&**, and I'm pretty sure you would've choosen it too.", "**#&**, if you ask me.",
				"I think #& would be wiser."};

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
			switch(minMax.Length)
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
			if(user == null)
			{
				await ReplyAsync("Usage: `>poke @User`");
				return;
			}

			await (await user.CreateDMChannelAsync()).SendMessageAsync(Context.User.Username + " poked you in **" + Context.Channel.Name + "**! (" + Context.Guild.Name + ")");
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



			switch(input.ToLower())
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
	}
}
