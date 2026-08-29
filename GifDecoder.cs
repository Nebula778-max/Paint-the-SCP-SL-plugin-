using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using UnityEngine;
using Color = UnityEngine.Color;
using SharpImage = SixLabors.ImageSharp.Image;

namespace SCPCanvasPaint
{
    public class GifFrame
    {
        public Color[] Pixels;
        public float Delay;
    }

    public static class GifDecoder
    {
        public static List<GifFrame> Decode(byte[] bytes, int targetSize, float ratio)
        {
            List<GifFrame> frames = new List<GifFrame>();
            try
            {
                using (var image = SharpImage.Load<Rgba32>(bytes))
                {
                    int targetWidth = targetSize;
                    int targetHeight = ratio >= 1f ? Mathf.RoundToInt(targetSize / ratio) : targetSize;
                    if (targetWidth <= 0) targetWidth = 1;
                    if (targetHeight <= 0) targetHeight = 1;

                    int totalPixels = targetWidth * targetHeight;

                    for (int i = 0; i < image.Frames.Count; i++)
                    {
                        var frame = image.Frames[i];
                        using (var singleFrameImage = image.Frames.CloneFrame(i))
                        {
                            singleFrameImage.Mutate(x => x.Resize(targetWidth, targetHeight));

                            Color[] framePixels = new Color[totalPixels];

                            for (int x = 0; x < targetWidth; x++)
                            {
                                for (int y = 0; y < targetHeight; y++)
                                {
                                    Rgba32 pixel = singleFrameImage[x, targetHeight - 1 - y];

                                    int index = x + (y * targetWidth);

                                    if (pixel.A == 0)
                                    {
                                        framePixels[index] = Color.clear;
                                    }
                                    else
                                    {
                                        framePixels[index] = new Color(
                                            pixel.R / 255f,
                                            pixel.G / 255f,
                                            pixel.B / 255f,
                                            pixel.A / 255f
                                        );
                                    }
                                }
                            }

                            var frameMetadata = frame.Metadata.GetGifMetadata();
                            float delay = frameMetadata.FrameDelay > 0 ? frameMetadata.FrameDelay / 100f : 0.1f;
                            if (delay < 0.1f) delay = 0.1f;


                            frames.Add(new GifFrame { Pixels = framePixels, Delay = delay });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Exiled.API.Features.Log.Error($"[SCPCanvasPaint] Ошибка декодирования GIF через ImageSharp: {ex.Message}");
                return null;
            }
            return frames;
        }
    }
}
