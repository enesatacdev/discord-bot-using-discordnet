using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CabbariyeBot.Services
{
    public class CommandHandler
    {
        public static IServiceProvider _provider;
        public static DiscordSocketClient _discord;
        public static CommandService _commands;
        public static IConfigurationRoot _config;
        public CommandHandler(DiscordSocketClient discord, CommandService commands, IConfigurationRoot config, IServiceProvider provider)
        {
            _provider = provider;
            _config = config;
            _discord = discord;
            _commands = commands;

            _discord.Ready += OnReady;
            _discord.MessageReceived += OnMessageReceived;
            _discord.JoinedGuild += OnGuildJoined;
        }

        private async Task OnGuildJoined(SocketGuild arg)
        {
            await arg.DefaultChannel.SendMessageAsync("Sunucunuza Beni Davet Ettiğiniz için Teşekkür Ederim! Tam Enayiymişsiniz");
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

                    await context.Channel.SendMessageAsync($"Hata ile karşılaşıldı, karşılaşılan hata : \n {reason}");
                    Console.WriteLine(reason);
                }
            }
        }

        private Task OnReady()
        {
            Console.WriteLine($"{_discord.CurrentUser.Username}#{_discord.CurrentUser.Discriminator} Olarak Bağlandım!");
            return Task.CompletedTask;

        }
    }
}
