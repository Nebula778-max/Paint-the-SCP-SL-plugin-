using CommandSystem;
using Exiled.API.Features;
using Exiled.CustomItems.API.Features;
using SCPCanvasPaint.SCPCanvasPaint;
using System;
using UnityEngine;

namespace SCPCanvasPaint.Commands
{
    public class GrabSubCommand : ICommand
    {
        public string Command => "grab";
        public string[] Aliases => new[] { "move", "hold" };
        public string Description => "Взять или отпустить холст перед собой";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (player == null) { response = "Только для игроков!"; return false; }

            if (!Plugin.Singleton.CanvasManager.Sessions.TryGetValue(player, out var session))
            {
                session = new PlayerPaintSession();
                Plugin.Singleton.CanvasManager.Sessions[player] = session;
            }

            if (session.GrabbedCanvas != null)
            {
                session.GrabbedCanvas = null;
                response = "Вы успешно отпустили и зафиксировали холст!";
                return true;
            }

            if (player.CurrentItem == null || !CustomItem.TryGet(player.CurrentItem, out CustomItem cBrush) || cBrush.Id != Plugin.Singleton.BrushItem.Id)
            {
                response = "Для захвата холста необходимо держать кисть художника в руках!";
                return false;
            }

            if (!Physics.Raycast(player.CameraTransform.position, player.CameraTransform.forward, out RaycastHit hit, 15f))
            {
                response = "Вы должны смотреть на холст, чтобы схватить его!";
                return false;
            }

            if (!hit.collider.gameObject.TryGetComponent<CanvasPixelData>(out var pixelData))
            {
                response = "Наведитесь точнее на пиксель холста!";
                return false;
            }

            CanvasInstance canvas = pixelData.Canvas;
            if (canvas == null) { response = "Холст не найден!"; return false; }

            foreach (var s in Plugin.Singleton.CanvasManager.Sessions.Values)
            {
                if (s.GrabbedCanvas == canvas)
                {
                    response = "Этот холст уже перетаскивает другой игрок!";
                    return false;
                }
            }

            bool isTrusted = Plugin.TrustedArtists.TryGetValue(canvas.OwnerId, out var artists) && artists.Contains(player.UserId);
            if (!canvas.IsPublic && canvas.OwnerId != player.UserId && !isTrusted && !player.RemoteAdminAccess)
            {
                response = "Вы не можете двигать этот холст, так как он приватный!";
                return false;
            }

            session.GrabbedCanvas = canvas;
            session.GrabOffset = player.CameraTransform.worldToLocalMatrix.MultiplyPoint3x4(canvas.RootObject.transform.position);

            Plugin.Singleton.EventHandler.CheckAndStartRaycast(player, session);


            response = "Холст захвачен! Он будет следовать за движениями вашей камеры, пока вы держите кисть.";
            return true;
        }
    }
}
