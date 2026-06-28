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
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;

namespace Beasts;

public partial class Beasts
{
    private const int CIRCLE_SEGMENTS = 15;
    private static readonly Vector2[] s_circleBuffer = new Vector2[CIRCLE_SEGMENTS * 2];

    public override void Render()
    {
        DrawInGameBeasts();
        //DrawBestiaryPanel();
        DrawBeastsWindow();
    }

    private void DrawInGameBeasts()
    {
        foreach (var id in _trackedBeasts.Keys)
        {
            var tracked = _trackedBeasts[id];
            if (!_enabledBeastPaths.Contains(tracked.Beast.Path)) continue;

            var entity = tracked.Entity;
            var isLive = entity.IsValid;

            if (isLive)
            {
                tracked.LastGridPos = entity.GridPosNum;
                tracked.HasValidPos = true;
            }
            else if (!tracked.HasValidPos)
            {
                continue;
            }

            var gridPos = tracked.LastGridPos;
            var beastColor = GetColorBeastByType(tracked.Type);
            if (!isLive) beastColor.A = (byte)(beastColor.A * 0.35f);

            if (Settings.DrawOnMap.Value) DrawLargeMap(tracked.Beast.DisplayName, gridPos, beastColor);

            var pos = GameController.IngameState.Data.ToWorldWithTerrainHeight(gridPos);
            if (!WorldPositionOnScreenBool(pos)) continue;

            var colorText = Settings.ColorSettings.CustomColors.Value ? Settings.ColorSettings.TextColor.Value : Color.White;
            if (!isLive) colorText.A = (byte)(colorText.A * 0.35f);
            Graphics.DrawText(tracked.Beast.DisplayName, GameController.IngameState.Camera.WorldToScreen(pos), colorText, FontAlign.Center);
            DrawFilledCircleInWorldPosition(pos, 50, beastColor);

            _trackedBeasts[id] = tracked;
        }
    }

    private void DrawBestiaryPanel()
    {
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

            for (var i = 0; i < _trackedBeastRows.Count; i++)
            {
                var beastMetadata = _trackedBeastRows[i];

                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                ImGui.Text(Settings.BeastPrices.TryGetValue(beastMetadata.DisplayName, out var price)
                    ? $"{price.ToString(CultureInfo.InvariantCulture)}c"
                    : "0c");

                ImGui.TableNextColumn();

                ImGui.Text(beastMetadata.DisplayName);

                if (!Settings.DrawCrafts.Value) continue;
                for (var j = 0; j < beastMetadata.Crafts.Length; j++)
                {
                    ImGui.Text(beastMetadata.Crafts[j]);
                }
            }
            ImGui.EndTable();
        }

        ImGui.End();
    }

    private void DrawFilledCircleInWorldPosition(Vector3 position, float radius, Color color)
    {
        const float segmentAngle = 2f * MathF.PI / CIRCLE_SEGMENTS;
        var camera = GameController.IngameState.Camera;
        var fillColor = color with { A = Color.ToByte((int)(0.2f * byte.MaxValue)) };

        for (var i = 0; i < CIRCLE_SEGMENTS; i++)
        {
            var angle = i * segmentAngle;
            var currentOffset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius;
            var nextOffset = new Vector2(MathF.Cos(angle + segmentAngle), MathF.Sin(angle + segmentAngle)) * radius;

            s_circleBuffer[i * 2] = camera.WorldToScreen(position + new Vector3(currentOffset, 0));
            s_circleBuffer[i * 2 + 1] = camera.WorldToScreen(position + new Vector3(nextOffset, 0));
        }

        Graphics.DrawConvexPolyFilled(s_circleBuffer, fillColor);
        Graphics.DrawPolyLine(s_circleBuffer, color, 2);
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
                Settings.MapSettings.Thickness.Value, CIRCLE_SEGMENTS);
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
        var rect = GameController.Window.GetWindowRectangle();
        var screenPos = GameController.IngameState.Camera.WorldToScreen(worldPos);

        return screenPos.X >= rect.X + edgeBounds
            && screenPos.X <= rect.X + rect.Width - edgeBounds
            && screenPos.Y >= rect.Y + edgeBounds
            && screenPos.Y <= rect.Y + rect.Height - edgeBounds;
    }

    private Color GetColorBeastByType(BeastType type)
    {
        if (!Settings.ColorSettings.CustomColors.Value)
        {
            return type switch
            {
                BeastType.Vivid => new Color(255, 250, 0),
                BeastType.Wild => new Color(255, 0, 235),
                BeastType.Primal => new Color(0, 245, 255),
                BeastType.Fenumal => new Color(0, 150, 40),
                BeastType.Black => new Color(255, 255, 255),
                _ => Color.Red,
            };
        }

        return type switch
        {
            BeastType.Vivid => Settings.ColorSettings.VididColor.Value,
            BeastType.Wild => Settings.ColorSettings.WildColor.Value,
            BeastType.Primal => Settings.ColorSettings.PrimalColor.Value,
            BeastType.Fenumal => Settings.ColorSettings.FenumalColor.Value,
            BeastType.Black => Settings.ColorSettings.BlackColor.Value,
            _ => Settings.ColorSettings.NormalColor.Value,
        };
    }

    private BeastType GetBeastType(string beastName)
    {
        if (string.IsNullOrEmpty(beastName)) return BeastType.Normal;

        if (beastName.Contains("Vivid")) return BeastType.Vivid;
        if (beastName.Contains("Wild")) return BeastType.Wild;
        if (beastName.Contains("Primal")) return BeastType.Primal;
        if (beastName.Contains("Fenumal")) return BeastType.Fenumal;
        if (beastName.Contains("Black")) return BeastType.Black;
        return BeastType.Normal;
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
