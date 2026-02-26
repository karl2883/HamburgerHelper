namespace Celeste.Mod.HamburgerHelper.Entities.Vsrg;

public enum Judgement
{
    Miss,
    Good,
    Great,
    Critical,
    Marvelous,
}

public static class VsrgJudgements
{
    private const int BumperCrit = 150;
    
    private const int NoteMarv = 35;
    private const int NoteCrit = 70;
    private const int NoteGreat = 110;
    private const int NoteGood = 150;
    private const int NoteMiss = 200;
    
    public static Judgement JudgementAtOffset(double msOffset, NoteType type)
    {
        msOffset = Math.Abs(msOffset);
        
        if (type is NoteType.Bumper or NoteType.LongNote)
        {
            return Judgement.Marvelous;
        }
        else
        {
            switch (msOffset)
            {
                case <= NoteMarv:
                    return Judgement.Marvelous;
                case <= NoteCrit:
                    return Judgement.Critical;
                case <= NoteGreat:
                    return Judgement.Great;
                case <= NoteGood:
                    return Judgement.Good;
                case <= NoteMiss:
                    return Judgement.Miss;
            }
        }
        
        return Judgement.Miss;
    }
    
    public static bool CanHitNote(double msOffset, NoteType type)
    {
        msOffset = Math.Abs(msOffset);
        return msOffset <= (type is NoteType.Bumper or NoteType.LongNote ? BumperCrit : NoteMiss); 
    }
    
    public static bool MissedNote(double msOffset)
    {
        return msOffset < -NoteMiss;
    }
}
