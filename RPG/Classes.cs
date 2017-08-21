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
    public class Dungeon : Location
    {
    }

    public class Effect
    {
        public Status effect;
        public string name;
        public int turn;
        public int value;

        public Effect()
        {
            this.name = "";
        }

        public Effect(Effect copy)
        {
            this.name = copy.name;
            this.turn = copy.turn;
            this.value = copy.value;
            this.effect = copy.effect;
        }

        public Effect(string _name, int _turn, int _value, Status _effect)
        {
            this.name = _name;
            this.turn = _turn;
            this.value = _value;
            this.effect = _effect;
        }

        public enum Status
        {
            Sleep,
            Poison,
            Weakness
        }
    }

    public class Item
    {
        public int Id;
        public string Name;
    }

    public class Location
    {
        public int Id;
        public string Name;
    }

    public class Monster
    {
        public int asleep;
        public string Name;
        public Point position;
        public Stats stats;

        public Monster(Monster copy)
        {
            this.Name = copy.Name;
            this.stats = new Stats(copy.stats);
        }

        public Monster(string _name, Stats _stats)
        {
            this.Name = _name;
            this.stats = _stats;
        }
    }

    public class Player
    {
        public bool isProcessing;
        public bool isReady;
        public string nickName;
        public List<Skill> skills;
        public Stats stats;
        public IUser user;

        public Player(IUser _user)
        {
            this.isReady = true;
            this.isProcessing = false;
            this.skills = new List<Skill>();
            this.stats = new Stats(30, 20, 5, 0, 0, 0, 0);
            this.skills.Add(new Skill("Fireball", "Shoots a fireball", 5, 15, false, new Effect()));
            this.user = _user;
            this.nickName = (_user.Username.Length > 8) ? _user.Username.Substring(0, 8) : _user.Username;
        }

        public Player(Player copy)
        {
            this.isReady = true;
            this.isProcessing = false;
            this.skills = new List<Skill>();
            this.user = copy.user;
            this.nickName = copy.nickName;
            this.stats = new Stats(copy.stats);
            this.skills = copy.skills;
        }

        public bool HasSkill(string name, out Skill skill)
        {
            skill = new Skill("null", -1);
            foreach (Skill skill2 in this.skills)
            {
                if (skill2.Name.ToLower() == name.ToLower())
                {
                    skill = new Skill(skill2);
                    return true;
                }
            }
            return false;
        }
    }

    public class Result
    {
        public bool isFile;
        public bool isPrivate;
        public bool StartEnemyTurn;
        public string Text;

        public Result()
        {
            this.Text = "";
            this.isFile = true;
            this.StartEnemyTurn = false;
            this.isPrivate = false;
            this.Text = "";
        }

        public Result(string _text, bool _isPrivate = false, bool _startEnemyTurn = false)
        {
            this.Text = "";
            this.isFile = true;
            this.StartEnemyTurn = false;
            this.isPrivate = false;
            this.Text = _text;
            this.isPrivate = _isPrivate;
            this.StartEnemyTurn = _startEnemyTurn;
        }
    }

    public class Skill
    {
        public string Desc;
        public Effect effect;
        public bool isAoe;
        public int MpCost;
        public string Name;
        public int value;

        public Skill()
        {
            this.Name = "";
        }

        public Skill(Skill copy)
        {
            this.Name = copy.Name;
            this.Desc = copy.Desc;
            this.MpCost = copy.MpCost;
            this.value = copy.value;
            this.isAoe = copy.isAoe;
            this.effect = new Effect(copy.effect);
        }

        public Skill(string _name, int _value)
        {
            this.Name = _name;
            this.value = _value;
        }

        public Skill(string _name, string _desc, int _mpcost, int _value, bool _isAoe, Effect effect)
        {
            this.Name = _name;
            this.Desc = _desc;
            this.MpCost = _mpcost;
            this.value = _value;
            this.isAoe = _isAoe;
            this.effect = new Effect(effect);
        }
    }

    public class Stats
    {
        public int gold;
        public int Hp;
        public int MaxHp;
        public int MaxMp;
        public int Mp;
        public int Str;
        public int xp;

        public Stats(Stats copy)
        {
            this.gold = copy.gold;
            this.Hp = copy.Hp;
            this.Mp = copy.Mp;
            this.MaxHp = copy.MaxHp;
            this.MaxMp = copy.MaxMp;
            this.Str = copy.Str;
            this.xp = copy.xp;
        }

        public Stats(int _hp, int _mp, int _str, int _xp, int _gold, int _maxHp = 0, int _maxMp = 0)
        {
            this.gold = _gold;
            this.Hp = _hp;
            this.Mp = _mp;
            this.MaxHp = (_maxHp == 0) ? _hp : _maxHp;
            this.MaxMp = (_maxMp == 0) ? _mp : _maxMp;
            this.Str = _str;
            this.xp = _xp;
        }

        public int Level
        {
            get
            {
                int num = 1;
                int num2 = 50;
                int xp = this.xp;
                while (num2 < xp)
                {
                    xp -= num2;
                    num2 = Convert.ToInt32((double)(num2 * 1.2000000476837158));
                    num++;
                }
                return num;
            }
        }
    }

    public class Town : Location
    {
        public List<Item> shopItems;
    }
}
