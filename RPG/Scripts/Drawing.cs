using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using AnimatedGif;
using Discord;
using System.Drawing.Drawing2D;
using System.Net;
using System.IO;

namespace NoireBot.Rpg
{

	class Drawing
	{

		static string path_Resources
		{
			get { return RPG.path_Resources; }
		}

		public static void DrawColumn(ref Graphics source, Pen color, int Size, PointF position)
		{
			PointF tf = position;
			tf.Y += Size;
			source.DrawLine(color, position, tf);
		}

		public static void DrawMonster(ref Graphics source, Monster monster, int id, string anim = "Idle", int frame = 0, bool drawId = true, float OffsetY = 0f, float OffsetX = 0f)
		{
			PointF point = new PointF(monster.position.X + OffsetX, monster.position.Y + OffsetY);
			anim = (frame < 10) ? (anim + "0" + frame.ToString()) : (anim + frame.ToString());
			anim = anim + ".png";
			string[] textArray1 = new string[] { path_Resources, "Monsters/", monster.Name, "/", anim };
			source.DrawImage(System.Drawing.Image.FromFile(string.Concat(textArray1)), point);
			if (drawId)
			{
				DrawText(ref source, id.ToString(), point, Brushes.White, 0x12, 0, -10);
			}
		}

		public static void DrawPlayer(ref Graphics source, ref Player player, Point position)
		{
			Font font = new Font("Consolas", 18f);
			source.DrawString(player.user.Username, font, Brushes.White, new PointF(165f, 336f));
			int num = Convert.ToInt32(Math.Round((double)(100.0 * (((double)player.stats.Hp) / ((double)player.stats.MaxHp)))));
			int num2 = Convert.ToInt32(Math.Round((double)(100.0 * (((double)player.stats.Mp) / ((double)player.stats.MaxMp)))));
			for (int i = 0; i < num; i++)
			{
				System.Drawing.Color color = System.Drawing.Color.FromArgb(0x1d, 0xf3, 0);
				PointF tf = new PointF((float)(0x11f + i), (float)(position.Y + 4));
				DrawColumn(ref source, new Pen(color), 12, tf);
			}
			for (int j = 0; j < num2; j++)
			{
				System.Drawing.Color color2 = System.Drawing.Color.FromArgb(0, 210, 0xff);
				PointF tf2 = new PointF((float)(0x18d + j), (float)(position.Y + 4));
				DrawColumn(ref source, new Pen(color2), 12, tf2);
			}
			source.DrawImage(System.Drawing.Image.FromFile(path_Resources + "Bars.png"), new PointF(282f, (float)position.Y));
			source.DrawString(player.nickName, font, Brushes.Black, new PointF((float)(position.X + 3), (float)(position.Y + 3)));
			source.DrawString(player.nickName, font, Brushes.White, (PointF)position);
			Font font2 = new Font("Consolas", 16f, FontStyle.Bold);
			GraphicsPath path = new GraphicsPath();
			path.AddString(player.stats.Hp.ToString(), font.FontFamily, 1, 14f, new PointF(287f, (float)(position.Y + 2)), StringFormat.GenericDefault);
			source.DrawPath(new Pen(Brushes.Black, 3f), path);
			source.FillPath(Brushes.White, path);
			GraphicsPath path2 = new GraphicsPath();
			path2.AddString(player.stats.Mp.ToString(), font.FontFamily, 1, 14f, new PointF(397f, (float)(position.Y + 2)), StringFormat.GenericDefault);
			source.DrawPath(new Pen(Brushes.Black, 3f), path2);
			source.FillPath(Brushes.White, path2);
		}

		public static void DrawText(ref Graphics source, string Text, PointF pos, Brush color, int size = 5, int OffsetX = 0, int OffsetY = 0)
		{
			GraphicsPath path = new GraphicsPath();
			Point origin = new Point(Convert.ToInt32(pos.X), Convert.ToInt32(pos.Y));
			origin.Y += OffsetY;
			origin.X += OffsetX + 40;
			path.AddString(Text, FontFamily.GenericSansSerif, 1, (float)size, origin, new StringFormat());
			Pen pen = new Pen(Brushes.Black, (float)(size / 6));
			source.SmoothingMode = SmoothingMode.HighQuality;
			source.DrawPath(pen, path);
			source.FillPath(color, path);
		}

		public static Bitmap GetBattle(Group group, IUser user, bool drawMonsters = true, int frame = 0, bool drawIDs = true)
		{
			System.Drawing.Image image;
			Bitmap bitmap = new Bitmap(path_Resources + "Background.png");
			Graphics source = Graphics.FromImage(bitmap);
			Font font = new Font("Consolas", 18f);
			Player player = group.players[user.Id];
			if (drawMonsters)
			{
				for (int j = 0; j < group.monsters.Count; j++)
				{
					if (group.monsters[j].stats.Hp > 0)
					{
						DrawMonster(ref source, group.monsters[j], j, "Idle", frame, drawIDs, 0f, 0f);
					}
				}
			}
			source.DrawImage(System.Drawing.Image.FromFile(path_Resources + "Layout.png"), new PointF(0f, 0f));
			using (WebClient client = new WebClient())
			{
				image = System.Drawing.Image.FromStream(new MemoryStream(client.DownloadData(new Uri(user.GetAvatarUrl(ImageFormat.Png, 0x80)))));
			}
			source.DrawImage(image, new Rectangle(10, 0xd5, 0x52, 0x52));
			for (int i = 0; i < group.players.Count; i++)
			{
				Player player2 = group.players[group.players.Keys.ElementAt<ulong>(i)];
				DrawPlayer(ref source, ref player2, new Point(187, 208 + (i * 22)));
			}
			return bitmap;
		}
		
	}
}
