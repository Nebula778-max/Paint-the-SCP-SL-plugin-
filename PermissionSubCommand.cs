using System;
using CommandSystem;
using Exiled.API.Features;

namespace SCPCanvasPaint.Commands
{
    public class PermissionSubCommand : ICommand
    {
        public string Command => "permission";
        public string[] Aliases => new[] { "perm" };
        public string Description => TranslationManager.Instance.Get("perm_desc");

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is Player playerSender && !playerSender.RemoteAdminAccess)
            {
                response = TranslationManager.Instance.Get("no_admin_perms");
                return false;
            }

            if (arguments.Count < 1)
            {
                response = TranslationManager.Instance.Get("perm_usage");
                return false;
            }

            string target = arguments.At(0).ToLower();

            if (target == "all")
            {
                Plugin.AllowAllPlayers = true;
                Plugin.AllowedPlayers.Clear();
                response = TranslationManager.Instance.Get("perm_all");
                return true;
            }

            if (target == "none")
            {
                Plugin.AllowAllPlayers = false;
                Plugin.AllowedPlayers.Clear();
                response = TranslationManager.Instance.Get("perm_none");
                return true;
            }

            Player targetPlayer = Player.Get(target);
            if (targetPlayer == null)
            {
                response = string.Format(TranslationManager.Instance.Get("player_not_found"), target);
                return false;
            }

            if (Plugin.AllowedPlayers.Contains(targetPlayer.UserId))
            {
                Plugin.AllowedPlayers.Remove(targetPlayer.UserId);
                response = string.Format(TranslationManager.Instance.Get("perm_revoked"), targetPlayer.Nickname);
            }

            {
                Plugin.AllowAllPlayers = false;
                Plugin.AllowedPlayers.Add(targetPlayer.UserId);
                response = string.Format(TranslationManager.Instance.Get("perm_granted"), targetPlayer.Nickname);
            }

            return true;
        }
    }
}
