namespace Celeste.Mod.HamburgerHelper.Entities.Vsrg.HitObjects;

public class Note : HitObject
{
    public readonly int Column;
    protected MTexture NoteTexture;
    
    public Note(int timingMs, int column, VsrgManager manager = null)
        : base(timingMs, manager)
    {
        Column = column;
        NoteType = NoteType.Note;
        
        // ReSharper disable once VirtualMemberCallInConstructor
        GetObjectTextures();
    }
    
    public override void Draw()
    {
        if (IsHit) return;
        
        float drawX = GetXFromColumn(Column);
        float drawY = GetYFromTime(TimingMs, Manager.SmoothSongPosition);
        
        if (drawY < Manager.ScreenPosition.Y - 16f || drawY > Manager.ScreenPosition.Y + Engine.ViewHeight)
            return;
        
        Vector2 drawPos = new Vector2(drawX, drawY);
        
        NoteTexture?.Draw(drawPos, Vector2.Zero, GetColor());
    }
    
    protected virtual void GetObjectTextures()
    {
        NoteTexture = Column is 1 or 2
            ? GFX.Game["objects/hamburger/vsrg/note/noteLeft"]
            : GFX.Game["objects/hamburger/vsrg/note/noteRight"];
    }
}
