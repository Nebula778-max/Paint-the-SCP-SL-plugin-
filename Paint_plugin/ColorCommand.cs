using System;
using CommandSystem;
using Exiled.API.Features;
using UnityEngine;
using ICommand = CommandSystem.ICommand;

namespace SCPCanvasPaint.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class ColorCommand : ICommand
    {
        public string Command => "color";
        public string[] Aliases => new[] { "hexcolor" };
        public string Description => "Задать точный HEX-код цвета кисти и сохранить в свою палитру";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (!Plugin.Singleton.CanvasManager.Sessions.TryGetValue(player, out var session))
            {
                response = "Сначала включите режим рисования, выбросив медпакет.";
                return false;
            }

            if (arguments.Count < 1)
            {
                response = "Укажите HEX-код цвета. Пример: .color #FF5733";
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

                response = $"Цвет успешно изменен на кастомный HEX: {hex}! Он добавлен в вашу палитру прокрутки.";
                player.ShowHint($"Выбран цвет: <color={hex}>Кастомный █</color>", 3f);
                return true;
            }

            response = "Не удалось распознать HEX-код. Используйте формат #RRGGBB или #RGB.";
            return false;
        }
    }
}
