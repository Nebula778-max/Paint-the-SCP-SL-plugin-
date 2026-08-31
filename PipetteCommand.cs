using System;
using CommandSystem;
using Exiled.API.Features;
using UnityEngine;

namespace SCPCanvasPaint.Commands
{

    public class PipetteSubCommand : ICommand
    {
        public string Command => "pipette";
        public string[] Aliases => new[] { "pipetka", "pick" };
        public string Description => TranslationManager.Instance.Get("pipette_desc");

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (!Plugin.Singleton.CanvasManager.Sessions.TryGetValue(player, out var session))
            {
                response = TranslationManager.Instance.Get("need_mode");
                return false;
            }

            if (Physics.Raycast(player.CameraTransform.position, player.CameraTransform.forward, out RaycastHit hit, Plugin.Singleton.Config.MaxDrawDistance))
            {
                GameObject hitObj = hit.collider.gameObject;

                foreach (var canvas in Plugin.Singleton.CanvasManager.ActiveCanvases)
                {
                    int width = canvas.Grid.GetLength(0);
                    int height = canvas.Grid.GetLength(1);
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            if (canvas.Grid[x, y] != null && canvas.Grid[x, y].Base.gameObject == hitObj)
                            {
                                Color pickedColor = canvas.Grid[x, y].Color;
                                session.CurrentColor = pickedColor;
                                session.IsEraser = false;
                                string hex = "#" + ColorUtility.ToHtmlStringRGB(pickedColor);
                                response = string.Format(TranslationManager.Instance.Get("pipette_success"), hex);
                                player.ShowHint(string.Format(TranslationManager.Instance.Get("pipette_hint"), hex), 3f);
                                return true;
                            }
                        }
                    }
                }
            }

            response = TranslationManager.Instance.Get("pipette_fail");
            return false;
        }
    }
}
