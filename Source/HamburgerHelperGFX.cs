using Celeste.Mod.Roslyn.ModLifecycleAttributes;

namespace Celeste.Mod.HamburgerHelper;

public static class HamburgerHelperGFX
{
    private static Dictionary<string, Effect> Effects = new Dictionary<string, Effect>();
    
    [OnLoadContent]
    internal static void LoadContent(bool firstLoad)
    {
    }
    
    internal static void UnloadContent()
    {
        ClearEffects();
    }

    [OnLoad]
    internal static void Load()
    {
        On.Celeste.Level.Begin += LevelOnBegin;
    }

    [OnUnload]
    internal static void Unload()
    {
        On.Celeste.Level.Begin -= LevelOnBegin;
    }
    
    private static void LevelOnBegin(On.Celeste.Level.orig_Begin orig, Level self)
    {
        orig(self);
        ClearEffects();
    }

    public static void ClearEffects()
    {
        foreach (Effect eff in Effects.Values.ToList())
        {
            eff?.Dispose();
        }
        Effects.Clear();
    }
    
    /// <summary>
    /// Loads an Effect from from a file path
    /// </summary>
    /// <param name="id">Used for internal shaders (in HamburgerHelper)</param>
    /// <param name="fullpath">Used for external shaders (outside of HamburgerHelper)</param>
    /// <returns></returns>
    public static Effect LoadEffect(string id, string fullpath = null)
    {
        string path = $"Effects/HamburgerHelper/{id}.cso";
        if (fullpath != null) path = $"{fullpath}.cso";

        if (Effects.TryGetValue(path, out Effect cachedEff)) return cachedEff;
        
        if (!Everest.Content.TryGet(path, out ModAsset effect))
            Logger.Log(LogLevel.Error, "HamburgerHelperGFX", $"Failed loading effect from {path}");
        
        Effects[path] = new Effect(Engine.Graphics.GraphicsDevice, effect.Data);
        Logger.Log(LogLevel.Verbose, "HamburgerHelperGFX", $"Loaded effect from {path}");
        
        return Effects.Values.Last();
    }
}
