using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace Beasts.Api;

/// <summary>Одна запись из talisman_prices.json (снимок с trade-сайта).</summary>
public sealed class TalismanPrice
{
    public string Base;
    public string Beast;
    public string Enchant;
    public int Level;
    public int Total;
    public int Listings;
    public float MinChaos;
    public float MedianChaos;
    public float MaxChaos;
    public DateTime FetchedAt;

    public bool HasPrice => MedianChaos > 0f;

    /// <summary>Мало лотов — цена нестабильна, стоит помечать в UI.</summary>
    public bool ThinMarket => Total < 20;
}

/// <summary>
/// Загрузчик цен талисманов, которые собирает tools/fetch_talisman_prices.py.
/// Ключ — metadata path зверя (совпадает с Beast.Path).
/// </summary>
public static class TalismanPrices
{
    public static Dictionary<string, TalismanPrice> ByBeastPath { get; private set; } = new();
    public static string League { get; private set; }
    public static DateTime? GeneratedAt { get; private set; }
    public static string FilePath { get; private set; }
    public static DateTime? FileWriteTime { get; private set; }

    public static TalismanPrice ForBeast(string beastPath)
    {
        return !string.IsNullOrEmpty(beastPath) && ByBeastPath.TryGetValue(beastPath, out var price)
            ? price
            : null;
    }

    public static bool Load(string configuredPath, string pluginDir, out string error)
    {
        error = null;
        var candidates = ResolveCandidates(configuredPath, pluginDir);
        var searched = new List<string>();

        foreach (var candidate in candidates)
        {
            searched.Add(candidate);
            if (!File.Exists(candidate)) continue;

            try
            {
                var json = JObject.Parse(File.ReadAllText(candidate));
                var byPath = new Dictionary<string, TalismanPrice>();

                foreach (var token in json["talismans"] as JArray ?? new JArray())
                {
                    var beastPath = token["beast_path"]?.ToString();
                    if (string.IsNullOrEmpty(beastPath)) continue;

                    byPath[beastPath] = new TalismanPrice
                    {
                        Base = token["base"]?.ToString(),
                        Beast = token["beast"]?.ToString(),
                        Enchant = token["enchant"]?.ToString(),
                        Level = Int(token, "level"),
                        Total = Int(token, "total"),
                        Listings = Int(token, "listings_count"),
                        MinChaos = Float(token, "min_chaos"),
                        MedianChaos = Float(token, "median_chaos"),
                        MaxChaos = Float(token, "max_chaos"),
                        FetchedAt = ParseDate(token["fetched_at"]?.ToString())
                    };
                }

                ByBeastPath = byPath;
                League = json["league"]?.ToString();
                GeneratedAt = ParseDate(json["generated_at"]?.ToString());
                FilePath = candidate;
                FileWriteTime = File.GetLastWriteTime(candidate);
                return true;
            }
            catch (Exception ex)
            {
                error = $"{candidate}: {ex.Message}";
            }
        }

        error ??= "файл цен не найден, искали: " + string.Join("; ", searched);
        return false;
    }

    private static int Int(JToken token, string name) => token[name]?.Value<int?>() ?? 0;

    private static float Float(JToken token, string name) => token[name]?.Value<float?>() ?? 0f;

    private static DateTime ParseDate(string value)
    {
        return DateTime.TryParse(value, null,
            System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal,
            out var parsed)
            ? parsed
            : default;
    }

    private static List<string> ResolveCandidates(string configuredPath, string pluginDir)
    {
        var result = new List<string>();
        if (string.IsNullOrWhiteSpace(configuredPath)) return result;

        var expanded = Environment.ExpandEnvironmentVariables(configuredPath.Trim());
        result.Add(expanded);
        if (Path.IsPathRooted(expanded)) return result;

        var baseDir = string.IsNullOrWhiteSpace(pluginDir) ? AppContext.BaseDirectory : pluginDir;
        result.Add(Path.Combine(baseDir, expanded));
        // Плагин грузится из Plugins/Temp/<Name>, а скрипт кладёт json в Plugins/Source/<Name>/tools.
        result.Add(Path.GetFullPath(Path.Combine(baseDir, "..", "..", "Source", "Beasts", expanded)));
        result.Add(Path.GetFullPath(Path.Combine(baseDir, "..", "..", "Beasts", expanded)));
        result.Add(Path.GetFullPath(Path.Combine(baseDir, "..", "Source", "Beasts", expanded)));

        return result;
    }
}
