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
        Settings.FetchBeastPrices.OnPressed += async () => await FetchPrices();
        Settings.FetchLeagues.OnPressed += async () => await FetchLeagues();
        Settings.ColorSettings.Default.OnPressed = ResetColor;

        Settings.League.OnValueSelected += selectedUrl =>
        {
            Settings.CurrentLeague.SetValueNoEvent(selectedUrl);
            PoeNinja.SetLeague(Settings.CurrentLeague.Value);
        };

        PoeNinja.SetLeague(Settings.CurrentLeague.Value);
        Task.Run(FetchLeagues);

        _trackedBeastsMetadata = BeastsDatabase.AllBeasts.Select(beast => beast.Path).ToHashSet();
        // Some beasts are renamed between patches but keep the same metadata path,
        // so the database may contain aliases. Pick one entry per path instead of throwing.
        _beastsByPath = BeastsDatabase.AllBeasts
            .GroupBy(beast => beast.Path)
            .ToDictionary(group => group.Key, group => group.First());

        LogMessage($"FetchPrices: {Settings.CurrentLeague.Value}");
        Task.Run(FetchPrices);
    }

    private void ResetColor()
    {
        Settings.ColorSettings.VididColor.Value = new Color(255, 250, 0);
        Settings.ColorSettings.WildColor.Value = new Color(255, 0, 235);
        Settings.ColorSettings.PrimalColor.Value = new Color(0, 245, 255);
        Settings.ColorSettings.FenumalColor.Value = new Color(0, 150, 40);
        Settings.ColorSettings.BlackColor.Value = new Color(255, 255, 255);
        Settings.ColorSettings.NormalColor.Value = Color.Red;
    }

    private async Task FetchLeagues()
    {
        var urls = await LeagueUrlExtractor.GetAllLeagueUrls();
        Settings.League.SetListValues(urls);
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
