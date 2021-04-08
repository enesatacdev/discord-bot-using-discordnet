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
using static CabbariyeBot.Modules.CovidModule.CovidModuleHelper;

namespace CabbariyeBot.Modules
{
    public class CovidTracker : ModuleBase
    {
        [Command("korona-takip")]
        public async Task CovidTrack()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://covid-19-tracking.p.rapidapi.com/v1/turkey"),
                Headers =
    {
        { "x-rapidapi-key", "789bd6eba7mshdf73752d6f4c2cdp16b9a9jsnafaa502a3d82" },
        { "x-rapidapi-host", "covid-19-tracking.p.rapidapi.com" },
    },
            };
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            Root tracked = JsonConvert.DeserializeObject<Root>(json);
            string gunlukvaka = tracked.NewCasesText.ToString();
            string toplamvaka = tracked.ActiveCasesText.ToString();
            string toplamvefat = tracked.TotalDeathsText.ToString();
            string oluSayisi = tracked.NewDeathsText.ToString();

            var builder = new EmbedBuilder()
         .WithColor(new Color(33, 176, 255))
         .WithDescription("Maske Hayattır!")
         .AddField("Günlük Vaka Sayısı", gunlukvaka, false)
         .AddField("Toplam Vaka Sayısı", toplamvaka, false)
         .AddField("Vefat edenlerin Sayısı", oluSayisi, false)
         .AddField("Toplam Vefat Edenlerin Sayısı", toplamvefat, false)
         .WithAuthor(Context.Client.CurrentUser)
         .WithFooter(footer => footer.Text = "Cebbariye Bot")
         .WithCurrentTimestamp()
         .Build();
            await ReplyAsync(embed: builder);







        }
    }
}
