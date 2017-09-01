using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Discord;
using Discord.Commands;
using System.Threading;
using System.Net;
using System.IO;

namespace NoireBot
{
	public class APICalls : ModuleBase
	{
		[Command("anime")]
		public async Task getAnime([Remainder]string search = "")
		{
			await Context.Channel.SendMessageAsync("soon");
		}


	}
}
