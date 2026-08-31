using System;
using CommandSystem;
using Exiled.API.Features;
using ICommand = CommandSystem.ICommand;

namespace SCPCanvasPaint.Commands
{

    public class BrushSubCommand : ICommand
    {
        public string Command => "brush";
        public string[] Aliases => new[] { "size" };
        public string Description => TranslationManager.Instance.Get("brush_size_desc");

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (!Plugin.Singleton.CanvasManager.Sessions.TryGetValue(player, out var session))
            {
                response = TranslationManager.Instance.Get("need_mode");
                return false;
            }

            if (arguments.Count < 1 || !int.TryParse(arguments.At(0), out int size) || size <= 0 || size > 5)
            {
                response = TranslationManager.Instance.Get("brush_usage");
                return false;
            }

            session.BrushSize = size;
            response = string.Format(TranslationManager.Instance.Get("brush_success"), size);
            return true;
        }
    }
}
