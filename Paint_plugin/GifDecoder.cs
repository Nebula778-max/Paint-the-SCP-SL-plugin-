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
        public static List<GifFrame> Decode(byte[] bytes, int targetSize)
        {
            List<GifFrame> frames = new List<GifFrame>();
            try
            {
                using (var image = SharpImage.Load<Rgba32>(bytes))
                {
                    for (int i = 0; i < image.Frames.Count; i++)
                    {
                        var frame = image.Frames[i];

                        using (var singleFrameImage = image.Frames.CloneFrame(i))
                        {
                            singleFrameImage.Mutate(x => x.Resize(targetSize, targetSize));

                            Color[] framePixels = new Color[targetSize * targetSize];

                            for (int x = 0; x < targetSize; x++)
                            {
                                for (int y = 0; y < targetSize; y++)
                                {
                                    Rgba32 pixel = singleFrameImage[x, targetSize - 1 - y];

                                    if (pixel.A == 0)
                                    {
                                        framePixels[x + y * targetSize] = Color.clear;
                                    }
                                    else
                                    {
                                        framePixels[x + y * targetSize] = new Color(
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

                            frames.Add(new GifFrame { Pixels = framePixels, Delay = delay });

                            if (frames.Count >= 30) break;
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
