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

            if (arguments.Count < 3)
            {
                response = "Используйте: .canvas spawn <размер_сетки> <физ_размер> <public/private>";
                return false;
            }

            if (!int.TryParse(arguments.At(0), out int size) || size <= 0 || !float.TryParse(arguments.At(1), out float physicalSize) || physicalSize <= 0f)
            {
                response = "Укажите корректные числовые параметры сетки и размера.";
                return false;
            }

            if (Plugin.Singleton.Config.MaxMatrixSize > 0 && size > Plugin.Singleton.Config.MaxMatrixSize)
            {
                response = $"Размер матрицы превышает лимит сервера ({Plugin.Singleton.Config.MaxMatrixSize})!";
                return false;
            }
            if (Plugin.Singleton.Config.MaxPhysicalSize > 0 && physicalSize > Plugin.Singleton.Config.MaxPhysicalSize)
            {
                response = $"Физический размер превышает лимит сервера ({Plugin.Singleton.Config.MaxPhysicalSize}м)!";
                return false;
            }

            string accessType = arguments.At(2).ToLower();
            bool isPublic = accessType == "public";

            Vector3 spawnPos = player.CameraTransform.position + (player.CameraTransform.forward * 3f);
            Quaternion rotation = player.CameraTransform.rotation;

            Timing.RunCoroutine(Plugin.Singleton.CanvasManager.SpawnCanvasCoroutine(spawnPos, rotation, size, physicalSize, player.UserId, isPublic));
            Plugin.Singleton.BrushItem.Give(player);

            response = $"Холст {size}x{size} ({physicalSize}м, доступ: {accessType}) создается. Кисть выдана.";
            return true;
        }

    }
}
