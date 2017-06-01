using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using Discord.Commands;


namespace NoireBot
{
    public class Program
    {

        public static List<string> Zippies = new List<string>();
        public static int[] lastZippies = new int[20];
        public static string help;
        public Random rand;
        public static RPG rpg;
        public static ulong LumiID = 128182611376996352;


        // Convert our sync main to an async main.
        public static void Main(string[] args) {
            try {
                new Program().Start().GetAwaiter().GetResult();
            } catch(Exception exc)
            {
                Console.WriteLine(exc);
                Console.ReadKey();
                Console.ForegroundColor = ConsoleColor.White;
            }
            }
        public static DiscordSocketClient client;
        private CommandHandler handler;
        

        public async Task Start()
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(WelcomeImage());
            SendColorHelp();
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

            handler = new CommandHandler();
            await handler.Install(map);

            LoadDatas();

            // add logger
            client.Log += Log;

            // Log the invite URL on client ready
            client.Ready += Client_Ready;

            client.MessageReceived += Client_MessageReceived;
            await Task.Delay(3000);
            SetGame();
            SaveData();
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
                // Block this program until it is closed.

                foreach (var guild in client.Guilds)
                {
                    foreach (var user in guild.Users)
                    {
                        int index = ProfileCommands.CheckUser(user);
                        ProfileCommands.profiles[index].Point++;
                        ProfileCommands.profiles[index].WriteFile();
                    }
                }
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