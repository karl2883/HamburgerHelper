namespace Celeste.Mod.HamburgerHelper.Entities.Vsrg.HitObjects;

public class Mine : Note
{
    public Mine(int timingMs, int column, VsrgManager manager = null)
        : base(timingMs, column, manager)
    {
        NoteType = NoteType.Mine;
    }

    protected override void GetObjectTextures()
    {
        NoteTexture = GFX.Game["objects/hamburger/vsrg/note/mine"];
    }
}
