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
        public string Description => "Удалить холст по взгляду (доступно владельцу или админу)";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (player == null) { response = "Только для игроков!"; return false; }

            if (Physics.Raycast(player.CameraTransform.position, player.CameraTransform.forward, out RaycastHit hit, 15f))
            {
                var primitiveComponent = hit.collider.GetComponentInParent<AdminToys.PrimitiveObjectToy>();

                if (primitiveComponent == null)
                {
                    response = "Вы должны смотреть на холст, чтобы удалить его!";
                    return false;
                }

                CanvasInstance targetCanvas = null;

                foreach (var canvas in Plugin.Singleton.CanvasManager.ActiveCanvases)
                {
                    for (int x = 0; x < canvas.Size; x++)
                    {
                        for (int y = 0; y < canvas.Size; y++)
                        {
                            if (canvas.Grid[x, y] != null && canvas.Grid[x, y].Base == primitiveComponent)
                            {
                                targetCanvas = canvas;
                                break;
                            }
                        }
                        if (targetCanvas != null) break;
                    }
                }

                if (targetCanvas != null)
                {
                    if (targetCanvas.OwnerId != player.UserId && !player.RemoteAdminAccess)
                    {
                        response = "Вы можете удалять только свои холсты! Админы могут удалять любые.";
                        return false;
                    }

                    if (targetCanvas.AnimationCoroutine.IsRunning)
                        MEC.Timing.KillCoroutines(targetCanvas.AnimationCoroutine);

                    Plugin.Singleton.CanvasManager.ActiveCanvases.Remove(targetCanvas);

                    for (int x = 0; x < targetCanvas.Size; x++)
                    {
                        for (int y = 0; y < targetCanvas.Size; y++)
                        {
                            if (targetCanvas.Grid[x, y] != null)
                            {
                                UnityEngine.Object.Destroy(targetCanvas.Grid[x, y].GameObject);
                                targetCanvas.Grid[x, y] = null;
                            }
                        }
                    }

                    MEC.Timing.RunCoroutine(DestroyCanvasSafe(targetCanvas));
                    response = "Холст успешно удален!";
                    return true;

                }
            }

            response = "Вы должны смотреть на холст, чтобы удалить его!";
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
