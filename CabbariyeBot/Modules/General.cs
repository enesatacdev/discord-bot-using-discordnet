using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace CabbariyeBot.Modules
{
    public class General : ModuleBase
    {
        [Command("heythere")]
        public async Task Selam()
        {
            await Context.Channel.SendMessageAsync("Generol Kenobi :)");
        }

        [Command("bilgi")]
        public async Task Bilgi(SocketGuildUser user = null)
        {
            if(user == null)
            {
                var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                .WithDescription("Bu mesajı okuyorsan enayisindir!")
                .AddField("Kullanıcı Adı", (Context.User as SocketGuildUser).Username, true)
                .AddField("Katılma Tarihi", (Context.User as SocketGuildUser).JoinedAt.Value.ToString("dd/mm/yyyy"), true)
                .AddField("Roller", string.Join(" ", (Context.User as SocketGuildUser).Roles.Select(x => x.Mention)))
                .WithColor(new Color(33, 176, 252))
                .WithCurrentTimestamp();
                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(null, false, embed);
            }
            else
            {
                var builder = new EmbedBuilder()
                .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                .WithDescription("Bu mesajı okuyorsan enayisindir!")
                .AddField("Kullanıcı Adı", user.Username, true)
                .AddField("Katılma Tarihi", user.JoinedAt.Value.ToString("dd/mm/yyyy"), true)
                .AddField("Roller", string.Join(" ", user.Roles.Select(x => x.Mention)))
                .WithColor(new Color(33, 176, 252))
                .WithCurrentTimestamp();
                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(null, false, embed);
            }
            
        }

        [Command("server")]
        public async Task Server()
        {
            var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithDescription("Server hakkında bilgiler!")
                .WithTitle($"{Context.Guild.Name}")
                .WithColor(new Color(33, 176, 252))
                .AddField("Kullanıcılar", (Context.Guild as SocketGuild).MemberCount, true)
                .AddField("Online Kullanıcılar", (Context.Guild as SocketGuild).Users.Where(x => x.Status != UserStatus.Offline).Count(), true)
                .AddField("Oluşturulma Tarihi", Context.Guild.CreatedAt.ToString("dd/mm/yyyy"), true);

            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }

        
    }
}
