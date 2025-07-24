using Beasts.Data;
using Beasts.ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements;
using ExileCore.Shared.Enums;
using ImGuiNET;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace Beasts;

public partial class Beasts
{
    private const int SEGMENTS_CIRCLE = 20;

    public override void Render()
    {
        DrawInGameBeasts();
        DrawBestiaryPanel();
        DrawBeastsWindow();
    }

    private void DrawInGameBeasts()
    {
        foreach (var trackedBeast in _trackedBeasts
                     .Select(beast => new { Positioned = beast.Value.GetComponent<Positioned>(), beast.Value.Metadata })
                     .Where(beast => beast.Positioned != null))
        {
            var beast = BeastsDatabase.AllBeasts.Where(beast => trackedBeast.Metadata == beast.Path).First();

            var beastType = GetBeastType(beast.DisplayName);
            var beastColor = GetColorBeastByType(beastType);

            if (Settings.DrawOnMap.Value) DrawLargeMap(beast.DisplayName, trackedBeast.Positioned.GridPosNum, beastColor);

            if (!Settings.Beasts.Any(b => b.Path == beast.Path)) continue;

            var pos = GameController.IngameState.Data.ToWorldWithTerrainHeight(trackedBeast.Positioned.GridPosition);
            if (!WorldPositionOnScreenBool(pos)) continue;

            var colorText = Settings.ColorSettings.CustomColors.Value ? Settings.ColorSettings.TextColor.Value : Color.White;
            Graphics.DrawText(beast.DisplayName, GameController.IngameState.Camera.WorldToScreen(pos), colorText, FontAlign.Center);
            DrawFilledCircleInWorldPosition(pos, 50, beastColor);
        }
    }

    private void DrawBestiaryPanel()
    {
        return;

        var bestiary = GameController.IngameState.IngameUi.GetBestiaryPanel();
        if (bestiary == null || bestiary.IsVisible == false) return;

        var capturedBeastsPanel = bestiary.CapturedBeastsPanel;
        if (capturedBeastsPanel == null || capturedBeastsPanel.IsVisible == false) return;

        var beasts = bestiary.CapturedBeastsPanel.CapturedBeasts;
        foreach (var beast in beasts)
        {
            var beastMetadata = Settings.Beasts.Find(b => b.DisplayName == beast.DisplayName);
            if (beastMetadata == null) continue;
            if (!Settings.BeastPrices.ContainsKey(beastMetadata.DisplayName)) continue;

            var center = new Vector2(beast.GetClientRect().Center.X, beast.GetClientRect().Center.Y);

            Graphics.DrawBox(beast.GetClientRect(), new Color(0, 0, 0, 0.5f));
            Graphics.DrawFrame(beast.GetClientRect(), Color.White, 2);
            Graphics.DrawText(beastMetadata.DisplayName, center, Color.White, FontAlign.Center);

            var text = Settings.BeastPrices[beastMetadata.DisplayName].ToString(CultureInfo.InvariantCulture) + "c";
            var textPos = center + new Vector2(0, 20);
            Graphics.DrawText(text, textPos, Color.White, FontAlign.Center);
        }
    }

    private void DrawBeastsWindow()
    {
        ImGui.SetNextWindowSize(new Vector2(0, 0));
        ImGui.SetNextWindowBgAlpha(0.6f);
        ImGui.Begin("Beasts Window", ImGuiWindowFlags.NoDecoration);

        if (ImGui.BeginTable("Beasts Table", 2, ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.BordersV))
        {
            ImGui.TableSetupColumn("Price", ImGuiTableColumnFlags.WidthFixed, 48);
            ImGui.TableSetupColumn("Beast");

            foreach (var beastMetadata in _trackedBeasts
                         .Select(trackedBeast => trackedBeast.Value)
                         .Select(beast => Settings.Beasts.Find(b => b.Path == beast.Metadata))
                         .Where(beastMetadata => beastMetadata != null))
            {
                ImGui.TableNextRow();

                ImGui.TableNextColumn();

                ImGui.Text(Settings.BeastPrices.TryGetValue(beastMetadata.DisplayName, out var price)
                    ? $"{price.ToString(CultureInfo.InvariantCulture)}c"
                    : "0c");

                ImGui.TableNextColumn();

                ImGui.Text(beastMetadata.DisplayName);
                foreach (var craft in beastMetadata.Crafts)
                {
                    ImGui.Text(craft);
                }
            }
            ImGui.EndTable();
        }

        ImGui.End();
    }

    private void DrawFilledCircleInWorldPosition(Vector3 position, float radius, Color color)
    {
        var circlePoints = new List<Vector2>();
        const int segments = 15;
        const float segmentAngle = 2f * MathF.PI / segments;

        for (var i = 0; i < segments; i++)
        {
            var angle = i * segmentAngle;
            var currentOffset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
            var nextOffset = new Vector2(MathF.Cos(angle + segmentAngle), MathF.Sin(angle + segmentAngle)) * radius;

            var currentWorldPos = position + new Vector3(currentOffset, 0);
            var nextWorldPos = position + new Vector3(nextOffset, 0);

            circlePoints.Add(GameController.Game.IngameState.Camera.WorldToScreen(currentWorldPos));
            circlePoints.Add(GameController.Game.IngameState.Camera.WorldToScreen(nextWorldPos));
        }

        Graphics.DrawConvexPolyFilled(circlePoints.ToArray(), color with { A = Color.ToByte((int)((double)0.2f * byte.MaxValue)) });
        Graphics.DrawPolyLine(circlePoints.ToArray(), color, 2);
    }

