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
        public string Description => TranslationManager.Instance.Get("grab_desc");

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (player == null) { response = TranslationManager.Instance.Get("only_players"); return false; }

            if (!Plugin.Singleton.CanvasManager.Sessions.TryGetValue(player, out var session))
            {
                session = new PlayerPaintSession();
                Plugin.Singleton.CanvasManager.Sessions[player] = session;
            }

            if (session.GrabbedCanvas != null)
            {
                session.GrabbedCanvas = null;
                response = TranslationManager.Instance.Get("grab_release");
                return true;
            }

            if (player.CurrentItem == null || !CustomItem.TryGet(player.CurrentItem, out CustomItem cBrush) || cBrush.Id != Plugin.Singleton.BrushItem.Id)
            {
                response = TranslationManager.Instance.Get("grab_need_brush");
                return false;
            }

            if (!Physics.Raycast(player.CameraTransform.position, player.CameraTransform.forward, out RaycastHit hit, 15f))
            {
                response = TranslationManager.Instance.Get("grab_look_at");
                return false;
            }

            if (!hit.collider.gameObject.TryGetComponent<CanvasPixelData>(out var pixelData))
            {
                response = TranslationManager.Instance.Get("grab_precision");
                return false;
            }

            CanvasInstance canvas = pixelData.Canvas;
            if (canvas == null) { response = "Холст не найден!"; return false; }

            foreach (var s in Plugin.Singleton.CanvasManager.Sessions.Values)
            {
                if (s.GrabbedCanvas == canvas)
                {
                    response = TranslationManager.Instance.Get("grab_already_holding");
                    return false;
                }
            }

            bool isTrusted = Plugin.TrustedArtists.TryGetValue(canvas.OwnerId, out var artists) && artists.Contains(player.UserId);
            if (!canvas.IsPublic && canvas.OwnerId != player.UserId && !isTrusted && !player.RemoteAdminAccess)
            {
                response = TranslationManager.Instance.Get("grab_private_error");
                return false;
            }

            session.GrabbedCanvas = canvas;
            session.GrabOffset = player.CameraTransform.worldToLocalMatrix.MultiplyPoint3x4(canvas.RootObject.transform.position);

            Plugin.Singleton.EventHandler.CheckAndStartRaycast(player, session);


            response = TranslationManager.Instance.Get("grab_success");
            return true;
        }
    }
}
