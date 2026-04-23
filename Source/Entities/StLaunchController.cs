namespace Celeste.Mod.HamburgerHelper.Entities;

[CustomEntity("HamburgerHelper/StLaunchController")]
public class StLaunchController : Entity
{
    public StLaunchController(EntityData data, Vector2 offset) 
        : base(data.Position + offset)
    {
    }
    
    [OnLoad]
    internal static void Load()
    {
        On.Celeste.Player.LaunchUpdate += PlayerOnLaunchUpdate;
    }

    [OnUnload]
    internal static void Unload()
    {
        On.Celeste.Player.LaunchUpdate -= PlayerOnLaunchUpdate;
    }
    
    private static int PlayerOnLaunchUpdate(On.Celeste.Player.orig_LaunchUpdate orig, Player self)
    {
        // just to add it to the tracker for sure :sobs:
        self.Scene.Tracker.GetEntitiesTrackIfNeeded<StLaunchController>();
        
        int controllerCount = self.Scene.Tracker.CountEntities<StLaunchController>();
        if (controllerCount == 0) return orig(self);

        return self.onGround ? Player.StNormal : orig(self);
    }
}
