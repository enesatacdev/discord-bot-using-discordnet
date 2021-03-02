using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            var sohbetMesaji = await Context.Channel.SendMessageAsync("Silinen Mesaj Sayısı : " + messages.Count());
            await Task.Delay(3500);
            await sohbetMesaji.DeleteAsync();
        }
    }
}
