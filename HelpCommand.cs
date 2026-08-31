using System;
using CommandSystem;
using Exiled.API.Features;
using ICommand = CommandSystem.ICommand;

namespace SCPCanvasPaint.Commands
{
    public class HelpSubCommand : ICommand
    {
        public string Command => "help";
        public string[] Aliases => new[] { "info" };
        public string Description => TranslationManager.Instance.Get("help_desc");

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = TranslationManager.Instance.Get("help_text");
            return true;
        }
    }
}
