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

        public static bool CheckURLValid(string source) => Uri.TryCreate(source, UriKind.Absolute, out Uri uriResult) && uriResult.Scheme == Uri.UriSchemeHttps;

        public static List<LavaTrack> queue = new List<LavaTrack>();

        [Command("baglan", RunMode = RunMode.Async)]
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
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("oynat",RunMode= RunMode.Async)]
        public async Task PlayAsync([Remainder] string query)
        {

            var voiceState = Context.User as IVoiceState;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("Bir Ses Kanalına Bağlı Olmak Zorundasınız!");
                return;
            }
            if (!_lavaNode.HasPlayer(Context.Guild))
            {
                await _lavaNode.JoinAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
            }

            var player = _lavaNode.GetPlayer(Context.Guild);
            if (voiceState.VoiceChannel != player.VoiceChannel)
            {
                await ReplyAsync("Benimle Aynı Ses Kanalında Bulunman Gerekiyor!");
                return;
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                await ReplyAsync("Oynatmak istediğiniz parçayı tanımlayınız!");
                return;
            }

            if (!_lavaNode.HasPlayer(Context.Guild))
            {
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

        [Command("karistir", RunMode = RunMode.Async)]
        public async Task ShuffleTrackAsync()
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
            var queue = player.Queue.ToList();
            var queueCount = player.Queue.Count;

            if (queueCount == 0)
            {
                await Context.Channel.SendMessageAsync("Kuyrukta herhangi bir parça yok!");
                return;
            }
            else if (queueCount == 1)
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

        [Command("duraklat", RunMode = RunMode.Async)]
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

        [Command("kuyruk", RunMode = RunMode.Async)]
        public async Task QueueList()
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
            var trackTitles = new List<Tuple<int, string>>();
            queue = player.Queue.ToList();
            var queueCount = player.Queue.Count;

            if (queueCount == 0)
            {
                await Context.Channel.SendMessageAsync("Kuyrukta herhangi bir parça yok!");
                return;
            }
            int pos = 1;
            foreach (var track in queue)
            {

                trackTitles.Add(new Tuple<int, string>(pos, track.Title));
                pos = pos + 1;
            }
            var builder = new EmbedBuilder()
            .WithTitle("Kuyruktaki Parçalar")
            .AddField("Kuyruk", string.Join("\n", trackTitles.Select(t => string.Format("{0} - {1}", t.Item1, t.Item2))), false);


            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);


        }

        [Command("atla", RunMode = RunMode.Async)]
        public async Task SkipMusicAsync(int amount = 1)
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

            for(int i = 0; i < amount; i++)
            {
                await player.SkipAsync();
            }
            
            await ReplyAsync("Parça Atlandı!");
        }

        [Command("devam-et", RunMode = RunMode.Async)]
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

        [Command("siktir-git", RunMode = RunMode.Async)]
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

        [Command("durdur", RunMode = RunMode.Async)]
        public async Task StopTrackAsync()
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

        [Command("yer-degistir", RunMode = RunMode.Async)]
        [Summary("kuyruk degistir")]
        public async Task TrackMoveAsync(int toBeChanged, int modified)
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

            List<LavaTrack> queue = player.Queue.ToList();
            var queueCount = player.Queue.Count;
            if (queueCount < 2)
            {
                await Context.Channel.SendMessageAsync("Yerlerini değiştirebiliceğim parçalar bulunmamakta!");
                return;
            }
            var oldQueue = queue[toBeChanged - 1];
            queue[toBeChanged - 1] = queue[modified - 1];
            queue[modified - 1] = oldQueue;

            player.Queue.RemoveRange(0, queueCount);


            foreach (var track in queue)
            {
                player.Queue.Enqueue(track);
            }

            await Context.Channel.SendMessageAsync($"{oldQueue.Title} {modified}. Sıraya Taşındı!");
        }

        [Command("parca-sil",RunMode = RunMode.Async)]
        [Summary("kuyruktan-sil")]
        public async Task DeleteTrackFromQueueAynsc(int toBeDeleted)
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

            List<LavaTrack> queue = player.Queue.ToList();
            var queueCount = player.Queue.Count;
            if (queueCount < 1)
            {
                await Context.Channel.SendMessageAsync("Kuyrukta Silebiliceğim bir parça yok!");
                return;
            }
            var tobedeletedtrack = queue[toBeDeleted - 1];
            await Context.Channel.SendMessageAsync($"{tobedeletedtrack.Title} Kuyruktan Silindi!");
            player.Queue.Remove(tobedeletedtrack);

        }
    }
}
