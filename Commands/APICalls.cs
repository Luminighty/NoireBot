using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Discord;
using Discord.Commands;
using System.Threading;
using System.Net;
using System.IO;

namespace NoireBot
{
	public class APICalls : ModuleBase
	{
		
		[Command("strawpoll")]
		[Alias("poll")]
		public async Task createPoll([Remainder]string search = "")
		{
			string Usage = "Usage: `>strawpoll {Title} | {Option 1} | {Option 2} | ...`";
			string[] args = search.Split('|');
			if (search == "" || args.Length < 2)
			{
				await Context.Channel.SendMessageAsync(Usage);
				return;
			}

			List<string> options = new List<string>();

			for(int i = 1; i < args.Length; i++)
			{
				string arg = args[i];
				if (arg[0] == ' ')
					arg.Remove(0, 1);
				if (arg[arg.Length - 1] == ' ')
					arg.Remove(arg.Length - 1);
				options.Add(args[i]);
			}

			StrawpollPOST post = new StrawpollPOST(args[0], options.ToArray(), false);
			var req = (HttpWebRequest)WebRequest.Create("https://strawpoll.me/api/v2/polls");

			req.ContentType = "application/json";
			req.Method = "POST";

			using (StreamWriter stream = new StreamWriter(req.GetRequestStream()))
			{
				string json = "\"title\": \"" + post.title + "\", \"options\": [ ";

				for (int i = 0; i < post.options.Length - 1; i++)
					json += "\"" + post.options[i] + "\", ";

				json += "\"" + post.options[post.options.Length - 1] + "\" ], ";
				json += "\"multi\": " + post.multi.ToString().ToLower() + " }";

				await Program.Log(LogSeverity.Debug, "API", json);

				stream.Write(json);
				stream.Close();
			}

			var response = (HttpWebResponse)req.GetResponse();
			using (StreamReader reader = new StreamReader(response.GetResponseStream()))
			{
				await Program.Log(LogSeverity.Debug, "API", reader.ReadToEnd());
			}

		}

		class StrawpollPOST
		{
			public string title = "";
			public string[] options;
			public bool multi = false;

			public StrawpollPOST(string _title, string[] _options, bool _multi)
			{
				this.title = _title;
				this.options = _options;
				this.multi = _multi;
			}

		}
		class StarpollPOSTResp
		{
			public int id;
			public string title;
			public string[] options;
			public bool multi = false;
			public string dupcheck;
			public bool captcha;
		}


	}
}
