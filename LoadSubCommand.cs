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
        public string Description => TranslationManager.Instance.Get("load_desc");

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (player == null) { response = TranslationManager.Instance.Get("only_players"); return false; }

            if (!player.RemoteAdminAccess)
            {
                response = TranslationManager.Instance.Get("only_admins_load");
                return false;
            }

            if (arguments.Count < 1) { response = TranslationManager.Instance.Get("load_name_needed"); return false; }

            string name = arguments.At(0);
            Vector3 spawnPos = player.CameraTransform.position + (player.CameraTransform.forward * 4f);
            Quaternion rotation = player.CameraTransform.rotation;

            if (Plugin.Singleton.CanvasManager.LoadCanvas(name, spawnPos, rotation, player.UserId))
            {
                response = string.Format(TranslationManager.Instance.Get("load_success"), name);
                return true;
            }

            response = string.Format(TranslationManager.Instance.Get("load_fail"), name);
            return false;
        }

    }
}
