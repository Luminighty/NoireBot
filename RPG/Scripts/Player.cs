using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoireBot.Rpg
{
	public class Player
	{
		public bool isProcessing = false;
		public bool isReady = true;
		public string nickName = "";
		public List<int> items = new List<int>();
		public Stats stats = new Stats();
		public IUser user;
		public int[] equipment = new int[4] { -1, -1, -1, -1 };

		public int currentClass;
		public List<int> classLevels = new List<int>();

		public Player(IUser _user)
		{
			this.isReady = true;
			this.isProcessing = false;
			this.stats = new Stats(30, 20, 5, 0, 0, 0, 0);
			//this.skills.Add(new Skill("Fireball", "Shoots a fireball", 5, 15, false, new Effect()));
			this.user = _user;
			this.nickName = (_user.Username.Length > 8) ? _user.Username.Substring(0, 8) : _user.Username;
			this.classLevels = new List<int>();
			this.currentClass = 0;
			if (Class.database == null || Class.database.Count == 0)
				Program.Log(new LogMessage(LogSeverity.Critical, "RPG", "A player was created before the classes were loaded! (The player doesn't have any class level!)"));
			foreach (Class c in Class.database)
				classLevels.Add(0);
		}

		public Player(Player copy)
		{
			this.isReady = true;
			this.isProcessing = false;
			this.currentClass = copy.currentClass;
			this.user = copy.user;
			this.nickName = copy.nickName;
			this.stats = new Stats(copy.stats);
			this.classLevels = copy.classLevels;
		}

		public bool HasSkill(string name, out Skill skill)
		{
			skill = new Skill();
			foreach (int s in Class.database[currentClass].skills)
			{
				if (Skill.database[s].Name.ToLower() == name.ToLower() && Skill.database[s].level <= this.classLevels[currentClass])
				{
					skill = new Skill(Skill.database[s]);
					return true;
				}
			}
			return false;
		}
	}

}
