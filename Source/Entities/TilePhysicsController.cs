namespace Celeste.Mod.HamburgerHelper.Entities;

// a large amount of this code was recycled from an old project
// this originally was made to specifically block refills on this block, and apply a pseudo-climb blocker to it

[CustomEntity("HamburgerHelper/TilePhysicsController")]
public class TilePhysicsController : Entity
{
    private readonly char TileType;
    private readonly bool BlockInteractions;

    private readonly string FloorFlag;
    private readonly string WallFlag;

    private bool TouchingControlledWall;
    private bool TouchingControlledFloor;
    private bool TouchingOppositeControlledWall;
    
    public TilePhysicsController(EntityData data, Vector2 offset) 
        : base(data.Position + offset)
    {
        TileType = data.Char("tileType", '3');
        BlockInteractions = data.Bool("blockInteractions", false);

        FloorFlag = data.Attr("floorFlag", "TilePhysics_FloorX");
        WallFlag = data.Attr("wallFlag", "TilePhysics_WallX");
    }
    
    public override void Update()
    {
        if (Scene is not Level level) return;
        
        Player player = level.Tracker.GetEntity<Player>();
        if (player == null) return;
        
        char floorType = GetFloorTileType(player);
        char wallType = GetWallTileType(player, false);
        char oppositeWallType = GetWallTileType(player, true);
        
        TouchingControlledWall = wallType == TileType;
        TouchingControlledFloor = floorType == TileType;
        TouchingOppositeControlledWall = oppositeWallType == TileType;
    }

    private static char GetTileTypeAt(Vector2 scanPos, bool render = false)
    {
        if (Engine.Scene is null) return '0';
        
        SolidTiles tiles = Engine.Scene.Tracker.GetEntity<SolidTiles>();
        if (tiles == null) return '0';
        
        int tileX = (int)((scanPos.X - tiles.X) / 8f);
        int tileY = (int)((scanPos.Y - tiles.Y) / 8f);

        if (render)
        {
            Draw.Point(scanPos, Color.Red);   
        }

        if (tileX < 0 || tileY < 0 || tileX >= tiles.Grid.CellsX || tileY >= tiles.Grid.CellsY)
        {
            return '0';   
        }
        
        char type = tiles.tileTypes[tileX, tileY];
        return type;
    }
    
    private static char GetFloorTileType(Player player, bool render = false)
    {
        char tile = GetTileTypeAt(player.BottomCenter + Vector2.UnitY * 4f, render);
        if (tile == '0')
        {
            tile = GetTileTypeAt(player.BottomLeft + Vector2.UnitY * 4f, render);
        }
        if (tile == '0')
        {
            tile = GetTileTypeAt(player.BottomRight - Vector2.UnitX + Vector2.UnitY * 4f, render);
        }
        return tile;
    }
    
    private static char GetWallTileType(Player player, bool opposite = false, bool render = false)
    {
        int side = (int)player.Facing;
        if (opposite)
        {
            side *= -1;
        }
        
        float verticalOffset = 5f;
        if (player.Ducking)
        {
            verticalOffset = 2f;
        }
        
        char tile = GetTileTypeAt(player.Center + Vector2.UnitX * side * 9, render);
        if (tile == '0')
        {
            tile = GetTileTypeAt(player.Center + new Vector2(side * 9, -verticalOffset - 2), render);
        }
        if (tile == '0')
        {
            tile = GetTileTypeAt(player.Center + new Vector2(side * 9, verticalOffset), render);
        }
        return tile;
    }

    private static bool TouchingControlledSurface(Player player, bool floor, 
        bool opposite = false, bool checkBlockClimbing = false)
    {
        if (player.Scene is not Level level) return false;
        
        List<Entity> entities = level.Tracker.GetEntitiesTrackIfNeeded<TilePhysicsController>();
        if (entities is not { Count: > 0 }) return false;
            
        bool touchingControlledSurface = false;
            
        foreach (Entity ent in entities)
        {
            if (ent is not TilePhysicsController controller) continue;

            if (checkBlockClimbing && !controller.BlockInteractions)
                continue;
            
            if (floor)
            {
                if (!controller.TouchingControlledFloor)
                    continue;
            }
            else if (opposite)
            {
                if (!controller.TouchingOppositeControlledWall)
                    continue;
            }
            else
            {
                if (!controller.TouchingControlledWall)
                    continue;
            }
            
            touchingControlledSurface = true;
            break;
        }
        
        return touchingControlledSurface;
    }
    
    [OnLoad]
    internal static void Load()
    {
        On.Celeste.Player.Update += PlayerOnUpdate;
        
        On.Celeste.ClimbBlocker.Check_Scene_Entity += ClimbBlockerOnCheck_Scene_Entity;
        On.Celeste.ClimbBlocker.EdgeCheck += ClimbBlockerOnEdgeCheck;
    }

    [OnUnload]
    internal static void Unload()
    {
        On.Celeste.Player.Update -= PlayerOnUpdate;
        
        On.Celeste.ClimbBlocker.Check_Scene_Entity -= ClimbBlockerOnCheck_Scene_Entity;
        On.Celeste.ClimbBlocker.EdgeCheck -= ClimbBlockerOnEdgeCheck;
    }
    
    private static void PlayerOnUpdate(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);

        if (self.Scene is not Level level) return;
        
        List<Entity> entities = level.Tracker.GetEntitiesTrackIfNeeded<TilePhysicsController>();
        if (entities is not { Count: > 0 }) return;

        foreach (Entity ent in entities)
        {
            if (ent is not TilePhysicsController controller) continue;
            
            level.Session.SetFlag(controller.FloorFlag, controller.TouchingControlledFloor);
            level.Session.SetFlag(controller.WallFlag, controller.TouchingControlledWall);
        }
    }
    
    private static bool ClimbBlockerOnCheck_Scene_Entity(On.Celeste.ClimbBlocker.orig_Check_Scene_Entity orig, 
        Scene scene, Entity entity)
    {
        Player player = scene.Tracker.GetEntity<Player>();
        if (player == null) return orig(scene, entity);
        
        bool touchingSurface = TouchingControlledSurface(player, false, checkBlockClimbing: true);
        bool touchingOppositeSurface = TouchingControlledSurface(player, false, true, checkBlockClimbing: true);
        
        return orig(scene, entity) || touchingSurface || touchingOppositeSurface;
    }
    
    private static bool ClimbBlockerOnEdgeCheck(On.Celeste.ClimbBlocker.orig_EdgeCheck orig, 
        Scene scene, Entity entity, int dir)
    {
        Player player = scene.Tracker.GetEntity<Player>();
        if (player == null) return orig(scene, entity, dir);
        
        bool touchingSurface = TouchingControlledSurface(player, false, checkBlockClimbing: true);
        bool touchingOppositeSurface = TouchingControlledSurface(player, false, true, checkBlockClimbing: true);
        
        return orig(scene, entity, dir) || touchingSurface || touchingOppositeSurface;
    }
}
