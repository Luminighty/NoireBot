using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoireBot.Rpg
{
	public class Group
	{
		public string background;
		public ulong id;
		public bool inFight = false;
		public List<Monster> monsters = new List<Monster>();
		public PlaceType placeType = PlaceType.Town;
		public Dictionary<ulong, Player> players = new Dictionary<ulong, Player>();

		public Group(IUser user)
		{
			this.players.Add(user.Id, new Player(user));
			this.id = user.Id;
		}

		public void CreateGroup()
		{
			this.monsters.Clear();
			int num = new Random().Next(1, this.Level);
			num = Math.Min(RPG.MonsterPositions.Length, num);
			for (int i = 0; i < num; i++)
			{
				this.monsters.Add(new Monster(Monster.database[0]));
			}
			this.monsters.Sort((Comparison<Monster>)((a, b) => a.stats.Str.CompareTo(b.stats.Str)));
			for (int j = 0; j < this.monsters.Count; j++)
			{
				this.monsters[j].position = RPG.MonsterPositions[this.monsters.Count - 1, j];
			}
			this.inFight = true;
		}

		public bool HasEnemy
		{
			get
			{
				foreach (Monster monster in this.monsters)
				{
					if (monster.stats.Hp > 0)
					{
						return true;
					}
				}
				return false;
			}
		}

		public int Level
		{
			get
			{
				int num = 0;
				foreach (Player player in this.players.Values)
				{
					num += player.stats.Level;
				}
				return Convert.ToInt32(Math.Round((double)(num / this.players.Values.Count), 2));
			}
		}

		public enum PlaceType
		{
			Dungeon,
			Town
		}
	}

}