    private void DrawLargeMap(string text, Vector2 gridPos, Color color)
    {
        if (!Settings.DrawOnMap.Value) return;

        var map = GameController.Game.IngameState.IngameUi.Map;
        var largeMap = map.LargeMap.AsObject<SubMap>();

        if (!largeMap.IsVisible)
            return;

        var mapCenter = largeMap.MapCenter;
        var mapScale = largeMap.MapScale;

        var player = GameController.Game.IngameState.Data.LocalPlayer;
        var playerPos = player.GetComponent<Positioned>()?.GridPosNum ?? default;

        var delta = gridPos - new Vector2(playerPos.X, playerPos.Y);

        const double cameraAngle = 38.7 * System.Math.PI / 180;
        float cos = (float)System.Math.Cos(cameraAngle);
        float sin = (float)System.Math.Sin(cameraAngle);

        var mapOffset = mapScale * new Vector2((delta.X - delta.Y) * cos, -(delta.X + delta.Y) * sin);
        var finalPos = mapCenter + mapOffset;

        if (Settings.MapSettings.DrawPoint)
        {
            var finalPositon = new Vector2(finalPos.X, finalPos.Y);
            Graphics.DrawCircle(
                finalPositon,
                Settings.MapSettings.Radius.Value * (mapScale / 2),
                color with { A = (byte)Settings.MapSettings.Transparency },
                Settings.MapSettings.Thickness.Value, SEGMENTS_CIRCLE);
        }

        if (Settings.MapSettings.DrawText)
        {
            Color backgroundColor = Color.Black;
            if (Settings.ColorSettings.MapColorSettings.ChangeColorText.Value)
            {
                color = Settings.ColorSettings.MapColorSettings.ColorText.Value;
                backgroundColor = Settings.ColorSettings.MapColorSettings.BackgroundColorText.Value;
            }

            var finalPosition = new Vector2(finalPos.X + Settings.MapSettings.OffsetX, finalPos.Y + Settings.MapSettings.OffsetY);
            Graphics.DrawTextWithBackground(text, finalPosition, color, backgroundColor);
        }
    }

    private bool WorldPositionOnScreenBool(Vector3 worldPos, int edgeBounds = 70)
    {
        var windowRect = GameController.Window.GetWindowRectangle();
        var screenPos = GameController.IngameState.Camera.WorldToScreen(worldPos);

        windowRect.X -= windowRect.Location.X;
        windowRect.Y -= windowRect.Location.Y;

        var result = GameController.Window.ScreenToClient((int)screenPos.X, (int)screenPos.Y) + GameController.Window.GetWindowRectangle().Location;

        var rectBounds = new SharpDX.RectangleF(
            x: windowRect.X + edgeBounds,
            y: windowRect.Y + edgeBounds,
            width: windowRect.Width - (edgeBounds * 2),
            height: windowRect.Height - (edgeBounds * 2));

        return rectBounds.Contains(result);
    }

    private Color GetColorBeastByType(BeastType type)
    {
        switch (type)
        {
            case BeastType.Vivid:
                return Settings.ColorSettings.CustomColors ? Settings.ColorSettings.VididColor : new Color(255, 250, 0);
            case BeastType.Wild:
                return Settings.ColorSettings.CustomColors ? Settings.ColorSettings.WildColor : new Color(255, 0, 235);
            case BeastType.Primal:
                return Settings.ColorSettings.CustomColors ? Settings.ColorSettings.PrimalColor : new Color(0, 245, 255);
            case BeastType.Fenumal:
                return Settings.ColorSettings.CustomColors ? Settings.ColorSettings.FenumalColor : new Color(0, 150, 40);
            case BeastType.Black:
                return Settings.ColorSettings.CustomColors ? Settings.ColorSettings.BlackColor : new Color(255, 255, 255);
            default:
                return Settings.ColorSettings.CustomColors ? Settings.ColorSettings.NormalColor : Color.Red;
        }
    }

    private BeastType GetBeastType(string beastName)
    {
        if (string.IsNullOrEmpty(beastName)) return BeastType.Normal;

        switch (beastName)
        {
            case string name when name.Contains("Vivid"):
                return BeastType.Vivid;
            case string name when name.Contains("Wild"):
                return BeastType.Wild;
            case string name when name.Contains("Primal"):
                return BeastType.Primal;
            case string name when name.Contains("Fenumal"):
                return BeastType.Fenumal;
            case string name when name.Contains("Black"):
                return BeastType.Black;
            default:
                return BeastType.Normal;
        }
    }

    enum BeastType
    {
        Normal,
        Vivid,
        Wild,
        Primal,
        Fenumal,
        Black
    }
}