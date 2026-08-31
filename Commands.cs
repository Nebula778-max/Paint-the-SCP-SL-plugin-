using CommandSystem;
using Exiled.API.Features;
using MEC;
using System;
using System.Linq;
using UnityEngine;
using ICommand = CommandSystem.ICommand;
using System.Linq;
using Exiled.API.Enums;
using Exiled.API.Features.Spawn;
using Exiled.CustomItems.API.Features;

namespace SCPCanvasPaint.Commands
{
    public class SpawnSubCommand : ICommand
    {
        public string Command => "spawn";
        public string[] Aliases => new string[] { };
        public string Description => TranslationManager.Instance.Get("spawn_desc");

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (player == null) { response = TranslationManager.Instance.Get("only_players"); return false; }

            bool hasPerm = player.RemoteAdminAccess || Plugin.AllowAllPlayers || Plugin.AllowedPlayers.Contains(player.UserId);
            if (!hasPerm)
            {
                response = TranslationManager.Instance.Get("spawn_no_perms");
                return false;
            }

            if (arguments.Count < 4)
            {
                response = TranslationManager.Instance.Get("spawn_usage");
                return false;
            }

            if (!int.TryParse(arguments.At(0), out int width) || width <= 0 ||
                !int.TryParse(arguments.At(1), out int height) || height <= 0 ||
                !float.TryParse(arguments.At(2), out float physicalSize) || physicalSize <= 0f)
            {
                response = TranslationManager.Instance.Get("spawn_invalid_params");
                return false;
            }

            if (Plugin.Singleton.Config.MaxMatrixSize > 0 && (width > Plugin.Singleton.Config.MaxMatrixSize || height > Plugin.Singleton.Config.MaxMatrixSize))
            {
                response = string.Format(TranslationManager.Instance.Get("spawn_limit_matrix"), Plugin.Singleton.Config.MaxMatrixSize);
                return false;
            }
            if (Plugin.Singleton.Config.MaxPhysicalSize > 0 && physicalSize > Plugin.Singleton.Config.MaxPhysicalSize)
            {
                response = string.Format(TranslationManager.Instance.Get("spawn_limit_phys"), Plugin.Singleton.Config.MaxPhysicalSize);
                return false;
            }

            string accessType = arguments.At(3).ToLower();
            bool isPublic = accessType == "public";

            Vector3 spawnPos = player.CameraTransform.position + (player.CameraTransform.forward * 3f);
            Quaternion rotation = player.CameraTransform.rotation;

            float ratio = (float)width / height;

            Timing.RunCoroutine(Plugin.Singleton.CanvasManager.SpawnCanvasCoroutine(spawnPos, rotation, width, physicalSize, player.UserId, isPublic, null, ratio));
            if (!player.Items.Any(i => CustomItem.TryGet(i, out CustomItem cItem) && cItem.Id == Plugin.Singleton.BrushItem.Id)) Plugin.Singleton.BrushItem.Give(player);


            response = string.Format(TranslationManager.Instance.Get("spawn_success"), width, height, physicalSize, accessType);
            return true;
        }

    }
}
