// ReSharper disable ConvertIfStatementToReturnStatement
// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable InvertIf

using System.Text.RegularExpressions;

namespace Celeste.Mod.HamburgerHelper.Misc;

public static class GlobalRooms
{
    private const string GlobalController = "HamburgerHelper/GlobalRoomController";
    private const string LocalController = "HamburgerHelper/LocalRoomController";
    
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
            .Where(data => data.Entities.Any(e => e.Name is GlobalController or LocalController))
            .Select(data => data.Name)
            .ToHashSet();
        
        foreach (LevelData data in level.Session.MapData.Levels.Where(data => globalRooms.Contains(data.Name)))
        {
            EntityData globalController = data.Entities.FirstOrDefault(e => e.Name == GlobalController);
            EntityData localController = data.Entities.FirstOrDefault(e => e.Name == LocalController);
        
            if (globalController == null && localController == null)
                continue;

            if (globalController != null && isFromLoader)
            {
                InjectLevelData(data, GlobalData, level);
            }
            if (localController != null)
            {
                if (!IsLocalControllerActive(localController, level))
                    continue;
                
                InjectLevelData(data, LocalData, level);
            }
        }
        
        orig(level, playerIntro, isFromLoader);
        
        CleanupGlobalData(GlobalData, level);
        CleanupGlobalData(LocalData, level);
    }

    private static void InjectLevelData(LevelData data, Dictionary<EntityData, Vector2> entityList, Level level)
    {
        foreach (EntityData entityData in data.Entities.Where(e => e.Name is not (GlobalController or LocalController)))
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
    
    private static bool IsLocalControllerActive(EntityData data, Level level)
    {
        List<string> flags = data.Attr("flags").Split(',').ToList()
            .Where(flag => !string.IsNullOrEmpty(flag))
            .ToList();
        if (flags.Any(flag => !level.Session.GetFlag(flag)))
        {
            return false;
        }
        
        List<string> rooms = level.Session.MapData.ParseLevels(data.Attr("rooms"))
            .Where(room => !string.IsNullOrEmpty(room))
            .ToList();
        if (rooms.Count > 0 && !rooms.Contains(level.Session.Level))
        {
            return false;
        }
        
        return true;
    }
    
    // should be ParseLevelsList, but that overlaps with a private instance method
    private static List<string> ParseLevels(this MapData data, string list)
    {
        List<string> roomsList = [];
        string[] array = list.Split(',');
        
        foreach (string text in array)
        {
            if (Enumerable.Contains(text, '*'))
            {
                string pattern = "^" + Regex.Escape(text).Replace("\\*", ".*") + "$";
                
                // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
                foreach (LevelData level in data.Levels)
                {
                    if (Regex.IsMatch(level.Name, pattern))
                    {
                        roomsList.Add(level.Name);
                    }
                }
            }
            else
            {
                roomsList.Add(text);
            }
        }
        
        return roomsList;
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