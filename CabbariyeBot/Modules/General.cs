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


        [Command("sunucu")]
        public async Task Server()
        {
            var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithDescription("Sunucu hakkında bilgiler!")
                .WithTitle($"{Context.Guild.Name}")
                .WithColor(new Color(33, 176, 252))
                .AddField("Kullanıcılar", (Context.Guild as SocketGuild).MemberCount, true)
                .AddField("Online Kullanıcılar", (Context.Guild as SocketGuild).Users.Where(x => x.Status != UserStatus.Offline).Count(), true)
                .AddField("Oluşturulma Tarihi", Context.Guild.CreatedAt.ToString("dd/MM/yyyy"), true);

            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }

        
    }
}
