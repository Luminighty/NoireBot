using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using Discord.WebSocket;
using System.Drawing;
using System.IO;

namespace NoireBot
{
    [Group("RPG")]
    [Alias(new string[] { "game" })]
    public class RPGCommands : ModuleBase
    {
        [Command]
        public async Task RpgCommand()
        {
            await ReplyAsync("To start your adventure use `>rpg start`");
            //default
        }

        [Command("start")]
        public async Task Start()
        {
            int id = RPG.GetParty(Context.User as IGuildUser);

            if(RPG.Sessions[id].dungeon != null)
            {
                await ReplyAsync("You already inside a dungeon");
                return;
            }

            RPG.Sessions[id].dungeon = RPG.defaultDungeon;
            Vector2 pos = RPG.defaultDungeon.FindRoom(0);
            RPG.Sessions[id].position = pos;
            await ReplyAsync(RPG.Sessions[id].dungeon.StartText);
            await ReplyAsync(RPG.Sessions[id].dungeon.GetRoomText(pos));

        }

        [Command("stop")]
        [Alias(new string[] { "giveup", "abandon", "exit" })]
        public async Task stop()
        {
            int id = RPG.GetParty(Context.User as IGuildUser);
            if(RPG.Sessions[id].dungeon == null)
            {
                await ReplyAsync("You aren't in any dungeon!");
                return;
            }
            RPG.Sessions[id].dungeon = null;
            await ReplyAsync("Dungeon abandoned!");
        }

        [Command("go")]
        public async Task Go(string input = "")
        {

            if (!RPG.isLeader(Context.User as IGuildUser))
            {
                await ReplyAsync("Only the party leader can use this command!");
                return;
            }

            if (input == "")
            {
                await ReplyAsync("Usage: `>rpg go {N/E/S/W}");
                return;
            }

            string[] north = new string[] { "n", "north" };
            string[] east = new string[] { "e", "east" };
            string[] south = new string[] { "s", "south" };
            string[] west = new string[] { "w", "west" };
            int sessionId = RPG.GetParty(Context.User as IGuildUser);

            if(RPG.Sessions[sessionId].dungeon == null)
            {
                await ReplyAsync("You aren't in a dungeon! Use `>rpg start`");
                return;
            }

            Vector2 pos = RPG.Sessions[sessionId].position;
            Vector2 size = RPG.Sessions[sessionId].dungeon.Size;
            Vector2 direction = Vector2.Zero;
            #region getDirection
            foreach (string n in north)
                if (n == input.ToLower())
                {
                    direction = Vector2.Up;
                }
            foreach (string n in west)
                if (n == input.ToLower())
                {
                    direction = Vector2.Left;
                }
            foreach (string n in south)
                if (n == input.ToLower()) {
                    direction = Vector2.Down;
                }
            foreach (string n in east)
                if (n == input.ToLower()) {
                    direction = Vector2.Right;
                }
            #endregion
            
            if(!RPG.Sessions[sessionId].dungeon.isRoomExists(pos + direction))
            {
                await ReplyAsync("You don't see a path that way!");
                return;
            }

            RPG.Sessions[sessionId].position = pos + direction;
            pos = pos + direction;
            await ReplyAsync(RPG.Sessions[sessionId].dungeon.GetRoomText(pos));
            if(RPG.Sessions[sessionId].dungeon.winRoomPos == RPG.Sessions[sessionId].position)
            {
                RPG.Sessions[sessionId].dungeon = null;
            }
        }

        [Command("help")]
        public async Task Help()
        {
            await ReplyAsync("Hope this helps! :thinking:");
        }


        [Group("party")]
        [Alias(new string[] { "p", "group", "g" })]
        public class PartyCommands : ModuleBase {

