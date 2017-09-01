using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoireBot.Rpg
{
	public class Stats
	{

		public static Stats ReadStats(ref StreamReader reader)
		{
			Stats o = new Stats();
			o.MaxHp = Convert.ToInt32(reader.ReadLine());
			o.MaxMp = Convert.ToInt32(reader.ReadLine());
			o.Hp = o.MaxHp;
			o.Mp = o.MaxMp;
			o.Str = Convert.ToInt32(reader.ReadLine());
			o.Int = Convert.ToInt32(reader.ReadLine());
			o.Lck = Convert.ToInt32(reader.ReadLine());
			o.gold = Convert.ToInt32(reader.ReadLine());
			o.xp = Convert.ToInt32(reader.ReadLine());
			return o;
		}

		public Stats()
		{
			this.MaxMp = 1;
			this.MaxHp = 1;
			this.Hp = 0;
			this.Mp = 0;
			this.Str = 0;
			this.Int = 0;
			this.Lck = 0;
			this.gold = 0;
			this.xp = 0;
		}

		public Stats(Stats copy)
		{
			this.MaxHp = copy.MaxHp;
			this.MaxMp = copy.MaxMp;
			this.gold = copy.gold;
			this.Hp = copy.Hp;
			this.Mp = copy.Mp;
			this.Lck = copy.Lck;
			this.Int = copy.Int;
			this.Str = copy.Str;
			this.xp = copy.xp;
		}

		public Stats(int _hp, int _mp, int _str, int _int, int _lck, int _gold, int _xp)
		{
			this.MaxHp = _hp;
			this.MaxMp = _mp;
			this.Hp = _hp;
			this.Mp = _mp;
			this.Str = _str;
			this.Int = _int;
			this.Lck = _lck;
			this.gold = _gold;
			this.xp = _xp;
		}

		public int Hp
		{
			get { return _hp; }
			set { _hp = Math.Min(MaxHp, value); }
		}
		public int Mp
		{
			get { return _mp; }
			set { _mp = Math.Min(MaxMp, value); }
		}
		private int _hp;
		private int _mp;
		public int MaxHp;
		public int MaxMp;

		public int Str;
		public int Int;
		public int Lck;

		public int gold;
		public int xp;
		public int Level
		{
			get
			{
				int level = 1;
				int nextLevel = 50;
				int remainXp = xp;
				while (nextLevel < remainXp)
				{
					remainXp -= nextLevel;
					nextLevel = Convert.ToInt32((double)nextLevel * 1.2f);
					level++;
				}
				return level;
			}
		}
	}

}
