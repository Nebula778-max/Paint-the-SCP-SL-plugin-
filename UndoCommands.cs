using System;
using System.Collections.Generic;
using UnityEngine;
using CommandSystem;
using Exiled.API.Features;
using Exiled.API.Features.Toys;
using ICommand = CommandSystem.ICommand;

namespace SCPCanvasPaint.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class UndoCommand : ICommand
    {
        public string Command => "undo";
        public string[] Aliases => new[] { "назад" };
        public string Description => TranslationManager.Instance.Get("undo_desc");

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (!Plugin.Singleton.CanvasManager.Sessions.TryGetValue(player, out var session) || session.UndoStack.Count == 0)
            {
                response = TranslationManager.Instance.Get("undo_empty");
                return false;
            }

            Dictionary<Primitive, Color> previousState = session.UndoStack.Pop();
            Dictionary<Primitive, Color> redoState = new Dictionary<Primitive, Color>();

            CanvasInstance canvas = null;
            foreach (var firstPrimitive in previousState.Keys)
            {
                canvas = Plugin.Singleton.CanvasManager.ActiveCanvases.Find(c =>
                {
                    int width = c.Grid.GetLength(0);
                    int height = c.Grid.GetLength(1);
                    for (int x = 0; x < width; x++)
                        for (int y = 0; y < height; y++)
                            if (c.Grid[x, y] == firstPrimitive) return true;
                    return false;
                });
                if (canvas != null) break;
            }


            if (canvas == null)
            {
                response = TranslationManager.Instance.Get("canvas_destroyed");
                return false;
            }

            float offset = canvas.PhysicalSize / canvas.Size;
            float startOffsetX = -canvas.PhysicalSize / 2f;
            float startOffsetY = -(canvas.PhysicalSize / canvas.Ratio) / 2f;


            foreach (var pair in previousState)
            {
                Primitive p = pair.Key;
                Color prevColor = pair.Value;

                redoState[p] = p.Position.y < -500f ? Color.clear : p.Color;

                if (prevColor == Color.clear)
                {
                    p.Position = new Vector3(0f, -1000f, 0f);
                }
                else
                {
                    int targetX = -1, targetY = -1;
                    int width = canvas.Grid.GetLength(0);
                    int height = canvas.Grid.GetLength(1);
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            if (canvas.Grid[x, y] == p) { targetX = x; targetY = y; break; }
                        }
                        if (targetX != -1) break;
                    }


                    if (targetX != -1)
                    {
                        Vector3 localPos = new Vector3(startOffsetX + (targetX * offset) + (offset / 2f), startOffsetY + (targetY * offset * (1f / canvas.Ratio)) + ((offset * (1f / canvas.Ratio)) / 2f), 0);
                        p.Position = canvas.RootObject.transform.TransformPoint(localPos);
                        p.Color = prevColor;
                    }
                }
            }

            session.RedoStack.Push(redoState);
            response = TranslationManager.Instance.Get("undo_success");
            return true;
        }
    }

    [CommandHandler(typeof(ClientCommandHandler))]
    public class RedoCommand : ICommand
    {
        public string Command => "redo";
        public string[] Aliases => new[] { "вперед" };
        public string Description => TranslationManager.Instance.Get("redo_desc");

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (!Plugin.Singleton.CanvasManager.Sessions.TryGetValue(player, out var session) || session.RedoStack.Count == 0)
            {
                response = TranslationManager.Instance.Get("redo_empty");
                return false;
            }

            Dictionary<Primitive, Color> redoState = session.RedoStack.Pop();
            Dictionary<Primitive, Color> undoState = new Dictionary<Primitive, Color>();

            CanvasInstance canvas = null;
            foreach (var firstPrimitive in redoState.Keys)
            {
                canvas = Plugin.Singleton.CanvasManager.ActiveCanvases.Find(c =>
                {
                    int width = c.Grid.GetLength(0);
                    int height = c.Grid.GetLength(1);
                    for (int x = 0; x < width; x++)
                        for (int y = 0; y < height; y++)
                            if (c.Grid[x, y] == firstPrimitive) return true;
                    return false;
                });
                if (canvas != null) break;
            }


            if (canvas == null)
            {
                response = TranslationManager.Instance.Get("canvas_destroyed");
                return false;
            }

            float offset = canvas.PhysicalSize / canvas.Size;
            float startOffsetX = -canvas.PhysicalSize / 2f;
            float startOffsetY = -(canvas.PhysicalSize / canvas.Ratio) / 2f;

            foreach (var pair in redoState)
            {
                Primitive p = pair.Key;
                Color nextColor = pair.Value;

                undoState[p] = p.Position.y < -500f ? Color.clear : p.Color;

                if (nextColor == Color.clear)
                {
                    p.Position = new Vector3(0f, -1000f, 0f);
                }
                else
                {
                    int targetX = -1, targetY = -1;
                    int width = canvas.Grid.GetLength(0);
                    int height = canvas.Grid.GetLength(1);
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            if (canvas.Grid[x, y] == p) { targetX = x; targetY = y; break; }
                        }
                        if (targetX != -1) break;
                    }


                    if (targetX != -1)
                    {
                        Vector3 localPos = new Vector3(startOffsetX + (targetX * offset) + (offset / 2f), startOffsetY + (targetY * offset * (1f / canvas.Ratio)) + ((offset * (1f / canvas.Ratio)) / 2f), 0);
                        p.Position = canvas.RootObject.transform.TransformPoint(localPos);
                        p.Color = nextColor;
                    }
                }
            }

            session.UndoStack.Push(undoState);
            response = TranslationManager.Instance.Get("redo_success");
            return true;
        }
    }
}
