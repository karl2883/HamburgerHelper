namespace Celeste.Mod.HamburgerHelper.Entities.Indicators;

[CustomEntity("HamburgerHelper/EditorIndicator")]
public class EditorIndicator : Entity
{
    public EditorIndicator(EntityData data, Vector2 offset) 
        : base(data.Position + offset)
    {
        // this exists purely as a CustomEntity to make warnings not complain in logs
    }
}
