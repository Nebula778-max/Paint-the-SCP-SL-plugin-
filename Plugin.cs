using Exiled.API.Features;
using Exiled.CustomItems.API;
using Exiled.CustomItems.API.Features;
using System;
using System.Collections.Generic;
using Handlers = Exiled.Events.Handlers;

namespace SCPCanvasPaint
{
    public class Plugin : Plugin<Config>
    {
        public static Plugin Singleton;
        public static HashSet<string> AllowedPlayers = new HashSet<string>();
        public static bool AllowAllPlayers = false;
        public CanvasManager CanvasManager;
        public EventHandler EventHandler;
        public ArtistBrush BrushItem;

        public override string Name => "SCPCanvasPaint";
        public override string Author => "Nebula";
        public override Version Version => new Version(1, 0, 2);

        public override void OnEnabled()
        {
            Singleton = this;
            CanvasManager = new CanvasManager();
            EventHandler = new EventHandler(CanvasManager);

            BrushItem = new ArtistBrush();
            BrushItem.Register();

            Handlers.Player.DroppingItem += EventHandler.OnDroppingItem;
            Handlers.Player.UsingItem += EventHandler.OnUsingItem;
            Handlers.Player.Left += EventHandler.OnPlayerLeft;

            CanvasManager.InitSaveDirectory();
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Handlers.Player.DroppingItem -= EventHandler.OnDroppingItem;
            Handlers.Player.UsingItem -= EventHandler.OnUsingItem;
            Handlers.Player.Left -= EventHandler.OnPlayerLeft;

            BrushItem.Unregister();
            BrushItem = null;

            AllowedPlayers.Clear();
            AllowAllPlayers = false;

            CanvasManager.CleanUp();
            CanvasManager = null;
            EventHandler = null;
            Singleton = null;
            base.OnDisabled();
        }
    }
}
