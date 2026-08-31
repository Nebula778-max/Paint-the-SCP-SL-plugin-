using System;
using CommandSystem;
using Exiled.API.Features;

namespace SCPCanvasPaint.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class CanvasCommand : ParentCommand
    {
        public CanvasCommand() => LoadGeneratedCommands();

        public override string Command => "canvas";
        public override string[] Aliases => new[] { "cv" };
        public override string Description => "Управление холстами для рисования";

        public override void LoadGeneratedCommands()
        {
            RegisterCommand(new SpawnSubCommand());
            RegisterCommand(new SaveSubCommand());
            RegisterCommand(new LoadSubCommand());
            RegisterCommand(new DeleteSubCommand());
            RegisterCommand(new PermissionSubCommand());
            RegisterCommand(new UrlSubCommand());
            RegisterCommand(new GifSubCommand());
            RegisterCommand(new ClearAnimsSubCommand());
            RegisterCommand(new BrushSubCommand());
            RegisterCommand(new ColorSubCommand());
            RegisterCommand(new EraserSubCommand());
            RegisterCommand(new HelpSubCommand());
            RegisterCommand(new PipetteSubCommand());
            RegisterCommand(new TrustSubCommand());
            RegisterCommand(new TextSubCommand());
            RegisterCommand(new GrabSubCommand());
            RegisterCommand(new LangSubCommand());
        }


        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = TranslationManager.Instance.Get("canvas_usage");
            return false;
        }
    }
}
