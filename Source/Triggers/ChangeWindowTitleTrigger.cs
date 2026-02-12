using System.Text.RegularExpressions;

namespace Celeste.Mod.HamburgerHelper.Triggers;

[CustomEntity("HamburgerHelper/ChangeWindowTitleTrigger")]
public partial class ChangeWindowTitleTrigger : Trigger
{
    private string WindowTitle;
    
    private readonly bool UseFlag;
    private readonly string FlagName;
    
    public ChangeWindowTitleTrigger(EntityData data, Vector2 offset) 
            : base(data, offset)
    {
        WindowTitle = data.Attr("windowTitle", "Celeste");
        
        UseFlag = data.Bool("useFlag", false);
        FlagName = data.Attr("flagName", "");
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);

        if (HamburgerHelperModule.Settings.DisableWindowTitleChanges) return;
        
        if (Scene is not Level level) return;
        if (UseFlag && !level.Session.GetFlag(FlagName)) return;

        const string celeste = "Celeste";
        
        string mapName = Dialog.Clean(level.Session.Area.SID);
        string deathCount = $"{level.Session.Deaths}";
        string roomName = level.Session.Level;
        
        WindowTitle = CurlyBraceRegex().Replace(WindowTitle, match => {
            foreach (string inner in match.Groups[1].Value.Split(','))
            {
                return inner.ToLower() switch {
                    "celeste" => celeste,
                    "mapname" => mapName,
                    "deathcount" => deathCount,
                    "roomname" => roomName,
                    _ => Dialog.Clean(inner)
                };
            }
            
            return "";
        });
        
        WindowUtils.SetWindowTitle(WindowTitle);
    }
    
    [GeneratedRegex("{([^}]*)}")]
    private static partial Regex CurlyBraceRegex();
}