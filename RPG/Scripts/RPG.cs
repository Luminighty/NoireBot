using AnimatedGif;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NoireBot.Rpg
{

    public class RpgCommands : ModuleBase
    {
        [Command("rpg")]
        public async Task MainRpg([Remainder] string command = "")
		{
			IDisposable typing = Context.Channel.EnterTypingState();
			try
			{
				IUserMessage msg = await ReplyAsync("Processing...");
				if (command == "")
				{
					await Output(Program.rpg.Process(new string[] { }, Context.User), msg, Context);
					Program.rpg.EndProcess(Context.User);
					typing.Dispose();
					return;
				}

				string[] input = command.Split(' ');
				Result r = Program.rpg.Process(input, Context.User);
				await Output(r, msg, Context);
				if (r.StartEnemyTurn)
				{
					msg = await ReplyAsync("Processing Enemy turn...");
					r = Program.rpg.EnemyProcess(Context.User);
					await Output(r, msg, Context);
				}
				Program.rpg.EndProcess(Context.User);
				typing.Dispose();
			} catch(Exception exc)
			{
				await ReplyAsync("Error! Please report me!");
				typing.Dispose();
				Program.rpg.EndProcess(Context.User);
				await Program.Log(LogSeverity.Critical, "RPG", Context.Guild.Name + " - " + Context.Channel.Name);
				await Program.Log(LogSeverity.Critical, "RPG", "StackTrace: " + exc.StackTrace + "");
				await Program.Log(LogSeverity.Critical, "RPG", "TargetSite: " + exc.TargetSite + "");
				await Program.Log(LogSeverity.Critical, "RPG", "HelpLink: " + exc.HelpLink + "");
				await Program.Log(LogSeverity.Critical, "RPG", "Message: " + exc.Message + "");
				IDMChannel lumiChannel = await (await Context.Guild.GetUserAsync(Program.LumiID)).GetOrCreateDMChannelAsync();
				await lumiChannel.SendMessageAsync("**ERROR**: " + Context.Guild.Name + " - " + Context.Channel.Name);
				await lumiChannel.SendMessageAsync("```" + exc.StackTrace + "```");
				await lumiChannel.SendMessageAsync("```" + exc.TargetSite + "```");
				await lumiChannel.SendMessageAsync("```" + exc.HelpLink + "```");
				await lumiChannel.SendMessageAsync("```" + exc.Message + "```");
			}
        }

        public async Task Output(Result res, IUserMessage processMessage = null, ICommandContext c = null)
		{
			if (res == new Result() || res == null)
			{
				return;
			}
            IMessageChannel channel =  (res.isPrivate) ? await Context.User.GetOrCreateDMChannelAsync() : Context.Channel;

            if (File.Exists(res.Text))
			{
				if(channel == null)
					await Program.Log(LogSeverity.Critical, "RPG", "channel == null");
				await channel.SendFileAsync(res.Text);
				try
				{
					await processMessage.DeleteAsync(RequestOptions.Default);
				}
				catch (Exception exc)
				{
					await Program.Log(new LogMessage(LogSeverity.Info, "RPG", "Couldn't delete the Processing message!"));
				}
				await Task.Delay(500);
                File.Delete(res.Text);
                return;
			}

			if (res.Text.Length > 2000)
            {
                string[] lines = res.Text.Split('\n');
                string line = "";
                int currentLenght = 0;

                for (int i = 0; i < lines.Length; i++)
                {
                    currentLenght += lines.Length;
                    if (currentLenght > 2000)
                    {
                        await channel.SendMessageAsync(line);
                        line = "";
                        currentLenght = lines[i].Length;
                    }
                    line += lines[i];
				}
				await channel.SendMessageAsync(line);

			} else
			{
				await channel.SendMessageAsync(res.Text);
			}
			try
			{
				await processMessage.DeleteAsync(RequestOptions.Default);
			}
			catch (Exception exc)
			{
				await Program.Log(new LogMessage(LogSeverity.Info, "RPG", "Couldn't delete the Processing message!"));
			}

		}

        [Group("party")]
        [Alias("p")]
        public class PartyCommands : ModuleBase {

            [Command]
            public async Task MainParty()
            {
                await Context.Channel.SendMessageAsync(Program.rpg.GetParty(Context.User));
            }

            [Command("invite")]
            [Alias("i", "inv")]
            public async Task Invite(IUser user = null)
            {
                if (user == null)
                {
                    await Context.Channel.SendMessageAsync("Usage: `>rpg p inv @User`");
                    return;
                }
				if(user == Context.User)
				{

					await Context.Channel.SendMessageAsync("You can't invite yourself!");
					return;
				}
                Program.rpg.SendInvite(Context.User, user);
            }
            [Command("join")]
            [Alias("j")]
            public async Task Join()
            {
                if(!Program.rpg.HasInvite(Context.User))
                {
                    await Context.Channel.SendMessageAsync("You don't have an invite.");
                    return;
                }
                Program.rpg.Join(Context.User);

                await Context.Channel.SendMessageAsync("Joined.");
            }
            [Command("leave")]
            public async Task Leave()
            {
                Program.rpg.Leave(Context.User);
                await Context.Channel.SendMessageAsync("Party left.");
            }



        }

    }

    public class RPG
    {
        private static Dictionary<ulong, Group> groups = new Dictionary<ulong, Group>();
        private static string help;
        private Dictionary<ulong, Group> invitations = new Dictionary<ulong, Group>();
        public static Point[,] MonsterPositions;
		/// <summary>
		/// ../../RPG/Resources/
		/// </summary>
		public static readonly string path_Resources = "../../RPG/Resources/";
		
        public RPG()
        {
            LoadData();
        }
		
        public Result Process(string[] input, IUser user)
        {
            if (!groups.ContainsKey(user.Id))
            {
                groups.Add(user.Id, new Group(user));
            }
            Group current = groups[user.Id];
            Player player = current.players[user.Id];
            if (player.isProcessing)
            {
                return new Result("Please wait until the end of your last action.", false, false);
            }
            player.isProcessing = true;
            Result result = this.DefaultCommands(input, ref player);
            if (result.Text != "")
            {
                return result;
            }
            if (current.inFight)
            {
                return this.Process_Battle(input, user, ref current, ref player);
            }
            switch (current.placeType)
            {
                case Group.PlaceType.Dungeon:
                    return new Result("soon", false, false);

                case Group.PlaceType.Town:
                    return this.Process_Tavern(input, user, ref current, ref player);
            }
            return new Result("Idk what to do...", false, false);
        }


        #region process

        private Result Process_Battle(string[] input, IUser user, ref Group current, ref Player currentPlayer)
        {
            Skill skill;
            if (!current.inFight)
                return new Result();
            int num = 0;
            string str = "";
            for (int i = 0; i < current.players.Count; i++)
                if (current.players.Values.ElementAt<Player>(i).isReady)
                {
                    num++;
                    str = str + current.players.Values.ElementAt<Player>(i).nickName + ", ";
                }

            str = str.Remove(str.Length - 2);

            if (!current.HasEnemy)
                current.CreateGroup();

            int result = -1;
            if ((input.Length == 0) || (input[0] == ""))
            {
                using (AnimatedGifCreator creator = new AnimatedGifCreator(path_Resources + "currentBattle.gif", 500, 0))
                {
                    Bitmap img = Drawing.GetBattle(current, user, true, 0, true);
                    Bitmap bitmap2 = Drawing.GetBattle(current, user, true, 1, true);
                    creator.AddFrame(img, GifQuality.Bit8);
                    creator.AddFrame(bitmap2, GifQuality.Bit8);
                }
                return new Result(path_Resources + "currentBattle.gif", false, false);
            }
            string s = input[0].ToLower();
            switch (s)
            {
                case "attack":
                case "atk":
                case "a":
                    if (!currentPlayer.isReady)
                        return new Result("Please wait for the others' turn to end! (" + str + ")", false, false);
                    if (((input.Length < 2) || !int.TryParse(input[1], out result)) || (result < 0))
                        return new Result("Please select an enemy! Example: `>rpg attack 0`", false, false);
                    if (((current.monsters.Count <= result) || (result < -1)) || (current.monsters[result].stats.Hp <= 0))
                        return new Result("Please select an enemy that exists! Example: `>rpg attack 0`", false, false);
                    currentPlayer.isReady = false;
                    return new Result(this.Attack(ref current, ref currentPlayer, result, new Skill("attack", 10)), false, num < 2);

                case "skills":
                case "skill":
                case "skl":
                case "s":
                    if (!currentPlayer.isReady)
                        return new Result("Please wait for the others' turn to end! (" + str + ")", false, false);
                    if (((input.Length < 2) || (input[1] == "list")) || (input[1] == "l"))
                        return new Result(this.SkillList(ref currentPlayer), true, false);
                    if (!currentPlayer.HasSkill(input[1], out skill))
                        return new Result("You don't know that spell!", false, false);
                    if (currentPlayer.stats.Mp < skill.MpCost)
                        return new Result("Not enough Mana!", false, false);
					if (skill.useOn != UseOn.Enemy)
					{
						if(input.Length < 3)
							return new Result("Please select an teammate! Examples: `>rpg skill " + skill.Name + " 0` or `>rpg skill " + skill.Name + " " + current.players.Values.ElementAt(0).nickName + "`", false, false);

						string player = "";
						for (int i = 2; i < input.Length; i++)
							player += input[i] + " ";
						player = player.Remove(player.Length - 1);

						result = -1;
						for (int i = 0; i < current.players.Count; i++)
							if (current.players.Values.ElementAt(i).nickName.ToLower().Contains(player.ToLower()))
								result = i;
						if(result < 0)
							if (((input.Length < 3) || !int.TryParse(input[2], out result)) || (result < 0))
								return new Result("Please select an teammate! Examples: `>rpg skill " + skill.Name + " 0` or `>rpg skill " + skill.Name + " " + current.players.Values.ElementAt(0).nickName + "`", false, false);
						if(current.players.Values.ElementAt(result).stats.Hp <= 0)
						{
							if(skill.useOn == UseOn.Ally)
								return new Result("Please select an **alive** teammate! Examples: `>rpg skill " + skill.Name + " 0` or `>rpg skill " + skill.Name + " " + current.players.Values.ElementAt(0).nickName + "`", false, false);
						} else
						{

							if (skill.useOn == UseOn.DeadAlly)
								return new Result("Please select a **dead** teammate! Examples: `>rpg skill " + skill.Name + " 0` or `>rpg skill " + skill.Name + " " + current.players.Values.ElementAt(0).nickName + "`", false, false);
						}
					}
					else
					{
						if (((input.Length < 3) || !int.TryParse(input[2], out result)) || (result < 0))
							return new Result("Please select an enemy! Example: `>rpg skill " + skill.Name + " 0`", false, false);
					}
                    currentPlayer.isReady = false;

                    return new Result(this.Attack(ref current, ref currentPlayer, result, skill), false, num < 2);
                case "run":
                case "r":
                    return new Result("Not implemented yet.", false, false);
                default:
                    return new Result("I don't know this option! For help: `>rpg help`", false, false);
            }
        }

        private Result Process_Tavern(string[] input, IUser user, ref Group current, ref Player currentPlayer)
        {
            if ((input.Length == 0) || string.IsNullOrEmpty(input[0]))
            {
                return new Result("You're in **Deklamar**\n\n`>rpg shop` - Checkout the shop\n`>rpg training` - Fight against a group of monsters\n`>rpg class` - Change your class", false, false);
            }
            switch (input[0].ToLower())
            {
                case "shop":
                case "s":
                    return new Result("le shop", false, false);

                case "train":
                case "t":
                case "training":
                    current.CreateGroup();
                    return Process_Battle(new string[] {""}, user, ref current, ref currentPlayer);
				case "class":
				case "changeclass":
				case "c":
					if(input.Length < 1)
						return new Result("Currently you're a(n) **" + Class.database[currentPlayer.currentClass].Name + "**.\n To change it use: `>rpg class [class name]`");
					return new Result("Here, you'll be able to change your class!");
                default:
                    return new Result("I don't know this option!");
            }
        }

        private string Attack(ref Group group, ref Player player, int monster, Skill skill)
        {
			bool isAttackingMonster = (skill.useOn == UseOn.Enemy);

			AnimatedGifCreator creator = new AnimatedGifCreator(path_Resources + player.user.Id.ToString() + "attack.gif", 100, 0);
            for (int i = 0; i < 2; i++)
            {
                for (int m = 0; m < 4; m++)
                {
                    creator.AddFrame(Drawing.GetBattle(group, player.user, true, i % 2, false), GifQuality.Bit8);
                }
            }
            Random random = new Random();
			/*
			Equipment w1 = (player.equipment[2] == -1) ? null : Item.database[player.equipment[2]] as Equipment;
			Equipment w2 = (player.equipment[3] == -1) ? null : Item.database[player.equipment[3]] as Equipment;
			int weaponDmg = (w1.slot == Equipment.Slots.TwoHand) ? w1.stats.Str : w1.stats.Str + w2.stats.Str;
			*/
			int num = 0;
			if (isAttackingMonster)
			{
				int atk = (skill.Name.ToLower() != "attack") ? skill.value : 5;
				int Def = (skill.Name.ToLower() != "attack") ? group.monsters[monster].stats.Int : group.monsters[monster].stats.Str;
				num = (isAttackingMonster) ? random.Next(Math.Max(atk - (atk / 5), 0), atk + (atk / 5)) - (Def / 2) : atk;
				num = Math.Max(num, 1);
				int oldHp = group.monsters[monster].stats.Hp;
				group.monsters[monster].stats.Hp -= num;
				Program.Log(new LogMessage(LogSeverity.Debug, "RPG", group.monsters[monster].Name + " was damaged with " + num + " damage by " + player.nickName +". (New HP: " + group.monsters[monster].stats.Hp + " - Old HP: " + oldHp));

			} else
			{
				group.players.Values.ElementAt(monster).stats.Hp -= skill.value;
			}
			player.stats.Mp -= skill.MpCost;
			string[] files = Directory.GetFiles(path_Resources + "Skills/" + skill.Name.ToLower() + "/");
            for (int j = 0; j < files.Length; j++)
            {
                Bitmap image = Drawing.GetBattle(group, player.user, false, 0, true);
                Graphics source = Graphics.FromImage(image);
                for (int n = 0; n < group.monsters.Count; n++)
                {
                    if (n != monster || !isAttackingMonster)
                    {
                        if (group.monsters[n].stats.Hp > 0)
                        {
							Drawing.DrawMonster(ref source, group.monsters[n], n, "Idle", (j/4) % 2, false, 0f, 0f);
                        }
                    }
                    else
                    {

                        int num7 = Math.Min(j, 6);
                        if ((files.Length - j) < 4)
                        {
                            num7 = files.Length - j;
                        }
                        float offsetX = (((float)Math.Sin((double)(num7 * 90))) * num7) * 5f;
						Drawing.DrawMonster(ref source, group.monsters[n], n, "Hurt", 0, false, 0f, offsetX);

                        object[] objArray1 = new object[] { path_Resources, "Skills/", skill.Name.ToLower(), "/", j, ".png" };

                        source.DrawImage(System.Drawing.Image.FromFile(string.Concat(objArray1)), group.monsters[n].position);

                        PointF position = (PointF)group.monsters[monster].position;
						Drawing.DrawText(ref source, num.ToString(), position, Brushes.Red, 0x12, 0, Math.Max(j * -5, -30));
                    }
				}
				if(!isAttackingMonster)
					source.DrawImage(System.Drawing.Image.FromFile(path_Resources + "Skills/" + skill.Name.ToLower() + "/" + j + ".png"), new PointF(0, 0));

				creator.AddFrame(image, GifQuality.Bit8);
            }
            for (int k = 0; k < 2; k++)
            {
                for (int num10 = 0; num10 < 4; num10++)
                {
                    Bitmap bitmap2 = Drawing.GetBattle(group, player.user, true, k % 2, false);
                    Graphics graphics2 = Graphics.FromImage(bitmap2);
                    if (isAttackingMonster && k < 2)
                    {
						Drawing.DrawText(ref graphics2, num.ToString(), (PointF)group.monsters[monster].position, Brushes.Red, 18, 0, -30);
                    }
                    creator.AddFrame(bitmap2, GifQuality.Bit8);
                }
			}
			creator.AddFrame(new Bitmap(path_Resources + "Black.png"), GifQuality.Bit8);
			creator.AddFrame(new Bitmap(path_Resources + "Black.png"), GifQuality.Bit8);
			creator.Dispose();
            return (path_Resources + player.user.Id.ToString() + "attack.gif");
        }

        private Result DefaultCommands(string[] input, ref Player player)
        {
            if ((input.Length != 0) && (input[0] != ""))
            {
                switch (input[0].ToLower())
                {
                    case "nick":
                    case "nickname":
                        if (input.Length < 2)
                        {
                            return new Result("Sets your nickname (Max 8 letters!). Example: `>rpg nick Noirebot`", false, false);
                        }
                        if (input[1].Length > 8)
                        {
                            return new Result("You can use at most 8 letters!", false, false);
                        }
                        player.nickName = input[1];
                        return new Result("Nickname set to " + input[1] + "!", false, false);
                    case "inventory":
                    case "items":
                    case "item":
                        return new Result("soon");
                    case "help":
                    case "commands":
                    case "command":
                        return new Result(help, false, false);
                }
            }
            return new Result();
        }

        public void EndProcess(IUser user)
        {
            if (groups[user.Id].players.ContainsKey(user.Id))
            {
                groups[user.Id].players[user.Id].isProcessing = false;
            }
        }

        public Result EnemyProcess(IUser user)
        {
            if (!groups.ContainsKey(user.Id))
            {
                return new Result("This is probably an error!!! Please report me!!!", false, false);
            }
            Group group = groups[user.Id];
            if (!group.inFight)
            {
                return new Result();
            }
            Result result = new Result();
            if (!group.HasEnemy)
            {
                result = new Result(this.Rewards(ref group), false, false);
            }
            else
            {
                result = new Result(this.EnemyTurn(ref group, user), false, false);
            }
            for (int i = 0; i < group.players.Count; i++)
            {
                group.players.Values.ElementAt<Player>(i).isReady = true;
            }
            return result;
        }

        private string EnemyTurn(ref Group group, IUser user)
        {
            AnimatedGifCreator creator = new AnimatedGifCreator(path_Resources + group.id.ToString() + "enemy.gif", 100, 0);
            foreach (Monster monster in group.monsters)
            {
                if (monster.stats.Hp >= 1)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        for (int m = 0; m < 4; m++)
                        {
                            creator.AddFrame(Drawing.GetBattle(group, user, true, j % 2, false), GifQuality.Bit8);
                        }
                    }
                    for (int k = 0; k < 8; k++)
                    {
                        Bitmap image = Drawing.GetBattle(group, user, false, k % 2, false);
                        Graphics source = Graphics.FromImage(image);
                        for (int n = 0; n < group.monsters.Count; n++)
                        {
                            if (group.monsters[n].stats.Hp >= 1)
                            {
                                if (group.monsters[n] != monster)
                                {
									Drawing.DrawMonster(ref source, group.monsters[n], n, "Idle", (k / 4) % 2, false, 0f, 0f);
                                }
                                else
                                {
                                    float offsetY = Math.Min(k * 5, 10);
                                    if ((8 - k) < 3)
                                    {
                                        offsetY = (8 - k) * 5;
                                    }
									Drawing.DrawMonster(ref source, group.monsters[n], n, "Idle", 0, false, offsetY, 0f);
                                }
                            }
                        }
                        switch (k)
                        {
                            case 4:
                            case 5:
                                {
                                    int num7 = k - 4;
                                    source.DrawImage(System.Drawing.Image.FromFile(path_Resources + "HurtLayer" + num7.ToString() + ".png"), 0, 0);
                                    break;
                                }
                        }
                        creator.AddFrame(image, GifQuality.Bit8);
                    }
                    Random random = new Random();
                    int index = random.Next(0, group.players.Count);
                    group.players.Values.ElementAt<Player>(index).stats.Hp -= random.Next(Math.Max(monster.stats.Str - 2, 1), monster.stats.Str + 2);
                }
            }
            for (int i = 0; i < 4; i++)
            {
                for (int num9 = 0; num9 < 4; num9++)
                {
                    creator.AddFrame(Drawing.GetBattle(group, user, true, i % 2, false), GifQuality.Bit8);
                }
            }
            creator.Dispose();
            return (path_Resources + group.id.ToString() + "enemy.gif");
        }

		Skill enemyAttack(Monster monster, ref Group group, out int target)
		{
			target = -1;
			bool hasHealing = false;
			bool hasAoEHeal = false;
			List<Skill> healingSkills = new List<Skill>();
			List<Skill> attackSkills = new List<Skill>();
			foreach (int s in monster.skills)
				if (Skill.database[s].value < 0 && Skill.database[s].MpCost < monster.stats.Mp) {
					hasHealing = true;
					healingSkills.Add(new Skill(Skill.database[s]));
					if (Skill.database[s].isAoE)
						hasAoEHeal = true;
				} else
				{
					attackSkills.Add(new Skill(Skill.database[s]));
				}

			//Tries to heal someone if can
			if (hasHealing) {
				List<int> monstersNeedHealing = new List<int>();
				for (int i = 0; i < group.monsters.Count; i++)
					if (group.monsters[i].stats.Hp < Convert.ToInt32(Math.Round(group.monsters[i].stats.MaxHp * 0.6)))
						monstersNeedHealing.Add(i);

				if (monstersNeedHealing.Count > 0) {
					if (hasAoEHeal && monstersNeedHealing.Count > 2)
					{
						//Choose AoE heal
					}

					int minHp = monstersNeedHealing[0];
					for (int i = 1; i < monstersNeedHealing.Count; i++)
						if (group.monsters[monstersNeedHealing[i]].stats.Hp < group.monsters[monstersNeedHealing[minHp]].stats.Hp)
							minHp = i;

					Monster selected = group.monsters[monstersNeedHealing[minHp]];
					target = monstersNeedHealing[minHp];
					int selectedHeal = 0;
					int HealDifference = (selected.stats.MaxHp - selected.stats.Hp) - healingSkills[0].value;
					if(healingSkills[0].effect != null && healingSkills[0].effect.effect == Effect.Status.Poison && healingSkills[0].effect.EffectValue < 0)
						HealDifference -= (healingSkills[0].effect.EffectValue * healingSkills[0].effect.turn) / 5;

					for(int i = 1; i < healingSkills.Count; i++)
					{

						int newHealDifference = (selected.stats.MaxHp - selected.stats.Hp) - healingSkills[i].value;
						if (healingSkills[i].effect != null && healingSkills[i].effect.effect == Effect.Status.Poison && healingSkills[i].effect.EffectValue < 0)
							HealDifference -= (healingSkills[i].effect.EffectValue * healingSkills[i].effect.turn) / 5;
						if(Math.Abs(HealDifference) > Math.Abs(newHealDifference))
						{
							//if the difference is smaller, but won't overheal, check if it's more worth it not overhealing
							if (newHealDifference < 0)
								if (Math.Abs(newHealDifference) * 1.5f < Math.Abs(HealDifference))
									continue;
							HealDifference = newHealDifference;
							selectedHeal = i;
						}
					}
					return new Skill(healingSkills[selectedHeal]);
				}
			}



			return new Skill("attack", 10);
		}

        private string Rewards(ref Group group)
        {
            group.inFight = false;
            int num = 0;
            int num2 = 0;
            foreach (Monster monster in group.monsters)
            {
                num += monster.stats.xp;
                num2 += monster.stats.gold;
            }
            string str = "";
            for (int i = 0; i < group.players.Count; i++)
            {
                Player player = group.players[group.players.Keys.ElementAt<ulong>(i)];
                int level = player.stats.Level;
                player.stats.xp += num;
                player.stats.gold += num2;
                if (level != player.stats.Level)
                {
                    object[] objArray1 = new object[] { str, "\n**Level up!**\n", player.user.Username, " is lvl. ", player.stats.Level, "!" };
                    str = string.Concat(objArray1);
                }
            }
            string[] textArray1 = new string[] { "**Victory!**\n You've gained *", num.ToString(), "* experience points and found *", num2.ToString(), "* gold!", str };
            return string.Concat(textArray1);
        }

        #endregion
		
        #region misc

        public string GetParty(IUser user)
        {
            if (!groups.ContainsKey(user.Id))
            {
                return "You don't have a party!";
            }
            string str = "";
            Dictionary<ulong, Player> players = groups[user.Id].players;
            for (int i = 0; i < players.Values.Count; i++)
            {
                Player player = players.Values.ElementAt<Player>(i);
                object[] objArray1 = new object[] { str, player.nickName, " (Lvl. ", player.stats.Level, ")\n" };
                str = string.Concat(objArray1);
            }
            return str;
        }

        public bool HasEnemy(IUser user) =>
            groups[user.Id].HasEnemy;

        public bool HasInvite(IUser user) =>
            this.invitations.ContainsKey(user.Id);

        public void Join(IUser user)
        {
            Group group = this.invitations[user.Id];
            Player copy = groups.ContainsKey(user.Id) ? groups[user.Id].players[user.Id] : new Player(user);
			copy = new Player(copy);
			if (groups.ContainsKey(user.Id))
            {
                groups[user.Id].players.Remove(user.Id);
                groups.Remove(user.Id);
            }
            group.players.Add(user.Id, copy);
            groups.Add(user.Id, group);
			invitations.Remove(user.Id);
        }

        public void Leave(IUser user)
        {
            if (groups.ContainsKey(user.Id))
            {
                Player player = groups[user.Id].players[user.Id];
                groups[user.Id].players.Remove(user.Id);
                groups.Remove(user.Id);
                groups.Add(user.Id, new Group(user));
            }
        }

        public void SendInvite(IUser from, IUser to)
        {
            if (this.invitations.Keys.Contains<ulong>(to.Id))
            {
                this.invitations.Remove(to.Id);
            }
            if (!groups.ContainsKey(from.Id))
            {
                groups.Add(from.Id, new Group(from));
            }
            this.invitations.Add(to.Id, groups[from.Id]);
        }

        private string SkillList(ref Player player)
        {
            string str = "";
            foreach (int s in Class.database[player.currentClass].skills)
            {
                Skill skill = Skill.database[s];
				if (skill.level > player.classLevels[player.currentClass])
					continue;
				str = str + "**Name**: `" + skill.Name + "`\n";
                str += "**Cost**: `" + skill.MpCost + " Mp`\n";
                str += "**Damage**: `" + skill.value + "`\n";
                str += "**Description**: \n*" + skill.Desc + "*\n\n";
            }
            return str;
        }

        #endregion
        

        private void LoadData()
        {

            #region Group_MonsterPositions
            StreamReader reader = new StreamReader(path_Resources + "GroupPositions.txt");
            int num = int.Parse(reader.ReadLine());
            MonsterPositions = new Point[num, num];
            for (int i = 0; i < num; i++)
            {
                char[] separator = new char[] { ' ' };
                string[] strArray = reader.ReadLine().Split(separator);
                for (int j = 0; j < (strArray.Length / 2); j++)
                {
                    MonsterPositions[i, j] = new Point(int.Parse(strArray[j * 2]), int.Parse(strArray[(j * 2) + 1]));
                }
            }
            reader.Close();
            reader.Dispose();
            #endregion
            
            #region Guide
            reader = new StreamReader(path_Resources + "guide.txt");
            help = reader.ReadToEnd();
            reader.Close();
            reader.Dispose();
			#endregion

			Skill.LoadSkills(path_Resources + "Data/Skills.DAT");
			Class.LoadClasses(path_Resources + "Data/Classes.DAT");
			Item.LoadItems(path_Resources + "Data/Items.DAT");
			Monster.LoadMonsters(path_Resources + "Data/Monsters.DAT");
		}
	}

	public class Result
	{
		public bool isFile;
		public bool isPrivate;
		public bool StartEnemyTurn;
		public string Text;

		public Result()
		{
			this.Text = "";
			this.isFile = true;
			this.StartEnemyTurn = false;
			this.isPrivate = false;
			this.Text = "";
		}

		public Result(string _text, bool _isPrivate = false, bool _startEnemyTurn = false)
		{
			this.Text = "";
			this.isFile = true;
			this.StartEnemyTurn = false;
			this.isPrivate = false;
			this.Text = _text;
			this.isPrivate = _isPrivate;
			this.StartEnemyTurn = _startEnemyTurn;
		}
	}

}