using System;
using UnityEngine;
using CommandSystem;
using Exiled.API.Features;
using ICommand = CommandSystem.ICommand;

namespace SCPCanvasPaint.Commands
{
    public class LoadSubCommand : ICommand
    {
        public string Command => "load";
        public string[] Aliases => new string[] { };
        public string Description => "Загрузить холст из файла перед собой: .canvas load <имя_файла>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (player == null) { response = "Только для игроков!"; return false; }

            if (!player.RemoteAdminAccess)
            {
                response = "Только администраторы могут загружать холсты!";
                return false;
            }

            if (arguments.Count < 1) { response = "Укажите имя файла для загрузки!"; return false; }

            string name = arguments.At(0);
            Vector3 spawnPos = player.CameraTransform.position + (player.CameraTransform.forward * 4f);
            Quaternion rotation = player.CameraTransform.rotation;

            if (Plugin.Singleton.CanvasManager.LoadCanvas(name, spawnPos, rotation, player.UserId))
            {
                response = $"Холст '{name}' успешно найден и начинает рендериться!";
                return true;
            }

            response = $"Файл с сохраненным холстом 'SavedCanvases/{name}.json' не существует.";
            return false;
        }

    }
}
