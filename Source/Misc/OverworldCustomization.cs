using System.Collections;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;
using Celeste.Mod.UI;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mod.HamburgerHelper.Misc;

public static class OverworldCustomization
{
    [OnLoad]
    internal static void Load()
    {
        IL.Celeste.MountainRenderer.Render += MountainRendererOnRender;
        
        On.Celeste.HiresSnow.Render += HiresSnowOnRender;
        On.Celeste.Snow3D.Particle.Reset += ParticleOnReset;
        
        On.Celeste.OuiChapterSelect.Enter += OuiChapterSelectOnEnter;
    }

    [OnUnload]
    internal static void Unload()
    {
        IL.Celeste.MountainRenderer.Render -= MountainRendererOnRender;
        
        On.Celeste.HiresSnow.Render -= HiresSnowOnRender;
        On.Celeste.Snow3D.Particle.Reset -= ParticleOnReset;
        
        On.Celeste.OuiChapterSelect.Enter -= OuiChapterSelectOnEnter;
    }
    
    private static IEnumerator OuiChapterSelectOnEnter(On.Celeste.OuiChapterSelect.orig_Enter orig, OuiChapterSelect self, Oui from)
    {
        yield return new SwapImmediately(orig(self, from));
        
        Overworld ovr = self.Overworld;
        foreach (Component component in ovr.Tracker.GetComponents<Snow3D.Particle>())
        {
            Snow3D.Particle particle = component as Snow3D.Particle;
            particle?.Reset();
        }
    }
    
    private static void ParticleOnReset(On.Celeste.Snow3D.Particle.orig_Reset orig, Snow3D.Particle self, float percent)
    {
        orig(self, percent);
        self.Texture = OVR.Atlas["snow"];
        
        if (SaveData.Instance == null) return;
        AreaData data = AreaData.Get(SaveData.Instance.LastArea_Safe);
        
        if (!HamburgerHelperMetadata.TryGetMetadata(data, out HamburgerHelperMetadata metadata)) 
            return;
        
        HamburgerHelperMetadata.OverworldCustomizationSettingsData meta = metadata?.OverworldCustomization;
        HamburgerHelperMetadata.OverworldCustomizationSettingsData.CustomHiresSnow snow = meta?.CustomSnow;
        if (snow == null) return;
        
        self.Texture = OVR.Atlas[Calc.Random.Choose(snow.OverworldTexturePaths)];
        self.Scale = (Vector2.One * snow.OverworldSnowSize) * 0.05f * self.Manager.Range / 30f;
    }
    
    private static void HiresSnowOnRender(On.Celeste.HiresSnow.orig_Render orig, HiresSnow self, Scene scene)
    {
        if (SaveData.Instance == null)
        {
            orig(self, scene);
            return;
        };
        AreaData data = AreaData.Get(SaveData.Instance.LastArea_Safe);
        
        HamburgerHelperMetadata.TryGetMetadata(data, out HamburgerHelperMetadata metadata);
        
        HamburgerHelperMetadata.OverworldCustomizationSettingsData meta = metadata?.OverworldCustomization;
        self.overlay = meta == null ? OVR.Atlas["overlay"] : OVR.Atlas[meta.CustomOverlayTexture];
        
        HamburgerHelperMetadata.OverworldCustomizationSettingsData.CustomHiresSnow snow = meta?.CustomSnow;
        self.snow = snow == null ? 
            OVR.Atlas["snow"].GetSubtexture(1, 1, 254, 254, null) : OVR.Atlas[snow.TexturePath];
        
        orig(self, scene);
    }
    
    private static void MountainRendererOnRender(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        
        /*
         * IL_0028: ldsfld class Monocle.Atlas Celeste.OVR::Atlas
         * IL_002d: ldstr "vignette"
         */
        if (!cursor.TryGotoNext(MoveType.After,
            i => i.MatchLdsfld(typeof(OVR), "Atlas"),
            i => i.MatchLdstr("vignette")))
            throw new HookUtilities.HookException(il, "MountainRenderer.Render failed at find vignette");
        
        cursor.EmitDelegate(ModifyVignettePath);
    }
    
    private static string ModifyVignettePath(string orig)
    {
        if (SaveData.Instance == null) return orig;
        AreaData data = AreaData.Get(SaveData.Instance.LastArea_Safe);
        
        if (!HamburgerHelperMetadata.TryGetMetadata(data, out HamburgerHelperMetadata metadata)) 
            return orig;
        
        HamburgerHelperMetadata.OverworldCustomizationSettingsData meta = metadata?.OverworldCustomization;
        return meta == null ? orig : meta.CustomVignetteTexture;
    }
}
