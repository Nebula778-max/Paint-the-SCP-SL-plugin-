using System;
using CommandSystem;
using Exiled.API.Features;
using ICommand = CommandSystem.ICommand;

namespace SCPCanvasPaint.Commands
{

    public class EraserSubCommand : ICommand
    {
        public string Command => "eraser";
        public string[] Aliases => new[] { "erase", "lastik" };
        public string Description => TranslationManager.Instance.Get("eraser_desc");

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (Plugin.Singleton.CanvasManager.Sessions.TryGetValue(player, out var session))
            {
                session.IsEraser = !session.IsEraser;
                response = session.IsEraser ? TranslationManager.Instance.Get("eraser_on") : TranslationManager.Instance.Get("eraser_off");
                return true;
            }
            response = TranslationManager.Instance.Get("need_mode");
            return false;
        }
    }
}
