using Celeste.Mod.Roslyn.ModLifecycleAttributes;

namespace Celeste.Mod.HamburgerHelper.Entities;

// ReSharper disable once ClassNeverInstantiated.Global
[CustomEntity("HamburgerHelper/TilesetSoundSplitter")]
public class TilesetSoundSplitter : Entity
{
    private readonly char TileType;
    
    private readonly int WallSoundIndex;
    private readonly int StepSoundIndex;
    private readonly int LandSoundIndex;
    
    public TilesetSoundSplitter(EntityData data, Vector2 offset) 
        : base(data.Position + offset)
    {
        TileType = data.Char("tileType", '3');
        
        WallSoundIndex = data.Int("wallSoundIndex", 0);
        StepSoundIndex = data.Int("stepSoundIndex", 0);
        LandSoundIndex = data.Int("landSoundIndex", 0);
    }
    
    private static char GetTileTypeAt(Vector2 scanPos)
    {
        if (Engine.Scene == null) return '0';
        
        SolidTiles tiles = Engine.Scene.Tracker.GetEntity<SolidTiles>();
        if (tiles == null) return '0';
        
        int tileX = (int)((scanPos.X - tiles.X) / 8f);
        int tileY = (int)((scanPos.Y - tiles.Y) / 8f);
        
        if (tileX < 0 || tileY < 0 || tileX >= tiles.Grid.CellsX || tileY >= tiles.Grid.CellsY)
            return '0';
        
        char type = tiles.tileTypes[tileX, tileY];
        return type;
    }

    private static char GetWallTileType(Player player, int side)
    {
        char tile = GetTileTypeAt(player.Center + Vector2.UnitX * side * 8f);
        if (tile == '0')
        {
            tile = GetTileTypeAt(player.Center + new Vector2(side * 8, -6f));
        }
        if (tile == '0')
        {
            tile = GetTileTypeAt(player.Center + new Vector2(side * 8, 6f));
        }
        return tile;
    }

    private static char GetStepTileType(Entity entity)
    {
        char tile = GetTileTypeAt(entity.BottomCenter + Vector2.UnitY * 4f);
        if (tile == '0')
        {
            tile = GetTileTypeAt(entity.BottomLeft + Vector2.UnitY * 4f);
        }
        if (tile == '0')
        {
            tile = GetTileTypeAt(entity.BottomRight + Vector2.UnitY * 4f);
        }
        return tile;
    }

    private static char GetLandTileType(Entity entity)
    {
        char tile = GetTileTypeAt(entity.BottomCenter + Vector2.UnitY * 4f);
        if (tile == '0')
        {
            tile = GetTileTypeAt(entity.BottomLeft + Vector2.UnitY * 4f);
        }
        if (tile == '0')
        {
            tile = GetTileTypeAt(entity.BottomRight + Vector2.UnitY * 4f);
        }
        return tile;
    }
    
    [OnLoad]
    internal static void Load()
    {
        On.Celeste.SolidTiles.GetWallSoundIndex += SolidTilesOnGetWallSoundIndex;
        On.Celeste.SolidTiles.GetStepSoundIndex += SolidTilesOnGetStepSoundIndex;
        On.Celeste.SolidTiles.GetLandSoundIndex += SolidTilesOnGetLandSoundIndex;
    }

    [OnUnload]
    internal static void Unload()
    {
        On.Celeste.SolidTiles.GetWallSoundIndex -= SolidTilesOnGetWallSoundIndex;
        On.Celeste.SolidTiles.GetStepSoundIndex -= SolidTilesOnGetStepSoundIndex;
        On.Celeste.SolidTiles.GetLandSoundIndex -= SolidTilesOnGetLandSoundIndex;
    }
    
    private static int SolidTilesOnGetWallSoundIndex(On.Celeste.SolidTiles.orig_GetWallSoundIndex orig, SolidTiles self, Player player, int side)
    {
        if (self.Scene is not Level level ) return orig(self, player, side);
        
        List<Entity> entities = level.Tracker.GetEntitiesTrackIfNeeded<TilesetSoundSplitter>();
        foreach (Entity ent in entities)
        {
            if (ent is not TilesetSoundSplitter splitter)
                continue;
            
            if (GetWallTileType(player, side) == splitter.TileType)
            {
                return splitter.WallSoundIndex;
            }
        }
        
        return orig(self, player, side);
    }
    
    private static int SolidTilesOnGetStepSoundIndex(On.Celeste.SolidTiles.orig_GetStepSoundIndex orig, SolidTiles self, Entity entity)
    {
        if (self.Scene is not Level level ) return orig(self, entity);
        
        List<Entity> entities = level.Tracker.GetEntitiesTrackIfNeeded<TilesetSoundSplitter>();
        foreach (Entity ent in entities)
        {
            if (ent is not TilesetSoundSplitter splitter)
                continue;
            
            if (GetStepTileType(entity) == splitter.TileType)
            {
                return splitter.StepSoundIndex;
            }
        }
        
        return orig(self, entity);
    }
    
    private static int SolidTilesOnGetLandSoundIndex(On.Celeste.SolidTiles.orig_GetLandSoundIndex orig, SolidTiles self, Entity entity)
    {
        if (self.Scene is not Level level ) return orig(self, entity);
        
        List<Entity> entities = level.Tracker.GetEntitiesTrackIfNeeded<TilesetSoundSplitter>();
        foreach (Entity ent in entities)
        {
            if (ent is not TilesetSoundSplitter splitter)
                continue;
            
            if (GetLandTileType(entity) == splitter.TileType)
            {
                return splitter.LandSoundIndex;
            }
        }
        
        return orig(self, entity);
    }
}
