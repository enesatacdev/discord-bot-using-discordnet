using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;

namespace CabbariyeBot.Services
{
    public class CommandHandler
    {
        public static IServiceProvider _provider;
        public static DiscordSocketClient _discord;
        public static CommandService _commands;
        public static IConfigurationRoot _config;
        public static LavaNode _lavaNode;
        public CommandHandler(DiscordSocketClient discord, CommandService commands, IConfigurationRoot config, IServiceProvider provider, LavaNode lavanode)
        {
            _provider = provider;
            _config = config;
            _discord = discord;
            _commands = commands;
            _lavaNode = lavanode;
            _discord.Ready += OnReadyAsync;
            _discord.MessageReceived += OnMessageReceived;
            _discord.JoinedGuild += OnGuildJoined;
            _lavaNode.OnTrackEnded += OnTrackEnded;
        }

        private async Task OnTrackEnded(TrackEndedEventArgs args)
        {
            if (!args.Reason.ShouldPlayNext())
            {
                return;
            }

            var player = args.Player;
            if (!player.Queue.TryDequeue(out var queueable))
            {
                await player.TextChannel.SendMessageAsync("Kuyruktaki Parçalar Bitti!");
                return;
            }

            if (!(queueable is LavaTrack track))
            {
                return;
            }

            var builder = new EmbedBuilder()
                    .WithTitle($"Sıradaki Parçaya Geçiliyor")
                    .WithThumbnailUrl(track.FetchArtworkAsync().ToString())
                    .AddField("Parça Adı", track.Title, false)
                    .AddField("Parça Uzunluğu", track.Duration.ToString(), false)
                    .WithColor(new Color(33, 176, 252));
            var embed = builder.Build();
            var message = await args.Player.TextChannel.SendMessageAsync(null, false, embed);

            await args.Player.PlayAsync(track);

           
        }

        private async Task OnGuildJoined(SocketGuild arg)
        {
            var builder = new EmbedBuilder()
                .WithTitle("Merhaba " + arg.Name)
                .WithDescription("Beni Sunucunuza Davet Ettiğiniz İçin Teşekkür Ederim!")
                .WithColor(new Color(33, 175, 255));
            var embed = builder.Build();
            await arg.DefaultChannel.SendMessageAsync(null,false,embed);
        }
     
        private async Task OnReadyAsync()
        {
            if (!_lavaNode.IsConnected)
            {
                await _lavaNode.ConnectAsync();
            }
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {

            if (!(arg is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            int pos = 0;

            var msg = arg as SocketUserMessage;

            if (msg.Author.IsBot) return;
            var context = new SocketCommandContext(_discord, msg);
            

            if(msg.HasStringPrefix(_config["prefix"],ref pos) || msg.HasMentionPrefix(_discord.CurrentUser, ref pos))
            {
                var result = await _commands.ExecuteAsync(context, pos, _provider);

                if (!result.IsSuccess)
                {
                    var reason = result.Error;
                    var builder = new EmbedBuilder()
                        .WithTitle("Hata İle Karşılaşıldı")
                        .WithColor(new Color(33,175,255))
                        .AddField($"Karışılaşılan hata : {reason}", false);
                    var embed = builder.Build();
                    await context.Channel.SendMessageAsync(null,false,embed);
                    Console.WriteLine(reason);
                }
            }
        }

       
    }
}
