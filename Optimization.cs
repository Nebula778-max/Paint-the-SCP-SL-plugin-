using UnityEngine;
using Exiled.API.Features;
using System.Collections.Generic;

namespace SCPCanvasPaint
{
    public class CanvasDrawZone : MonoBehaviour
    {
        public CanvasInstance Canvas;

        private void OnTriggerEnter(Collider other)
        {
            Player player = Player.Get(other.gameObject);
            if (player == null || player.IsDead || player.IsOverwatchEnabled) return;

            if (!CanvasTriggerZone.PlayersInZone.Contains(player))
            {
                CanvasTriggerZone.PlayersInZone.Add(player);

                if (Plugin.Singleton.CanvasManager.Sessions.TryGetValue(player, out var session) && session.IsDrawing)
                {
                    Plugin.Singleton.EventHandler.CheckAndStartRaycast(player, session);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Player player = Player.Get(other.gameObject);
            if (player != null)
            {
                CanvasTriggerZone.PlayersInZone.Remove(player);
            }
        }
    }

    public class CanvasRenderZone : MonoBehaviour
    {
        public CanvasInstance Canvas;
        private readonly HashSet<Player> viewers = new HashSet<Player>();
        private bool isRendered = true;

        private void OnTriggerEnter(Collider other)
        {
            Player player = Player.Get(other.gameObject);
            if (player == null || player.IsDead || player.IsOverwatchEnabled) return;

            viewers.Add(player);

            if (!isRendered && Canvas != null)
            {
                isRendered = true;
                ToggleRender(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Player player = Player.Get(other.gameObject);
            if (player == null) return;

            viewers.Remove(player);

            if (viewers.Count == 0 && isRendered && Canvas != null)
            {
                isRendered = false;
                ToggleRender(false);
            }
        }

        private void OnDestroy()
        {
            viewers.Clear();
        }

        private void ToggleRender(bool shouldSpawn)
        {
            int width = Canvas.Size;
            int height = Canvas.Ratio >= 1f ? Mathf.RoundToInt(Canvas.Size / Canvas.Ratio) : Canvas.Size;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var primitive = Canvas.Grid[x, y];
                    if (primitive == null) continue;

                    if (primitive.Base == null || primitive.Base.gameObject == null) continue;

                    if (shouldSpawn)
                    {
                        if (primitive.Position.y > -500f)
                        {
                            Mirror.NetworkServer.Spawn(primitive.Base.gameObject);
                        }
                    }
                    else
                    {
                        Mirror.NetworkServer.UnSpawn(primitive.Base.gameObject);
                    }
                }
            }
        }
    }

    public static class CanvasTriggerZone
    {
        public static HashSet<Player> PlayersInZone = new HashSet<Player>();
    }
}
