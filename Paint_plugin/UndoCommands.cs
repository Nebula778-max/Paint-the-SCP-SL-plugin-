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
        public string Description => "Отменить последнее действие";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (!Plugin.Singleton.CanvasManager.Sessions.TryGetValue(player, out var session) || session.UndoStack.Count == 0)
            {
                response = "История действий пуста!";
                return false;
            }

            Dictionary<Primitive, Color> previousState = session.UndoStack.Pop();
            Dictionary<Primitive, Color> redoState = new Dictionary<Primitive, Color>();

            CanvasInstance canvas = null;
            foreach (var firstPrimitive in previousState.Keys)
            {
                canvas = Plugin.Singleton.CanvasManager.ActiveCanvases.Find(c =>
                {
                    for (int x = 0; x < c.Size; x++)
                        for (int y = 0; y < c.Size; y++)
                            if (c.Grid[x, y] == firstPrimitive) return true;
                    return false;
                });
                if (canvas != null) break;
            }

            if (canvas == null)
            {
                response = "Холст больше не существует.";
                return false;
            }

            float offset = canvas.PhysicalSize / canvas.Size;
            float startOffset = -canvas.PhysicalSize / 2f;

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
                    for (int x = 0; x < canvas.Size; x++)
                    {
                        for (int y = 0; y < canvas.Size; y++)
                        {
                            if (canvas.Grid[x, y] == p) { targetX = x; targetY = y; break; }
                        }
                        if (targetX != -1) break;
                    }

                    if (targetX != -1)
                    {
                        Vector3 localPos = new Vector3(startOffset + (targetX * offset) + (offset / 2f), startOffset + (targetY * offset) + (offset / 2f), 0);
                        p.Position = canvas.RootObject.transform.TransformPoint(localPos);
                        p.Color = prevColor;
                    }
                }
            }

            session.RedoStack.Push(redoState);
            response = "Действие отменено!";
            return true;
        }
    }

    [CommandHandler(typeof(ClientCommandHandler))]
    public class RedoCommand : ICommand
    {
        public string Command => "redo";
        public string[] Aliases => new[] { "вперед" };
        public string Description => "Повторить отмененное действие";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);
            if (!Plugin.Singleton.CanvasManager.Sessions.TryGetValue(player, out var session) || session.RedoStack.Count == 0)
            {
                response = "Нет действий для повтора!";
                return false;
            }

            Dictionary<Primitive, Color> redoState = session.RedoStack.Pop();
            Dictionary<Primitive, Color> undoState = new Dictionary<Primitive, Color>();

            CanvasInstance canvas = null;
            foreach (var firstPrimitive in redoState.Keys)
            {
                canvas = Plugin.Singleton.CanvasManager.ActiveCanvases.Find(c =>
                {
                    for (int x = 0; x < c.Size; x++)
                        for (int y = 0; y < c.Size; y++)
                            if (c.Grid[x, y] == firstPrimitive) return true;
                    return false;
                });
                if (canvas != null) break;
            }

            if (canvas == null)
            {
                response = "Холст больше не существует.";
                return false;
            }

            float offset = canvas.PhysicalSize / canvas.Size;
            float startOffset = -canvas.PhysicalSize / 2f;

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
                    for (int x = 0; x < canvas.Size; x++)
                    {
                        for (int y = 0; y < canvas.Size; y++)
                        {
                            if (canvas.Grid[x, y] == p) { targetX = x; targetY = y; break; }
                        }
                        if (targetX != -1) break;
                    }

                    if (targetX != -1)
                    {
                        Vector3 localPos = new Vector3(startOffset + (targetX * offset) + (offset / 2f), startOffset + (targetY * offset) + (offset / 2f), 0);
                        p.Position = canvas.RootObject.transform.TransformPoint(localPos);
                        p.Color = nextColor;
                    }
                }
            }

            session.UndoStack.Push(undoState);
            response = "Действие возвращено!";
            return true;
        }
    }
}
