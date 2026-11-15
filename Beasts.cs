using Beasts.Api;
using Beasts.Data;
using ExileCore;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using Vector2 = System.Numerics.Vector2;

namespace Beasts;

public partial class Beasts : BaseSettingsPlugin<BeastsSettings>
{
    private readonly Dictionary<long, TrackedBeast> _trackedBeasts = new();
    private HashSet<string> _trackedBeastsMetadata = new();
    private Dictionary<string, Beast> _beastsByPath = new();
    private HashSet<string> _enabledBeastPaths = new();
    private readonly List<Beast> _trackedBeastRows = new();

    public override void OnLoad()
    {
        PoeNinja.SetLeague(Settings.League);
        Task.Run(FetchLeagues);
        LoadTalismanPrices();

        _trackedBeastsMetadata = BeastsDatabase.AllBeasts.Select(beast => beast.Path).ToHashSet();
        // Some beasts are renamed between patches but keep the same metadata path,
        // so the database may contain aliases. Pick one entry per path instead of throwing.
        _beastsByPath = BeastsDatabase.AllBeasts
            .GroupBy(beast => beast.Path)
            .ToDictionary(group => group.Key, group => group.First());

        // Saved settings keep a copy of the beast entries, so refresh them from the database
        // to pick up updated crafts and talisman data after a patch.
        for (var i = 0; i < Settings.Beasts.Count; i++)
        {
            if (_beastsByPath.TryGetValue(Settings.Beasts[i].Path, out var actualBeast))
                Settings.Beasts[i] = actualBeast;
        }

        LogMessage($"FetchPrices: {Settings.League}");
        Task.Run(FetchPrices);
    }

    /// <summary>
    /// Единственный источник дефолтных цветов — инициализаторы <see cref="ColorSettings"/>,
    /// поэтому сброс просто копирует свежий экземпляр (без второй копии палитры).
    /// </summary>
    private void ResetColor()
    {
        var defaults = new ColorSettings();

        Settings.ColorSettings.CustomColors = defaults.CustomColors;
        Settings.ColorSettings.TextColor = defaults.TextColor;
        Settings.ColorSettings.VididColor = defaults.VididColor;
        Settings.ColorSettings.WildColor = defaults.WildColor;
        Settings.ColorSettings.PrimalColor = defaults.PrimalColor;
        Settings.ColorSettings.FenumalColor = defaults.FenumalColor;
        Settings.ColorSettings.BlackColor = defaults.BlackColor;
        Settings.ColorSettings.NormalColor = defaults.NormalColor;

        Settings.ColorSettings.MapColorSettings.ChangeColorText = defaults.MapColorSettings.ChangeColorText;
        Settings.ColorSettings.MapColorSettings.ColorText = defaults.MapColorSettings.ColorText;
        Settings.ColorSettings.MapColorSettings.BackgroundColorText = defaults.MapColorSettings.BackgroundColorText;
    }

    private bool _talismanPricesLoaded;
    private string _talismanPriceError;

    private void LoadTalismanPrices()
    {
        try
        {
            var pluginDir = string.IsNullOrWhiteSpace(DirectoryFullName) ? AppContext.BaseDirectory : DirectoryFullName;
            _talismanPricesLoaded = TalismanPrices.Load(Settings.TalismanPricesPath, pluginDir, out var error);
            _talismanPriceError = _talismanPricesLoaded ? null : error;

            if (!_talismanPricesLoaded)
                DebugWindow.LogMsg($"[Beasts] talisman prices: {error}", 5, Color.Orange);
        }
        catch (Exception ex)
        {
            _talismanPricesLoaded = false;
            _talismanPriceError = ex.Message;
            DebugWindow.LogMsg($"[Beasts] talisman prices: {ex.Message}", 5, Color.Red);
        }
    }

    public bool TalismanPricesLoaded => _talismanPricesLoaded;

    public string TalismanPriceError => _talismanPriceError;

    private async Task FetchLeagues()
    {
        var urls = await LeagueUrlExtractor.GetAllLeagueUrls();
        Settings.Leagues = urls;

        if (urls.Count > 0 && !urls.Contains(Settings.League))
            SetLeague(urls[0]);
    }

    private async Task FetchPrices()
    {
        DebugWindow.LogMsg("Fetching Beast Prices from PoeNinja...");
        var prices = await PoeNinja.GetBeastsPrices(Settings.League);
        foreach (var beast in BeastsDatabase.AllBeasts)
        {
            Settings.BeastPrices[beast.DisplayName] = prices.TryGetValue(beast.DisplayName, out var price) ? price : -1;
        }
        Settings.LastUpdate = DateTime.Now;
    }

    public override void AreaChange(AreaInstance area)
    {
        _trackedBeasts.Clear();
        _trackedBeastRows.Clear();
    }

    public override Job Tick()
    {
        _enabledBeastPaths.Clear();
        foreach (var b in Settings.Beasts)
            _enabledBeastPaths.Add(b.Path);

        _trackedBeastRows.Clear();
        foreach (var tracked in _trackedBeasts.Values)
        {
            if (_enabledBeastPaths.Contains(tracked.Beast.Path))
                _trackedBeastRows.Add(tracked.Beast);
        }

        foreach (var entity in GameController.Entities)
        {
            if (entity.Rarity is MonsterRarity.White 
                || entity.Rarity is MonsterRarity.Magic
                || entity.Rarity is MonsterRarity.Error)
                continue;

            if (!entity.IsValid) continue;
            if (!_beastsByPath.TryGetValue(entity.Metadata, out var beast)) continue;
            if (_trackedBeasts.ContainsKey(entity.Id)) continue;

            _trackedBeasts[entity.Id] = new TrackedBeast(entity, beast, GetBeastType(beast.DisplayName));
        }

        return null;
    }

    public override void EntityAdded(Entity entity)
    {
        if (entity is null) return;
        if (entity.Rarity != MonsterRarity.Rare) return;
        if (!_beastsByPath.TryGetValue(entity.Metadata, out var beast)) return;

        _trackedBeasts[entity.Id] = new TrackedBeast(entity, beast, GetBeastType(beast.DisplayName));
    }

    public override void EntityRemoved(Entity entity)
    {
        _trackedBeasts.Remove(entity.Id);
    }

    private struct TrackedBeast
    {
        public Entity Entity;
        public Beast Beast;
        public BeastType Type;
        public Vector2 LastGridPos;
        public bool HasValidPos;

        public TrackedBeast(Entity entity, Beast beast, BeastType type)
        {
            Entity = entity;
            Beast = beast;
            Type = type;
            LastGridPos = default;
            HasValidPos = false;
        }
    }
}
