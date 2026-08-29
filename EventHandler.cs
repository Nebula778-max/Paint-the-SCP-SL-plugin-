using Exiled.API.Features;
using Exiled.API.Features.Toys;
using Exiled.CustomItems.API.Features;
using Exiled.Events.EventArgs.Player;
using MEC;
using SCPCanvasPaint.SCPCanvasPaint;
using System.Collections.Generic;
using UnityEngine;

namespace SCPCanvasPaint
{
    public class EventHandler
    {
        private readonly CanvasManager manager;

        private readonly Dictionary<Player, float> colorSwitchCooldowns = new Dictionary<Player, float>();

        public EventHandler(CanvasManager manager) => this.manager = manager;

        public void OnDroppingItem(DroppingItemEventArgs ev)
        {
            if (CustomItem.TryGet(ev.Item, out CustomItem customItem) && customItem.Id == Plugin.Singleton.BrushItem.Id)
            {
                ev.IsAllowed = false;

                if (!manager.Sessions.TryGetValue(ev.Player, out var session))
                {
                    session = new PlayerPaintSession();
                    manager.Sessions[ev.Player] = session;
                }

                session.IsDrawing = !session.IsDrawing;

                if (session.IsDrawing)
                {
                    ev.Player.ShowHint("<color=green>Режим рисования включен!</color>", 3f);
                    CheckAndStartRaycast(ev.Player, session);
                }
                else
                {
                    ev.Player.ShowHint("<color=red>Режим рисования выключен!</color>", 3f);
                    Timing.KillCoroutines(session.DrawCoroutine);
                }
            }
        }

        public void CheckAndStartRaycast(Player player, PlayerPaintSession session)
        {
            if (!session.DrawCoroutine.IsRunning && CanvasTriggerZone.PlayersInZone.Contains(player))
            {
                session.DrawCoroutine = Timing.RunCoroutine(DrawRaycastLoop(player, session));
            }
        }


        public void OnUsingItem(UsingItemEventArgs ev)
        {
            if (CustomItem.TryGet(ev.Item, out CustomItem customItem) && customItem.Id == Plugin.Singleton.BrushItem.Id)
            {
                ev.IsAllowed = false;

                if (manager.Sessions.TryGetValue(ev.Player, out var session) && session.IsDrawing)
                {
                    if (colorSwitchCooldowns.TryGetValue(ev.Player, out float lastTime) && UnityEngine.Time.time - lastTime < 0.3f)
                        return;

                    colorSwitchCooldowns[ev.Player] = UnityEngine.Time.time;

                    int basePaletteCount = manager.ColorPalette.Count;
                    int customPaletteCount = session.CustomPalette.Count;
                    int totalCount = basePaletteCount + customPaletteCount;

                    int currentIndex = manager.ColorPalette.FindIndex(c => c == session.CurrentColor);
                    if (currentIndex == -1)
                    {
                        currentIndex = session.CustomPalette.FindIndex(c => c == session.CurrentColor);
                        if (currentIndex != -1) currentIndex += basePaletteCount;
                    }

                    int nextIndex = (currentIndex + 1) % totalCount;

                    Color nextColor = nextIndex < basePaletteCount
    ? manager.ColorPalette[nextIndex]
    : session.CustomPalette[nextIndex - basePaletteCount];

                    session.CurrentColor = nextColor;

                    string colorName = nextIndex < basePaletteCount ? "█" : "Кастомный █";
                    ev.Player.ShowHint($"Выбран цвет: <color=#{ColorUtility.ToHtmlStringRGB(session.CurrentColor)}>{colorName}</color>", 2f);
                }
            }
        }


        public void OnPlayerLeft(LeftEventArgs ev)
        {
            if (manager.Sessions.TryGetValue(ev.Player, out var session))
            {
                Timing.KillCoroutines(session.DrawCoroutine);
                manager.Sessions.Remove(ev.Player);
                CanvasTriggerZone.PlayersInZone.Remove(ev.Player);
                colorSwitchCooldowns.Remove(ev.Player);
            }
        }

