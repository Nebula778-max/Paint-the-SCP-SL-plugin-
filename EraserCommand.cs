using System;
using CommandSystem;
using Exiled.API.Features;
using ICommand = CommandSystem.ICommand;

namespace SCPCanvasPaint.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class EraserCommand : ICommand
    {
        public string Command => "eraser";
        public string[] Aliases => new[] { "erase", "lastik" };
        public string Description => "Переключить кисть в режим ластика и обратно";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (Plugin.Singleton.CanvasManager.Sessions.TryGetValue(player, out var session))
            {
                session.IsEraser = !session.IsEraser;
                response = session.IsEraser ? "Включен ЛАСТИК (стирание примитивов)!" : "Включена обычная КИСТЬ!";
                return true;
            }
            response = "Сначала активируйте режим рисования, выбросив медпакет.";
            return false;
        }
    }
}
