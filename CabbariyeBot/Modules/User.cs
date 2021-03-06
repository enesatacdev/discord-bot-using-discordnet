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
    public class User : ModuleBase
    {
        public static async Task PostUserInfo(SocketGuildUser user, SocketTextChannel channel)
        {
            var roles = user.Roles.Where(r => !r.Name.Contains("everyone")).ToList();
            roles.Sort((a, b) => a.CompareTo(b));

            var builder = new EmbedBuilder()
                 .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                 .WithDescription("#TEAMCABBARIYE!")
                 .AddField("Kullanıcı Adı", user.Username, true)
                 .AddField("Katılma Tarihi", user.JoinedAt.Value.ToString("dd/MM/yyyy"), true)
                 .AddField("Roller", string.Join(" ", user.Roles.Select(x => x.Mention)))
                 .WithColor(new Color(33, 176, 252))
                 .WithCurrentTimestamp();
            await channel.SendMessageAsync(embed: builder.Build());
        }

        [Command("bilgi")]
        public async Task Info(SocketGuildUser usr = null)
        {
            usr = usr is null ? Context.Message.Author as SocketGuildUser : usr;
            await PostUserInfo(usr, Context.Channel as SocketTextChannel);
        }

        [Command("pp")]
        public async Task Avatar(SocketGuildUser usr = null)
        {
            usr = usr is null ? Context.Message.Author as SocketGuildUser : usr;
            var e = new EmbedBuilder()
                .WithTitle("Profil Resmi")
                .WithImageUrl(usr.GetAvatarUrl(ImageFormat.Png, 2048))
                .WithColor(Color.Green)
                .WithAuthor(usr)
                .WithCurrentTimestamp();

            await ReplyAsync(embed: e.Build());
        }
    }
}