            public static Dictionary<IGuildUser, RPG.Party> Invites = new Dictionary<IGuildUser, RPG.Party>();
            [Command]
            public async Task MainParty(IGuildUser user = null)
            {
                if (user == null)
                {
                    RPG.Party party = RPG.FindParty(Context.User as IGuildUser);
                    if (party == new RPG.Party())
                    {
                        await ReplyAsync("You aren't a member of any party. Use `>rpg party @User` to invite someone.");
                        return;
                    }
                    if(party.heroes.Count < 1)
                    {
                        party.heroes.Add(new RPG.Hero(Context.User as IGuildUser));
                        RPG.Sessions.Add(party);
                    }
                    string PartyText = "Your party has " + party.heroes.Count + " hero";
                    if (party.heroes.Count > 1)
                        PartyText += "es";
                    PartyText += ":\n";
                    for(int i = 0; i < party.heroes.Count; i++)
                    {
                        if (i == 0)
                            PartyText += ":crown: ";
                        PartyText += "**" + party.heroes[i].Name + "** (" + party.heroes[i].stats.lvl + ")\n";
                    }
                    await ReplyAsync(PartyText);
                    return;
                }
                if (Invites.ContainsKey(user))
                    Invites.Remove(user);

                int id = RPG.GetParty(Context.User as IGuildUser);

                if(id == -1)
                {
                    RPG.Sessions.Add(new RPG.Party(Context.User as IGuildUser));
                }

                if(user.Id == 246933734010519552 && Context.User.Id == Program.LumiID)
                {
                    RPG.Party party = RPG.FindParty(Context.User as IGuildUser);
                    party.heroes.Add(new RPG.Hero(user));
                }
                Invites.Add(user, RPG.FindParty(Context.User as IGuildUser));
                await ReplyAsync(user.Mention + " you've been invited to " + Context.User.Username + "'s party! Please note: You can be part of only one party. If you join you'll leave the other one. \n Use `>rpg p {Y/N}' to accept or decline it!");

            }

            [Command("kick")]
            public async Task Kick(IGuildUser user = null)
            {
                if (user == null)
                {
                    await ReplyAsync("Usage: `>rpg party kick @User`");
                    return;
                }

                int id = RPG.GetParty(Context.User as IGuildUser);
                if (id == -1)
                {
                    await ReplyAsync("You don't have a party!");
                    return;
                }
                bool found = false;
                for (int i = 0; i < RPG.Sessions[id].heroes.Count; i++)
                    if (RPG.Sessions[id].heroes[i].owner == user.Id)
                    {
                        RPG.Sessions[id].heroes.RemoveAt(i);
                        found = true;
                    }
                for (int i = 0; i < RPG.Sessions[id].users.Count; i++)
                    if (RPG.Sessions[id].users[i].Id == user.Id)
                    {
                        RPG.Sessions[id].users.RemoveAt(i);
                        found = true;
                    }
                string reply = (found) ? user.Mention + " was kicked from the party!" : "User not found in your party!";

                await ReplyAsync(reply);
            }

            [Command("leave")]
            public async Task Leave()
            {
                foreach (RPG.Party party in RPG.Sessions)
                {
                    IGuildUser user = Context.User as IGuildUser;
                    foreach (RPG.Hero hero in party.heroes)
                        if (hero.owner == user.Id)
                            party.heroes.Remove(hero);
                    foreach (IGuildUser member in party.users)
                        if (member.Id == user.Id)
                            party.users.Remove(member);
                }
                await ReplyAsync(Context.User.Mention + " has left the party!");
            }

            [Command("invme")]
            public async Task InvMe(IGuildUser user = null)
            {
                if (Context.User.Id != Program.LumiID)
                    return;
                int id = RPG.GetParty(user);
                if(id == -1)
                {
                    RPG.Sessions.Add(new RPG.Party(user));
                    id = RPG.Sessions.Count - 1;
                }

                Invites.Add(Context.User as IGuildUser, RPG.Sessions[id]);
                 await ReplyAsync(Context.User.Mention + " you've been invited to " + user.Username + "'s party! Please note: You can be part of only one party. If you join you'll leave the other one. \n Use `>rpg p {Y/N}' to accept or decline it!");

            }

