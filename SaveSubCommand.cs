using System;
using UnityEngine;
using CommandSystem;
using Exiled.API.Features;
using Exiled.API.Features.Toys;
using ICommand = CommandSystem.ICommand;

namespace SCPCanvasPaint.Commands
{
    public class SaveSubCommand : ICommand
    {
        public string Command => "save";
        public string[] Aliases => new string[] { };
        public string Description => TranslationManager.Instance.Get("save_desc");

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (player == null) { response = TranslationManager.Instance.Get("only_players"); return false; }
            if (!player.RemoteAdminAccess)
            {
                response = TranslationManager.Instance.Get("only_admins_save");
                return false;
            }
            if (arguments.Count < 1) { response = TranslationManager.Instance.Get("save_name_needed"); return false; }

            string name = arguments.At(0);

            if (Physics.Raycast(player.CameraTransform.position, player.CameraTransform.forward, out RaycastHit hit, 15f))
            {
                GameObject hitObj = hit.collider.gameObject;

                var canvas = Plugin.Singleton.CanvasManager.ActiveCanvases.Find(c =>
                {
                    if (c.Grid == null) return false;
                    int width = c.Grid.GetLength(0);
                    int height = c.Grid.GetLength(1);

                    for (int x = 0; x < width; x++)
                        for (int y = 0; y < height; y++)
                            if (c.Grid[x, y] != null && c.Grid[x, y].GameObject == hitObj) return true;
                    return false;
                });


                if (canvas != null)
                {
                    Plugin.Singleton.CanvasManager.SaveCanvas(canvas, name);
                    response = string.Format(TranslationManager.Instance.Get("save_success"), name);
                    return true;
                }
            }


            response = TranslationManager.Instance.Get("save_look_at");
            return false;
        }

    }
}
