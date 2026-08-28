using System;
using CommandSystem;
using Exiled.API.Features;
using ICommand = CommandSystem.ICommand;

namespace SCPCanvasPaint.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class BrushCommand : ICommand
    {
        public string Command => "brush";
        public string[] Aliases => new[] { "size" };
        public string Description => "Изменить размер мазка кисти от 1 до 5";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (!Plugin.Singleton.CanvasManager.Sessions.TryGetValue(player, out var session))
            {
                response = "Сначала включите режим рисования медпакетом.";
                return false;
            }

            if (arguments.Count < 1 || !int.TryParse(arguments.At(0), out int size) || size <= 0 || size > 5)
            {
                response = "Укажите размер кисти от 1 до 5. Пример: .brush 3";
                return false;
            }

            session.BrushSize = size;
            response = $"Размер мазка кисти успешно изменен на {size}x{size} пикселей!";
            return true;
        }
    }
}
