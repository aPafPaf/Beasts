using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Beasts.Data;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Helpers;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using ImGuiNET;
using Newtonsoft.Json;
using SharpDX;

namespace Beasts;

public class BeastsSettings : ISettings
{
    public List<Beast> Beasts { get; set; } = new();
    public Dictionary<string, float> BeastPrices { get; set; } = new();
    public DateTime LastUpdate { get; set; } = DateTime.MinValue;

    public BeastsSettings()
    {
        BeastPicker = new CustomNode
        {
            DrawDelegate = () =>
            {
                ImGui.Separator();
                if (ImGui.BeginTable("BeastsTable", 4, ImGuiTableFlags.Resizable | ImGuiTableFlags.Reorderable | ImGuiTableFlags.Sortable | ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.BordersV | ImGuiTableFlags.NoBordersInBody | ImGuiTableFlags.ScrollY))
                {
                    ImGui.TableSetupColumn("Enabled", ImGuiTableColumnFlags.WidthFixed, 24);
                    ImGui.TableSetupColumn("Price", ImGuiTableColumnFlags.WidthFixed, 48);
                    ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed, 256);
                    ImGui.TableSetupColumn("Description", ImGuiTableColumnFlags.WidthStretch);
                    ImGui.TableSetupScrollFreeze(0, 1);
                    ImGui.TableHeadersRow();

                    var sortedBeasts = BeastsDatabase.AllBeasts;
                    if (ImGui.TableGetSortSpecs() is { SpecsDirty: true } sortSpecs)
                    {
                        int sortedColumn = sortSpecs.Specs.ColumnIndex;
                        var sortAscending = sortSpecs.Specs.SortDirection == ImGuiSortDirection.Ascending;

                        sortedBeasts = sortedColumn switch
                        {
                            0 => sortAscending ? [.. sortedBeasts.OrderBy(b => Beasts.Any(eb => eb.Path == b.Path))] : [.. sortedBeasts.OrderByDescending(b => Beasts.Any(eb => eb.Path == b.Path))],
                            1 => sortAscending ? [.. sortedBeasts.OrderBy(b => BeastPrices[b.DisplayName])] : [.. sortedBeasts.OrderByDescending(b => BeastPrices[b.DisplayName])],
                            2 => sortAscending ? [.. sortedBeasts.OrderBy(b => b.DisplayName)] : [.. sortedBeasts.OrderByDescending(x => x.DisplayName)],
                            3 => sortAscending ? [.. sortedBeasts.OrderBy(b => b.Crafts[0])] : [.. sortedBeasts.OrderByDescending(x => x.Crafts[0])],
                            _ => sortAscending ? [.. sortedBeasts.OrderBy(b => b.DisplayName)] : [.. sortedBeasts.OrderByDescending(x => x.DisplayName)]
                        };
                    }

                    foreach (var beast in sortedBeasts)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableNextColumn();

                        var isChecked = Beasts.Any(eb => eb.Path == beast.Path);
                        if (ImGui.Checkbox($"##{beast.Path}", ref isChecked))
                        {
                            if (isChecked)
                            {
                                Beasts.Add(beast);
                            }
                            else
                            {
                                Beasts.RemoveAll(eb => eb.Path == beast.Path);
                            }
                        }

                        if (isChecked)
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, Color.Green.ToImguiVec4());
                        }

                        ImGui.TableNextColumn();
                        ImGui.Text(BeastPrices.TryGetValue(beast.DisplayName, out var price) ? $"{price}c" : "0c");

                        ImGui.TableNextColumn();
                        ImGui.Text(beast.DisplayName);

                        ImGui.TableNextColumn();
                        // display all the crafts for the beast seperated by newline
                        foreach (var craft in beast.Crafts)
                        {
                            ImGui.Text(craft);
                        }

                        if (isChecked)
                        {
                            ImGui.PopStyleColor();
                        }

                        ImGui.NextColumn();
                    }
                    ImGui.EndTable();
                }
            }
        };

        LastUpdated = new CustomNode
        {
            DrawDelegate = () =>
            {
                ImGui.Text("PoeNinja prices as of:");
                ImGui.SameLine();
                ImGui.Text(LastUpdate.ToString("HH:mm:ss"));
            }
        };
    }

    public ToggleNode Enable { get; set; } = new ToggleNode(false);

    [Menu("League", "League name from poe ninja")]
    public ListNode League { get; set; } = new ListNode();

    public ButtonNode FetchLeagues { get; set; } = new ButtonNode();

    public ButtonNode FetchBeastPrices { get; set; } = new ButtonNode();

    [Menu("Draw On The Map")]
    public ToggleNode DrawOnMap { get; set; } = new(true); 

    [Menu("Draw Crafts Column")]
    public ToggleNode DrawCrafts { get; set; } = new(true);

    [Menu("Map Settings")]
    public MapSettings MapSettings { get; set; } = new();

    [Menu("Color Settings")]
    public ColorSettings ColorSettings { get; set; } = new();

    [JsonIgnore] public CustomNode LastUpdated { get; set; }

    [JsonIgnore] public CustomNode BeastPicker { get; set; }
}

[Submenu(CollapsedByDefault = true)]
public class ColorSettings
{
    [Menu("MapColorSettings")]
    public MapColorSettings MapColorSettings { get; set; } = new();

    [Menu("Custom colors")]
    public ToggleNode CustomColors { get; set; } = new ToggleNode(false);

    [Menu("TextColor")]
    public ColorNode TextColor { get; set; } = Color.White;

    [Menu("Vivid")]
    public ColorNode VididColor { get; set; } = new Color(255, 250, 0);

    [Menu("Wild")]
    public ColorNode WildColor { get; set; } = new Color(255, 0, 235);

    [Menu("Primal")]
    public ColorNode PrimalColor { get; set; } = new Color(0, 245, 255);

    [Menu("Fenumal")]
    public ColorNode FenumalColor { get; set; } = new Color(0, 150, 40);

    [Menu("Black")]
    public ColorNode BlackColor { get; set; } = new Color(255, 255, 255);

    [Menu("Normal")]
    public ColorNode NormalColor { get; set; } = new(Color.Red);

    public ButtonNode Default { get; set; } = new ButtonNode();
}

[Submenu(CollapsedByDefault = true)]
public class MapColorSettings
{
    [Menu("Change color text")]
    public ToggleNode ChangeColorText { get; set; } = new ToggleNode(false);

    [Menu("Color text")]
    public ColorNode ColorText { get; set; } = new(Color.White);

    [Menu("Background color text")]
    public ColorNode BackgroundColorText { get; set; } = new(Color.Black);
}

[Submenu(CollapsedByDefault = true)]
public class MapSettings
{
    [Menu("Draw point")]
    public ToggleNode DrawPoint { get; set; } = new ToggleNode(true);

    [Menu("Draw text")]
    public ToggleNode DrawText { get; set; } = new ToggleNode(true);

    [Menu("Transparency ")]
    public RangeNode<int> Transparency { get; set; } = new RangeNode<int>(255, 0, 255);

    [Menu("Radius")]
    public RangeNode<int> Radius { get; set; } = new RangeNode<int>(30, 0, 255);

    [Menu("Thickness")]
    public RangeNode<int> Thickness { get; set; } = new RangeNode<int>(5, 0, 30);

    [Menu("Offset Text X")]
    public RangeNode<int> OffsetX { get; set; } = new RangeNode<int>(0, -100, 100);

    [Menu("Offset Text Y")]
    public RangeNode<int> OffsetY { get; set; } = new RangeNode<int>(0, -100, 100);
}