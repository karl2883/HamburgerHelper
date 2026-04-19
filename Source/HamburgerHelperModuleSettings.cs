using Microsoft.Xna.Framework.Input;

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

    [DefaultButtonBinding(Buttons.A, Keys.Z)]
    public ButtonBinding VsrgColumn1 { get; set; }
    
    [DefaultButtonBinding(Buttons.B, Keys.X)]
    public ButtonBinding VsrgColumn2 { get; set; }
    
    [DefaultButtonBinding(Buttons.X, Keys.OemPeriod)]
    public ButtonBinding VsrgColumn3 { get; set; }
    
    [DefaultButtonBinding(Buttons.Y, Keys.OemQuestion)]
    public ButtonBinding VsrgColumn4 { get; set; }
    
    public bool ShowCustomDashIndicators { get; set; } = true;
}