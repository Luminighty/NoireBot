using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using System.Threading;

namespace NoireBot
{
    public class Music : ModuleBase<ICommandContext>
    {
        // Scroll down further for the AudioService.
        // Like, way down.
        // Hit 'End' on your keyboard if you still can't find it.
        private readonly AudioService _service;

        public Music(AudioService service)
        {
            _service = service;
        }

        // You *MUST* mark these commands with 'RunMode.Async'
        // otherwise the bot will not respond until the Task times out.
        [Command("join", RunMode = RunMode.Async)]
        public async Task JoinCmd()
        {
            if (_service == null)
                await Program.Log(new LogMessage(LogSeverity.Debug, "Music", "Service not found!"));
            await _service.JoinAudio(Context.Guild, (Context.User as IVoiceState).VoiceChannel);
        }

        // Remember to add preconditions to your commands,
        // this is merely the minimal amount necessary.
        // Adding more commands of your own is also encouraged.
        [Command("leave", RunMode = RunMode.Async)]
        public async Task LeaveCmd()
        {
            await _service.LeaveAudio(Context.Guild);
        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task PlayCmd([Remainder] string song)
        {
            await _service.SendAudioAsync(Context.Guild, Context.Channel, song);
        }

        [Command("stop", RunMode = RunMode.Async)]
        public async Task StopCmd()
        {
            _service.StopAudio(Context.Guild);
            await ReplyAsync("Music Stopped!");
        }

    }

    public class AudioService
    {
        private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();

        private ConcurrentDictionary<ulong, CancellationTokenSource> cancelTokens = new ConcurrentDictionary<ulong, CancellationTokenSource>();

        public async Task JoinAudio(IGuild guild, IVoiceChannel target)
        {
            IAudioClient client;
            if (ConnectedChannels.TryGetValue(guild.Id, out client))
            {
                return;
            }
            if (target.Guild.Id != guild.Id)
            {
                return;
            }

            var audioClient = await target.ConnectAsync();

            if (ConnectedChannels.TryAdd(guild.Id, audioClient))
            {
                await Program.Log(new LogMessage(LogSeverity.Info,"Music", $"Connected to voice on {guild.Name}."));
            }
        }

        public async Task LeaveAudio(IGuild guild)
        {
            IAudioClient client;
            if (ConnectedChannels.TryRemove(guild.Id, out client))
            {
                await client.StopAsync();
                //await Log(LogSeverity.Info, $"Disconnected from voice on {guild.Name}.");
            }
        }

        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string path)
        {
            path = "../../Music/" + path;
            // Your task: Get a full path to the file if the value of 'path' is only a filename.
            if (!File.Exists(path))
            {
                await channel.SendMessageAsync("File does not exist.");
                return;
            }
            IAudioClient client;
            if (ConnectedChannels.TryGetValue(guild.Id, out client))
            {
                //await Log(LogSeverity.Debug, $"Starting playback of {path} in {guild.Name}");
                var output = CreateStream(path).StandardOutput.BaseStream;

                // You can change the bitrate of the outgoing stream with an additional argument to CreatePCMStream().
                // If not specified, the default bitrate is 96*1024.
                var stream = client.CreatePCMStream(AudioApplication.Music,960);
                var source = new CancellationTokenSource();
                if(cancelTokens.TryGetValue(guild.Id, out source)) {
                    //stop the music first!
                    source.Cancel(true);
                    cancelTokens.TryRemove(guild.Id, out source);
                }
                source = new CancellationTokenSource();
                if (cancelTokens.TryAdd(guild.Id, source)) {
                    try
                    {
                        await output.CopyToAsync(stream, 81920, source.Token);
                        await stream.FlushAsync().ConfigureAwait(false);
                    } catch(System.Exception exc)
                    {
                        await Program.Log(new LogMessage(LogSeverity.Info, "Music", "Music stopped in " + guild.Name));
                        return;
                    }
                }
            }
        }
        
        public void StopAudio(IGuild guild)
        {
            var source = new CancellationTokenSource();
            if(cancelTokens.TryGetValue(guild.Id, out source)) {
                source.Cancel(true);
                cancelTokens.TryRemove(guild.Id, out source);
            }

        }

        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }
    }



}
