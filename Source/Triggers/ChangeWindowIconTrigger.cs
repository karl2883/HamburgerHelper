namespace Celeste.Mod.HamburgerHelper.Triggers;

[CustomEntity("HamburgerHelper/ChangeWindowIconTrigger")]
public class ChangeWindowIconTrigger : Trigger
{
    private readonly string IconPath;
    
    private readonly bool UseFlag;
    private readonly string FlagName;
    
    public ChangeWindowIconTrigger(EntityData data, Vector2 offset)
        : base(data, offset)
    {
        IconPath = data.Attr("iconPath", "strawberrysdl");
        
        UseFlag = data.Bool("useFlag", false);
        FlagName = data.Attr("flagName", "");
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);

        if (HamburgerHelperModule.Settings.DisableWindowIconChanges) return;
        
        if (Scene is not Level level) return;
        if (UseFlag && !level.Session.GetFlag(FlagName)) return;
        
        MTexture iconTexture = GFX.Game[$"hamburger/icons/{IconPath}"];
        nint windowHandle = Engine.Instance.Window.Handle;
        
        WindowUtils.SetWindowIconFromMTexture(windowHandle, iconTexture);
    }
}