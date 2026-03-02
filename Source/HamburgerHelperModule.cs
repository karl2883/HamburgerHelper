using System.Collections;
using Celeste.Mod.HamburgerHelper.Entities;
using Celeste.Mod.HamburgerHelper.Entities.Vsrg;
using Celeste.Mod.HamburgerHelper.Exports;
using Celeste.Mod.HamburgerHelper.Misc;
using Celeste.Mod.Helpers;
using Celeste.Pico8;
using MonoMod.Cil;
using MonoMod.ModInterop;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mod.HamburgerHelper;

public class HamburgerHelperModule : EverestModule 
{
    public static HamburgerHelperModule Instance { get; private set; }

    public override Type SettingsType => typeof(HamburgerHelperModuleSettings);
    public static HamburgerHelperModuleSettings Settings => (HamburgerHelperModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(HamburgerHelperModuleSession);
    public static HamburgerHelperModuleSession Session => (HamburgerHelperModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(HamburgerHelperModuleSaveData);
    public static HamburgerHelperModuleSaveData SaveData => (HamburgerHelperModuleSaveData) Instance._SaveData;

    public static HashSet<string> LoadedOptionalDependencies = new HashSet<string>();
    
    public HamburgerHelperModule() 
    {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(HamburgerHelperModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(HamburgerHelperModule), LogLevel.Info);
#endif
    }

    public override void Load()
    {
        LoadOptioanlDependencies();
        
        States.St.Load();
        
        StickyWalls.Load();
        TilesetSoundSplitter.Load();
        MoveBlockWaitController.Load();
        DreamerRefill.Load();
        
        VsrgManager.Load();
        
        ChapterPanelCustomization.Load();
        OverworldCustomization.Load();
        
        WindowUtils.Load();
        
        HamburgerHelperMetadata.Load();
        HamburgerHelperGFX.Load();
        
        typeof(WindowUtilsExports).ModInterop();
    }

    public override void Unload() 
    {
        States.St.Unload();
        
        StickyWalls.Unload();
        TilesetSoundSplitter.Unload();
        MoveBlockWaitController.Unload();
        DreamerRefill.Unload();
        
        VsrgManager.Unload();
        
        ChapterPanelCustomization.Unload();
        OverworldCustomization.Unload();
        
        WindowUtils.Unload();
        
        HamburgerHelperMetadata.Unload();
        HamburgerHelperGFX.Unload();
        
        HamburgerHelperGFX.UnloadContent();
    }
    
    public override void LoadContent(bool firstLoad)
    {
        ChapterPanelCustomization.LoadContent();
        
        HamburgerHelperGFX.LoadContent();
    }
    
    private static void LoadOptioanlDependencies()
    {
        List<EverestModuleMetadata> optionalDependencies = new List<EverestModuleMetadata>();
        LoadedOptionalDependencies.Clear();
        
        EverestModuleMetadata collabUtils2 = new EverestModuleMetadata() {
            Name = "CollabUtils2",
            Version = new Version(1, 12, 2),
        };
        optionalDependencies.Add(collabUtils2);
        
        foreach (EverestModuleMetadata mod in optionalDependencies)
        {
            if (Everest.Loader.DependencyLoaded(mod))
            {
                Logger.Log(LogLevel.Info, "HamburgerHelper", $"Loaded optional dependency {mod.Name}");
                LoadedOptionalDependencies.Add(mod.Name);   
            }
        }
    }
}