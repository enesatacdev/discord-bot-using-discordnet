using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Victoria;
using Victoria.Enums;
using Victoria.EventArgs;

namespace CabbariyeBot.Modules
{

    public class Music : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _lavaNode;

        public Music(LavaNode lavaNode)
        {
            _lavaNode = lavaNode;
        }

        [Command("baglan")]
        public async Task JoinAsync()
        {
            if (_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("Şuanda başka bir kanala bağlıyım!");
                return;
            }

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("Bir ses kanalına bağlı olman gerekiyor!!");
                return;
            }

            try
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                await ReplyAsync($"{voiceState.VoiceChannel.Name} kanalına katıldım!");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        public static bool CheckURLValid(string source) => Uri.TryCreate(source, UriKind.Absolute, out Uri uriResult) && uriResult.Scheme == Uri.UriSchemeHttps;


        [Command("oynat")]
        public async Task PlayAsync([Remainder] string query)
        {
            

            if (string.IsNullOrWhiteSpace(query))
            {
                await ReplyAsync("Oynatmak istediğiniz parçayı tanımlayınız!");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                var voiceState = Context.User as IVoiceState;
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                await ReplyAsync($"{voiceState.VoiceChannel.Name} kanalına katıldım!");
            }

            var searchResponse = await _lavaNode.SearchYouTubeAsync(query);
            if (CheckURLValid(query) == true)
            {
                searchResponse = await _lavaNode.SearchAsync(query);
            }


            if (searchResponse.LoadStatus == LoadStatus.LoadFailed ||
                searchResponse.LoadStatus == LoadStatus.NoMatches)
            {
                await ReplyAsync($"Aradığınız `{query}` parçayı bulamadım! .");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);

            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {

                var track = searchResponse.Tracks[0];
                player.Queue.Enqueue(track);
                var builder = new EmbedBuilder()
                .WithThumbnailUrl(track.FetchArtworkAsync().ToString())
                .WithTitle("Sıraya Alınan Parça ")
                .AddField("Parça Adı", track.Title, false)
                .AddField("Parça Uzunluğu", track.Duration.ToString(), false)
                .WithColor(new Color(33, 176, 252));
                var embed = builder.Build();

                var message = await Context.Channel.SendMessageAsync(null, false, embed);

            }
            else
            {
                var track = searchResponse.Tracks[0];
                await player.PlayAsync(track);

                var builder = new EmbedBuilder()
                    .WithTitle($"Şuanda Çalınan Parça")
                    .WithThumbnailUrl(track.FetchArtworkAsync().ToString())
                    .AddField("Parça Adı", track.Title, false)
                    .AddField("Parça Uzunluğu", track.Duration.ToString(), false)
                    .WithColor(new Color(33, 176, 252));
                var embed = builder.Build();
                var message = await Context.Channel.SendMessageAsync(null, false, embed);

            }

        }
        [Command("loop")]
        public async Task LoopTrackAsync()
        {
            bool isLoop = false;
            var player = _lavaNode.GetPlayer(Context.Guild);
            var queue = player.Queue.ToList();
            var queueCount = player.Queue.Count;
            if(queueCount == 0)
            {
                await Context.Channel.SendMessageAsync("Tekrarlanıcak herhangi bir parça yok!");
                return;
            }
            var loopTrack = player.Queue.FirstOrDefault();
        }

        [Command("karistir")]
        public async Task ShuffleTrackAsync()
        {
            var player = _lavaNode.GetPlayer(Context.Guild);
            var queue = player.Queue.ToList();
            var queueCount = player.Queue.Count;

            if (queueCount == 0)
            {
                await Context.Channel.SendMessageAsync("Kuyrukta herhangi bir parça yok!");
                return;
            }
            else if(queueCount == 1)
            {
                await Context.Channel.SendMessageAsync("Kuyrukta bir parça olduğundan karıştıramıyorum!");
                return;
            }
            else
            {
                await Context.Channel.SendMessageAsync("Kuyruktaki parçalar karıştırıldı!");
                player.Queue.Shuffle();
                return;
            }
        }

