namespace Celeste.Mod.HamburgerHelper;

public class HamburgerHelperModuleSession : EverestModuleSession
{
    public bool HasDreamerDash { get; set; } = false;
    public bool UsedDreamerDash { get; set; } = false;
    
    public bool HasDarkAngelWings { get; set; } = false;
    public bool UsingDarkAngelWings { get; set; } = false;
    
    public bool CanLight { get; set; } = false;
}