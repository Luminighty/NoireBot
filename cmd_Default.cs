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

        [Command("details")]
        [Alias(new string[] { "version", "v" })]
        public async Task Details()
        {
            ProfileCommands.SortProfiles();
            Program.LoadZippies();
            int points = 0;
            foreach (Profile prof in ProfileCommands.profiles)
                points += prof.Point;
            string zippyLine = "";
            string autism = "";
            string richest = "richest";
            if(!Context.IsPrivate)
            if (Context.Guild.Id == VofiServerID)
            {
                zippyLine = Program.Zippies.Count + " Zippy quotes, ";
                autism = " autism";
                richest = "most autistic";
            }
            string text = "Noirebot is currently on version 1.3.3.8" + Environment.NewLine +
                "There are " + zippyLine + ProfileCommands.profiles.Count + " profiles and the " + richest + " has " + ProfileCommands.profiles[0].Point + autism + " point." + Environment.NewLine +
                "Alltogether we have " + points + autism + " points.";
            await ReplyAsync(text);
        }

        [Command("Help")]
        public async Task Help()
        {
            string plusText = "";
            if(Context.Guild.Id == VofiServerID)
                plusText = "\n```> zippy - Sends a * **> zippy * **message    { tts}\n            -Sends the message as a Text - To - Speech message.```\n```> sendzippy[Message] - Send a message to the '>zippy' command. \nNote: type '#&' for New Line! (Only admins can use this)```";
            await ReplyAsync(Program.help + plusText);
        }
        
        [Command("Noire")]
        [Alias(new string[] { "waifu" })]
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
            await Context.Channel.SendFileAsync(files[i]);
        }

        [Command("Ping")]
        public async Task Ping() {
            int i = rand.Next(100);
            if (i == 69)
            {
                await ReplyAsync("Fuck off " + Context.User.Mention);
            }
            else
            {
                await ReplyAsync("Pong.");
            }
        }

        [Command("Zippy")]
        public async Task Zippy(string args = "")
        {
            if (Context.Guild.Id != VofiServerID && Context.User.Id != Program.LumiID)
                return;
            int lineCount = Program.Zippies.Count;
            int i = rand.Next(lineCount);
            bool alreadySent = false;
            bool skipRandomizer = false;


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
                if (args.ToLower() == "last")
                {
                    var user = await Context.Guild.GetUserAsync(Context.User.Id);
                    if (user.GuildPermissions.Administrator)
                    {
                        skipRandomizer = true;
                        i = lineCount - 1;

                    }
                }
                else
                {
                    if (args != "")
                        for (int j = 0; j < Program.Zippies.Count; j++)
                        {
                            if (Program.Zippies[j].Contains(args))
                            {
                                skipRandomizer = true;
                                i = j;
                                j = Program.Zippies.Count;
                            }
                        }
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
            if (args.ToLower() == "tts")
            {
                await Context.Channel.SendMessageAsync(text, true);
            }
            else
            {
                await ReplyAsync(text);
            }
        }

        [Command("sendZippy")]
        public async Task sendZippy(string text)
        {
            if (Context.Guild.Id != VofiServerID && Context.User.Id != Program.LumiID)
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
            var channel = await Context.User.CreateDMChannelAsync();
            if (Context.Channel == channel && Context.User.Id != Program.LumiID)
            {
                IGuildUser user = Context.User as IGuildUser;
                if (!user.GuildPermissions.Administrator)
                    return;
            }
            await Context.Message.DeleteAsync();
            if (autist == null || number == "")
            {
                await channel.SendMessageAsync("Usage: >autism @User point");
                return;
            }

            int sender = ProfileCommands.CheckUser(Context.User);

            if (0 < ProfileCommands.profiles[sender].LastAutism.AddSeconds(5).CompareTo(DateTime.Now))
            {
                int sec = ProfileCommands.profiles[sender].LastAutism.AddSeconds(5).Subtract(DateTime.Now).Seconds;
                string s = "";
                if (sec > 1)
                    s = "s";
                await channel.SendMessageAsync("Hold on! You're using delayed commands! Please wait " + sec.ToString() + " second" + s + " for your next >rep");
                return;
            }

            int pointUser = ProfileCommands.CheckUser(autist as IUser);
            
            try
            {
                int newPoint = Convert.ToInt32(number);
                ProfileCommands.profiles[pointUser].Point += newPoint;
                ProfileCommands.profiles[pointUser].WriteFile();
                if (newPoint > 0)
                    await ReplyAsync(ProfileCommands.profiles[pointUser].Name + " is more autistic now. Huzzah!");
                if (newPoint < 0)
                    await ReplyAsync(ProfileCommands.profiles[pointUser].Name + " is less autistic now... Somehow.");
                await Program.Log(new LogMessage(LogSeverity.Debug, "Autism", Context.User.Username + " used Autism."));

            } catch(Exception exc)
            {
                await channel.SendMessageAsync("Usage: >autism @User point");
                return;
            }


        }

        [Command("rep")]
        [Alias(new string[] { "repu", "r" })]
        public async Task Rep(IGuildUser user = null, [Remainder]string text = "")
        {
            if(user == null)
            {
                await ReplyAsync("Usage: > >rep @User { **+ / -** }");
                return;
            }

            int index = ProfileCommands.CheckUser(Context.User);

            if (0 < ProfileCommands.profiles[index].LastRep.AddMinutes(1).CompareTo(DateTime.Now))
            {
                int sec = ProfileCommands.profiles[index].LastRep.AddMinutes(1).Subtract(DateTime.Now).Seconds;
                string s = "";
                if (sec > 1)
                    s = "s";
                await ReplyAsync("Hold on! You're using delayed commands! Please wait " + sec.ToString() + " second" + s + " for your next >rep");
                return;
            }

            int Rep = ProfileCommands.CheckUser(user);

            if(ProfileCommands.profiles[Rep].id == ProfileCommands.profiles[index].id)
            {
                await ReplyAsync("You can't give rep to yourself!");
                return;
            }

            if (text == "-")
            {
                ProfileCommands.profiles[Rep].rep--;
                await ReplyAsync(Context.User.Username + " removed rep from " + ProfileCommands.profiles[Rep].Name + "!");
                ProfileCommands.profiles[Rep].WriteFile();
                ProfileCommands.profiles[index].LastRep = DateTime.Now;
                return;
            }
            if(text == "+" || text == "")
            {
                ProfileCommands.profiles[Rep].rep++;
                await ReplyAsync(Context.User.Username + " gave rep to " + ProfileCommands.profiles[Rep].Name + "!");
                ProfileCommands.profiles[Rep].WriteFile();
                ProfileCommands.profiles[index].LastRep = DateTime.Now;
                return;
            }

        }

        [Command("support")]
        [Alias(new string[] { "donate", "don", "sup" })]
        public async Task Donate([Remainder]string text = "")
        {
            string extratext = "";
            if(text == "")
                extratext = "If you want to leave a message aswell just use this form: `> donate[message]`";
            await ReplyAsync("Infinitely appreciated, but although I'm made this as a hobby, and running it on my own, currently there's no way to support me.\n Don't worry though, your kindness is saved.\n " + extratext);
            FileStream file = File.Open("../../donates.txt", FileMode.Append);
            StreamWriter writer = new StreamWriter(file);
            writer.WriteLine(Context.User.Id + " " + Context.User.Username + " - " + text);
            writer.Close();
            file.Close();

        }

    }
}
