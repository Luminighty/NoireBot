using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoireBot.Rpg
{

	public class Item
	{
		public static Item ReadItem(ref StreamReader reader)
		{
			int type = Convert.ToInt32(reader.ReadLine());
			Item it = new Item();
			it.Name = reader.ReadLine();
			it.Description = reader.ReadLine();
			it.cost = Convert.ToInt32(reader.ReadLine());

			switch (type)
			{
				case 1:
					Equipment eq = new Equipment(it);
					eq.slot = (Equipment.Slots)Convert.ToInt32(reader.ReadLine());
					string[] txtclasses = reader.ReadLine().Split(' ');
					for (int i = 0; i < txtclasses.Length; i++)
						if (txtclasses[i] != "")
							eq.forClass.Add(Convert.ToInt32(txtclasses[i]));
					eq.level = Convert.ToInt32(reader.ReadLine());
					eq.stats = Stats.ReadStats(ref reader);
					eq.Armor = Convert.ToInt32(reader.ReadLine());
					return eq;
				case 2:
					Consumable con = new Consumable(it);
					con.stat = (Consumable.Stat)Convert.ToInt32(reader.ReadLine());
					con.value = Convert.ToInt32(reader.ReadLine());
					con.turn = Convert.ToInt32(reader.ReadLine());
					con.isCuring = Convert.ToBoolean(reader.ReadLine());
					con.cures = (Effect.Status)Convert.ToInt32(reader.ReadLine());
					return con;
				default:
					return it;
			}
		}

		public static List<Item> database = new List<Item>();
		public static int GetItemId(string name)
		{
			for (int i = 0; i < database.Count; i++)
				if (database[i].Name == name)
					return i;
			return -1;
		}

		public static int GetItemId(Item i)
		{
			return GetItemId(i.Name);
		}


		public static void LoadItems(string file)
		{
			StreamReader reader = new StreamReader(file);
			while (reader.Peek() > -1)
				database.Add(ReadItem(ref reader));
			reader.Close();
			reader.Dispose();
		}

		public Item()
		{
			this.Name = "";
			this.Description = "";
			this.cost = 0;
		}
		public Item(string name, string desc, int _cost)
		{
			this.Name = name;
			this.Description = desc;
			this.cost = _cost;
		}

		public Item(Item item)
		{
			this.Name = item.Name;
			this.Description = item.Description;
			this.cost = item.cost;
		}

		public Item(Equipment equip)
		{
			this.Name = equip.Name;
			this.Description = equip.Description;
			this.cost = equip.cost;
		}
		public Item(Consumable item)
		{
			this.Name = item.Name;
			this.Description = item.Description;
			this.cost = item.cost;
		}

		public string Name;
		public string Description;
		public int cost;
	}

	public class Equipment : Item
	{


		public Equipment(Consumable cons)
		{
			this.Name = cons.Name;
			this.Description = cons.Description;
			this.cost = cons.cost;

		}
		public Equipment(Item item)
		{
			this.Name = item.Name;
			this.Description = item.Description;
			this.cost = item.cost;
		}
		public enum Slots
		{
			Head,
			Chest,
			OneHand,
			TwoHand
		}

		public Slots slot = Slots.Head;
		public List<int> forClass = new List<int>();
		public int level = 0;
		public Stats stats = new Stats();
		public int Armor = 0;
	}

	public class Consumable : Item
	{
		public Consumable(Equipment equip)
		{
			this.Name = equip.Name;
			this.Description = equip.Description;
			this.cost = equip.cost;
		}
		public Consumable(Item item)
		{
			this.Name = item.Name;
			this.Description = item.Description;
			this.cost = item.cost;
		}

		public Stat stat = Stat.Hp;
		public int value = 0;
		public int turn = 0;
		public bool isCuring;
		public Effect.Status cures = Effect.Status.Poison;

		public UseOn usableOn;

		public enum UseOn
		{
			Ally,
			DeadAlly,
			Enemy
		}

		public enum Stat
		{
			MaxHp,
			MaxMp,
			Hp,
			Mp,
			Str,
			Int,
			Lck,
			xp,
		}
	}

}
