using System;
using CommandSystem;
using Exiled.API.Features;
using MEC;
using UnityEngine;
using ICommand = CommandSystem.ICommand;
namespace SCPCanvasPaint.Commands
{
    public class SpawnSubCommand : ICommand
    {
        public string Command => "spawn";
        public string[] Aliases => new string[] { };
        public string Description => "Заспавнить холст перед собой: .canvas spawn <размер>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (player == null) { response = "Команда доступна только игрокам!"; return false; }

            bool hasPerm = player.RemoteAdminAccess || Plugin.AllowAllPlayers || Plugin.AllowedPlayers.Contains(player.UserId);
            if (!hasPerm)
            {
                response = "У вас нет прав на создание холстов!";
                return false;
            }

            if (arguments.Count < 4)
            {
                response = "Используйте: .canvas spawn <ширина_сетки> <высота_сетки> <физ_размер> <public/private>";
                return false;
            }

            if (!int.TryParse(arguments.At(0), out int width) || width <= 0 ||
                !int.TryParse(arguments.At(1), out int height) || height <= 0 ||
                !float.TryParse(arguments.At(2), out float physicalSize) || physicalSize <= 0f)
            {
                response = "Укажите корректные числовые параметры сетки и размера.";
                return false;
            }

            if (Plugin.Singleton.Config.MaxMatrixSize > 0 && (width > Plugin.Singleton.Config.MaxMatrixSize || height > Plugin.Singleton.Config.MaxMatrixSize))
            {
                response = $"Размер матрицы превышает лимит сервера ({Plugin.Singleton.Config.MaxMatrixSize})!";
                return false;
            }
            if (Plugin.Singleton.Config.MaxPhysicalSize > 0 && physicalSize > Plugin.Singleton.Config.MaxPhysicalSize)
            {
                response = $"Физический размер превышает лимит сервера ({Plugin.Singleton.Config.MaxPhysicalSize}м)!";
                return false;
            }

            string accessType = arguments.At(3).ToLower();
            bool isPublic = accessType == "public";

            Vector3 spawnPos = player.CameraTransform.position + (player.CameraTransform.forward * 3f);
            Quaternion rotation = player.CameraTransform.rotation;

            float ratio = (float)width / height;

            Timing.RunCoroutine(Plugin.Singleton.CanvasManager.SpawnCanvasCoroutine(spawnPos, rotation, width, physicalSize, player.UserId, isPublic, null, ratio));
            Plugin.Singleton.BrushItem.Give(player);

            response = $"Прямоугольный холст {width}x{height} ({physicalSize}м, доступ: {accessType}) создается. Кисть выдана.";
            return true;
        }

    }
}
