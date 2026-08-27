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
        public string Description => "Заспавнить холст по прямой URL-ссылке на картинку: .canvas url <размер_матрицы> <физ_размер> <ссылка>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (player == null) { response = "Только для игроков!"; return false; }

            if (!player.RemoteAdminAccess)
            {
                response = "Только администраторы могут импортировать картинки по URL!";
                return false;
            }

            if (arguments.Count < 3)
            {
                response = "Используйте: .canvas url <размер_сетки> <физический_размер> <прямая_ссылка_на_картинку>";
                return false;
            }

            if (!int.TryParse(arguments.At(0), out int size) || size <= 0 || !float.TryParse(arguments.At(1), out float physicalSize) || physicalSize <= 0f)
            {
                response = "Укажите корректные числовые параметры сетки и размера.";
                return false;
            }

            
            if (Plugin.Singleton.Config.MaxMatrixSize > 0 && size > Plugin.Singleton.Config.MaxMatrixSize)
            {
                response = $"Размер матрицы превышает лимит сервера ({Plugin.Singleton.Config.MaxMatrixSize})!";
                return false;
            }


            string url = arguments.At(2);
            Vector3 spawnPos = player.CameraTransform.position + (player.CameraTransform.forward * 4f);
            Quaternion rotation = player.CameraTransform.rotation;

            Timing.RunCoroutine(DownloadAndSpawn(url, size, physicalSize, spawnPos, rotation, player.UserId));

            response = "Запрос отправлен на сервер. Картинка скачивается и скоро отрендерится перед вами...";
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

                try
                {
                    using (var image = SixLabors.ImageSharp.Image.Load<Rgba32>(bytes))
                    {
                        // Проверяем, содержит ли файл больше 1 кадра (анимация)
                        if (image.Frames.Count > 1)
                        {
                            isAnimatedGif = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"[SCPCanvasPaint] ImageSharp не смог распознать файл из интернета: {ex.Message}");
                    yield break;
                }

                if (isAnimatedGif)
                {
                    // ШАГ А: Спавним чистый белый холст нужного размера
                    // Запускаем корутину спавна и ждем, пока ActiveCanvases пополнится
                    yield return Timing.WaitUntilDone(Timing.RunCoroutine(
                        Plugin.Singleton.CanvasManager.SpawnCanvasCoroutine(pos, rot, size, physicalSize, ownerId, true, null)
                    ));

                    // Находим только что созданный холст (он последний в списке)
                    var activeCanvases = Plugin.Singleton.CanvasManager.ActiveCanvases;
                    if (activeCanvases.Count > 0)
                    {
                        CanvasInstance targetCanvas = activeCanvases[activeCanvases.Count - 1];

                        // ШАГ Б: Запускаем проигрывание GIF (используем ваш метод из GifSubCommand)
                        // Для этого объект GifSubCommand должен быть доступен, либо метод DownloadAndPlayGif должен быть статическим/перенесен в CanvasManager.
                        // Самый простой способ без изменения архитектуры — создать экземпляр подкоманды:
                        GifSubCommand gifHandler = new GifSubCommand();
                        targetCanvas.AnimationCoroutine = Timing.RunCoroutine(gifHandler.DownloadAndPlayGif(url, targetCanvas));
                    }
                }
                else
                {
                    // Если это обычная статичная картинка (1 кадр), запускаем ваш старый код
                    List<string> hexColors = new List<string>();
                    try
                    {
                        using (var image = SixLabors.ImageSharp.Image.Load<Rgba32>(bytes))
                        {
                            using (var singleFrameImage = image.Frames.CloneFrame(0))
                            {
                                singleFrameImage.Mutate(x => x.Resize(size, size));
                                for (int x = 0; x < size; x++)
                                {
                                    for (int y = 0; y < size; y++)
                                    {
                                        Rgba32 pixel = singleFrameImage[x, size - 1 - y];
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

                    Timing.RunCoroutine(Plugin.Singleton.CanvasManager.SpawnCanvasCoroutine(pos, rot, size, physicalSize, ownerId, true, hexColors));
                }
            }
        }

    }
}
