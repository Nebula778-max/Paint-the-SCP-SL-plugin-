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
        public string Description => "Сохранить холст по взгляду: .canvas save <имя_файла>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (player == null) { response = "Только для игроков!"; return false; }
            if (!player.RemoteAdminAccess)
            {
                response = "Только администраторы могут сохранять холсты!";
                return false;
            }
            if (arguments.Count < 1) { response = "Укажите имя для сохранения!"; return false; }

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
                    response = $"Холст успешно сохранен в SavedCanvases/{name}.json!";
                    return true;
                }
            }


            response = "Вы должны смотреть на холст, чтобы сохранить его!";
            return false;
        }

    }
}
