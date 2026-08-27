using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Exiled.API.Features;
using Exiled.API.Features.Toys; using MEC;
using Newtonsoft.Json;


namespace SCPCanvasPaint
{
    public class CanvasData
    {
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public int Size { get; set; }
        public float PhysicalSize { get; set; }
        public string OwnerId { get; set; }
        public bool IsPublic { get; set; }
        public List<string> HexColors { get; set; }
    }


    public class PlayerPaintSession
    {
        public bool IsDrawing { get; set; } = false;
        public bool IsEraser { get; set; } = false;         public Color CurrentColor { get; set; } = Color.red;
        public int BrushSize { get; set; } = 1;
        public Stack<Dictionary<Primitive, Color>> UndoStack { get; set; } = new Stack<Dictionary<Primitive, Color>>();
        public Stack<Dictionary<Primitive, Color>> RedoStack { get; set; } = new Stack<Dictionary<Primitive, Color>>();
        public CoroutineHandle DrawCoroutine { get; set; }
        public List<Color> CustomPalette { get; set; } = new List<Color>();

    }


    public class CanvasInstance
    {
        public int Size { get; set; }
        public float PhysicalSize { get; set; }
        public string OwnerId { get; set; }
        public bool IsPublic { get; set; }
        public Primitive[,] Grid { get; set; }
        public GameObject RootObject { get; set; }
        public MEC.CoroutineHandle AnimationCoroutine { get; set; }

    }


    public class CanvasManager
    {
        public Dictionary<Player, PlayerPaintSession> Sessions = new Dictionary<Player, PlayerPaintSession>();
        public List<CanvasInstance> ActiveCanvases = new List<CanvasInstance>();
        private readonly string saveFolder = Path.Combine(Paths.Configs, "SavedCanvases");

        public readonly List<Color> ColorPalette = new List<Color>
        {
            Color.red, Color.green, Color.blue, Color.white, Color.black, Color.yellow, Color.cyan, Color.magenta
        };

        public void InitSaveDirectory() => Directory.CreateDirectory(saveFolder);

        public void CleanUp()
        {
            foreach (var canvas in ActiveCanvases)
            {
                if (canvas.RootObject != null)
                    UnityEngine.Object.Destroy(canvas.RootObject);
            }
            ActiveCanvases.Clear();
            Sessions.Clear();
        }

        public IEnumerator<float> SpawnCanvasCoroutine(Vector3 centerPos, Quaternion rotation, int size, float physicalSize, string ownerId, bool isPublic, List<string> loadedColors = null)
        {
            Vector3 euler = rotation.eulerAngles;
            Quaternion flatRotation = Quaternion.Euler(0f, euler.y, 0f);

                        if (Physics.Raycast(centerPos, Vector3.down, out RaycastHit floorHit, 5f))
            {
                centerPos.y = floorHit.point.y + (physicalSize / 2f) + 0.01f;
            }

            centerPos.y += 2.5f;

            GameObject root = new GameObject("Canvas_Anchor");
            root.transform.position = centerPos;
            root.transform.rotation = flatRotation;

            CanvasInstance instance = new CanvasInstance
            {
                Size = size,
                PhysicalSize = physicalSize,
                OwnerId = ownerId,
                IsPublic = isPublic,
                Grid = new Primitive[size, size],
                RootObject = root
            };

            float offset = physicalSize / size;
            float startOffset = -physicalSize / 2f;
            int pixelIndex = 0;


            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    Vector3 localPos = new Vector3(startOffset + (x * offset) + (offset / 2f), startOffset + (y * offset) + (offset / 2f), 0);
                    Vector3 spawnPos = root.transform.TransformPoint(localPos);

                    Primitive cube = Primitive.Create(spawnPos, flatRotation.eulerAngles);

                    cube.Type = UnityEngine.PrimitiveType.Cube;
                    cube.GameObject.name = $"{x}_{y}";
                    cube.Scale = new Vector3(offset, offset, 0.02f);

                                        
                    Color targetColor = Color.white;

                    if (loadedColors != null && pixelIndex < loadedColors.Count)
                    {
                        ColorUtility.TryParseHtmlString(loadedColors[pixelIndex], out targetColor);
                    }
                    cube.Color = targetColor;

                    instance.Grid[x, y] = cube;
                    pixelIndex++;

                    if (pixelIndex % 50 == 0) yield return Timing.WaitForOneFrame;
                }
            }
            ActiveCanvases.Add(instance);
        }


        public void SaveCanvas(CanvasInstance canvas, string name)
        {
            List<string> hexList = new List<string>();
            for (int x = 0; x < canvas.Size; x++)
            {
                for (int y = 0; y < canvas.Size; y++)
                {
                    hexList.Add("#" + ColorUtility.ToHtmlStringRGB(canvas.Grid[x, y].Color));
                }
            }

            CanvasData data = new CanvasData
            {
                Name = name,
                Position = canvas.RootObject.transform.position,
                Rotation = canvas.RootObject.transform.rotation,
                Size = canvas.Size,
                PhysicalSize = canvas.PhysicalSize,
                OwnerId = canvas.OwnerId,
                IsPublic = canvas.IsPublic,
                HexColors = hexList
            };


            var settings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            File.WriteAllText(Path.Combine(saveFolder, $"{name}.json"), JsonConvert.SerializeObject(data, Formatting.Indented, settings));

        }

        public bool LoadCanvas(string name, Vector3 spawnPos, Quaternion rotation, string ownerId)
        {
            string path = Path.Combine(saveFolder, $"{name}.json");
            if (!File.Exists(path)) return false;

            CanvasData data = JsonConvert.DeserializeObject<CanvasData>(File.ReadAllText(path));
            Timing.RunCoroutine(SpawnCanvasCoroutine(spawnPos, rotation, data.Size, data.PhysicalSize, ownerId, data.IsPublic, data.HexColors));
            return true;
        }
    }
}
