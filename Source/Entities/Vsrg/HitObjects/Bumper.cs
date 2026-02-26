namespace Celeste.Mod.HamburgerHelper.Entities.Vsrg.HitObjects;

public enum BumperPosition
{
    Left,
    Middle,
    Right,
}

public enum BumperType
{
    Left,
    Middle,
    Right,
    JudgeLeft,
    JudgeMiddle,
    JudgeRight,
}

public class Bumper : HitObject
{
    public BumperPosition BumperPosition;
    protected MTexture BumperTexture;
    
    public Bumper(int timingMs, BumperPosition bumperPosition, VsrgManager manager = null)
        : base(timingMs, manager)
    {
        BumperPosition = bumperPosition;
        NoteType = NoteType.Bumper;
        
        // ReSharper disable once VirtualMemberCallInConstructor
        GetObjectTextures();
    }

    public override void Draw()
    {
        if (IsHit) return;
        
        float drawX = GetXFromBumperPosition(BumperPosition);
        float drawY = GetYFromTime(TimingMs, Manager.SmoothSongPosition);
        
        if (drawY < Manager.ScreenPosition.Y - 16f || drawY > Manager.ScreenPosition.Y + Engine.ViewHeight)
            return;
        
        Vector2 drawPos = new Vector2(drawX, drawY);
        
        BumperTexture?.Draw(drawPos, Vector2.Zero, GetColor());
    }

    private int GetXFromBumperPosition(BumperPosition bumperPosition)
    {
        return bumperPosition switch {
            BumperPosition.Left => (int)ScreenPosition.X + 109,
            BumperPosition.Middle => (int)ScreenPosition.X + 108,
            BumperPosition.Right => (int)ScreenPosition.X + 162,
            _ => (int)ScreenPosition.X,
        };
    }
    
    protected virtual void GetObjectTextures()
    {
        BumperTexture = BumperPosition switch {
            BumperPosition.Left => GFX.Game["objects/hamburger/vsrg/note/bumper/bumperL"],
            BumperPosition.Middle => GFX.Game["objects/hamburger/vsrg/note/bumper/bumperM"],
            BumperPosition.Right => GFX.Game["objects/hamburger/vsrg/note/bumper/bumperR"],
            _ => null
        };
    }
}