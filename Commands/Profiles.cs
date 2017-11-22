using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Net;
using System.IO;
using System.Drawing;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace NoireBot
{
    [Group("profile")]
    [Alias(new string[] {"p", "prof"})]
    public class ProfileCommands : ModuleBase
    {

        #region variables
        public static List<Profile> profiles = new List<NoireBot.Profile>();
        static Dictionary<string, int> overlays = new Dictionary<string, int>();
        static Dictionary<string, int> backgrounds = new Dictionary<string, int>();
        #endregion

        #region Commands
        [Command]
        [Summary("The main profile command")]
        public async Task Profile([Remainder]IGuildUser user = null)
        {
            if (user == null)
                user = Context.User as IGuildUser;
            SortProfiles();
            int id = CheckUser(user);
            await Context.Channel.SendFileAsync(profiles[id].GeneratePicture(id, user));
        }

        #region background
        [Command("bg")]
        [Alias(new string[] { "background" })]
        public async Task Background([Remainder]string bg)
        {
            int id = CheckUser(Context.User);
            await ReplyAsync(profiles[id].SetBackground(bg, backgrounds));
        }

        [Command("bg")]
        [Alias(new string[] { "background" })]
        public async Task Background()
        {
            await ReplyAsync("Backgrounds: <https://sites.google.com/view/noirebot/home/backgrounds>");
        }

        #endregion

        #region overlay


        [Command("ov")]
        [Alias(new string[] { "overlay" })]
        public async Task Overlay([Remainder]string bg)
        {
            int id = CheckUser(Context.User);
            await ReplyAsync(profiles[id].SetOverlay(bg, overlays));
        }

        [Command("ov")]
        [Alias(new string[] { "overlay" })]
        public async Task Overlay()
        {
            await ReplyAsync("Overlays: <https://sites.google.com/view/noirebot/home/overlays>");
        }
        #endregion

        #region description
        [Command("description")]
        [Alias(new string[] { "d", "desc" })]
        public async Task Description([Remainder]string bg)
        {
            int id = CheckUser(Context.User);
            await ReplyAsync(profiles[id].SetDescription(bg));
        }


        #endregion

        #region points

        [Command("points")]
        [Alias(new string[] { "p" })]
        public async Task Points([Remainder]IGuildUser user = null)
        {
            if (user == null)
                user = Context.User as IGuildUser;
            int id = CheckUser(user);
            await ReplyAsync(profiles[id].GetPoint(user.Mention));
        }


		#endregion

		#endregion

		#region Methods

		/// <summary>
		/// Checks if the user exists in the list
		/// If not, it generates it's profile
		/// </summary>
		/// <param name="user"></param>
		/// <returns>User index</returns>
		public static int CheckUser(IUser user)
        {
            int id = GetProfile(user.Id);
			
			if (id == -1)
            {
                id = profiles.Count;
				profiles.Add(new Profile(user));
            }

            return id;
        }

        public static void LoadProfiles()
        {

            Program.Log(new LogMessage(LogSeverity.Debug, "Profiles", "Profiles Update Started!"));

            string[] folders = Directory.GetDirectories("../../Profiles/Users/");

            foreach (string folder in folders)
            {


				FileStream file = File.Open(folder + "/main.profile", FileMode.Open);
				StreamReader reader = new StreamReader(file);
				Profile newProf = JsonConvert.DeserializeObject<Profile>(reader.ReadToEnd());
				/*
				BinaryFormatter formatter = new BinaryFormatter();
				Profile newProf = ((Profile)formatter.Deserialize(file));*/
                profiles.Add(newProf);
				file.Close();
            }

            Program.Log(new LogMessage(LogSeverity.Debug, "Profiles", "Profiles Updated!"));
        }

        public static int GetProfile(ulong id)
        {
            if (!IsExists(id))
                return -1;
            for (int i = 0; i < profiles.Count; i++)
                if (profiles[i].id == id)
                    return i;
            return -1;
        }

        public static bool IsExists(ulong id)
        {
            return Directory.Exists("../../Profiles/Users/" + id);
        }

        public static void SortProfiles()
        {
            profiles.Sort(delegate (Profile p1, Profile p2) { return (-1 * p1.xp).CompareTo((-1 * p2.xp)); });
        }

        /// <summary>
        /// Loads the shop.list file
        /// It stores the backgrounds and overlays
        /// </summary>
        public static void LoadProfileBuilders()
        {
            FileStream file = File.Open("../../shop.list", FileMode.Open);
            StreamReader reader = new StreamReader(file);
            bool isOverlay = false;
            while (reader.Peek() > -1)
            {
                string line = reader.ReadLine();
                if (line[0] == ';')
                    continue;
                if (line == "Overlays")
                {
                    isOverlay = true;
                    continue;
                }
                line = line.ToLower();
                if (!isOverlay)
                {
                    backgrounds.Add(line, Convert.ToInt16(reader.ReadLine()));
                }
                else
                {
                    overlays.Add(line, Convert.ToInt16(reader.ReadLine()));
                }
            }
            reader.Close();
            file.Close();

        }

		#endregion

	}

	[System.Serializable]
    public class Profile
    {
		public Profile()
		{

		}
        public Profile(ulong _id = 0)
        {
            this.id = _id;
            this.Name = "";
            this.desc = "";
			this.credit = 0;
			this.xp = 0;
			this.bg = "empty";
            this.overlay = "overlay";
            this.owned_overlays = new List<string>();
            this.owned_backgrounds = new List<string>();
            this.rand = new Random();
        }

        public Profile(IUser user, bool iswritefile = true)
		{
			Program.Log(new LogMessage(LogSeverity.Debug, "Profiles", "New Profile(" + user.Username +")"));
			if (Directory.Exists("../../Profiles/Users/" + user.Id) && File.Exists("../../Profiles/Users/" + user.Id + "/main.profile"))
			{
				Program.Log(new LogMessage(LogSeverity.Debug, "Profiles", "Folder already exists! (" + user.Id + ")"));
				return;
			}
			this.id = user.Id;
            this.Name = user.Username;
            this.desc = "";
			this.credit = 0;
			this.xp = 0;
			this.bg = "empty";
            this.overlay = "overlay";
            this.owned_overlays = new List<string>();
            this.owned_backgrounds = new List<string>();
            this.rand = new Random();
            this.tag = user.DiscriminatorValue;
			if (iswritefile)
				WriteFile();
        }
		
        #region variables
        public string Name;
        public string desc;

        public string bg;
        public string overlay;

        public List<string> owned_backgrounds = new List<string>();
        public List<string> owned_overlays = new List<string>();

		/// <summary>
		/// DO NOT USER IT TO ADD BADGES!
		/// </summary>
		public List<Badge> badges = new List<Badge>();

        public ulong id;
        public ushort tag;
		
		public int xp;
		public int remainingXp = 0;
		public int xpNeeded = 0;

		public int lvl
		{
			get {
				int _lvl = 1;
				int nextLvl = 100;
				remainingXp = xp;
				xpNeeded = nextLvl;
				while (remainingXp >= nextLvl)
				{
					remainingXp -= nextLvl;
					nextLvl = Convert.ToInt32(nextLvl * 1.2f);
					xpNeeded = nextLvl;
				}
				return _lvl;
			}
		}

		/// <summary>
		/// The last time the user recieved xp or credits from messages
		/// </summary>
		public DateTime MessageCd;

		/// <summary>
		/// The last time the user used Dailies
		/// </summary>
		public DateTime dailyCd;

        public int credit;
        public int reputation;

        public DateTime prof_LastUpdated;
        public DateTime LastRep;

        private Random rand;
		#endregion

		#region Methods
		public string GeneratePicture(int rank, IUser user)
        {
            if (string.IsNullOrEmpty(this.bg))
                this.bg = "empty";
            if (string.IsNullOrEmpty(this.overlay))
                this.overlay = "overlay";
            System.Drawing.Image avatar;
            using (WebClient client = new WebClient())
            {
                byte[] im = client.DownloadData(new Uri(user.GetAvatarUrl()));
                avatar = System.Drawing.Image.FromStream(new MemoryStream(im));
            }
            avatar = (System.Drawing.Image)(new Bitmap(avatar, 96, 96));
            Bitmap bitmap = (Bitmap)System.Drawing.Image.FromFile("../../Profiles/Customizations/Backgrounds/" + this.bg + ".png");
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                using (Font cooper14px = new Font("Cambria", 14))
				{
					Brush brush = Brushes.White;
					Font cooper20px = new Font("Cambria", 20);
					Font cooper24px = new Font("Cambria", 24);
					Font cooper12px = new Font("Cambria", 12);

					graphics.DrawImage(System.Drawing.Image.FromFile("../../Profiles/Customizations/Overlays/" + this.overlay + ".png"), new PointF(0F, 0F));
                    RectangleF namePoint = new RectangleF(118F, 115F, 170F, 28F);
					RectangleF AvatarPoint = new RectangleF(17F, 79F, 96,96);

                    RectangleF descRect = new RectangleF(15, 280, 255, 60);

					RectangleF Rank = new RectangleF(138, 185, 71, 16);
					RectangleF credits = new RectangleF(138, 203, 71, 16);
					RectangleF xp = new RectangleF(138, 222, 71, 16);
					
					RectangleF level = new RectangleF(15, 205, 56, 21);
					RectangleF Rep = new RectangleF(77, 205, 56, 21);
					StringFormat format = new StringFormat
                    {
                        Alignment = StringAlignment.Near,
                        LineAlignment = StringAlignment.Near
                    };
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    graphics.DrawString(desc, cooper12px, brush, descRect);

					graphics.DrawString((rank+1) + "#".ToString(), cooper14px, brush, Rank, format);
					graphics.DrawString(credit.ToString(), cooper14px, brush, credits, format);
					graphics.DrawString(this.xp.ToString(), cooper14px, brush, xp, format);

					graphics.DrawString(Name, cooper24px, brush, namePoint, format);

                    format.Alignment = StringAlignment.Center;
                    graphics.DrawString(reputation.ToString(), cooper20px, brush, Rep, format);
                    graphics.DrawString(lvl.ToString(), cooper20px, brush, level, format);

                    graphics.DrawImage(avatar, AvatarPoint);
                }
            }
            bitmap.Save("../../Profiles/Profile.png");

            return "../../Profiles/Profile.png";
        }

        public string SetDescription(string newDesc)
        {
            if (0 < this.prof_LastUpdated.AddSeconds(5).CompareTo(DateTime.Now))
            {
                int sec = this.prof_LastUpdated.AddSeconds(5).Subtract(DateTime.Now).Seconds;
                string s = "";
                if (sec > 1)
                    s = "s";
                return "Hold on! You're using delayed commands! Please wait " + sec.ToString() + " second" + s + ".";
            }
            if (newDesc.Length > 256)
            {
                return "Description is too long. (Limit: 256)";
            }
            if (this.desc != newDesc)
            {
                this.desc = newDesc;
                WriteFile();
            }
            this.prof_LastUpdated = DateTime.Now;
            return "New description set!";
        }

        public string SetBackground(string bg, Dictionary<string, int> backgrounds)
        {
            
            if (0 < this.prof_LastUpdated.AddSeconds(5).CompareTo(DateTime.Now))
            {
                int sec = this.prof_LastUpdated.AddSeconds(5).Subtract(DateTime.Now).Seconds;
                string s = "";
                if (sec > 1)
                    s = "s";
                return "Hold on! You're using delayed commands too frequently! Please wait " + sec.ToString() + " second" + s + ".";
            }
            if (backgrounds.ContainsKey(bg))
            {
                if (backgrounds[bg] > this.credit)
                {
                    return "You don't have enough point! It needs " + backgrounds[bg] + " autism point.";
                }
                if (this.bg != bg)
                {
                    this.bg = bg;
                    WriteFile();
                }
                this.prof_LastUpdated = DateTime.Now;
                return "Background set!";
            }
            else
            {
                return "Background not found!";
            }
        }

        public string SetOverlay(string overlay, Dictionary<string, int> overlays)
        {
            if (0 < this.prof_LastUpdated.AddSeconds(5).CompareTo(DateTime.Now))
            {
                int sec = this.prof_LastUpdated.AddSeconds(5).Subtract(DateTime.Now).Seconds;
                string s = "";
                if (sec > 1)
                    s = "s";
                return "Hold on! You're using delayed commands! Please wait " + sec.ToString() + " second" + s + ".";
            }
            if (overlays.ContainsKey(overlay))
            {
                if (overlays[overlay] > this.credit)
                {
                    return "You don't have enough point! It needs " + overlays[overlay] + " autism point.";
                }
                if (this.overlay != overlay)
                {
                    this.overlay = overlay;
                    WriteFile();
                }
                this.prof_LastUpdated = DateTime.Now;
                return "Overlay set!";
            }
            else
            {
                return "Background not found!";
            }
        }

        public string GetPoint(string mention)
        {
            int tempPoint = this.credit / 50;
            string plustext = "";
            string[] emojies = new string[] { "🙃", "🤶", "🤤", "💆", "💩", "🤥", "🤡" };
            string emoji = emojies[rand.Next(0, emojies.Length)];
            switch (tempPoint)
            {
                case 0:
                    plustext = "Great job! So far you're normal";
                    break;
                case 1:
                    plustext = "Still good! Though worse than before.";
                    break;
                case 2:
                    plustext = "Oh, no. You have a slight chance of Autism.";
                    break;
                case 3:
                    plustext = "Just stop, while you can!";
                    break;
                case 4:
                    plustext = "You have autism.";
                    break;
                default:
                    plustext = "(That means you're autistic)";
                    break;
            }
            if (this.credit > 1000)
                plustext = "Autism overloaded. Kys!";
            return mention + ", your Autism Points is on " + this.credit + "! " + plustext + " " + emoji;
        }

        public static void SortProfiles(ref List<Profile> profiles)
        {
            profiles.Sort(delegate (Profile p1, Profile p2) { return (-1 * p1.credit).CompareTo((-1 * p2.credit)); });
        }

		public static List<ulong> profileWritings = new List<ulong>();

		/// <summary>
		/// If it's already writing the profile then prevent another process from doing so;
		/// </summary>
		bool alreadyWriting = false;

		static List<ulong> writingIds = new List<ulong>();

        public void WriteFile()
        {
			if (alreadyWriting) { 
				Program.Log(new LogMessage(LogSeverity.Debug, "Profiles", "Already Writing"));
				return;
			}
			if(writingIds.Contains(this.id))
			{

				Program.Log(new LogMessage(LogSeverity.Critical, "Profiles", "Duplicate detected! (" + this.id + ")"));
				return;
			}

			if (this.id == 0)
				Program.Log(LogSeverity.Debug, "Profiles", "Writing a file with ID = 0");

			writingIds.Add(this.id);
			alreadyWriting = true;
            if (!Directory.Exists("../../Profiles/Users/" + this.id))
                Directory.CreateDirectory("../../Profiles/Users/" + this.id);

			try
			{
				FileStream file = File.Create("../../Profiles/Users/" + this.id + "/main.profile");
				StreamWriter writer = new StreamWriter(file);
				/*writer.WriteLine(this.Name);
                writer.WriteLine(this.id);
                writer.WriteLine(this.tag);
                writer.WriteLine(this.desc);
                writer.WriteLine(this.Point);
                writer.WriteLine(this.rep);
                writer.WriteLine(this.bg);
                writer.WriteLine(this.overlay);
                writer.WriteLine("{");
                foreach (string bg in this.owned_backgrounds)
                    writer.WriteLine(bg);
                writer.WriteLine("}");
                writer.WriteLine("{");
                foreach (string ov in this.owned_overlays)
                    writer.WriteLine(ov);
                writer.WriteLine("}");
                writer.Close();*/
				string json = JsonConvert.SerializeObject(this, Formatting.None);
				writer.WriteLine(json);
				writer.Close();
				/*
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(file, this);*/

                file.Close();
			} catch(Exception e)
            {
                Program.Log(new LogMessage(LogSeverity.Info, "Profiles", e.Message));
			}
			alreadyWriting = false;
			writingIds.Remove(this.id);
		}

		public void AddBadge(Badge badge)
		{
			for (int i = 0; i < badges.Count; i++)
				if (badges[i].id == badge.removes)
				{
					badges.RemoveAt(i);
					break;
				}
			badges.Add(badge);
		}

		public string ToString(string separator = "; ")
		{
			string a = "Profile (" + Name + "#" + tag + "): ";
			a += "{id:\"" + id + "\"" + separator;
			a += "desc:\"" + desc + "\"" + separator;
			a += "bg:\"" + bg + "\"" + separator;
			a += "overlay:\"" + overlay + "\"" + separator;
			a += "credits:\"" + credit + "\"" + separator;
			a += "reputation:\"" + reputation + "\"" + separator;
			a += "xp:\"" + xp + " (" + remainingXp + "/" + xpNeeded + ") Lvl." + lvl + "\"" + separator;
			a += "LastDaily:\"" + dailyCd.ToShortDateString() + " " + dailyCd.ToShortTimeString() + "\"" + separator;
			a += "LastRep:\"" + LastRep.ToShortDateString() + " " + LastRep.ToShortTimeString() + "\"" + separator;
			a += "}";
			return a;
		}
		public string ToShortString(string separator = "; ")
		{

			string a = "Profile (" + Name + "#" + tag + "): ";
			a += "{id:\"" + id + "\"" + separator;
			a += "}";
			return a;
		}

		#endregion

		[System.Serializable]
		public class Badge
		{

			public int id;
			public string name;
			public string description;
			public string badgeImg;
			public int removes;

			public Badge(int _id, string _name, string _description, string _badgeImg, int _removes)
			{
				this.id = _id;
				this.name = _name;
				this.description = _description;
				this.badgeImg = _badgeImg;
				this.removes = _removes;
			}


		}

	}

}
