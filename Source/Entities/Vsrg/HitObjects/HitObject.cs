namespace Celeste.Mod.HamburgerHelper.Entities.Vsrg.HitObjects;

public abstract class HitObject : Entity
{
    public VsrgManager Manager;

    public readonly int TimingMs;
    public bool IsHit;
    public NoteType NoteType;

    protected Vector2 ScreenPosition => Manager.ScreenPosition;
    protected float HitPosition => ScreenPosition.Y + 137;
    
    public HitObject(int timingMs, VsrgManager manager = null)
    {
        TimingMs = timingMs;
        Manager = manager;
    }

    protected virtual Color GetColor()
    {
        return Color.White * Manager.GameFade;
    }
    
    protected float GetYFromTime(int timingMs, float songPosition)
    {
        return HitPosition - ((timingMs - songPosition) * (0.25f * 1.5f));
    }
    
    protected float GetXFromColumn(int column)
    {
        return column switch {
            1 => (int)ScreenPosition.X + 116,
            2 => (int)ScreenPosition.X + 139,
            3 => (int)ScreenPosition.X + 162,
            4 => (int)ScreenPosition.X + 185,
            _ => (int)ScreenPosition.X,
        };
    }
    
    public abstract void Draw();
}