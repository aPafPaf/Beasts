using Beasts.Api;
using Beasts.Data;
using ExileCore.Shared.Helpers;
using ImGuiNET;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using Vector4 = System.Numerics.Vector4;

namespace Beasts;

/// <summary>
/// Ручной UI настроек: значения живут в <see cref="BeastsSettings"/> обычными полями,
/// кнопки — это прямые вызовы методов плагина (нод-обёрток больше нет).
/// </summary>
public partial class Beasts
{
    private int _beastSortColumn = 3; // 0 = enabled, 1 = poe.ninja price, 2 = talisman, 3 = name, 4 = crafts
    private bool _beastSortAscending = true;

    public override void DrawSettings()
    {
        base.DrawSettings();

        DrawLeagueSettings();
        ImGui.Separator();
        DrawTalismanSettings();
        ImGui.Separator();
        DrawDisplaySettings();
        ImGui.Separator();
        DrawBeastPicker();
        ImGui.Separator();

        if (ImGui.CollapsingHeader("Map Settings"))
            DrawMapSettings();

        if (ImGui.CollapsingHeader("Color Settings"))
            DrawColorSettings();
    }

    private void DrawLeagueSettings()
    {
        var leagues = Settings.Leagues;
        if (leagues.Count > 0 && string.IsNullOrEmpty(Settings.League))
            SetLeague(leagues[0]);

        var index = Math.Max(0, leagues.IndexOf(Settings.League));
        ImGui.PushItemWidth(220);
        if (leagues.Count > 0 && ImGui.Combo("League##leagueCombo", ref index, leagues.ToArray(), leagues.Count))
            SetLeague(leagues[index]);
        ImGui.PopItemWidth();

        ImGui.SameLine();
        if (ImGui.Button("Fetch leagues"))
            _ = FetchLeagues();

        ImGui.SameLine();
        if (ImGui.Button("Fetch prices"))
            _ = FetchPrices();

        ImGui.TextUnformatted($"League: {Settings.League}");
        ImGui.SameLine();
        TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1f), $"poe.ninja prices as of: {Settings.LastUpdate:HH:mm:ss}");
    }

    private void SetLeague(string league)
    {
        Settings.League = league;
        PoeNinja.SetLeague(league);
    }

    private void DrawTalismanSettings()
    {
        ImGui.PushItemWidth(420);
        var path = Settings.TalismanPricesPath ?? string.Empty;
        if (ImGui.InputText("Prices file##talismanPricesPath", ref path, 512))
            Settings.TalismanPricesPath = path;
        ImGui.PopItemWidth();

        ImGui.SameLine();
        if (ImGui.Button("Reload talisman prices"))
            LoadTalismanPrices();

        if (TalismanPrices.ByBeastPath.Count > 0)
        {
            var fileTime = TalismanPrices.FileWriteTime?.ToString("dd.MM HH:mm") ?? "—";
            ImGui.TextUnformatted($"Talisman prices: {TalismanPrices.ByBeastPath.Count} баз · " +
                                  $"лига {TalismanPrices.League} · файл {fileTime}");
        }
        else
        {
            TextColored(new Vector4(1f, 0.55f, 0.45f, 1f),
                $"Talisman prices: {TalismanPriceError ?? "не загружены"}");
        }
    }

    private void DrawDisplaySettings()
    {
        Checkbox("Draw on the map", () => Settings.DrawOnMap, value => Settings.DrawOnMap = value);
        Checkbox("Draw crafts column", () => Settings.DrawCrafts, value => Settings.DrawCrafts = value);
    }

    private void DrawBeastPicker()
    {
        ImGui.TextUnformatted($"Tracked: {Settings.Beasts.Count} / {BeastsDatabase.AllBeasts.Count}");

        const ImGuiTableFlags flags =
            ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Sortable |
            ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.BordersV |
            ImGuiTableFlags.NoBordersInBody | ImGuiTableFlags.ScrollY;

        if (!ImGui.BeginTable("BeastsTable", 5, flags)) return;

        ImGui.TableSetupColumn("Enabled", ImGuiTableColumnFlags.WidthFixed, 24);
        ImGui.TableSetupColumn("Price", ImGuiTableColumnFlags.WidthFixed, 48);
        ImGui.TableSetupColumn("Talisman", ImGuiTableColumnFlags.WidthFixed, 72);
        ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed, 230);
        ImGui.TableSetupColumn("Description", ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableHeadersRow();

        if (ImGui.TableGetSortSpecs() is { SpecsDirty: true } sortSpecs)
        {
            _beastSortColumn = sortSpecs.Specs.ColumnIndex;
            _beastSortAscending = sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending;
        }

        var beasts = SortBeasts(BeastsDatabase.AllBeasts);
        for (var row = 0; row < beasts.Count; row++)
        {
            var beast = beasts[row];

            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            var isChecked = Settings.Beasts.Any(eb => eb.Path == beast.Path);
            // Index-based ID keeps widgets unique even when two entries share a path.
            if (ImGui.Checkbox($"##{beast.Path}##{row}", ref isChecked))
            {
                if (isChecked)
                    Settings.Beasts.Add(beast);
                else
                    Settings.Beasts.RemoveAll(eb => eb.Path == beast.Path);
            }

            if (isChecked)
                ImGui.PushStyleColor(ImGuiCol.Text, Color.Green.ToImguiVec4());

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(Settings.BeastPrices.TryGetValue(beast.DisplayName, out var price) ? $"{price}c" : "0c");

            ImGui.TableNextColumn();
            DrawTalismanPrice(beast);

            ImGui.TableNextColumn();
            ImGui.TextUnformatted(beast.DisplayName);

            ImGui.TableNextColumn();
            foreach (var craft in beast.Crafts)
                ImGui.TextUnformatted(craft);

            if (!string.IsNullOrEmpty(beast.DropsTalisman))
            {
                ImGui.PushStyleColor(ImGuiCol.Text, new Color(255, 216, 96).ToImguiVec4());
                ImGui.TextUnformatted($"Talisman: {beast.DropsTalisman}");
                ImGui.PopStyleColor();
            }

            if (isChecked)
                ImGui.PopStyleColor();
        }

        ImGui.EndTable();
    }

    private static void DrawTalismanPrice(Beast beast)
    {
        if (string.IsNullOrEmpty(beast.DropsTalisman))
        {
            ImGui.TextUnformatted("—");
            return;
        }

        var price = TalismanPrices.ForBeast(beast.Path);
        if (price is not { HasPrice: true })
        {
            ImGui.TextUnformatted("?");
            return;
        }

        // Звёздочка — мало лотов, медиана по трём ценам нестабильна.
        ImGui.TextUnformatted(price.ThinMarket ? $"{price.MedianChaos:0.##}c*" : $"{price.MedianChaos:0.##}c");
    }

    private List<Beast> SortBeasts(List<Beast> source)
    {
        var ascending = _beastSortAscending;
        IEnumerable<Beast> ordered = _beastSortColumn switch
        {
            0 => ascending
                ? source.OrderBy(beast => Settings.Beasts.Any(eb => eb.Path == beast.Path))
                : source.OrderByDescending(beast => Settings.Beasts.Any(eb => eb.Path == beast.Path)),
            1 => ascending
                ? source.OrderBy(beast => Settings.BeastPrices.GetValueOrDefault(beast.DisplayName))
                : source.OrderByDescending(beast => Settings.BeastPrices.GetValueOrDefault(beast.DisplayName)),
            2 => ascending
                ? source.OrderBy(beast => TalismanPrices.ForBeast(beast.Path)?.MedianChaos ?? 0f)
                : source.OrderByDescending(beast => TalismanPrices.ForBeast(beast.Path)?.MedianChaos ?? 0f),
            4 => ascending
                ? source.OrderBy(beast => beast.Crafts.FirstOrDefault() ?? string.Empty)
                : source.OrderByDescending(beast => beast.Crafts.FirstOrDefault() ?? string.Empty),
            _ => ascending
                ? source.OrderBy(beast => beast.DisplayName)
                : source.OrderByDescending(beast => beast.DisplayName)
        };

        return ordered.ToList();
    }

    private void DrawMapSettings()
    {
        Checkbox("Draw point", () => Settings.MapSettings.DrawPoint, value => Settings.MapSettings.DrawPoint = value);
        Checkbox("Draw text", () => Settings.MapSettings.DrawText, value => Settings.MapSettings.DrawText = value);

        ImGui.PushItemWidth(220);
        SliderInt("Transparency", () => Settings.MapSettings.Transparency, value => Settings.MapSettings.Transparency = value, 0, 255);
        SliderInt("Radius", () => Settings.MapSettings.Radius, value => Settings.MapSettings.Radius = value, 0, 255);
        SliderInt("Thickness", () => Settings.MapSettings.Thickness, value => Settings.MapSettings.Thickness = value, 0, 30);
        SliderInt("Offset X", () => Settings.MapSettings.OffsetX, value => Settings.MapSettings.OffsetX = value, -100, 100);
        SliderInt("Offset Y", () => Settings.MapSettings.OffsetY, value => Settings.MapSettings.OffsetY = value, -100, 100);
        ImGui.PopItemWidth();
    }

    private void DrawColorSettings()
    {
        Checkbox("Custom colors", () => Settings.ColorSettings.CustomColors, value => Settings.ColorSettings.CustomColors = value);

        ImGui.Separator();
        ImGui.TextUnformatted("Beast colors");
        ColorEdit("Text color", () => Settings.ColorSettings.TextColor, value => Settings.ColorSettings.TextColor = value);
        ColorEdit("Vivid", () => Settings.ColorSettings.VididColor, value => Settings.ColorSettings.VididColor = value);
        ColorEdit("Wild", () => Settings.ColorSettings.WildColor, value => Settings.ColorSettings.WildColor = value);
        ColorEdit("Primal", () => Settings.ColorSettings.PrimalColor, value => Settings.ColorSettings.PrimalColor = value);
        ColorEdit("Fenumal", () => Settings.ColorSettings.FenumalColor, value => Settings.ColorSettings.FenumalColor = value);
        ColorEdit("Black", () => Settings.ColorSettings.BlackColor, value => Settings.ColorSettings.BlackColor = value);
        ColorEdit("Normal", () => Settings.ColorSettings.NormalColor, value => Settings.ColorSettings.NormalColor = value);

        if (ImGui.Button("Reset colors"))
            ResetColor();

        ImGui.Separator();
        ImGui.TextUnformatted("Map text");
        Checkbox("Change map text color",
            () => Settings.ColorSettings.MapColorSettings.ChangeColorText,
            value => Settings.ColorSettings.MapColorSettings.ChangeColorText = value);
        ColorEdit("Text",
            () => Settings.ColorSettings.MapColorSettings.ColorText,
            value => Settings.ColorSettings.MapColorSettings.ColorText = value);
        ColorEdit("Background",
            () => Settings.ColorSettings.MapColorSettings.BackgroundColorText,
            value => Settings.ColorSettings.MapColorSettings.BackgroundColorText = value);
    }

    private static void Checkbox(string label, Func<bool> get, Action<bool> set)
    {
        var value = get();
        if (ImGui.Checkbox(label, ref value))
            set(value);
    }

    private static void SliderInt(string label, Func<int> get, Action<int> set, int min, int max)
    {
        var value = get();
        if (ImGui.SliderInt(label, ref value, min, max))
            set(value);
    }

    private static void ColorEdit(string label, Func<Color> get, Action<Color> set)
    {
        var color = get();
        var vector = new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        if (!ImGui.ColorEdit4(label, ref vector)) return;

        set(new Color(
            (byte)Math.Clamp(vector.X * 255f, 0f, 255f),
            (byte)Math.Clamp(vector.Y * 255f, 0f, 255f),
            (byte)Math.Clamp(vector.Z * 255f, 0f, 255f),
            (byte)Math.Clamp(vector.W * 255f, 0f, 255f)));
    }

    /// <summary>
    /// Цветной текст. Обычные ImGui.Text / TextColored в этой обёртке передают строку
    /// как printf-формат, поэтому текст с '%' ("23% Quality...") роняет процесс —
    /// рисуем через TextUnformatted + PushStyleColor.
    /// </summary>
    private static void TextColored(Vector4 color, string text)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        ImGui.TextUnformatted(text);
        ImGui.PopStyleColor();
    }
}
