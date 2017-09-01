using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoireBot.Rpg
{
	public class Skill
	{
		public static List<Skill> database = new List<Skill>();

		public static int GetSkillId(string name)
		{
			for (int i = 0; i < database.Count; i++)
				if (database[i].Name == name)
					return i;
			return -1;
		}

		public static int GetSkillId(Skill s)
		{
			return GetSkillId(s.Name);
		}

		public static void LoadSkills(string file)
		{
			StreamReader reader = new StreamReader(file);
			while (reader.Peek() > -1)
				database.Add(ReadSkills(ref reader));
			reader.Close();
			reader.Dispose();
		}

		public static Skill ReadSkills(ref StreamReader reader)
		{
			Skill s = new Skill();
			s.Name = reader.ReadLine();
			s.Desc = reader.ReadLine();
			s.level = Convert.ToInt32(reader.ReadLine());
			s.MpCost = Convert.ToInt32(reader.ReadLine());
			s.value = Convert.ToInt32(reader.ReadLine());
			s.isAoE = Convert.ToBoolean(reader.ReadLine());
			s.effect.name = reader.ReadLine();
			s.effect.turn = Convert.ToInt32(reader.ReadLine());
			s.effect.EffectValue = Convert.ToInt32(reader.ReadLine());
			s.effect.effect = (Effect.Status)Convert.ToInt32(reader.ReadLine());
			s.effect.statChange = (Consumable.Stat)Convert.ToInt32(reader.ReadLine());
			s.effect.changeValue = Convert.ToInt32(reader.ReadLine());
			s.useOn = (UseOn)Convert.ToInt32(reader.ReadLine());
			return s;
		}

		public Skill()
		{
			this.Name = "";
		}

		public Skill(string _name, string _desc, int _mpcost, int _value, bool _isAoe, int _level, Effect effect)
		{
			this.Name = _name;
			this.Desc = _desc;
			this.MpCost = _mpcost;
			this.value = _value;
			this.isAoE = _isAoe;
			this.level = _level;
			this.effect = new Effect(effect);
		}

		public Skill(Skill copy)
		{
			this.level = copy.level;
			this.Name = copy.Name;
			this.Desc = copy.Desc;
			this.MpCost = copy.MpCost;
			this.value = copy.value;
			this.isAoE = copy.isAoE;
			this.effect = new Effect(copy.effect);
			this.useOn = copy.useOn;
		}

		public Skill(string name, int damage)
		{
			this.Name = name;
			this.value = damage;
		}

		public int level = 0;
		public string Name = "";
		public string Desc = "";
		public int MpCost = 0;
		public int value = 0;
		public bool isAoE = false;
		public Effect effect = new Effect();
		public UseOn useOn = UseOn.Enemy;
	}

	public class Effect
	{

		public Effect()
		{
			this.name = "";
		}

		public Effect(string _name, int _turn, int _EffectValue, Status _effect, Consumable.Stat _statChange, int _changeValue)
		{
			this.name = _name;
			this.turn = _turn;
			this.EffectValue = _EffectValue;
			this.effect = _effect;
			this.statChange = _statChange;
			this.changeValue = _changeValue;
		}

		public Effect(Effect copy)
		{
			this.name = copy.name;
			this.turn = copy.turn;
			this.EffectValue = copy.EffectValue;
			this.effect = copy.effect;
			this.statChange = copy.statChange;
			this.changeValue = copy.changeValue;

		}

		public string name;
		public int turn;
		public int EffectValue;
		public Status effect;

		public Consumable.Stat statChange;
		public int changeValue;

		public enum Status
		{
			None,
			Sleep,
			Poison,
		}

	}

}
