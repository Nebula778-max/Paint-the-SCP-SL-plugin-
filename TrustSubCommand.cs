using CommandSystem;
using Exiled.API.Features;
using System;
using System.Collections.Generic;

namespace SCPCanvasPaint.Commands
{
    public class TrustSubCommand : ICommand
    {
        public string Command => "trust";
        public string[] Aliases => new[] { "addartist", "friend" };
        public string Description => "Разрешить игроку рисовать на ваших приватных холстах";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player owner = Player.Get(sender);
            if (owner == null) { response = "Только для игроков!"; return false; }

            bool hasPrivateCanvas = false;
            foreach (var canvas in Plugin.Singleton.CanvasManager.ActiveCanvases)
            {
                if (!canvas.IsPublic && canvas.OwnerId == owner.UserId)
                {
                    hasPrivateCanvas = true;
                    break;
                }
            }

            if (!hasPrivateCanvas)
            {
                response = "У вас должен быть хотя бы один созданный приватный холст, чтобы использовать эту команду!";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "Используйте: .canvas trust [ник/id]";
                return false;
            }

            Player targetPlayer = Player.Get(arguments.At(0));
            if (targetPlayer == null)
            {
                response = $"Игрок '{arguments.At(0)}' не найден.";
                return false;
            }

            if (targetPlayer == owner)
            {
                response = "Вы не можете добавить самого себя в белый список.";
                return false;
            }

            if (!Plugin.TrustedArtists.ContainsKey(owner.UserId))
            {
                Plugin.TrustedArtists[owner.UserId] = new HashSet<string>();
            }

            if (Plugin.TrustedArtists[owner.UserId].Contains(targetPlayer.UserId))
            {
                Plugin.TrustedArtists[owner.UserId].Remove(targetPlayer.UserId);
                response = $"Игрок {targetPlayer.Nickname} удален из вашего белого списка художников.";
                targetPlayer.ShowHint($"<color=red>{owner.Nickname} запретил вам рисовать на его холстах.</color>", 5f);
            }
            else
            {
                Plugin.TrustedArtists[owner.UserId].Add(targetPlayer.UserId);
                response = $"Игрок {targetPlayer.Nickname} теперь может рисовать на всех ваших приватных холстах!";
                targetPlayer.ShowHint($"<color=green>{owner.Nickname} разрешил вам рисовать на его приватных холстах!</color>", 5f);
            }

            return true;
        }
    }
}