        private IEnumerator<float> DrawRaycastLoop(Player player, PlayerPaintSession session)
        {
            while (session.IsDrawing)
            {
                if (!CanvasTriggerZone.PlayersInZone.Contains(player))
                {
                    yield break;
                }

                if (player.CurrentItem == null || !CustomItem.TryGet(player.CurrentItem, out CustomItem customItem) || customItem.Id != Plugin.Singleton.BrushItem.Id)
                {
                    player.ShowHint("<color=red>Режим рисования приостановлен! Возьмите кисть в руки.</color>", 1f);
                    yield return Timing.WaitForSeconds(0.5f);
                    continue;
                }

                Transform cam = player.CameraTransform;
                if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, Plugin.Singleton.Config.MaxDrawDistance, 1 << 0))
                {
                    if (hit.collider.gameObject.TryGetComponent<CanvasPixelData>(out var pixelData))
                    {
                        CanvasInstance targetCanvas = pixelData.Canvas;
                        Primitive targetPrimitive = targetCanvas.Grid[pixelData.X, pixelData.Y];

                        if (targetCanvas != null && targetPrimitive != null)
                        {
                            bool isTrusted = Plugin.TrustedArtists.TryGetValue(targetCanvas.OwnerId, out var artists) && artists.Contains(player.UserId);
                            if (!targetCanvas.IsPublic && targetCanvas.OwnerId != player.UserId && !isTrusted)
                            {
                                player.ShowHint("<color=red>Этот холст приватный! Вы не можете на нем рисовать.</color>", 1f);
                            }

                            else
                            {
                                ApplyBrushDirect(targetCanvas, pixelData.X, pixelData.Y, session, player);
                            }
                        }
                    }
                }
                yield return Timing.WaitForSeconds(Plugin.Singleton.Config.DrawInterval);
            }
        }


        private void ApplyBrushDirect(CanvasInstance canvas, int targetX, int targetY, PlayerPaintSession session, Player player)
        {
            bool isTrustedEraser = Plugin.TrustedArtists.TryGetValue(canvas.OwnerId, out var eraserArtists) && eraserArtists.Contains(player.UserId);
            if (session.IsEraser && !canvas.IsPublic && canvas.OwnerId != player.UserId && !player.RemoteAdminAccess && !isTrustedEraser)
            {
                player.ShowHint("<color=red>Нельзя стирать пиксели на чужом приватном холсте!</color>", 1f);
                return;
            }


            Dictionary<Primitive, Color> changedPixels = new Dictionary<Primitive, Color>();
            int radiusX = session.BrushSize - 1;
            int radiusY = Mathf.RoundToInt((session.BrushSize - 1) * canvas.Ratio);
            float offset = canvas.PhysicalSize / canvas.Size;
            float startOffsetX = -canvas.PhysicalSize / 2f;
            float startOffsetY = -(canvas.PhysicalSize / canvas.Ratio) / 2f;
            Transform rootTransform = canvas.RootObject.transform;

            int canvasHeight = canvas.Ratio >= 1f ? Mathf.RoundToInt(canvas.Size / canvas.Ratio) : canvas.Size;

            for (int x = targetX - radiusX; x <= targetX + radiusX; x++)
            {
                if (x < 0 || x >= canvas.Size) continue;

                for (int y = targetY - radiusY; y <= targetY + radiusY; y++)
                {
                    if (y < 0 || y >= canvasHeight) continue;


                    Primitive p = canvas.Grid[x, y];
                    if (p == null) continue;

                    if (session.IsEraser)
                    {
                        if (p.Position.y > -500f)
                        {
                            if (!changedPixels.ContainsKey(p)) changedPixels[p] = p.Color;
                            p.Position = new Vector3(0f, -1000f, 0f);
                        }
                    }
                    else
                    {
                        if (p.Position.y < -500f || p.Color != session.CurrentColor)
                        {
                            if (!changedPixels.ContainsKey(p)) changedPixels[p] = p.Position.y < -500f ? Color.clear : p.Color;

                            Vector3 localPos = new Vector3(startOffsetX + (x * offset) + (offset / 2f), startOffsetY + (y * offset * (1f / canvas.Ratio)) + ((offset * (1f / canvas.Ratio)) / 2f), 0);
                            p.Position = rootTransform.TransformPoint(localPos);
                            p.Color = session.CurrentColor;
                        }
                    }
                }
            }

            if (changedPixels.Count > 0)
            {
                session.UndoStack.Push(changedPixels);
                session.RedoStack.Clear();
            }
        }
    }
}