            [Command("lead")]
            [Alias(new string[] { "leader", "setlead", "setleader", "givelead" })]
            public async Task SetLeader(IGuildUser user = null)
            {
                if (user == null)
                {
                    await ReplyAsync("Usage: `>rpg party lead @User`");
                    return;
                }
                for(int i = 0; i < RPG.Sessions.Count; i++)
                    if(RPG.Sessions[i].users[0].Id == Context.User.Id)
                    {
                        //leader found!
                        for(int j = 0; j < RPG.Sessions[i].users.Count; j++)
                            if(RPG.Sessions[i].users[j].Id == user.Id)
                            {
                                RPG.Sessions[i].users[0] = user;
                                RPG.Sessions[i].users[j] = Context.User as IGuildUser;
                                await ReplyAsync(RPG.Sessions[i].users[0].Mention + " is the new party leader!");
                                return;
                            }

                        await ReplyAsync(Context.User.Username + " isn't in your party!");
                        return;
                    }
                await ReplyAsync("You aren't a party leader!");
            }

            [Command]
            public async Task MainParty(string answer)
            {
                switch (answer.ToLower())
                {
                    case "kick":
                        await Kick();
                        return;
                    case "leave":
                        await Leave();
                        return;
                    case "invme":
                        await InvMe();
                        return;
                    case "lead":
                        await SetLeader();
                        return;
                    default:
                        break;
                }
                if (!Invites.ContainsKey(Context.User as IGuildUser))
                {
                    await ReplyAsync("You weren't invited to any party. Use `>rpg p @User` to invite someone!");
                    return;
                }
                string[] accepts = new string[] { "accept", "a", "y", "yes" };
                string[] declines = new string[] { "decline", "d", "n", "no" };

                switch (answer.ToLower())
                {
                    case "a":
                    case "y":
                    case "yes":
                    case "accept":

                        foreach (RPG.Party oldparty in RPG.Sessions)
                            foreach (RPG.Hero hero in oldparty.heroes)
                                if (hero.owner == Context.User.Id)
                                    oldparty.heroes.Remove(hero);

                        RPG.Party party = Invites[Context.User as IGuildUser];
                        for (int i = 0; i < RPG.Sessions.Count; i++)
                            if (RPG.Sessions[i] == party)
                            {
                                RPG.Sessions[i].heroes.Add(RPG.GetUserHero(Context.User as IGuildUser));
                                await ReplyAsync("You accepted the invite to " + RPG.Sessions[i].users[0].Username + "'s party!");
                                Invites.Remove(Context.User as IGuildUser);
                                return;
                            }

                        break;
                    case "decline":
                    case "d":
                    case "n":
                    case "no":

                        await ReplyAsync("Party invitation declined.");
                        Invites.Remove(Context.User as IGuildUser);
                        return;
                    default:
                        await ReplyAsync("Usage: `>rpg p {Y/N}`");
                        break;
                }
            }

        }
    }

    public class RPG
    {
        public static List<Monster> Monsters = new List<Monster>();
        public static List<Item> Items = new List<Item>();
        public static List<Party> Sessions = new List<Party>();
        public static List<Hero> heroes = new List<Hero>();
        public static Dungeon defaultDungeon;

        #region Methods

        private static bool isDataLoaded = false;

        public static void LoadData()
        {
            if (isDataLoaded)
                return;

        }

        public RPG() {
            RPG.LoadData();
            DefaultDungeon();
        }

        /// <summary>
        /// NOT DONE! use DefaultDungeon()
        /// </summary>
        public void GenerateDungeon()
        {
            //Dungeon Generator

        }

