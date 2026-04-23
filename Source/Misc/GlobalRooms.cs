// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace Celeste.Mod.HamburgerHelper.Misc;

public static class GlobalRooms
{
    private const string ControllerName = "HamburgerHelper/GlobalRoomController";
    
    private static readonly Dictionary<EntityData, Vector2> GlobalData = [];
    private static readonly Dictionary<EntityData, Vector2> LocalData = [];
    
    [OnLoad]
    internal static void Load()
    {
        On.Celeste.Level.LoadLevel += LevelOnLoadLevel;
        On.Monocle.Scene.Add_Entity += SceneOnAdd_Entity;
    }
    
    [OnUnload]
    internal static void Unload()
    {
        On.Celeste.Level.LoadLevel -= LevelOnLoadLevel;
        On.Monocle.Scene.Add_Entity -= SceneOnAdd_Entity;
    }
    
    private static void LevelOnLoadLevel(On.Celeste.Level.orig_LoadLevel orig, Level level, 
        Player.IntroTypes playerIntro, bool isFromLoader)
    {
        GlobalData.Clear();
        LocalData.Clear();
        
        HashSet<string> globalRooms = level.Session.MapData.Levels
            .Where(data => data.Entities.Any(e => e.Name == ControllerName))
            .Select(data => data.Name)
            .ToHashSet();
        
        foreach (LevelData data in level.Session.MapData.Levels.Where(data => globalRooms.Contains(data.Name)))
        {
            EntityData controller = data.Entities.First(e => e.Name == ControllerName);
            
            bool localRoom = controller.Bool("localRoom", true);
            if (!localRoom && !isFromLoader)
                continue;
            
            Dictionary<EntityData, Vector2> entityList = localRoom ? LocalData : GlobalData;
            InjectLevelData(data, entityList, level);
        }
        
        orig(level, playerIntro, isFromLoader);
        
        CleanupGlobalData(GlobalData, level);
        CleanupGlobalData(LocalData, level);
    }

    private static void InjectLevelData(LevelData data, Dictionary<EntityData, Vector2> entityList, Level level)
    {
        foreach (EntityData entityData in data.Entities.Where(e => e.Name != ControllerName))
        {
            InjectEntityData(entityData, data.Position, entityList, level.Session.LevelData.Entities);
        }
        
        foreach (EntityData entityData in data.Triggers)
        {
            InjectEntityData(entityData, data.Position, entityList, level.Session.LevelData.Triggers);
        }
    }

    private static void InjectEntityData(EntityData entityData, Vector2 offset, 
        Dictionary<EntityData, Vector2> entityList, List<EntityData> entities)
    {
        entityData.Position -= offset;
        for (int i = 0; i < entityData.Nodes.Length; i++)
        {
            entityData.Nodes[i] -= offset;
        }
        
        if (!entities.Contains(entityData))
        {
            entities.Add(entityData);
        }
        entityList[entityData] = offset;
    }
    
    private static void CleanupGlobalData(Dictionary<EntityData, Vector2> roomData, Level level)
    {
        foreach ((EntityData entityData, Vector2 offset) in roomData)
        {
            level.Session.LevelData.Entities.Remove(entityData);
            level.Session.LevelData.Triggers.Remove(entityData);
            
            entityData.Position += offset;
            for (int i = 0; i < entityData.Nodes.Length; i++)
            {
                entityData.Nodes[i] += offset;
            }
        }
    }
    
    // originally i had hooks for all other overloads of Scene.Add, but i don't think they're relevant here
    // if somehow they are, and someone needs me to add them back, let me know!
    private static void SceneOnAdd_Entity(On.Monocle.Scene.orig_Add_Entity orig, Scene self, Entity entity)
    {
        EntityData data = entity.SourceData;
        if (data == null)
        {
            orig(self, entity);
            return;
        }
        
        if (GlobalData.TryGetValue(data, out Vector2 _))
        {
            entity.AddTag(Tags.Global);
        }
        orig(self, entity);
    }
}