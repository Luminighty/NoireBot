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

namespace NoireBot
{

    public class RpgCommands : ModuleBase
    {
        [Command("rpg")]
        public async Task MainRpg([Remainder] string command = "")
        {
            string[] input = command.Split(' ');
            Result r = Program.rpg.Process(input, Context.User);
            await Output(r);
            if (r.StartEnemyTurn)
            {
                r = Program.rpg.EnemyProcess(Context.User);
                await Output(r);
            }
            Program.rpg.EndProcess(Context.User);
        }

        public async Task Output(Result res)
        {
            if (res == new Result())
                return;
            IMessageChannel channel =  (res.isPrivate) ? await Context.User.CreateDMChannelAsync() : Context.Channel;

            if (!File.Exists(res.Text))
            {
                await channel.SendFileAsync(res.Text);
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
                        currentLenght = lines.Length;
                    }
                    line += lines[i];
                }
            } else
            {
                await channel.SendMessageAsync(res.Text);
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
                Program.rpg.SendInvite(Context.User, user);
            }
            [Command("join")]
            [Alias("j")]
            public async Task Join()
            {
                if(!Program.rpg.hasInvite(Context.User))
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
        private static Point[,] MonsterPositions;
        private static string path_Resources = "../../RPG/Resources/";

        public RPG()
        {
            this.LoadData();
        }

        private string Attack(ref Group group, ref Player player, int monster, Skill skill)
        {
            player.stats.Mp -= skill.MpCost;
            AnimatedGifCreator creator = new AnimatedGifCreator(path_Resources + player.user.Id.ToString() + "attack.gif", 100, 0);
            for (int i = 0; i < 2; i++)
            {
                for (int m = 0; m < 4; m++)
                {
                    creator.AddFrame(this.GetBattle(group, player.user, true, i % 2, false), GifQuality.Bit8);
                }
            }
            Random random = new Random();
            int num = random.Next(Math.Max(skill.value - (skill.value / 5), 0), skill.value + (skill.value / 5)) - (group.monsters[monster].stats.Str / 2);
            num = Math.Max(num, 1);
            group.monsters[monster].stats.Hp -= num;
            string[] files = Directory.GetFiles(path_Resources + "Skills/" + skill.Name.ToLower() + "/");
            for (int j = 0; j < files.Length; j++)
            {
                Bitmap image = this.GetBattle(group, player.user, false, 0, true);
                Graphics source = Graphics.FromImage(image);
                for (int n = 0; n < group.monsters.Count; n++)
                {
                    if (n != monster)
                    {
                        if (group.monsters[n].stats.Hp > 0)
                        {
                            this.DrawMonster(ref source, group.monsters[n], n, "Idle", 1, false, 0f, 0f);
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
                        this.DrawMonster(ref source, group.monsters[n], n, "Hurt", 0, false, 0f, offsetX);
                        object[] objArray1 = new object[] { path_Resources, "Skills/", skill.Name.ToLower(), "/", j, ".png" };
                        source.DrawImage(System.Drawing.Image.FromFile(string.Concat(objArray1)), group.monsters[n].position);
                        PointF position = (PointF)group.monsters[monster].position;
                        this.DrawText(ref source, num.ToString(), position, Brushes.Red, 0x12, 0, Math.Max(j * -5, -30));
                    }
                }
                creator.AddFrame(image, GifQuality.Bit8);
            }
            for (int k = 0; k < 2; k++)
            {
                for (int num10 = 0; num10 < 4; num10++)
                {
                    Bitmap bitmap2 = this.GetBattle(group, player.user, true, k % 2, false);
                    Graphics graphics2 = Graphics.FromImage(bitmap2);
                    if (k < 2)
                    {
                        this.DrawText(ref graphics2, num.ToString(), (PointF)group.monsters[monster].position, Brushes.Red, 0x12, 0, -30);
                    }
                    creator.AddFrame(bitmap2, GifQuality.Bit8);
                }
            }
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

                    case "help":
                    case "commands":
                    case "command":
                        return new Result(help, false, false);
                }
            }
            return new Result();
        }

