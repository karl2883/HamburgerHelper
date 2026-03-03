using System.Collections;
using Celeste.Mod.HamburgerHelper.Components;
using Celeste.Mod.HamburgerHelper.Entities;

namespace Celeste.Mod.HamburgerHelper.States;

public static class Cursor
{
    public static float CursorStateTime = 2f;
    public static float CursorStateSpeed = 3f;
    
    private static Facings FacingOnCursorStart = Facings.Left;
    
    public static int CursorUpdate(Player player)
    {
        player.Facing = FacingOnCursorStart;
        
        player.Speed = Calc.Approach(player.Speed, Vector2.Zero, 2500f * Engine.DeltaTime);
        
        player.Sprite.Play(PlayerSprite.FallSlow);
        player.Sprite.Scale = Vector2.One;
        
        CursorStateTime -= Engine.DeltaTime;
        
        float moveAmount = CursorStateSpeed;
        
        switch (Input.Aim.Value.X)
        {
            case > 0:
                player.MoveH(moveAmount);
                break;
            case < 0:
                player.MoveH(-moveAmount);
                break;
        }

        switch (Input.Aim.Value.Y)
        {
            case > 0:
                player.MoveV(moveAmount);
                break;
            case < 0:
                player.MoveV(-moveAmount);
                break;
        }
        
        return player.CanDash ? player.StartDash() : St.Cursor;
    }
    
    public static IEnumerator CursorRoutine(Player player)
    {
        while (CursorStateTime > 0f)
        {
            yield return null;
        }
        
        player.StateMachine.State = Player.StNormal;
    }
    
    public static void CursorBegin(Player player)
    {
        FacingOnCursorStart = player.Facing;
        
        player.RefillDash();
        player.RefillStamina();
        
        player.Ducking = false;
        player.Facing = Facings.Left;
        
        player.Play("event:/HamburgerHelper/sfx/mouse_down");
        CursorComp?.Pickup();
    }
    
    public static void CursorEnd(Player player)
    {
        player.Play("event:/HamburgerHelper/sfx/mouse_up");
        CursorComp?.Release();
    }
    
    #region Sprite Hooks
    
    private static CursorComponent CursorComp;
    
    internal static void Load()
    {
        On.Celeste.Player.Added += PlayerOnAdded;
        On.Celeste.Player.Die += PlayerOnDie;
        
        On.Celeste.Player.UpdateSprite += PlayerOnUpdateSprite;
    }
    
    internal static void Unload()
    {
        On.Celeste.Player.Added -= PlayerOnAdded;
        On.Celeste.Player.Die -= PlayerOnDie;
        
        On.Celeste.Player.UpdateSprite -= PlayerOnUpdateSprite;
    }
    
    private static void PlayerOnAdded(On.Celeste.Player.orig_Added orig, Player self, Scene scene)
    {
        orig(self, scene);
        
        if (CursorComp != null)
        {
            self.Remove(CursorComp);
            CursorComp = null;
        }
        
        CursorComp = new CursorComponent()
        {
            Position = new Vector2(-8f, -22f)
        };
        
        self.Add(CursorComp);
    }
    
    private static PlayerDeadBody PlayerOnDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
    {
        if (self.StateMachine.State == St.Cursor)
            CursorEnd(self);
        
        return orig(self, direction, evenIfInvincible, registerDeathInStats);
    }
    
    private static void PlayerOnUpdateSprite(On.Celeste.Player.orig_UpdateSprite orig, Player self)
    {
        if (self.StateMachine.State == St.Cursor) return;
        orig(self);
    }
    
    #endregion
}
