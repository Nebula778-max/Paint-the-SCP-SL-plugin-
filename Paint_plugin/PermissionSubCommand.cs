using System;
using CommandSystem;
using Exiled.API.Features;

namespace SCPCanvasPaint.Commands
{
    public class PermissionSubCommand : ICommand
    {
        public string Command => "permission";
        public string[] Aliases => new[] { "perm" };
        public string Description => "Управление правами создания холстов: .canvas permission [all/none/ник]";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is Player playerSender && !playerSender.RemoteAdminAccess)
            {
                response = "У вас нет прав администратора!";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "Используйте: .canvas permission [all / none / ник]";
                return false;
            }

            string target = arguments.At(0).ToLower();

            if (target == "all")
            {
                Plugin.AllowAllPlayers = true;
                Plugin.AllowedPlayers.Clear();
                response = "Права выданы ВСЕМ игрокам.";
                return true;
            }

            if (target == "none")
            {
                Plugin.AllowAllPlayers = false;
                Plugin.AllowedPlayers.Clear();
                response = "Права обычных игроков сброшены. Доступ только у админов.";
                return true;
            }

            Player targetPlayer = Player.Get(target);
            if (targetPlayer == null)
            {
                response = $"Игрок '{target}' не найден.";
                return false;
            }

            if (Plugin.AllowedPlayers.Contains(targetPlayer.UserId))
            {
                Plugin.AllowedPlayers.Remove(targetPlayer.UserId);
                response = $"Права на создание холстов удалены у {targetPlayer.Nickname}.";
            }

            {
                Plugin.AllowAllPlayers = false;
                Plugin.AllowedPlayers.Add(targetPlayer.UserId);
                response = $"Игрок {targetPlayer.Nickname} получил права на создание холстов!";
            }

            return true;
        }
    }
}
