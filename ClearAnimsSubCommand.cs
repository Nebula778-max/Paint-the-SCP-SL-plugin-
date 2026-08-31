using System;
using System.Collections.Generic;
using CommandSystem;
using Exiled.API.Features;
using MEC;

namespace SCPCanvasPaint.Commands
{
    public class ClearAnimsSubCommand : ICommand
    {
        public string Command => "clearanims";
        public string[] Aliases => new[] { "wipegif", "clearallgif" };
        public string Description => TranslationManager.Instance.Get("clearanims_desc");

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (player != null && !player.RemoteAdminAccess)
            {
                response = TranslationManager.Instance.Get("only_admins_anims");
                return false;
            }

            List<CanvasInstance> canvasesToRemove = new List<CanvasInstance>();

            foreach (var canvas in Plugin.Singleton.CanvasManager.ActiveCanvases)
            {
                if (canvas.AnimationCoroutine.IsRunning)
                {
                    canvasesToRemove.Add(canvas);
                }
            }

            if (canvasesToRemove.Count == 0)
            {
                response = TranslationManager.Instance.Get("no_anims");
                return true;
            }

            int count = canvasesToRemove.Count;

            foreach (var canvas in canvasesToRemove)
            {
                if (canvas.AnimationCoroutine.IsRunning)
                    Timing.KillCoroutines(canvas.AnimationCoroutine);

                Plugin.Singleton.CanvasManager.ActiveCanvases.Remove(canvas);

                int width = canvas.Grid.GetLength(0);
                int height = canvas.Grid.GetLength(1);
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (canvas.Grid[x, y] != null)
                        {
                            if (canvas.Grid[x, y].GameObject != null)
                                UnityEngine.Object.Destroy(canvas.Grid[x, y].GameObject);

                            canvas.Grid[x, y] = null;
                        }
                    }
                }

                if (canvas.RootObject != null)
                    UnityEngine.Object.Destroy(canvas.RootObject);
            }

            response = string.Format(TranslationManager.Instance.Get("anims_removed"), count);
            return true;
        }
    }
}
