using System;
using CommandSystem;
using Exiled.API.Features;
using UnityEngine;
using ICommand = CommandSystem.ICommand;

namespace SCPCanvasPaint.Commands
{

    public class ColorSubCommand : ICommand
    {
        public string Command => "color";
        public string[] Aliases => new[] { "hexcolor" };
        public string Description => TranslationManager.Instance.Get("color_desc");

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (!Plugin.Singleton.CanvasManager.Sessions.TryGetValue(player, out var session))
            {
                response = TranslationManager.Instance.Get("need_mode");
                return false;
            }

            if (arguments.Count < 1)
            {
                response = TranslationManager.Instance.Get("color_usage");
                return false;
            }

            string hex = arguments.At(0);
            if (!hex.StartsWith("#")) hex = "#" + hex;

            if (ColorUtility.TryParseHtmlString(hex, out Color customColor))
            {
                session.CurrentColor = customColor;

                if (!Plugin.Singleton.CanvasManager.ColorPalette.Contains(customColor) && !session.CustomPalette.Contains(customColor))
                {
                    session.CustomPalette.Add(customColor);
                }

                response = string.Format(TranslationManager.Instance.Get("color_success"), hex);
                player.ShowHint(string.Format(TranslationManager.Instance.Get("color_selected"), hex), 3f);
                return true;
            }

            response = TranslationManager.Instance.Get("color_fail");
            return false;
        }
    }
}
