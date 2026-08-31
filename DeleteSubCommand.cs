using CommandSystem;
using Exiled.API.Features;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SCPCanvasPaint.Commands
{
    public class DeleteSubCommand : ICommand
    {
        public string Command => "delete";
        public string[] Aliases => new[] { "remove" };
        public string Description => TranslationManager.Instance.Get("delete_desc");

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (player == null) { response = TranslationManager.Instance.Get("only_players"); return false; }

            if (Physics.Raycast(player.CameraTransform.position, player.CameraTransform.forward, out RaycastHit hit, 15f))
            {
                var primitiveComponent = hit.collider.GetComponentInParent<AdminToys.PrimitiveObjectToy>();

                if (primitiveComponent == null)
                {
                    response = TranslationManager.Instance.Get("look_at_canvas");
                    return false;
                }

                CanvasInstance targetCanvas = null;

                foreach (var canvas in Plugin.Singleton.CanvasManager.ActiveCanvases)
                {
                    
                    int width = canvas.Grid.GetLength(0);
                    int height = canvas.Grid.GetLength(1);

                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            if (canvas.Grid[x, y] != null && canvas.Grid[x, y].Base == primitiveComponent)
                            {
                                targetCanvas = canvas;
                                break;
                            }
                        }
                        if (targetCanvas != null) break;
                    }
                    if (targetCanvas != null) break;
                }

                if (targetCanvas != null)
                {
                    if (targetCanvas.OwnerId != player.UserId && !player.RemoteAdminAccess)
                    {
                        response = TranslationManager.Instance.Get("delete_own_only");
                        return false;
                    }

                    if (targetCanvas.AnimationCoroutine.IsRunning)
                        MEC.Timing.KillCoroutines(targetCanvas.AnimationCoroutine);

                    Plugin.Singleton.CanvasManager.ActiveCanvases.Remove(targetCanvas);

                    
                    int targetWidth = targetCanvas.Grid.GetLength(0);
                    int targetHeight = targetCanvas.Grid.GetLength(1);

                    for (int x = 0; x < targetWidth; x++)
                    {
                        for (int y = 0; y < targetHeight; y++)
                        {
                            if (targetCanvas.Grid[x, y] != null)
                            {
                                if (targetCanvas.Grid[x, y].GameObject != null)
                                    UnityEngine.Object.Destroy(targetCanvas.Grid[x, y].GameObject);

                                targetCanvas.Grid[x, y] = null;
                            }
                        }
                    }

                    MEC.Timing.RunCoroutine(DestroyCanvasSafe(targetCanvas));
                    response = TranslationManager.Instance.Get("delete_success");
                    return true;
                }
            }

            response = TranslationManager.Instance.Get("look_at_canvas");
            return false;
        }

        private IEnumerator<float> DestroyCanvasSafe(CanvasInstance canvas)
        {
            if (canvas.RootObject != null)
                UnityEngine.Object.Destroy(canvas.RootObject);

            yield break;
        }
    }
}
