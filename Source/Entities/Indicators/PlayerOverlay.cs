using Celeste.Mod.HamburgerHelper.States;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;

namespace Celeste.Mod.HamburgerHelper.Entities.Indicators;

public class PlayerOverlay : Entity
{
    private readonly MTexture Texture;
    private readonly Func<bool> Condition;
    
    public PlayerOverlay(MTexture texture, Func<bool> condition, int depth)
        : base()
    {
        Depth = depth;
        
        Texture = texture;
        Condition = condition;
        
        AddTag(Tags.Global);
        AddTag(Tags.PauseUpdate);
        AddTag(Tags.TransitionUpdate);
    }

    public override void Render()
    {
        base.Render();
        
        Player player = Scene.Tracker.GetEntity<Player>();
        if (player == null) return;

        if (Condition.Invoke())
        {
            Vector2 baseScale = player.Hair.GetHairScale(0);
            
            Vector2 sign = new Vector2(MathF.Sign(baseScale.X), MathF.Sign(baseScale.Y));
            Vector2 scale = new Vector2(MathF.Abs(baseScale.X), MathF.Abs(baseScale.Y));
            
            Vector2 dampened = Vector2.Lerp(Vector2.One, scale, 0.75f);
            Vector2 dampenedScale = sign * dampened;

            Texture.DrawCentered(player.TopCenter - Vector2.UnitY * 4f, Color.White, dampenedScale);
        }
    }
}