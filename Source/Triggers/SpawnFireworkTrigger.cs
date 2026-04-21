using Celeste.Mod.HamburgerHelper.Backdrops;

namespace Celeste.Mod.HamburgerHelper.Triggers;

[CustomEntity("HamburgerHelper/SpawnFireworkTrigger")]
public class SpawnFireworkTrigger : Trigger
{
    private readonly List<Color> Colors;
    
    public SpawnFireworkTrigger(EntityData data, Vector2 offset) 
        : base(data, offset)
    {
        Colors = data.Attr("colors", "313188,282c6f,4c2f81,652b76,783ba6")
            .Split(',')
            .Select(str => Calc.HexToColor(str.Trim()))
            .ToList();
    }
    
    public override void OnEnter(Player player)
    {
        base.OnEnter(player);

        if (Scene is not Level level) return;
        
        foreach (Backdrop bg in level.Background.Backdrops)
        {
            if (bg is Fireworks fw)
            {
                fw.CreateFirework(Colors);
            }
        }
        
        foreach (Backdrop bg in level.Foreground.Backdrops)
        {
            if (bg is Fireworks fw)
            {
                fw.CreateFirework(Colors);
            }
        }
    }
}
