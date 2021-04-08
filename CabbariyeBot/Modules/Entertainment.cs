using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using static CabbariyeBot.Helpers.WikipediaSeach;

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
                .AddField($"Bu Mesaj Kendini {timer} Saniye İçerisinde Yokedicek!", true)
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

        [Command("wikipedia",RunMode = RunMode.Async)]
        public async Task SearchFromWikipediaAsync([Remainder]string wikisearch = "kedi")
        {
            var client = new HttpClient();
            var json = await client.GetStringAsync($"https://tr.wikipedia.org/w/api.php?format=json&action=query&prop=extracts|pageimages&exintro&explaintext&titles={wikisearch}");
            var myPages = JsonConvert.DeserializeObject<Root>(json);
            var first = myPages.Query.Pages.Values.First();
            var title = first.Title;
            string thumbnail = first.Thumbnail.source;
            var extract = first.Extract.Substring(0,500) + "...";

            var builder = new EmbedBuilder()
               .WithColor(new Color(33, 176, 255))
               .WithThumbnailUrl(thumbnail)
               .AddField("Wikipedia'da Aranan",wikisearch,false)
               .AddField("Wikipedia'da Bulunan",title,false)
               .AddField($"{title} ile İlgili Bilgiler",extract,false)
               .AddField("Wikipedia Linki", $"[https://tr.wikipedia.org/wiki/{title}](https://tr.wikipedia.org/wiki/{title})",false)
               .WithAuthor(Context.Client.CurrentUser)
               .WithFooter(footer => footer.Text = "Cebbariye Bot")
               .WithCurrentTimestamp()
               .Build();
            await ReplyAsync(embed: builder);


        }
    }
}
