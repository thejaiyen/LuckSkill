using System;
using Microsoft.Xna.Framework;
using SpaceShared;
using StardewValley;
using StardewValley.Events;
using StardewValley.Network;

namespace SpaceCore.Events
{
    public class SpaceEvents
    {
        // This occurs before loading starts.
        // Locations should be added here so that SaveData.loadDataToLocations picks them up
        public static event EventHandler OnBlankSave;

        // When the shipping menu pops up, level up menus, ...
        public static event EventHandler<EventArgsShowNightEndMenus> ShowNightEndMenus;

        // Lets you hook into Utillity.pickFarmEvent
        public static event EventHandler<EventArgsChooseNightlyFarmEvent> ChooseNightlyFarmEvent;

        // When the player is done eating an item.
        // Check what item using player.itemToEat
        public static event EventHandler OnItemEaten;

        // When a tile "Action" is activated
        public static event EventHandler<EventArgsAction> ActionActivated;

        // When a tile "TouchAction" is activated
        public static event EventHandler<EventArgsAction> TouchActionActivated;

        // Server side, when a client joins
        public static event EventHandler<EventArgsServerGotClient> ServerGotClient;

        // Right before a gift is given to someone. Sender is farmer.
        public static event EventHandler<EventArgsBeforeReceiveObject> BeforeGiftGiven;

        // When a gift is given to someone. Sender is farmer.
        public static event EventHandler<EventArgsGiftGiven> AfterGiftGiven;

        // Before the player is about to warp. Can cancel warping or change the target location.
        public static event EventHandler<EventArgsBeforeWarp> BeforeWarp;

        // When a bomb explodes
        public static event EventHandler<EventArgsBombExploded> BombExploded;

        internal static void InvokeOnBlankSave()
        {
            Log.trace("Event: OnBlankSave");
            if (SpaceEvents.OnBlankSave == null)
                return;
            Util.invokeEvent("SpaceEvents.OnBlankSave", SpaceEvents.OnBlankSave.GetInvocationList(), null);
        }

        internal static void InvokeShowNightEndMenus(EventArgsShowNightEndMenus args)
        {
            Log.trace("Event: ShowNightEndMenus");
            if (SpaceEvents.ShowNightEndMenus == null)
                return;
            Util.invokeEvent("SpaceEvents.ShowNightEndMenus", SpaceEvents.ShowNightEndMenus.GetInvocationList(), null, args);
        }

        internal static FarmEvent InvokeChooseNightlyFarmEvent(FarmEvent vanilla)
        {
            var args = new EventArgsChooseNightlyFarmEvent();
            args.NightEvent = vanilla;

            Log.trace("Event: ChooseNightlyFarmEvent");
            if (SpaceEvents.ChooseNightlyFarmEvent == null)
                return args.NightEvent;
            Util.invokeEvent("SpaceEvents.ChooseNightlyFarmEvent", SpaceEvents.ChooseNightlyFarmEvent.GetInvocationList(), null, args);
            return args.NightEvent;
        }

        internal static void InvokeOnItemEaten(Farmer farmer)
        {
            Log.trace("Event: OnItemEaten");
            if (SpaceEvents.OnItemEaten == null || !farmer.IsLocalPlayer)
                return;
            Util.invokeEvent("SpaceEvents.OnItemEaten", SpaceEvents.OnItemEaten.GetInvocationList(), farmer);
        }

        internal static bool InvokeActionActivated(Farmer who, string action, xTile.Dimensions.Location pos)
        {
            Log.trace("Event: ActionActivated");
            if (SpaceEvents.ActionActivated == null || !who.IsLocalPlayer)
                return false;
            var arg = new EventArgsAction(false, action, pos);
            return Util.invokeEventCancelable("SpaceEvents.ActionActivated", SpaceEvents.ActionActivated.GetInvocationList(), who, arg);
        }

        internal static bool InvokeTouchActionActivated(Farmer who, string action, xTile.Dimensions.Location pos)
        {
            Log.trace("Event: TouchActionActivated");
            if (SpaceEvents.TouchActionActivated == null || !who.IsLocalPlayer)
                return false;
            var arg = new EventArgsAction(true, action, pos);
            return Util.invokeEventCancelable("SpaceEvents.TouchActionActivated", SpaceEvents.TouchActionActivated.GetInvocationList(), who, arg);
        }

        internal static void InvokeServerGotClient(GameServer server, long peer)
        {
            var args = new EventArgsServerGotClient();
            args.FarmerID = peer;

            Log.trace("Event: ServerGotClient");
            if (SpaceEvents.ServerGotClient == null)
                return;
            Util.invokeEvent("SpaceEvents.ServerGotClient", SpaceEvents.ServerGotClient.GetInvocationList(), server, args);
        }

        internal static bool InvokeBeforeReceiveObject(NPC npc, StardewValley.Object obj, Farmer farmer)
        {
            Log.trace("Event: BeforeReceiveObject");
            if (SpaceEvents.BeforeGiftGiven == null)
                return false;
            var arg = new EventArgsBeforeReceiveObject(npc, obj);
            return Util.invokeEventCancelable("SpaceEvents.BeforeReceiveObject", SpaceEvents.BeforeGiftGiven.GetInvocationList(), farmer, arg);
        }

        internal static void InvokeAfterGiftGiven(NPC npc, StardewValley.Object obj, Farmer farmer)
        {
            Log.trace("Event: AfterGiftGiven");
            if (SpaceEvents.AfterGiftGiven == null)
                return;
            var arg = new EventArgsGiftGiven(npc, obj);
            Util.invokeEvent("SpaceEvents.AfterGiftGiven", SpaceEvents.AfterGiftGiven.GetInvocationList(), farmer, arg);
        }

        internal static bool InvokeBeforeWarp(ref LocationRequest req, ref int targetX, ref int targetY, ref int facing)
        {
            Log.trace("Event: BeforeWarp");
            if (SpaceEvents.BeforeWarp == null)
                return false;
            var arg = new EventArgsBeforeWarp(req, targetX, targetY, facing);
            bool ret = Util.invokeEventCancelable("SpaceEvents.BeforeWarp", SpaceEvents.BeforeWarp.GetInvocationList(), Game1.player, arg);
            req = arg.WarpTargetLocation;
            targetX = arg.WarpTargetX;
            targetY = arg.WarpTargetY;
            facing = arg.WarpTargetFacing;
            return ret;
        }

        internal static void InvokeBombExploded(Farmer who, Vector2 tileLocation, int radius)
        {
            Log.trace("Event: BombExploded");
            if (SpaceEvents.BombExploded == null)
                return;
            var arg = new EventArgsBombExploded(tileLocation, radius);
            Util.invokeEvent("SpaceEvents.BombExploded", SpaceEvents.BombExploded.GetInvocationList(), who, arg);
        }
    }
}