        public void DefaultDungeon()
        {
            FileStream file = File.Open("../../RPG/Dungeon.txt", FileMode.Open);
            StreamReader reader = new StreamReader(file);

            Dungeon newDungeon = new Dungeon();

            string[] size = reader.ReadLine().Split(";".ToCharArray(), 2);
            newDungeon.Size = new Vector2(Convert.ToInt32(size[0]), Convert.ToInt32(size[1]));
            newDungeon.rooms = new Room[newDungeon.Size.x, newDungeon.Size.y];

            for(int y = newDungeon.Size.y - 1; y > -1; y--)
            {
                string[] Xline = reader.ReadLine().Split(";".ToCharArray(), newDungeon.Size.x);
                for (int x = 0; x < newDungeon.Size.x; x++)
                {
                    int roomID = Convert.ToInt32(Xline[x]);
                    newDungeon.rooms[x, y] = new Room(roomID);
                    if (newDungeon.winRoom < roomID)
                    {
                        newDungeon.winRoom = roomID;
                        newDungeon.winRoomPos = new Vector2(x, y);
                    }
                }
            }

            newDungeon.StartText = reader.ReadLine();
            
            while(reader.Peek() > -1)
            {
                int id = Convert.ToInt32(reader.ReadLine());
                Vector2 pos = newDungeon.FindRoom(id);
                if(pos.x < 0 || pos.y < 0)
                {
                    return;
                }
                newDungeon.rooms[pos.x, pos.y] = LoadRoom(reader, id);
            }
            reader.Close();
            file.Close();
            defaultDungeon = newDungeon;
        }

        public static Party FindParty(IGuildUser user)
        {
            foreach (Party party in Sessions)
                foreach (Hero hero in party.heroes)
                    if (hero.owner == user.Id)
                        return party;
            return new Party();
        }
        
        public static int GetParty(IGuildUser user)
        {
            for(int i = 0; i < Sessions.Count; i++)
                foreach (Hero hero in Sessions[i].heroes)
                    if (hero.owner == user.Id)
                        return i;
            Sessions.Add(new Party(user));
            return Sessions.Count - 1;
        }
        

        public static bool isLeader(IGuildUser user)
        {

            int id = GetParty(user);
            return (Sessions[id].users[0].Id == user.Id);
        }

        public static Hero GetUserHero(IGuildUser user)
        {
            foreach (Hero hero in heroes)
                if (hero.owner == user.Id)
                    return hero;
            return new Hero(user);
        }

        Room LoadRoom(StreamReader reader, int id)
        {
            Room newRoom = new Room(id);
            newRoom.FillText = reader.ReadLine();
            for (int i = 0; i < newRoom.WayText.Length; i++)
                newRoom.WayText[i] = reader.ReadLine();
            string Line = reader.ReadLine();
            while((Line = reader.ReadLine()) != "}")
                newRoom.monsters.Add(Convert.ToInt32(Line));
            reader.ReadLine();
            while ((Line = reader.ReadLine()) != "}")
                newRoom.items.Add(Convert.ToInt32(Line));

            return newRoom;
        }

#endregion

        public class Party
        {
            public string Name;
            public List<IGuildUser> users = new List<IGuildUser>();
            public List<Hero> heroes = new List<Hero>();
            public Dungeon dungeon;
            public Vector2 position;
            public bool isLeader(IGuildUser user)
            {
                return (users[0].Id == user.Id);
            }

            public Party()
            {
                heroes = new List<Hero>();
                users = new List<IGuildUser>();
            }

            public Party(IGuildUser leader)
            {
                heroes = new List<Hero>();
                users = new List<IGuildUser>();
                users.Add(leader);
                heroes.Add(new Hero(leader));
            }

        }


    public class Dungeon
    {
        public string StartText;
        public Vector2 Size;
        public Room[,] rooms = new Room[0,0];
            public int winRoom;
            public Vector2 winRoomPos = new Vector2(-1, -1);

        public Dungeon()
        {
            this.Size = Vector2.Zero;
        }

        public bool isRoomExists(Vector2 v)
        {
            return (v.x > -1 && v.x < rooms.GetLength(0) && v.y > -1 && v.y < rooms.GetLength(1) && rooms[v.x, v.y] != null);
        }

        public Room GetRoom(Vector2 v)
        {
            if (!isRoomExists(v))
                return new Room();
            return rooms[v.x, v.y];
        }

