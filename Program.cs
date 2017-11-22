using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Audio;
using NoireBot.Rpg;
using System.Management;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace NoireBot
{
    public class Program
    {

        public static List<string> Zippies = new List<string>();
        public static int[] lastZippies = new int[20];
        public static int[] lastMemes = new int[20];
		public static List<string> vofiNicks = new List<string>();
		public static string help;
        public static Random rand;
        public static RPG rpg;
        public static ulong LumiID = 128182611376996352;
        public static ulong botID = 246933734010519552;
		public static ulong noireServerID = 314878458348175361;
		public static List<ulong> spamChannels = new List<ulong>();
        public static IAudioClient audClient;
        public static List<UtilityCommands.Remind> reminders = new List<UtilityCommands.Remind>();
		static bool DataLoaded = false;
		/// <summary>
		/// ../../
		/// </summary>
		public static string sourcePath = "../../";

        // Convert our sync main to an async main.
        public static void Main(string[] args) {
            try {
                new Program().Start(args).GetAwaiter().GetResult();
            } catch(Exception exc)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(exc);
				System.Threading.Thread.Sleep(10000);
				Main(new string[0]);
            }
            }

        public static DiscordSocketClient client;
		//private IConfiguration config;
        private CommandHandler handler;
        
        public void GetArgs(string[] args)
        {
            for(int i = 0; i < args.Length; i++)
            {
                switch(args[i])
                {
                    case "-admin":
                        //set admin
                        isAdmin = true;
                        break;
                    case "-game":
                        if((i + 1) < args.Length)
                        {
                            StartingGame = args[i + 1];
                        }
                        break;
                    default:
                        break;
                }

            }

        }

        private string StartingGame = "";
        private bool isAdmin = false;
        private string lockKey = "";
        private int tryKick = 0;

        public async Task Start(string[] args)
        {

			client = new DiscordSocketClient();
			//config = BuildConfig();

			var token = "MjQ2OTMzNzM0MDEwNTE5NTUy.DKcMkg.HX_ilw_n5RfojvsXjFgS8NXTdBw";

			var services = ConfigureServices();
			services.GetRequiredService<LogService>();
			await services.GetRequiredService<CommandHandler>().Install(services);


			lockKey = Environment.UserName + Environment.TickCount.ToString();
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(WelcomeImage());
            GetArgs(args);
            rand = new Random();
            // Define the DiscordSocketClient with a DiscordSocketConfig

            // Login and connect to Discord.
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            if(isAdmin)
            await Log(new LogMessage(LogSeverity.Info, "Control", "Admin Detected"));
			if (!DataLoaded)
			{
				LoadDatas();
				LoadVofiNicks();
				DataLoaded = true;
			}
			changeVofiNick();

			// add logger
			// Log the invite URL on client ready
			client.Ready += Client_Ready;

            client.MessageReceived += Client_MessageReceived;
			client.MessageDeleted += Client_MessageDeleted;
            await Task.Delay(5000);
            SetGame();
            SaveData();
            CheckLock();
            onSecondTick();
            await Task.Delay(-1);
        }
		private IServiceProvider ConfigureServices()
		{
			return new ServiceCollection().
				AddSingleton(client)
				.AddSingleton<CommandService>()
				.AddSingleton<CommandHandler>()
				.AddSingleton<AudioService>()
				.AddLogging()
				.AddSingleton<LogService>()
				//.AddSingleton(config)
				.BuildServiceProvider();
		}
		/*
		private IConfiguration BuildConfig()
		{
			return new ConfigurationBuilder().
				SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("config.json")
				.Build();
		}*/

        private void LoadDatas()
        {
            ProfileCommands.LoadProfileBuilders();
            ProfileCommands.LoadProfiles();
            LoadZippies();
            LoadHelp();
            LoadReminders();
            rpg = new RPG();
        }

        private async void SetGame()
        {
            if(StartingGame != "" && !string.IsNullOrEmpty(StartingGame))
            {
                await client.SetGameAsync(StartingGame);
                await Task.Delay(3600000);
				StartingGame = "";
                SetGame();
                return;
            }
                int i = rand.Next(4);
                switch (i)
                {
                    case 0:
                        await client.SetGameAsync(">help for help");
                        break;
                    case 1:
                        await client.SetGameAsync("on " + client.Guilds.Count + " servers.");
                        break;
                    case 2:
                        await client.SetGameAsync("with " + ProfileCommands.profiles.Count + " users.");
                        break;
                    case 3:
                        await client.SetGameAsync("Hyperdimension Neptunia");
                        break;
                    default:
                        await client.SetGameAsync(">help for help");
                        break;
                }
                await Task.Delay(3600000);
            SetGame();

        }

        public static async void SaveData(bool isCalled = false)
        {
			if(!isCalled)
            await Task.Delay(60000);
			foreach (Profile prof in ProfileCommands.profiles)
			{
				try
				{
					prof.WriteFile();
				} catch (Exception e)
				{
					string name = (prof == null) ? "null" : prof.Name;
					await Log(LogSeverity.Error, "Profiles", "Couldn't save profile for " + name + "! Exception: " + e.Message);
				}
				if (isCalled)
					Console.WriteLine(prof.ToString("\n"));
			}
			if (!isCalled)
			{
				SaveData();
			} else
			{
				await Log(LogSeverity.Info, "Save", "Saved.");
			}
        }

        private string WelcomeImage()
        {
            FileStream file = File.Open("../../WelcomeImage.txt", FileMode.Open);
            StreamReader reader = new StreamReader(file);
            string text = reader.ReadToEnd();
            reader.Close();
            file.Close();
            return text;
        }
		
		private async void onZippyMessage(SocketMessage arg)
		{

			IUserMessage msg = await arg.Channel.GetMessageAsync(arg.Id) as IUserMessage;
			await msg.AddReactionAsync(Emote.Parse("<:noire:360395650648768513>") as IEmote);
			await msg.AddReactionAsync(Emote.Parse("<:Zippy:236772422991347712>") as IEmote);
		}
		public static bool ZippyMessageReactions = false;

		void LoadVofiNicks()
		{
			if (!File.Exists("../../vofi.nick"))
				return;
			StreamReader reader = new StreamReader("../../vofi.nick");
			while(reader.Peek() > -1)
			{
				vofiNicks.Add(reader.ReadLine());
			}

			reader.Close();
			reader.Dispose();

		}

        private async void onSecondTick()
        {
            //ReminderCheck
            for(int i = 0; i<reminders.Count; i++)
            {
                if(reminders[i].time.CompareTo(DateTime.UtcNow) <= 0)
                {
                    var channel = await client.GetUser(reminders[i].user).GetOrCreateDMChannelAsync() as IMessageChannel;
                    await channel.SendMessageAsync(":alarm_clock: **Reminder:**" + reminders[i].text + "! :alarm_clock:");
                    
                    File.Delete(sourcePath + "reminders/" + reminders[i].id + ".rem");
                    reminders.RemoveAt(i);
                }
            }

            await Task.Delay(1000);
            onSecondTick();
        }

		private Dictionary<ulong, IUserMessage> ai_msgCopies = new Dictionary<ulong, IUserMessage>();

		private Task Client_MessageDeleted(Cacheable<IMessage, ulong> arg1, ISocketMessageChannel arg2)
		{
			if(ai_msgCopies.ContainsKey(arg1.Id))
			{
				var channel = client.GetGuild(206475498153443329).GetChannel(206475498153443329) as IMessageChannel;
				ai_msgCopies[arg1.Id].DeleteAsync();
				ai_msgCopies.Remove(arg1.Id);
			}
		
			return Task.CompletedTask;
		}

		private async void changeVofiNick()
		{
			await Task.Delay(1300000);
			if (vofiNicks.Count > 0)
			{
				Action<GuildUserProperties> action = new Action<GuildUserProperties>((x) => {
					x.Nickname = vofiNicks[rand.Next(0, vofiNicks.Count)];
				});

				await client.GetGuild(206475498153443329).CurrentUser.ModifyAsync(action);

			}
			changeVofiNick();
		}

		private async void onAIMessage(SocketMessage arg)
		{
			if (arg.Content.Length > 2)
				if (arg.Content[0] == '/' && arg.Content[1] == '/')
					return;
			if (arg.Author.IsBot && arg.Author.Id != 246933734010519552)
				return;
			var channel = client.GetGuild(206475498153443329).GetChannel(206475498153443329) as IMessageChannel;
			IUserMessage msg = null;
			if (arg.Attachments.Count != 0)
			{
				foreach (var c in arg.Attachments)
				{
					string name = Zippies[rand.Next(0, Zippies.Count)];
					WebClient web = new WebClient();
					web.DownloadFile(c.Url, "../" + name + ".png");
					msg = await channel.SendFileAsync("../" + name + ".png", arg.Content);
					File.Delete("../" + name + ".png");
				}
			}
			else if (!string.IsNullOrEmpty(arg.Content))
			{
				msg = await channel.SendMessageAsync(arg.Content);
			}
			ai_msgCopies.Add(arg.Id, msg);
		}

        private Task Client_MessageReceived(SocketMessage arg)
        {
			if (arg.Channel.Id == 369541623979442176)
				onAIMessage(arg);
			if(arg.Author.Id == 133871176199045120)
			{
				if(Program.ZippyMessageReactions)
					onZippyMessage(arg);

			}

			
            int index = ProfileCommands.CheckUser(arg.Author);
			Profile p = ProfileCommands.profiles[index];
			DateTime t = p.MessageCd;
			if (DateTime.Compare(t.AddSeconds(10), DateTime.UtcNow) > 0)
			{
				p.credit++;
				int lvl = p.lvl;
				p.xp += rand.Next(1, 5);
				if(lvl != p.lvl)
				{
					arg.Channel.SendMessageAsync("**Level Up!** " + arg.Author.Username + " is now **Lvl" + p.lvl + "**!");
				}
				p.MessageCd = DateTime.UtcNow;
			}
			return Task.CompletedTask;
        }
        
        private async void CheckLock()
        {
            if(File.Exists("../../Lock"))
            {
                StreamReader reader = new StreamReader("../../Lock");
                string line = reader.ReadLine();
                reader.Close();
                if (line != lockKey)
                {
                    if (tryKick > 2)
                        Environment.Exit(-1);
                    if(!isAdmin)
                        tryKick++;
                    File.Delete("../../Lock");
                } else
                {
                    await Task.Delay(6000);
                    CheckLock();
                    return;
                }

            }
            StreamWriter writer = new StreamWriter("../../Lock");
            writer.WriteLine(lockKey);
            writer.Close();
            await Task.Delay(6000);
            CheckLock();

        }

        // log the OAuth2 Invite URL of the bot on client ready so that user can see it on startup
        private async Task Client_Ready()
        {
            var application = await client.GetApplicationInfoAsync();
            await Log(new LogMessage(LogSeverity.Info, "Discord",
                $"Invite URL: <https://discordapp.com/oauth2/authorize?client_id={application.Id}&scope=bot>"));
        }
		
        // Bare minimum Logging function for both DiscordSocketClient and CommandService
        public static Task Log(LogMessage msg)
        {
			LogService.Log(msg);
            return Task.CompletedTask;
        }
        
		public static Task Log(LogSeverity severity, string Source, string Message)
		{
			Log(new LogMessage(severity, Source, Message));
			return Task.CompletedTask;
		}

        public static void LoadHelp()
        {
            FileStream file = File.Open("../../Help.txt", FileMode.Open);
            StreamReader reader = new StreamReader(file);
            help = reader.ReadToEnd();
            reader.Close();
            file.Close();
        }

        public static void LoadZippies()
        {
            Log(new LogMessage(LogSeverity.Debug, "Zippy", "Zippies update started"));
            FileStream file = new FileStream("../../zippyInsults", FileMode.Open);
            StreamReader reader = new StreamReader(file);
            Zippies = new List<string>();
            while (reader.Peek() > -1)
            {
                string newLine = reader.ReadLine();
                Zippies.Add(newLine);
            }
            Log(new LogMessage(LogSeverity.Debug, "Zippy", "Zippies updated"));
            file.Close();
            reader.Close();
        }

        public void LoadReminders()
        {
            BinaryFormatter formatter = new BinaryFormatter();
            string[] files = Directory.GetFiles(sourcePath + "reminders/");
            foreach(string file in files)
            {
                FileStream f = File.Open(file, FileMode.Open);
                UtilityCommands.Remind remindme = formatter.Deserialize(f) as UtilityCommands.Remind;
                reminders.Add(remindme);
                f.Close();
            }
            Program.Log(LogSeverity.Info, "Reminders", "Reminders Loaded.");
        }

    }
}