        private void DrawColumn(ref Graphics source, Pen color, int Size, PointF position)
        {
            PointF tf = position;
            tf.Y += Size;
            source.DrawLine(color, position, tf);
        }

        private void DrawMonster(ref Graphics source, Monster monster, int id, string anim = "Idle", int frame = 0, bool drawId = true, float OffsetY = 0f, float OffsetX = 0f)
        {
            PointF point = new PointF(monster.position.X + OffsetX, monster.position.Y + OffsetY);
            anim = (frame < 10) ? (anim + "0" + frame.ToString()) : (anim + frame.ToString());
            anim = anim + ".png";
            string[] textArray1 = new string[] { path_Resources, "Monsters/", monster.Name, "/", anim };
            source.DrawImage(System.Drawing.Image.FromFile(string.Concat(textArray1)), point);
            if (drawId)
            {
                this.DrawText(ref source, id.ToString(), point, Brushes.White, 0x12, 0, -10);
            }
        }

        private void DrawPlayer(ref Graphics source, ref Player player, Point position)
        {
            Font font = new Font("Consolas", 18f);
            source.DrawString(player.user.Username, font, Brushes.White, new PointF(165f, 336f));
            int num = Convert.ToInt32((double)(100.0 * (((double)player.stats.Hp) / ((double)player.stats.MaxHp))));
            int num2 = Convert.ToInt32((double)(100.0 * (((double)player.stats.Mp) / ((double)player.stats.MaxMp))));
            for (int i = 0; i < num; i++)
            {
                System.Drawing.Color color = System.Drawing.Color.FromArgb(0x1d, 0xf3, 0);
                PointF tf = new PointF((float)(0x11f + i), (float)(position.Y + 4));
                this.DrawColumn(ref source, new Pen(color), 12, tf);
            }
            for (int j = 0; j < num2; j++)
            {
                System.Drawing.Color color2 = System.Drawing.Color.FromArgb(0, 210, 0xff);
                PointF tf2 = new PointF((float)(0x18d + j), (float)(position.Y + 4));
                this.DrawColumn(ref source, new Pen(color2), 12, tf2);
            }
            source.DrawImage(System.Drawing.Image.FromFile(path_Resources + "Bars.png"), new PointF(282f, (float)position.Y));
            source.DrawString(player.nickName, font, Brushes.Black, new PointF((float)(position.X + 3), (float)(position.Y + 3)));
            source.DrawString(player.nickName, font, Brushes.White, (PointF)position);
            Font font2 = new Font("Consolas", 16f, FontStyle.Bold);
            GraphicsPath path = new GraphicsPath();
            path.AddString(player.stats.Hp.ToString(), font.FontFamily, 1, 14f, new PointF(287f, (float)(position.Y + 2)), StringFormat.GenericDefault);
            source.DrawPath(new Pen(Brushes.Black, 3f), path);
            source.FillPath(Brushes.White, path);
            GraphicsPath path2 = new GraphicsPath();
            path2.AddString(player.stats.Mp.ToString(), font.FontFamily, 1, 14f, new PointF(397f, (float)(position.Y + 2)), StringFormat.GenericDefault);
            source.DrawPath(new Pen(Brushes.Black, 3f), path2);
            source.FillPath(Brushes.White, path2);
        }

