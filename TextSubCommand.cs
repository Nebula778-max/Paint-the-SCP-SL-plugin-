using CommandSystem;
using Exiled.API.Features;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SCPCanvasPaint.Commands
{
    public class TextSubCommand : ICommand
    {
        public string Command => "text";
        public string[] Aliases => new[] { "write", "print" };
        public string Description => TranslationManager.Instance.Get("text_desc");

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (player == null) { response = TranslationManager.Instance.Get("only_players"); return false; }

            bool hasPerm = player.RemoteAdminAccess || Plugin.AllowAllPlayers || Plugin.AllowedPlayers.Contains(player.UserId);
            if (!hasPerm) { response = TranslationManager.Instance.Get("spawn_no_perms"); return false; }

            if (arguments.Count < 1) { response = TranslationManager.Instance.Get("text_desc"); return false; }

            string rawText = arguments.At(0).ToUpper();
            Color textColor = Color.red;
            float physicalSize = 3f;
            Color bgColor = Color.white;
            bool isPublic = false;

            int currentArg = 1;

            if (arguments.Count > currentArg && ColorUtility.TryParseHtmlString(arguments.At(currentArg), out Color parsedTextColor))
            {
                textColor = parsedTextColor;
                currentArg++;
            }

            if (arguments.Count > currentArg && float.TryParse(arguments.At(currentArg), out float parsedSize))
            {
                physicalSize = parsedSize;
                currentArg++;
            }

            if (arguments.Count > currentArg && ColorUtility.TryParseHtmlString(arguments.At(currentArg), out Color parsedBgColor))
            {
                bgColor = parsedBgColor;
                currentArg++;
            }

            if (arguments.Count > currentArg)
            {
                string access = arguments.At(currentArg).ToLower();
                if (access == "public" || access == "private")
                {
                    isPublic = access == "public";
                }
            }

            if (Plugin.Singleton.Config.MaxPhysicalSize > 0 && physicalSize > Plugin.Singleton.Config.MaxPhysicalSize)
            {
                response = string.Format(TranslationManager.Instance.Get("spawn_limit_phys"), Plugin.Singleton.Config.MaxPhysicalSize);
                return false;
            }

            int textWidth = 0;
            foreach (char c in rawText)
            {
                textWidth += 18;
            }
            if (textWidth > 0) textWidth -= 2;

            int padding = 4;
            int matrixWidth = textWidth + (padding * 2);
            int matrixHeight = (8 * 5) + (padding * 2);


            if (Plugin.Singleton.Config.MaxMatrixSize > 0 && (matrixWidth > Plugin.Singleton.Config.MaxMatrixSize || matrixHeight > Plugin.Singleton.Config.MaxMatrixSize))
            {
                response = TranslationManager.Instance.Get("text_too_long");
                return false;
            }

            List<string> generatedHexColors = new List<string>(matrixWidth * matrixHeight);
            Color[,] renderGrid = new Color[matrixWidth, matrixHeight];

            for (int x = 0; x < matrixWidth; x++)
            {
                for (int y = 0; y < matrixHeight; y++)
                {
                    renderGrid[x, y] = bgColor;
                }
            }

            int currentX = padding;
            int textStartY = padding;

            foreach (char c in rawText)
            {
                if (c == ' ')
                {
                    currentX += 18;
                    continue;
                }

                if (PixelFontRu.Alphabet.TryGetValue(c, out byte[,] matrix))
                {
                    for (int h = 0; h < 8; h++)
                    {
                        for (int w = 0; w < 8; w++)
                        {
                            if (matrix[h, w] == 1)
                            {
                                int pixelX1 = currentX + (w * 2);
                                int pixelX2 = currentX + (w * 2) + 1;

                                for (int v = 0; v < 5; v++)
                                {
                                    int pixelY = matrixHeight - 1 - (textStartY + (h * 5) + v);

                                    if (pixelY >= 0 && pixelY < matrixHeight)
                                    {
                                        if (pixelX1 >= 0 && pixelX1 < matrixWidth) renderGrid[pixelX1, pixelY] = textColor;
                                        if (pixelX2 >= 0 && pixelX2 < matrixWidth) renderGrid[pixelX2, pixelY] = textColor;
                                    }
                                }
                            }
                        }
                    }
                }
                currentX += 18;
            }


            for (int x = 0; x < matrixWidth; x++)
            {
                for (int y = 0; y < matrixHeight; y++)
                {
                    generatedHexColors.Add("#" + ColorUtility.ToHtmlStringRGB(renderGrid[x, y]));
                }
            }

            Vector3 spawnPos = player.CameraTransform.position + (player.CameraTransform.forward * 3f);
            Quaternion rotation = player.CameraTransform.rotation;
            float ratio = (float)matrixWidth / matrixHeight;

            Timing.RunCoroutine(Plugin.Singleton.CanvasManager.SpawnCanvasCoroutine(spawnPos, rotation, matrixWidth, physicalSize, player.UserId, isPublic, generatedHexColors, ratio));


            if (!player.Items.Any(i => Exiled.CustomItems.API.Features.CustomItem.TryGet(i, out Exiled.CustomItems.API.Features.CustomItem cItem) && cItem.Id == Plugin.Singleton.BrushItem.Id))
            {
                Plugin.Singleton.BrushItem.Give(player);
            }

            response = string.Format(TranslationManager.Instance.Get("text_success"), matrixWidth, matrixHeight, isPublic ? "public" : "private");
            return true;
        }
    }
}
