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

    public class Class
    {
        public static List<Class> database = new List<Class>();
        public static int GetClassId(string name)
        {
            for (int i = 0; i < database.Count; i++)
                if (database[i].Name == name)
                    return i;
            return -1;
        }
        public static int GetClassId(Skill s)
        {
            return GetClassId(s.Name);
        }

        public static void LoadClasses(string file)
        {
            StreamReader reader = new StreamReader(file);
            while (reader.Peek() > -1)
                database.Add(ReadClass(ref reader));
            reader.Close();
			reader.Dispose();
        }

        public static Class ReadClass(ref StreamReader reader)
        {
            Class c = new Class();
            c.Name = reader.ReadLine();
            c.Description = reader.ReadLine();
            string[] spellList = reader.ReadLine().Split(' ');
            for (int i = 0; i < spellList.Length; i++)
                if (spellList[i] != "")
                    c.skills.Add(Convert.ToInt32(spellList[i]));
            c.multiplier = Stats.ReadStats(ref reader);
            return c;
        }

        public string Name = "";
        public string Description = "";
        public List<int> skills = new List<int>();
        public Stats multiplier = new Stats(100, 100, 100, 100, 100, 0, 0);

    }
    
    public class Dungeon : Location
    {
    }
    
    public class Location
    {
        public int Id;
        public string Name;
    }
    
    
    public class Town : Location
    {
        public List<Item> shopItems;
	}
	public enum UseOn
	{

		Ally,
		DeadAlly,
		Enemy
	}
}
