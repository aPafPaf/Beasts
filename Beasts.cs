using Beasts.Api;
using Beasts.Data;
using ExileCore;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beasts;

public partial class Beasts : BaseSettingsPlugin<BeastsSettings>
{
    private readonly Dictionary<long, Entity> _trackedBeasts = new();

    public override void OnLoad()
    {
        Settings.FetchBeastPrices.OnPressed += async () => await FetchPrices();
        Settings.FetchLeagues.OnPressed += async () => await FetchLeagues();
        Settings.ColorSettings.Default.OnPressed = ResetColor;

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

        // Устанавливаем обработчик выбора значения
        Settings.League.OnValueSelected += selectedUrl =>
        {
        };
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
    }

    public override void EntityAdded(Entity entity)
    {
        if (entity.Rarity != MonsterRarity.Rare) return;
        foreach (var _ in BeastsDatabase.AllBeasts.Where(beast => entity.Metadata == beast.Path))
        {
            _trackedBeasts.Add(entity.Id, entity);
        }
    }

    public override void EntityRemoved(Entity entity)
    {
        if (_trackedBeasts.ContainsKey(entity.Id))
        {
            _trackedBeasts.Remove(entity.Id);
        }
    }
}