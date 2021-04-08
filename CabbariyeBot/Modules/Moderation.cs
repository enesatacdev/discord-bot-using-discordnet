using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CabbariyeBot.Common;

namespace CabbariyeBot.Modules
{
    public class Moderation : ModuleBase
    {
        [Command("sohbeti-temizle")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        public async Task SohbetiTemizle(int silinecekMesajSayisi)
        {
            var messages = await Context.Channel.GetMessagesAsync(silinecekMesajSayisi + 1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
            var builder = new EmbedBuilder()
                .WithTitle("Sohbet Temizleyici")
                .AddField(messages.Count().ToString() ," mesaj silindi!", false)
                .WithColor(new Color(33, 176, 252));
            var embed = builder.Build();
            var sohbetMesaji = await Context.Channel.SendMessageAsync(null,false,embed);
            await Task.Delay(4000);
            await sohbetMesaji.DeleteAsync();
        }

        

    }

}