        [Command("duraklat")]
        public async Task PauseMusicAsync()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("Bir Ses Kanalına Bağlı Olmak Zorundasınız!");
                return;
            }
            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("Bir Ses Kanalına Bağlı Değilim!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync("Benimle Aynı Ses Kanalında Bulunman Gerekiyor!");
                return;
            }
            if (player.PlayerState == PlayerState.Paused)
            {
                await ReplyAsync("Parça Zaten Durdurulmuş Durumda!");
                return;
            }

            await player.PauseAsync();
            await ReplyAsync("Parça Durduruldu");
        }

        [Command("kuyruk")]
        public async Task QueueList()
        {

            var trackTitles = new List<Tuple<int,string>>();
            var player = _lavaNode.GetPlayer(Context.Guild);
            var queue = player.Queue.ToList();
            var queueCount = player.Queue.Count;

            if (queueCount == 0)
            {
                await Context.Channel.SendMessageAsync("Kuyrukta herhangi bir parça yok!");
                return;
            }
            int pos = 1;
            foreach (var track in queue)
            {

                trackTitles.Add(new Tuple<int,string>(pos,track.Title));
                pos = pos + 1;
            }
            var builder = new EmbedBuilder()
            .WithTitle("Kuyruktaki Parçalar")
            .AddField("Kuyruk", string.Join("\n", trackTitles.Select(t => string.Format("{0} - {1}", t.Item1, t.Item2))), false);


            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);


        }

        [Command("atla")]
        public async Task SkipMusicAsync()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("Bir Ses Kanalına Bağlı Olmak Zorundasınız!");
                return;
            }
            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("Bir Ses Kanalına Bağlı Değilim!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync("Benimle Aynı Ses Kanalında Bulunman Gerekiyor!");
                return;
            }

            await player.SkipAsync();
            await ReplyAsync("Parça Atlandı!");
        }

        [Command("devam-et")]
        public async Task ResumeMusicAsync()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("Bir Ses Kanalına Bağlı Olmak Zorundasınız!");
                return;
            }
            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("Bir Ses Kanalına Bağlı Değilim!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync("Benimle Aynı Ses Kanalında Bulunman Gerekiyor!");
                return;
            }
            if (player.PlayerState != PlayerState.Paused)
            {
                await ReplyAsync("Parça Şuanda Devam Etmekte!");
                return;
            }
            else
            {
                await player.ResumeAsync();
                await ReplyAsync("Parça Devam Ettiriliyor");
            }


        }

        [Command("siktir-git")]
        public async Task SiktirGitAsync(IVoiceChannel channel = null)
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("Beni kovabilmek için benimle aynı ses kanalında olman gerekiyor!");
                return;
            }
            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("Beni kovabiliceğin bir ses kanalına bağlı değilim!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync("Beni kovabilmek için benimle aynı ses kanalında olman gerekiyor!");
                return;
            }

            await _lavaNode.LeaveAsync(voiceState.VoiceChannel);
            await ReplyAsync($"{voiceState.VoiceChannel.Name} kanalından Kovuldum!");

        }

        [Command("durdur")]
        public async Task StopMusicAsync()
        {
            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("Bir Ses Kanalına Bağlı Olmak Zorundasınız!");
                return;
            }
            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await ReplyAsync("Bir Ses Kanalına Bağlı Değilim!");
                return;
            }

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync("Benimle Aynı Ses Kanalında Bulunman Gerekiyor!");
                return;
            }
            if (player.PlayerState == PlayerState.Stopped)
            {
                await ReplyAsync("Parça Zaten Durdurulmuş Durumda!");
                return;
            }

            await player.PauseAsync();
            await ReplyAsync("Parça Kapatıldı!");
        }

    }
}
