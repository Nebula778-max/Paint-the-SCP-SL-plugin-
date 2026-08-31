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
        public string Description => TranslationManager.Instance.Get("trust_desc");

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player owner = Player.Get(sender);
            if (owner == null) { response = TranslationManager.Instance.Get("only_players"); return false; }

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
                response = TranslationManager.Instance.Get("trust_no_canvas");
                return false;
            }

            if (arguments.Count < 1)
            {
                response = TranslationManager.Instance.Get("trust_usage");
                return false;
            }

            Player targetPlayer = Player.Get(arguments.At(0));
            if (targetPlayer == null)
            {
                response = string.Format(TranslationManager.Instance.Get("player_not_found"), arguments.At(0));
                return false;
            }

            if (targetPlayer == owner)
            {
                response = TranslationManager.Instance.Get("trust_self");
                return false;
            }

            if (!Plugin.TrustedArtists.ContainsKey(owner.UserId))
            {
                Plugin.TrustedArtists[owner.UserId] = new HashSet<string>();
            }

            if (Plugin.TrustedArtists[owner.UserId].Contains(targetPlayer.UserId))
            {
                Plugin.TrustedArtists[owner.UserId].Remove(targetPlayer.UserId);
                response = string.Format(TranslationManager.Instance.Get("trust_removed"), targetPlayer.Nickname);
                targetPlayer.ShowHint(string.Format(TranslationManager.Instance.Get("trust_revoked_hint"), owner.Nickname), 5f);
            }
            else
            {
                Plugin.TrustedArtists[owner.UserId].Add(targetPlayer.UserId);
                response = string.Format(TranslationManager.Instance.Get("trust_granted"), targetPlayer.Nickname);
                targetPlayer.ShowHint(string.Format(TranslationManager.Instance.Get("trust_granted_hint"), owner.Nickname), 5f);
            }

            return true;
        }
    }
}
