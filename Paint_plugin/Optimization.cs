using UnityEngine;
using Exiled.API.Features;
using System.Collections.Generic;

namespace SCPCanvasPaint
{
    public class CanvasTriggerZone : MonoBehaviour
    {
        public CanvasInstance Canvas;
        public static HashSet<Player> PlayersInZone = new HashSet<Player>();

        private void OnTriggerEnter(Collider other)
        {
            Player player = Player.Get(other.gameObject);
            if (player != null && !PlayersInZone.Contains(player))
            {
                PlayersInZone.Add(player);

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
                PlayersInZone.Remove(player);
            }
        }
    }
}
