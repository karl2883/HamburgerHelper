namespace Celeste.Mod.HamburgerHelper.Entities;

[CustomEntity("HamburgerHelper/GlobalRoomController")]
public class GlobalRoomController : Entity
{
    public GlobalRoomController(EntityData data, Vector2 offset) 
        : base(data.Position + offset)
    {
        // true global setting is done via editor plugin, it's not necessary to process here
    }
}