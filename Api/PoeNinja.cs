using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Beasts.Api;

public static class PoeNinja
{
    private class PoeNinjaLine
    {
        [JsonProperty("name")] public string Name;
        [JsonProperty("chaosValue")] public float ChaosValue;
    }

    private class PoeNinjaResponse
    {
        [JsonProperty("lines")] public List<PoeNinjaLine> Lines;
    }

    public static async Task<Dictionary<string, float>> GetBeastsPrices(string league)
    {
        using var httpClient = new HttpClient();

        var PoeNinjaUrl = $"https://poe.ninja/api/data/itemoverview?league={league}&type=Beast";

        var response = await httpClient.GetAsync(PoeNinjaUrl);
        if (!response.IsSuccessStatusCode) throw new HttpRequestException("Failed to get poe.ninja response");

        var json = await response.Content.ReadAsStringAsync();
        var poeNinjaResponse = JsonConvert.DeserializeObject<PoeNinjaResponse>(json);

        return poeNinjaResponse.Lines.ToDictionary(line => line.Name, line => line.ChaosValue);
    }
}

public static class LeagueUrlExtractor
{
    public static async Task<List<string>> GetAllLeagueUrls()
    {
        using var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:45.0) Gecko/20100101 Firefox/45.0");

        var response = await httpClient.GetAsync("https://api.pathofexile.com/leagues");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        // Парсим JSON и извлекаем только URL
        var leagues = JArray.Parse(json);
        var urls = new List<string>();

        foreach (var league in leagues)
        {
            var url = league["url"]?.ToString();
            if (!string.IsNullOrEmpty(url))
            {
                int lastSlashIndex = url.LastIndexOf('/');
                string lastPart = url.Substring(lastSlashIndex + 1);
                urls.Add(lastPart);
            }
        }

        return urls;
    }
}