        public Room GetRoom(int i)
        {
            foreach (Room room in rooms)
                if (room.id == i)
                    return room;
            return new Room();
        }

        public Vector2 FindRoom(int i)
        {
            for (int y = 0; y < rooms.GetLength(1); y++)
                for (int x = 0; x < rooms.GetLength(0); x++)
                    if (rooms[x, y].id == i)
                        return new Vector2(x, y);
            return Vector2.Left + Vector2.Down;
        }

        public string GetRoomText(Vector2 v)
        {
            string text = "";
            if (!isRoomExists(v))
                return "Room not found!";

            Room room = GetRoom(v);
            text = room.FillText;
            text += Environment.NewLine + room.WayText[0] + Environment.NewLine + room.WayText[1] + Environment.NewLine + room.WayText[2] + Environment.NewLine + room.WayText[3];

            return text;
        }

    }

    public class Monster
    {
        public int id;
        public string name;
        public string FillText;
        public Stats stats;
        public List<Spell> spells = new List<Spell>();
    }

    public class Hero
    {
        public string Name;
        public ulong owner;
        public Stats stats;
        public List<int> inventory = new List<int>();
        public int[] equipment = new int[3];
        public List<Spell> spells = new List<Spell>();

            public Hero(IGuildUser user)
            {
                this.owner = user.Id;
                this.Name = user.Username;
                this.stats.lvl = 1;
            }
    }

    public struct Stats
    {
        public int Hp;
            public int Hp_max;
            public int Mp_max;
            public int Mp;
        public int Strenght;
        public int lvl;

    }

    public class Room
    {
        public int id = -1;
        public string FillText = "";
        public string[] WayText = new string[4];
        public List<int> items = new List<int>();
        public List<int> monsters = new List<int>();

        public Room()
        {
            this.id = -1;
        }

        public Room(int _id)
        {
            this.id = _id;
        }

    }

    public enum Directions
    {
        North,
        East,
        South,
        West
    }

    public class Item
    {
        public int id;
        public string Name;
        public EquipmentSlots Slot;

        public enum EquipmentSlots
        {
            Armor,
            Weapon,
            Accessory
        }

    }

    public class Spell
    {
        public int id;
        public string name;
        public string description;
    }

    }

    public class Vector2
    {
        public int x = 0;
        public int y = 0;

        public double Lenght()
        {
            return Math.Sqrt(x * x + y * y);
        }

        public static double Distance(Vector2 v1, Vector2 v2)
        {
            return Math.Sqrt((v2.x - v1.x) * (v2.x - v1.x) + (v2.y - v1.y) * (v2.y - v1.y));
        }

        [NonSerialized]
        public static Vector2 One = new Vector2(1, 1);
        [NonSerialized]
        public static Vector2 Zero = new Vector2(0, 0);
        [NonSerialized]
        public static Vector2 Right = new Vector2(1, 0);
        [NonSerialized]
        public static Vector2 Left = new Vector2(-1, 0);
        [NonSerialized]
        public static Vector2 Up = new Vector2(0, 1);
        [NonSerialized]
        public static Vector2 Down = new Vector2(0, -1);

        public Vector2(int _x, int _y)
        {
            this.x = _x;
            this.y = _y;
        }

        public static Vector2 operator +(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.x + v2.x, v1.y + v2.y);
        }

        public static Vector2 operator *(Vector2 v1, Vector2 v2)
        {
            return new Vector2(v1.x * v2.x, v1.y * v2.y);
        }

        public static bool operator ==(Vector2 v1, Vector2 v2)
        {
            return (v1.x == v2.x && v1.y == v2.y);
        }

        public static bool operator !=(Vector2 v1, Vector2 v2)
        {
            return !(v1.x == v2.x && v1.y == v2.y);
        }

        public static Vector2 operator *(Vector2 v1, int x)
        {
            return new Vector2(v1.x * x, v1.y * x);
        }

    }

}
