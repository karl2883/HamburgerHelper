namespace Celeste.Mod.HamburgerHelper.Entities.Vsrg.HitObjects;

public class JudgementBumper : Bumper
{
    public JudgementBumper(int timingMs, BumperPosition bumperPosition, VsrgManager manager = null)
        : base(timingMs, bumperPosition, manager)
    {
        NoteType = NoteType.JudgementBumper;
    }
    
    protected override void GetObjectTextures()
    {
        BumperTexture = BumperPosition switch {
            BumperPosition.Left => GFX.Game["objects/hamburger/vsrg/note/bumper/judgementBumperL"],
            BumperPosition.Middle => GFX.Game["objects/hamburger/vsrg/note/bumper/judgementBumperM"],
            BumperPosition.Right => GFX.Game["objects/hamburger/vsrg/note/bumper/judgementBumperR"],
            _ => null
        };
    }
}