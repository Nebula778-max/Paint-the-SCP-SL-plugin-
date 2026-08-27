using System.Collections.Generic;
using UnityEngine;
using Exiled.API.Features;
using Exiled.API.Features.Toys;
using Exiled.Events.EventArgs.Player;
using MEC;
using Exiled.CustomItems.API.Features;

namespace SCPCanvasPaint
{
    public class EventHandler
    {
        private readonly CanvasManager manager;

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
                    session.DrawCoroutine = Timing.RunCoroutine(DrawRaycastLoop(ev.Player, session));
                }
                else
                {
                    ev.Player.ShowHint("<color=red>Режим рисования выключен!</color>", 3f);
                    Timing.KillCoroutines(session.DrawCoroutine);
                }
            }
        }

        public void OnUsingItem(UsingItemEventArgs ev)
        {
            if (CustomItem.TryGet(ev.Item, out CustomItem customItem) && customItem.Id == Plugin.Singleton.BrushItem.Id)
            {
                if (manager.Sessions.TryGetValue(ev.Player, out var session) && session.IsDrawing)
                {
                    ev.IsAllowed = false;

                    List<Color> fullPalette = new List<Color>(manager.ColorPalette);
                    fullPalette.AddRange(session.CustomPalette);

                    int currentIndex = fullPalette.FindIndex(c => c == session.CurrentColor);
                    int nextIndex = (currentIndex + 1) % fullPalette.Count;
                    session.CurrentColor = fullPalette[nextIndex];

                    string colorName = manager.ColorPalette.Contains(session.CurrentColor) ? "█" : "Кастомный █";
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
            }
        }

        private IEnumerator<float> DrawRaycastLoop(Player player, PlayerPaintSession session)
        {
            while (session.IsDrawing)
            {
                if (player.CurrentItem == null || !CustomItem.TryGet(player.CurrentItem, out CustomItem customItem) || customItem.Id != Plugin.Singleton.BrushItem.Id)
                {
                    player.ShowHint("<color=red>Режим рисования приостановлен! Возьмите кисть в руки.</color>", 1f);
                    yield return Timing.WaitForSeconds(0.5f);
                    continue;
                }

                Transform cam = player.CameraTransform;
                if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, Plugin.Singleton.Config.MaxDrawDistance))
                {
                    GameObject hitObj = hit.collider.gameObject;
                    CanvasInstance targetCanvas = null;
                    Primitive targetPrimitive = null;

                    foreach (var canvas in manager.ActiveCanvases)
                    {
                        string[] coords = hitObj.name.Split('_');
                        if (coords.Length == 2 && int.TryParse(coords[0], out int pX) && int.TryParse(coords[1], out int pY))
                        {
                            if (pX >= 0 && pX < canvas.Size && pY >= 0 && pY < canvas.Size)
                            {
                                if (canvas.Grid[pX, pY] != null && canvas.Grid[pX, pY].GameObject == hitObj)
                                {
                                    targetCanvas = canvas;
                                    targetPrimitive = canvas.Grid[pX, pY];
                                    break;
                                }
                            }
                        }

                        for (int x = 0; x < canvas.Size; x++)
                        {
                            for (int y = 0; y < canvas.Size; y++)
                            {
                                if (canvas.Grid[x, y] != null && canvas.Grid[x, y].GameObject == hitObj)
                                {
                                    targetCanvas = canvas;
                                    targetPrimitive = canvas.Grid[x, y];
                                    break;
                                }
                            }
                            if (targetCanvas != null) break;
                        }

                        if (targetCanvas != null) break;
                    }

                    if (targetCanvas != null && targetPrimitive != null)
                    {
                        if (!targetCanvas.IsPublic && targetCanvas.OwnerId != player.UserId)
                        {
                            player.ShowHint("<color=red>Этот холст приватный! Вы не можете на нем рисовать.</color>", 1f);
                        }
                        else
                        {
                            ApplyBrush(targetCanvas, targetPrimitive, session, player);
                        }

                        yield return Timing.WaitForSeconds(Plugin.Singleton.Config.DrawInterval);
                        continue;
                    }
                }
                yield return Timing.WaitForSeconds(Plugin.Singleton.Config.DrawInterval);
            }
        }

        private void ApplyBrush(CanvasInstance canvas, Primitive hitPrimitive, PlayerPaintSession session, Player player)
        {
            int targetX = -1, targetY = -1;
            for (int x = 0; x < canvas.Size; x++)
            {
                for (int y = 0; y < canvas.Size; y++)
                {
                    if (canvas.Grid[x, y] == hitPrimitive) { targetX = x; targetY = y; break; }
                }
                if (targetX != -1) break;
            }

            if (targetX == -1) return;

            if (session.IsEraser && !canvas.IsPublic && canvas.OwnerId != player.UserId && !player.RemoteAdminAccess)
            {
                player.ShowHint("<color=red>Нельзя стирать пиксели на чужом приватном холсте!</color>", 1f);
                return;
            }


            Dictionary<Primitive, Color> changedPixels = new Dictionary<Primitive, Color>();
            int radius = session.BrushSize - 1;
            float offset = canvas.PhysicalSize / canvas.Size;
            float startOffset = -canvas.PhysicalSize / 2f;


            for (int x = targetX - radius; x <= targetX + radius; x++)
            {
                for (int y = targetY - radius; y <= targetY + radius; y++)
                {
                    if (x >= 0 && x < canvas.Size && y >= 0 && y < canvas.Size)
                    {
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
                            Vector3 localPos = new Vector3(startOffset + (x * offset) + (offset / 2f), startOffset + (y * offset) + (offset / 2f), 0);
                            Vector3 originalWorldPos = canvas.RootObject.transform.TransformPoint(localPos);

                            if (p.Position.y < -500f || p.Color != session.CurrentColor)
                            {
                                if (!changedPixels.ContainsKey(p)) changedPixels[p] = p.Position.y < -500f ? Color.clear : p.Color;

                                p.Position = originalWorldPos;
                                p.Color = session.CurrentColor;
                            }
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
