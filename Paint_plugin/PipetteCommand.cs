using System;
using CommandSystem;
using Exiled.API.Features;
using UnityEngine;

namespace SCPCanvasPaint.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class PipetteCommand : ICommand
    {
        public string Command => "pipette";
        public string[] Aliases => new[] { "pipetka", "pick" };
        public string Description => "Скопировать цвет пикселя, на который вы смотрите";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (!Plugin.Singleton.CanvasManager.Sessions.TryGetValue(player, out var session))
            {
                response = "Сначала включите режим рисования медпакетом.";
                return false;
            }

            if (Physics.Raycast(player.CameraTransform.position, player.CameraTransform.forward, out RaycastHit hit, Plugin.Singleton.Config.MaxDrawDistance))
            {
                GameObject hitObj = hit.collider.gameObject;

                foreach (var canvas in Plugin.Singleton.CanvasManager.ActiveCanvases)
                {
                    for (int x = 0; x < canvas.Size; x++)
                    {
                        for (int y = 0; y < canvas.Size; y++)
                        {
                            if (canvas.Grid[x, y] != null && canvas.Grid[x, y].GameObject == hitObj)
                            {
                                Color pickedColor = canvas.Grid[x, y].Color;
                                session.CurrentColor = pickedColor;
                                session.IsEraser = false;
                                string hex = "#" + ColorUtility.ToHtmlStringRGB(pickedColor);
                                response = $"Цвет успешно скопирован: {hex}!";
                                player.ShowHint($"Пипетка: <color={hex}>█</color>", 3f);
                                return true;
                            }
                        }
                    }
                }
            }

            response = "Вы должны смотреть на пиксель холста!";
            return false;
        }
    }
}
