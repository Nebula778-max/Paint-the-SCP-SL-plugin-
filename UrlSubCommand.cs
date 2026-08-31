using CommandSystem;
using Exiled.API.Features;
using MEC;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace SCPCanvasPaint.Commands
{
    public class UrlSubCommand : ICommand
    {
        public string Command => "url";
        public string[] Aliases => new[] { "image", "png", "jpg" };
        public string Description => TranslationManager.Instance.Get("url_desc");

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (player == null) { response = TranslationManager.Instance.Get("only_players"); return false; }

            if (!player.RemoteAdminAccess)
            {
                response = TranslationManager.Instance.Get("only_admins_url");
                return false;
            }

            if (arguments.Count < 3)
            {
                response = TranslationManager.Instance.Get("url_usage");
                return false;
            }

            if (!int.TryParse(arguments.At(0), out int size) || size <= 0 || !float.TryParse(arguments.At(1), out float physicalSize) || physicalSize <= 0f)
            {
                response = TranslationManager.Instance.Get("spawn_invalid_params");
                return false;
            }


            if (Plugin.Singleton.Config.MaxMatrixSize > 0 && size > Plugin.Singleton.Config.MaxMatrixSize)
            {
                response = string.Format(TranslationManager.Instance.Get("spawn_limit_matrix"), Plugin.Singleton.Config.MaxMatrixSize);
                return false;
            }


            string url = arguments.At(2);
            Vector3 spawnPos = player.CameraTransform.position + (player.CameraTransform.forward * 4f);
            Quaternion rotation = player.CameraTransform.rotation;

            Timing.RunCoroutine(DownloadAndSpawn(url, size, physicalSize, spawnPos, rotation, player.UserId));

            response = TranslationManager.Instance.Get("url_success");
            return true;
        }

        private IEnumerator<float> DownloadAndSpawn(string url, int size, float physicalSize, Vector3 pos, Quaternion rot, string ownerId)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return Timing.WaitUntilDone(webRequest.SendWebRequest());

                if (webRequest.isNetworkError || webRequest.isHttpError)
                {
                    Log.Error($"[SCPCanvasPaint] Не удалось скачать изображение по URL. Ошибка: {webRequest.error}");
                    yield break;
                }

                byte[] bytes = webRequest.downloadHandler.data;
                bool isAnimatedGif = false;
                float ratio = 1f;

                try
                {
                    using (var image = SixLabors.ImageSharp.Image.Load<Rgba32>(bytes))
                    {
                        if (image.Frames.Count > 1) isAnimatedGif = true;
                        ratio = (float)image.Width / image.Height;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"[SCPCanvasPaint] ImageSharp ошибка: {ex.Message}");
                    yield break;
                }

                int width = ratio >= 1f ? size : Mathf.RoundToInt(size * ratio);
                int height = ratio >= 1f ? Mathf.RoundToInt(size / ratio) : size;
                if (width <= 0) width = 1;
                if (height <= 0) height = 1;


                if (isAnimatedGif)
                {
                    yield return Timing.WaitUntilDone(Timing.RunCoroutine(
    Plugin.Singleton.CanvasManager.SpawnCanvasCoroutine(pos, rot, size, physicalSize, ownerId, true, null, ratio)
));


                    var activeCanvases = Plugin.Singleton.CanvasManager.ActiveCanvases;
                    if (activeCanvases.Count > 0)
                    {
                        CanvasInstance targetCanvas = activeCanvases[activeCanvases.Count - 1];

                        GifSubCommand gifHandler = new GifSubCommand();
                        targetCanvas.AnimationCoroutine = Timing.RunCoroutine(gifHandler.DownloadAndPlayGif(url, targetCanvas));
                    }
                }
                else
                {
                    List<string> hexColors = new List<string>();
                    try
                    {
                        using (var image = SixLabors.ImageSharp.Image.Load<Rgba32>(bytes))
                        {
                            using (var singleFrameImage = image.Frames.CloneFrame(0))
                            {
                                singleFrameImage.Mutate(x => x.Resize(width, height));
                                for (int x = 0; x < width; x++)
                                {
                                    for (int y = 0; y < height; y++)
                                    {
                                        Rgba32 pixel = singleFrameImage[x, height - 1 - y];
                                        hexColors.Add("#" + ColorUtility.ToHtmlStringRGB(new Color(pixel.R / 255f, pixel.G / 255f, pixel.B / 255f, pixel.A / 255f)));
                                    }
                                }
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        Log.Error($"[SCPCanvasPaint] Ошибка обработки статичного кадра: {ex.Message}");
                        yield break;
                    }

                    Timing.RunCoroutine(Plugin.Singleton.CanvasManager.SpawnCanvasCoroutine(pos, rot, size, physicalSize, ownerId, true, hexColors, ratio));
                }
            }
        }

    }
}
