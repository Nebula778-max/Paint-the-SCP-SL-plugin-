using CommandSystem;
using Exiled.API.Features;
using Exiled.API.Features.Toys;
using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SCPCanvasPaint.Commands
{
    public class GifSubCommand : ICommand
    {
        public string Command => "gif";
        public string[] Aliases => new[] { "anim", "animation" };
        public string Description => "Установить GIF-анимацию на холст, на который вы смотрите: .canvas gif <ссылка>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (player == null) { response = "Только для игроков!"; return false; }

            if (arguments.Count < 1)
            {
                response = "Укажите прямую URL ссылку на GIF файл!";
                return false;
            }

            if (!Physics.Raycast(player.CameraTransform.position, player.CameraTransform.forward, out RaycastHit hit, Plugin.Singleton.Config.MaxDrawDistance))
            {
                response = "Вы должны смотреть на холст!";
                return false;
            }

            GameObject hitObj = hit.collider.gameObject;
            CanvasInstance targetCanvas = null;

            foreach (var canvas in Plugin.Singleton.CanvasManager.ActiveCanvases)
            {
                for (int x = 0; x < canvas.Size; x++)
                {
                    for (int y = 0; y < canvas.Size; y++)
                    {
                        if (canvas.Grid[x, y] != null && (canvas.Grid[x, y].GameObject == hitObj || hitObj.transform.IsChildOf(canvas.Grid[x, y].GameObject.transform)))
                        {
                            targetCanvas = canvas; break;
                        }
                    }
                    if (targetCanvas != null) break;
                }
            }

            if (targetCanvas == null) { response = "Холст не найден!"; return false; }

            if (targetCanvas.OwnerId != player.UserId && !player.RemoteAdminAccess)
            {
                response = "Вы не можете устанавливать анимации на чужие приватные холсты!";
                return false;
            }

            string url = arguments.At(0);

            if (targetCanvas.AnimationCoroutine.IsRunning)
                Timing.KillCoroutines(targetCanvas.AnimationCoroutine);

            targetCanvas.AnimationCoroutine = Timing.RunCoroutine(DownloadAndPlayGif(url, targetCanvas));

            response = "Запрос отправлен! Дождитесь скачивания и обработки кадров GIF...";
            return true;
        }

        public IEnumerator<float> DownloadAndPlayGif(string url, CanvasInstance canvas)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return Timing.WaitUntilDone(webRequest.SendWebRequest());

                if (webRequest.isNetworkError || webRequest.isHttpError)
                {
                    Log.Error($"Ошибка скачивания GIF: {webRequest.error}");
                    yield break;
                }

                byte[] bytes = webRequest.downloadHandler.data;
                List<GifFrame> gifFrames = GifDecoder.Decode(bytes, canvas.Size);

                if (gifFrames == null || gifFrames.Count == 0)
                {
                    Log.Error("Не удалось раскодировать GIF файл или в нем нет кадров.");
                    yield break;
                }

                float offset = canvas.PhysicalSize / canvas.Size;
                float startOffset = -canvas.PhysicalSize / 2f;
                int currentFrameIdx = 0;

                while (canvas.RootObject != null)
                {
                    GifFrame frame = gifFrames[currentFrameIdx];

                    for (int y = 0; y < canvas.Size; y++)
                    {
                        for (int x = 0; x < canvas.Size; x++)
                        {
                            Primitive p = canvas.Grid[x, y];
                            if (p == null) continue;

                            int pixelIdx = (y * canvas.Size) + x;

                            if (pixelIdx < 0 || pixelIdx >= frame.Pixels.Length) continue;

                            Color pixelColor = frame.Pixels[pixelIdx];

                            if (pixelColor == Color.clear)
                            {
                                p.Position = new Vector3(0f, -1000f, 0f);
                            }
                            else
                            {
                                Vector3 localPos = new Vector3(startOffset + (x * offset) + (offset / 2f), startOffset + (y * offset) + (offset / 2f), 0);
                                p.Position = canvas.RootObject.transform.TransformPoint(localPos);
                                p.Color = pixelColor;
                            }
                        }
                    }

                    currentFrameIdx = (currentFrameIdx + 1) % gifFrames.Count;
                    yield return Timing.WaitForSeconds(frame.Delay);
                }
            }
        }
    }
}
