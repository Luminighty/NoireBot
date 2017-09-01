using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoireBot.Rpg
{
	public class Monster
	{
		public static List<Monster> database = new List<Monster>();


		public static int GetMonsterId(string name)
		{
			for (int i = 0; i < database.Count; i++)
				if (database[i].Name == name)
					return i;
			return -1;
		}

		public static int GetMonsterId(Monster m)
		{
			return GetMonsterId(m.Name);
		}


		public static void LoadMonsters(string file)
		{
			StreamReader reader = new StreamReader(file);
			while (reader.Peek() > -1)
				database.Add(ReadMonster(ref reader));
			reader.Close();
			reader.Dispose();
		}


		public static Monster ReadMonster(ref StreamReader reader)
		{
			Monster m = new Monster();
			m.Name = reader.ReadLine();
			m.Desc = reader.ReadLine();


			string[] equipList = reader.ReadLine().Split(' ');
			for (int i = 0; i < equipList.Length; i++)
				if (equipList[i] != "")
					m.equipment[i] = Convert.ToInt32(equipList[i]);


			string[] spellList = reader.ReadLine().Split(' ');
			for (int i = 0; i < spellList.Length; i++)
				if (spellList[i] != "")
					m.skills.Add(Convert.ToInt32(spellList[i]));
			m.stats = Stats.ReadStats(ref reader);
			return m;
		}

		public Monster()
		{
			this.Name = "";
			this.stats = new Stats();
		}

		public Monster(string _name, Stats _stats)
		{
			this.Name = _name;
			this.stats = _stats;
		}

		public Monster(Monster copy)
		{
			this.Name = copy.Name;
			this.stats = new Stats(copy.stats);
		}

		public List<int> skills = new List<int>();
		public int[] equipment = new int[4] { -1, -1, -1, -1 };
		public string Desc = "";
		public string Name = "";
		public Stats stats = new Stats();

		public PointF position = new PointF();
	}

}
