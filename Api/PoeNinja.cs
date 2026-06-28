using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Beasts.Api;

public static class PoeNinja
{
    private static readonly string PoeBaseNinjaUrl = "https://poe.ninja/poe1/api/economy/stash/current/item/overview?league=";
    private static readonly string PoeNinjaType = "&type=Beast";
    private static string League = "Mirage";
    private class PoeNinjaLine
    {
        [JsonProperty("name")] public string Name;
        [JsonProperty("chaosValue")] public float ChaosValue;
    }

    private class PoeNinjaResponse
    {
        [JsonProperty("lines")] public List<PoeNinjaLine> Lines;
    }

    public static void SetLeague(string league) => League = league;

    public static async Task<Dictionary<string, float>> GetBeastsPrices(string league)
    {
        using var httpClient = new HttpClient();

        string url = PoeBaseNinjaUrl + League + PoeNinjaType;

        var response = await httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) throw new HttpRequestException("Failed to get poe.ninja response");

        var json = await response.Content.ReadAsStringAsync();
        var poeNinjaResponse = JsonConvert.DeserializeObject<PoeNinjaResponse>(json);

        return poeNinjaResponse.Lines.ToDictionary(line => line.Name, line => line.ChaosValue);
    }
}
