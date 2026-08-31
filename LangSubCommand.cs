using System;
using CommandSystem;
using Exiled.API.Features;

namespace SCPCanvasPaint.Commands
{
    public class LangSubCommand : ICommand
    {
        public string Command => "lang";
        public string[] Aliases => new[] { "language", "язык" };
        public string Description => "Переключить язык плагина / Switch plugin language (RU/EN)";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (player != null && !player.RemoteAdminAccess)
            {
                response = TranslationManager.Instance.IsEnglish ? "Only admins can change language!" : "Только администраторы могут менять язык!";
                return false;
            }

            TranslationManager.Instance.IsEnglish = !TranslationManager.Instance.IsEnglish;

            if (TranslationManager.Instance.IsEnglish)
            {
                response = "Plugin language successfully changed to English!";
            }
            else
            {
                response = "Язык плагина успешно изменен на Русский!";
            }
            return true;
        }
    }
}
