using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CabbariyeBot.Modules
{
    public class Entertainment : ModuleBase
    {

        [Command("echo")]
        public async Task EchoAsync([Remainder] string text)
        {
            var messages = await Context.Channel.GetMessagesAsync(1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
            await (Context.Channel as SocketTextChannel).SendMessageAsync(text);
        }

        [Command("hatirlatici")]
        public async Task Reminder(int timer)
        {
            var lastmessage = await Context.Channel.GetMessagesAsync(1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(lastmessage);
            var remindermessage = await Context.Channel.SendMessageAsync($"{timer} saniye sonra sizi uyarıcam!");
            await Task.Delay(2000);
            await Context.Channel.DeleteMessageAsync(remindermessage);
            int sure = timer * 1000;
            await Task.Delay(sure);
            var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                .WithDescription($"{timer} saniye sonra sizi uyarmamı istemiştiniz :)")
                .AddField("Kullanıcı Adı", (Context.User as SocketGuildUser).Mention.ToString(), true)
                .AddField("Hatırlatıcı Süresi", timer + " Saniye", true)
                .AddField($"Bu Mesaj Kendini {timer} Saniye İçerisinde Yokedicek!",true)
                .WithColor(new Color(33, 176, 252))
                .WithCurrentTimestamp();
            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
            await Task.Delay(5000);
            var messages = await Context.Channel.GetMessagesAsync(1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
        }


        [Command("reddit")]
        [Alias("memes")]
        public async Task Reddit(string subreddit = null)
        {

            var client = new HttpClient();
            var result = await client.GetStringAsync($"https://reddit.com/r/{subreddit ?? "memes"}/random.json?limit=1");
            if (!result.StartsWith("["))
            {
                await Context.Channel.SendMessageAsync("Böyle Bir Subreddit Yok!");
            }
            else
            {
                JArray arr = JArray.Parse(result);
                JObject post = JObject.Parse(arr[0]["data"]["children"][0]["data"].ToString());

                var builder = new EmbedBuilder()
                    .WithImageUrl(post["url"].ToString())
                    .WithColor(new Color(33, 176, 255))
                    .WithTitle(post["title"].ToString())
                    .WithUrl("Https://reddit.com" + post["permalink"].ToString());
                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(null, false, embed);
            }
           
        }
    }
}
