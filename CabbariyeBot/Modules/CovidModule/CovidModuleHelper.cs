using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CabbariyeBot.Modules.CovidModule
{
    public class CovidModuleHelper
    {
        public class Root
        {
            [JsonProperty("Active Cases_text")]
            public string ActiveCasesText { get; set; }

            [JsonProperty("Country_text")]
            public string CountryText { get; set; }

            [JsonProperty("Last Update")]
            public string LastUpdate { get; set; }

            [JsonProperty("New Cases_text")]
            public string NewCasesText { get; set; }

            [JsonProperty("New Deaths_text")]
            public string NewDeathsText { get; set; }

            [JsonProperty("Total Cases_text")]
            public string TotalCasesText { get; set; }

            [JsonProperty("Total Deaths_text")]
            public string TotalDeathsText { get; set; }

            [JsonProperty("Total Recovered_text")]
            public string TotalRecoveredText { get; set; }
        }
    }
}
