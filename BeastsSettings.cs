using System;
using System.Collections.Generic;
using Beasts.Data;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using Newtonsoft.Json;
using SharpDX;

namespace Beasts;

/// <summary>
/// Данные настроек без UI-обёрток: единственная нода здесь — Enable, её рисует HUD
/// (PluginWrapper.IsEnable). Всё остальное — обычные значения, UI рисует
/// <see cref="Beasts.DrawSettings"/>.
/// </summary>
public class BeastsSettings : ISettings
{
    public ToggleNode Enable { get; set; } = new ToggleNode(false);

    public List<Beast> Beasts { get; set; } = new();
    public Dictionary<string, float> BeastPrices { get; set; } = new();
    public DateTime LastUpdate { get; set; } = DateTime.MinValue;

    // лига poe.ninja: выбранная + список, который приезжает из api.pathofexile.com
    public string League { get; set; } = "Mirage";
    [JsonIgnore] public List<string> Leagues { get; set; } = new();

    // отображение
    public bool DrawOnMap { get; set; } = true;
    public bool DrawCrafts { get; set; } = true;

    // цены талисманов (json собирает tools/fetch_talisman_prices.py)
    public string TalismanPricesPath { get; set; } = @"tools\talisman_prices.json";

    public MapSettings MapSettings { get; set; } = new();
    public ColorSettings ColorSettings { get; set; } = new();
}

public class ColorSettings
{
    public MapColorSettings MapColorSettings { get; set; } = new();

    public bool CustomColors { get; set; }
    public Color TextColor { get; set; } = Color.White;

    public Color VididColor { get; set; } = new Color(255, 250, 0);
    public Color WildColor { get; set; } = new Color(255, 0, 235);
    public Color PrimalColor { get; set; } = new Color(0, 245, 255);
    public Color FenumalColor { get; set; } = new Color(0, 150, 40);
    public Color BlackColor { get; set; } = new Color(255, 255, 255);
    public Color NormalColor { get; set; } = Color.Red;
}

public class MapColorSettings
{
    public bool ChangeColorText { get; set; }
    public Color ColorText { get; set; } = Color.White;
    public Color BackgroundColorText { get; set; } = Color.Black;
}

public class MapSettings
{
    public bool DrawPoint { get; set; } = true;
    public bool DrawText { get; set; } = true;

    public int Transparency { get; set; } = 255;
    public int Radius { get; set; } = 30;
    public int Thickness { get; set; } = 5;
    public int OffsetX { get; set; }
    public int OffsetY { get; set; }
}
