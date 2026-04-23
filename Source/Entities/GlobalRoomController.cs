namespace Celeste.Mod.HamburgerHelper.Entities;

[CustomEntity("HamburgerHelper/GlobalRoomController")]
public class GlobalRoomController : Entity
{
    protected GlobalRoomController(EntityData data, Vector2 offset) 
        : base(data.Position + offset)
    {
        // EntityData for this is parsed in Misc/GlobalRooms.cs
    }
}

[CustomEntity("HamburgerHelper/LocalRoomController")]
public class LocalRoomController : Entity
{
    public LocalRoomController(EntityData data, Vector2 offset) 
        : base(data.Position + offset)
    {
        // EntityData for this is parsed in Misc/GlobalRooms.cs
    }
}