        private void DrawText(ref Graphics source, string Text, PointF pos, Brush color, int size = 5, int OffsetX = 0, int OffsetY = 0)
        {
            GraphicsPath path = new GraphicsPath();
            Point origin = new Point(Convert.ToInt32(pos.X), Convert.ToInt32(pos.Y));
            origin.Y += OffsetY;
            origin.X += OffsetX + 40;
            path.AddString(Text, FontFamily.GenericSansSerif, 1, (float)size, origin, new StringFormat());
            Pen pen = new Pen(Brushes.Black, (float)(size / 6));
            source.SmoothingMode = SmoothingMode.HighQuality;
            source.DrawPath(pen, path);
            source.FillPath(color, path);
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
            group.CreateGroup();
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
                            creator.AddFrame(this.GetBattle(group, user, true, j % 2, false), GifQuality.Bit8);
                        }
                    }
                    for (int k = 0; k < 8; k++)
                    {
                        Bitmap image = this.GetBattle(group, user, false, k % 2, false);
                        Graphics source = Graphics.FromImage(image);
                        for (int n = 0; n < group.monsters.Count; n++)
                        {
                            if (group.monsters[n].stats.Hp >= 1)
                            {
                                if (group.monsters[n] != monster)
                                {
                                    this.DrawMonster(ref source, group.monsters[n], n, "Idle", (k / 4) % 2, false, 0f, 0f);
                                }
                                else
                                {
                                    float offsetY = Math.Min(k * 5, 10);
                                    if ((8 - k) < 3)
                                    {
                                        offsetY = (8 - k) * 5;
                                    }
                                    this.DrawMonster(ref source, group.monsters[n], n, "Idle", 0, false, offsetY, 0f);
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
                    creator.AddFrame(this.GetBattle(group, user, true, i % 2, false), GifQuality.Bit8);
                }
            }
            creator.Dispose();
            return (path_Resources + group.id.ToString() + "enemy.gif");
        }

        private Bitmap GetBattle(Group group, IUser user, bool drawMonsters = true, int frame = 0, bool drawIDs = true)
        {
            System.Drawing.Image image;
            Bitmap bitmap = new Bitmap(path_Resources + "Background.png");
            Graphics source = Graphics.FromImage(bitmap);
            Font font = new Font("Consolas", 18f);
            Player player = group.players[user.Id];
            if (drawMonsters)
            {
                for (int j = 0; j < group.monsters.Count; j++)
                {
                    if (group.monsters[j].stats.Hp > 0)
                    {
                        this.DrawMonster(ref source, group.monsters[j], j, "Idle", frame, drawIDs, 0f, 0f);
                    }
                }
            }
            source.DrawImage(System.Drawing.Image.FromFile(path_Resources + "Layout.png"), new PointF(0f, 0f));
            using (WebClient client = new WebClient())
            {
                image = System.Drawing.Image.FromStream(new MemoryStream(client.DownloadData(new Uri(user.GetAvatarUrl(ImageFormat.Png, 0x80)))));
            }
            source.DrawImage(image, new Rectangle(10, 0xd5, 0x52, 0x52));
            for (int i = 0; i < group.players.Count; i++)
            {
                Player player2 = group.players[group.players.Keys.ElementAt<ulong>(i)];
                this.DrawPlayer(ref source, ref player2, new Point(0xbb, 0xd0 + (i * 0x16)));
            }
            return bitmap;
        }

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

        public bool hasEnemy(IUser user) =>
            groups[user.Id].HasEnemy;

        public bool hasInvite(IUser user) =>
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

        private void LoadData()
        {
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
            reader = new StreamReader(path_Resources + "guide.txt");
            help = reader.ReadToEnd();
            Program.Log(new LogMessage(LogSeverity.Debug, "RPG", help, null));
            reader.Close();
            reader.Dispose();
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
            if (result != new Result())
            {
                return result;
            }
            if (current.inFight)
            {
                return this.process_Battle(input, user, ref current, ref player);
            }
            switch (current.placeType)
            {
                case Group.PlaceType.Dungeon:
                    return new Result("soon", false, false);

                case Group.PlaceType.Town:
                    return this.process_Tavern(input, user, ref current, ref player);
            }
            return new Result("Idk what to do...", false, false);
        }

        private Result process_Battle(string[] input, IUser user, ref Group current, ref Player currentPlayer)
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
                    Bitmap img = this.GetBattle(current, user, true, 0, true);
                    Bitmap bitmap2 = this.GetBattle(current, user, true, 1, true);
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
                    return new Result(this.Attack(ref current, ref currentPlayer, result, new Skill("Attack", 10)), true, num < 2);

                case "skills":
                case "skill":
                case "skl":
                case "s":
                    if (!currentPlayer.isReady)
                        return new Result("Please wait for the others' turn to end! (" + str + ")", false, false);
                    if (((input.Length < 2) || (input[1] == "list")) || (input[1] == "l"))
                        return new Result(this.SkillList(ref currentPlayer), false, false);
                    if (!currentPlayer.HasSkill(input[1], out skill))
                        return new Result("You don't have that spell!", false, false);
                    if (currentPlayer.stats.Mp < skill.MpCost)
                        return new Result("Not enough Mana!", false, false);
                    if (((input.Length < 3) || !int.TryParse(input[2], out result)) || (result < 0))
                        return new Result("Please select an enemy! Example: `>rpg skill " + skill.Name + " 0`", false, false);
                    currentPlayer.isReady = false;

                    return new Result(this.Attack(ref current, ref currentPlayer, result, skill), true, num < 2);
                case "run":
                case "r":
                    return new Result("Not implemented yet.", false, false);
                default:
                    return new Result("I don't know this option! For help: `>rpg help`", false, false);
            }
        }

        private Result process_Tavern(string[] input, IUser user, ref Group current, ref Player currentPlayer)
        {
            if ((input.Length == 0) || (input[0] == ""))
            {
                return new Result("You're in **Deklamar**\n\n`>rpg shop` - Checkout the shop\n`>rpg training` - Fight against a group of monsters", false, false);
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
                    return this.process_Battle(input, user, ref current, ref currentPlayer);
            }
            return new Result();
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
            foreach (Skill skill in player.skills)
            {
                str = str + "**Name**: `" + skill.Name + "`\n";
                object[] objArray1 = new object[] { str, "**Cost**: `", skill.MpCost, " Mp`\n" };
                str = string.Concat(objArray1);
                object[] objArray2 = new object[] { str, "**Damage**: `", skill.value, "`\n" };
                str = string.Concat(objArray2);
                str = str + "**Description**: \n*" + skill.Desc + "*\n\n";
            }
            return str;
        }

        public class Group
        {
            public string background;
            public ulong id;
            public bool inFight = false;
            public List<Monster> monsters = new List<Monster>();
            public PlaceType placeType = PlaceType.Town;
            public Dictionary<ulong, Player> players = new Dictionary<ulong, Player>();

            public Group(IUser user)
            {
                this.players.Add(user.Id, new Player(user));
                this.id = user.Id;
            }

            public void CreateGroup()
            {
                this.monsters.Clear();
                int num = new Random().Next(1, this.Level);
                num = Math.Min(MonsterPositions.Length, num);
                for (int i = 0; i < num; i++)
                {
                    this.monsters.Add(new Monster("Slime", new Stats(15, 5, 5, 10, 20, 0, 0)));
                }
                this.monsters.Sort((Comparison<Monster>)((a, b) => a.stats.Str.CompareTo(b.stats.Str)));
                for (int j = 0; j < this.monsters.Count; j++)
                {
                    this.monsters[j].position = MonsterPositions[this.monsters.Count - 1, j];
                }
                this.inFight = true;
            }

            public bool HasEnemy
            {
                get
                {
                    foreach (Monster monster in this.monsters)
                    {
                        if (monster.stats.Hp > 0)
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }

            public int Level
            {
                get
                {
                    int num = 0;
                    foreach (Player player in this.players.Values)
                    {
                        num += player.stats.Level;
                    }
                    return Convert.ToInt32(Math.Round((double)(num / this.players.Values.Count), 2));
                }
            }

            public enum PlaceType
            {
                Dungeon,
                Town
            }
        }

    }

    public class Dungeon : Location
    {
    }

    public class Effect
    {
        public Status effect;
        public string name;
        public int turn;
        public int value;

        public Effect()
        {
            this.name = "";
        }

        public Effect(Effect copy)
        {
            this.name = copy.name;
            this.turn = copy.turn;
            this.value = copy.value;
            this.effect = copy.effect;
        }

        public Effect(string _name, int _turn, int _value, Status _effect)
        {
            this.name = _name;
            this.turn = _turn;
            this.value = _value;
            this.effect = _effect;
        }

        public enum Status
        {
            Sleep,
            Poison,
            Weakness
        }
    }

    public class Item
    {
        public int Id;
        public string Name;
    }

    public class Location
    {
        public int Id;
        public string Name;
    }

    public class Monster
    {
        public int asleep;
        public string Name;
        public Point position;
        public Stats stats;

        public Monster(Monster copy)
        {
            this.Name = copy.Name;
            this.stats = new Stats(copy.stats);
        }

        public Monster(string _name, Stats _stats)
        {
            this.Name = _name;
            this.stats = _stats;
        }
    }

    public class Player
    {
        public bool isProcessing;
        public bool isReady;
        public string nickName;
        public List<Skill> skills;
        public Stats stats;
        public IUser user;

        public Player(IUser _user)
        {
            this.isReady = true;
            this.isProcessing = false;
            this.skills = new List<Skill>();
            this.stats = new Stats(30, 20, 5, 0, 0, 0, 0);
            this.skills.Add(new Skill("Fireball", "Shoots a fireball", 5, 15, false, new Effect()));
            this.user = _user;
            this.nickName = (_user.Username.Length > 8) ? _user.Username.Substring(0, 8) : _user.Username;
        }

        public Player(Player copy)
        {
            this.isReady = true;
            this.isProcessing = false;
            this.skills = new List<Skill>();
            this.user = copy.user;
            this.nickName = copy.nickName;
            this.stats = new Stats(copy.stats);
            this.skills = copy.skills;
        }

        public bool HasSkill(string name, out Skill skill)
        {
            skill = new Skill("null", -1);
            foreach (Skill skill2 in this.skills)
            {
                if (skill2.Name.ToLower() == name.ToLower())
                {
                    skill = new Skill(skill2);
                    return true;
                }
            }
            return false;
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

    public class Skill
    {
        public string Desc;
        public Effect effect;
        public bool isAoe;
        public int MpCost;
        public string Name;
        public int value;

        public Skill()
        {
            this.Name = "";
        }

        public Skill(Skill copy)
        {
            this.Name = copy.Name;
            this.Desc = copy.Desc;
            this.MpCost = copy.MpCost;
            this.value = copy.value;
            this.isAoe = copy.isAoe;
            this.effect = new Effect(copy.effect);
        }

        public Skill(string _name, int _value)
        {
            this.Name = _name;
            this.value = _value;
        }

        public Skill(string _name, string _desc, int _mpcost, int _value, bool _isAoe, Effect effect)
        {
            this.Name = _name;
            this.Desc = _desc;
            this.MpCost = _mpcost;
            this.value = _value;
            this.isAoe = _isAoe;
            this.effect = new Effect(effect);
        }
    }

    public class Stats
    {
        public int gold;
        public int Hp;
        public int MaxHp;
        public int MaxMp;
        public int Mp;
        public int Str;
        public int xp;

        public Stats(Stats copy)
        {
            this.gold = copy.gold;
            this.Hp = copy.Hp;
            this.Mp = copy.Mp;
            this.MaxHp = copy.MaxHp;
            this.MaxMp = copy.MaxMp;
            this.Str = copy.Str;
            this.xp = copy.xp;
        }

        public Stats(int _hp, int _mp, int _str, int _xp, int _gold, int _maxHp = 0, int _maxMp = 0)
        {
            this.gold = _gold;
            this.Hp = _hp;
            this.Mp = _mp;
            this.MaxHp = (_maxHp == 0) ? _hp : _maxHp;
            this.MaxMp = (_maxMp == 0) ? _mp : _maxMp;
            this.Str = _str;
            this.xp = _xp;
        }

        public int Level
        {
            get
            {
                int num = 1;
                int num2 = 50;
                int xp = this.xp;
                while (num2 < xp)
                {
                    xp -= num2;
                    num2 = Convert.ToInt32((double)(num2 * 1.2000000476837158));
                    num++;
                }
                return num;
            }
        }
    }

    public class Town : Location
    {
        public List<Item> shopItems;
    }
}