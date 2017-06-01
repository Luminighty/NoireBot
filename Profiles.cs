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
            int id = GetProfile(user.Id);
            if(id == -1)
            {
                id = profiles.Count;
                profiles.Add(new Profile(user));
            }
            await Context.Channel.SendFileAsync(profiles[id].GeneratePicture(GetProfile(user.Id), user));
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

            string[] folders = Directory.GetDirectories("../../Profiles/");

            foreach (string folder in folders)
            {
                FileStream file = File.Open(folder + "/main.profile", FileMode.Open);
                StreamReader reader = new StreamReader(file);

                Profile newProf = new Profile();
                newProf.Name = reader.ReadLine();
                newProf.id = Convert.ToUInt64(reader.ReadLine());
                newProf.tag = Convert.ToUInt16(reader.ReadLine());
                newProf.desc = reader.ReadLine();
                newProf.Point = Convert.ToInt16(reader.ReadLine());
                newProf.rep = Convert.ToInt16(reader.ReadLine());
                newProf.bg = reader.ReadLine();
                newProf.overlay = reader.ReadLine();
                string bg = reader.ReadLine(); //bg start
                int counter = 0;
                while (bg != "}" && counter < 100)
                {
                    counter++;
                    bg = reader.ReadLine();
                    if (bg != "}")
                        newProf.owned_backgrounds.Add(bg);
                }
                counter = 0;
                bg = reader.ReadLine(); //ov start
                while (bg != "}" && counter < 100)
                {
                    counter++;
                    bg = reader.ReadLine();
                    if (bg != "}")
                        newProf.owned_overlays.Add(bg);
                }


                reader.Close();
                file.Close();
                profiles.Add(newProf);
            }

            Program.Log(new LogMessage(LogSeverity.Debug, "Profiles", "Profiles Updated!"));
        }

        public static int GetProfile(ulong id)
        {
            if (!isExists(id))
                return -1;
            for (int i = 0; i < profiles.Count; i++)
                if (profiles[i].id == id)
                    return i;
            return -1;
        }

        public static bool isExists(ulong id)
        {
            return Directory.Exists("../../Profiles/" + id);
        }

        public static void SortProfiles()
        {
            profiles.Sort(delegate (Profile p1, Profile p2) { return (-1 * p1.Point).CompareTo((-1 * p2.Point)); });
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

    }

    public class Profile
    {
        public Profile()
        {
            this.id = 0;
            this.Name = "";
            this.desc = "";
            this.Point = 0;
            this.bg = "Empty";
            this.overlay = "PlainBlack";
            this.owned_overlays = new List<string>();
            this.owned_backgrounds = new List<string>();
            this.rand = new Random();
        }

        public Profile(IUser user)
        {
            Console.WriteLine("Creating new profile for: " + user.Username);
            this.id = user.Id;
            this.Name = user.Username;
            this.desc = "";
            this.Point = 0;
            this.bg = "Empty";
            this.overlay = "PlainBlack";
            this.owned_overlays = new List<string>();
            this.owned_backgrounds = new List<string>();
            this.rand = new Random();

            WriteFile();
        }


        #region variables
        public string Name;
        public string desc;

        public string bg;
        public string overlay;

        public List<string> owned_backgrounds = new List<string>();
        public List<string> owned_overlays = new List<string>();

        public ulong id;
        public ushort tag;

        public int Point;
        public int rep;

        public DateTime prof_LastUpdated;
        public DateTime LastAutism;
        public DateTime LastRep;

        private Random rand;

        #endregion

        public string GeneratePicture(int rank, IUser user)
        {
            if (this.bg == "default")
                this.bg = "Empty";
            if (this.overlay == "Default_Black")
                this.overlay = "PlainBlack";
            System.Drawing.Image avatar;
            using (WebClient client = new WebClient())
            {
                byte[] im = client.DownloadData(new Uri(user.GetAvatarUrl()));
                avatar = System.Drawing.Image.FromStream(new MemoryStream(im));
            }
            avatar = (System.Drawing.Image)(new Bitmap(avatar, 128, 128));
            Bitmap bitmap = (Bitmap)System.Drawing.Image.FromFile("../../Customizations/Backgrounds/" + this.bg + ".png");
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                using (Font cooper = new Font("Cooper Std Black", 15))
                {
                    graphics.DrawImage(System.Drawing.Image.FromFile("../../Customizations/Overlays/" + this.overlay + ".png"), new PointF(0F, 0F));
                    RectangleF namePoint = new RectangleF(13F, 17F, 224F, 31F);
                    PointF AvatarPoint = new PointF(77F, 68F);
                    RectangleF AutPoints = new RectangleF(24F, 230F, 73F, 40F);
                    RectangleF descRect = new RectangleF(13F, 295F, 226F, 85F);
                    RectangleF Rank = new RectangleF(185, 185, 45, 30);
                    RectangleF Rep = new RectangleF(185, 235, 45, 30);
                    StringFormat format = new StringFormat();
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;
                    graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    Font SmallCooper = new Font("Cooper Std Black", 11);
                    graphics.DrawString(this.desc, SmallCooper, Brushes.Black, descRect);
                    graphics.DrawString(this.Point.ToString(), cooper, Brushes.Black, AutPoints, format);
                    graphics.DrawString(this.Name, cooper, Brushes.Black, namePoint, format);
                    format.Alignment = StringAlignment.Far;
                    graphics.DrawString(this.rep.ToString(), cooper, Brushes.Black, Rep, format);
                    graphics.DrawString((rank + 1).ToString(), cooper, Brushes.Black, Rank, format);
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
                return "Hold on! You're using delayed commands! Please wait " + sec.ToString() + " second" + s + ".";
            }
            if (backgrounds.ContainsKey(bg))
            {
                if (backgrounds[bg] > this.Point)
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
                if (overlays[overlay] > this.Point)
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
            int tempPoint = this.Point / 50;
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
            if (this.Point > 1000)
                plustext = "Autism overloaded. Kys!";
            return mention + ", your Autism Points is on " + this.Point + "! " + plustext + " " + emoji;
        }

        public static void SortProfiles(ref List<Profile> profiles)
        {
            profiles.Sort(delegate (Profile p1, Profile p2) { return (-1 * p1.Point).CompareTo((-1 * p2.Point)); });
        }

        public void WriteFile()
        {
            if (!Directory.Exists("../../Profiles/" + this.id))
                Directory.CreateDirectory("../../Profiles/" + this.id);
            FileStream file = File.Create("../../Profiles/" + this.id + "/main.profile");
            StreamWriter writer = new StreamWriter(file);
            writer.WriteLine(this.Name);
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
            writer.Close();
            file.Close();
        }
    }

}
