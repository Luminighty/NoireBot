using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Audio;
using System.Management;

namespace NoireBot
{
    public class Program
    {

        public static List<string> Zippies = new List<string>();
        public static int[] lastZippies = new int[20];
        public static int[] lastMemes = new int[20];
        public static string help;
        public Random rand;
        public static RPG rpg;
        public static ulong LumiID = 128182611376996352;
        public static ulong botID = 246933734010519552;
        public static IAudioClient audClient;

        // Convert our sync main to an async main.
        public static void Main(string[] args) {
            try {
                new Program().Start(args).GetAwaiter().GetResult();
            } catch(Exception exc)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(exc);
                Console.ReadKey();
            }
            }
        public static DiscordSocketClient client;
        private CommandHandler handler;
        
        public void getArgs(string[] args)
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
            lockKey = Environment.UserName + Environment.TickCount.ToString();
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(WelcomeImage());
            SendColorHelp();
            getArgs(args);
            rand = new Random();
            // Define the DiscordSocketClient with a DiscordSocketConfig
            client = new DiscordSocketClient(new DiscordSocketConfig() {
                LogLevel = LogSeverity.Info
            });
            var token = "MjQ2OTMzNzM0MDEwNTE5NTUy.DAb7QA.TJSU1gzfw_SiRVMJZYJOd1vvxFA";

            // Login and connect to Discord.
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            var map = new DependencyMap();
            map.Add(client);
            map.Add(new AudioService());
            handler = new CommandHandler();
            await handler.Install(map);

            if(isAdmin)
            await Log(new LogMessage(LogSeverity.Info, "Control", "Admin Detected"));

            LoadDatas();

            // add logger
            client.Log += Log;

            // Log the invite URL on client ready
            client.Ready += Client_Ready;

            client.MessageReceived += Client_MessageReceived;
            await Task.Delay(5000);
            SetGame();
            SaveData();
            CheckLock();
            await Task.Delay(-1);
        }

        private void LoadDatas()
        {
            ProfileCommands.LoadProfileBuilders();
            ProfileCommands.LoadProfiles();
            LoadZippies();
            LoadHelp();
            rpg = new RPG();
        }

        private async void SetGame()
        {
            if(StartingGame != "" && !string.IsNullOrEmpty(StartingGame))
            {
                await client.SetGameAsync(StartingGame);
                await Task.Delay(3600000);
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

        private async void SaveData()
        {
            await Task.Delay(60000);
            foreach (Profile prof in ProfileCommands.profiles)
                prof.WriteFile();
            SaveData();
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

        private void SendColorHelp()
        {
            Console.WriteLine();
            for (int i = 0; i < 6; i++)
                Log(new LogMessage((LogSeverity)i, "Colors", ((LogSeverity)i).ToString()));
            Console.WriteLine();
        }

        private Task Client_MessageReceived(SocketMessage arg)
        {
            int index = ProfileCommands.CheckUser(arg.Author);
            ProfileCommands.profiles[index].Point++;
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
            switch(msg.Severity)
            {
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
                case LogSeverity.Verbose:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
            Console.WriteLine(msg.ToString());
            Console.ForegroundColor = ConsoleColor.Gray;
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


    }
}