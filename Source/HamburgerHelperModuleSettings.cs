namespace Celeste.Mod.HamburgerHelper;

public class HamburgerHelperModuleSettings : EverestModuleSettings
{
    private bool disableWindowTitleChanges = false;
    
    public bool DisableWindowTitleChanges
    {
        get => disableWindowTitleChanges;
        set
        {
            disableWindowTitleChanges = value;
            WindowUtils.UpdateWindowTitleSetting(value);
        }
    }

    private bool disableWindowIconChanges = false;
    
    public bool DisableWindowIconChanges
    {
        get => disableWindowIconChanges;
        set
        {
            disableWindowIconChanges = value;
            WindowUtils.UpdateWindowIconSetting(value);
        }
    }
    
}