namespace Celeste.Mod.HamburgerHelper.Entities.Vsrg.HitObjects;

public class LongNote : Note
{
    public int EndingMs;
    
    public bool IsHeld;
    public bool IsReleased;
    public bool IsMissed;
    
    public bool EarlyReleased;

    private MTexture TailTexture;

    private const int NoteHeight = 7;
    
    public LongNote(int timingMs, int column, int endingMs, VsrgManager manager = null)
        : base(timingMs, column, manager)
    {
        EndingMs = endingMs;
        NoteType = NoteType.LongNote;
    }
    
    public override void Draw()
    {
        float drawX = GetXFromColumn(Column);
        float bodyY = GetYFromTime(TimingMs, Manager.SmoothSongPosition);
        if (!IsMissed && IsHit && bodyY >= HitPosition) bodyY = HitPosition;
        
        float tailY = GetYFromTime(EndingMs, Manager.SmoothSongPosition);
        if (IsHeld && tailY > bodyY) tailY = bodyY;

        if (!IsMissed && IsHit && tailY >= HitPosition)
        {
            return;
        }
        
        Vector2 bodyPos = new Vector2(drawX, bodyY);
        Vector2 tailPos = new Vector2(drawX, tailY);

        bool bodyOffscreen = bodyY < Manager.ScreenPosition.Y - 16f
            || bodyY > Manager.ScreenPosition.Y + Engine.ViewHeight;
        bool tailOffscreen = tailY < Manager.ScreenPosition.Y - 16f 
            || tailY > Manager.ScreenPosition.Y + Engine.ViewHeight;

        if (bodyOffscreen && tailOffscreen)
            return;
        
        float tailSize = bodyPos.Y - tailPos.Y + NoteHeight;
        Vector2 tailScale = new Vector2(1f, tailSize);
        
        TailTexture.Draw(tailPos, Vector2.Zero, GetColor(), tailScale);
        
        if (!IsHit)
        {
            NoteTexture?.Draw(bodyPos, Vector2.Zero, GetColor());   
        }
        NoteTexture?.Draw(tailPos, Vector2.Zero, GetColor());
    }
    
    protected override Color GetColor()
    {
        return IsReleased ? Calc.HexToColor("808080") * Manager.GameFade : Color.White * Manager.GameFade;
    }
    
    protected override void GetObjectTextures()
    {
        base.GetObjectTextures();
        
        TailTexture = Column is 1 or 2
            ? GFX.Game["objects/hamburger/vsrg/note/leftHoldMiddle"]
            : GFX.Game["objects/hamburger/vsrg/note/rightHoldMiddle"];
    }
}