#pragma warning disable CA2208

using Celeste.Mod.CollabUtils2;
using Celeste.Mod.CollabUtils2.UI;
using Celeste.Mod.Helpers;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using MethodBody = Mono.Cecil.Cil.MethodBody;
using ELD = Celeste.Mod.HamburgerHelper.HamburgerHelperMetadata.EffectDataLayer;
using HookException = Celeste.Mod.HamburgerHelper.Utils.HookUtilities.HookException;

namespace Celeste.Mod.HamburgerHelper.Misc;

// this is the first time i've ever written an il hook :3
// shoutout to snip, zoey, catapillie, and jade for help with learning all of this

// originally made for a different project, but i migrated all of it over here w/ meta.yaml setup too
// mostly because of rughelper having severe conflicts with the original version of this

// i learned sooooo much while making this and it was really fun!
public static class ChapterPanelCustomization
{
    private static ILHook ilHookOuiChapterPanelSwapRoutine;
    private static ILHook ilHookOuiChapterPanelOrigDrawCheckpoint;
    private static ILHook ilHookOuiChapterSelectIconOrigUpdate;
    
    private static ILHook ilHookInGameOverworldHelperUpdateIconRoutine;
    
    [OnLoad]
    internal static void Load()
    {
        IL.Celeste.OuiChapterPanel.Render += OuiChapterPanelOnRender;
        IL.Celeste.OuiChapterPanel.Option.Render += OptionOnRender;
        IL.Celeste.OuiChapterPanel.Reset += OuiChapterPanel_Reset;

        IL.Celeste.OuiChapterSelectIcon.SetSelectedPercent += OuiChapterSelectIconOnSetSelectedPercent;
        IL.Celeste.OuiChapterSelectIcon.Render += OuiChapterSelectIconOnRender;
        
        ilHookOuiChapterPanelOrigDrawCheckpoint =
            new ILHook(typeof(OuiChapterPanel).GetMethod("orig_DrawCheckpoint", 
                    BindingFlags.NonPublic | BindingFlags.Instance)!,
            OuiChapterPanel_orig_DrawCheckpoint);
        
        ilHookOuiChapterPanelSwapRoutine =
            new ILHook(typeof(OuiChapterPanel).GetMethod("SwapRoutine", 
                    BindingFlags.NonPublic | BindingFlags.Instance)!.GetStateMachineTarget()!,
                OuiChapterPanel_SwapRoutine);

        ilHookOuiChapterSelectIconOrigUpdate =
            new ILHook(typeof(OuiChapterSelectIcon).GetMethod("orig_Update")!,
                OuiChapterSelectIcon_orig_Update);
        
        On.Celeste.MenuOptions.SetWindow += OnWindowSizeChange;
        
        if (HamburgerHelperModule.LoadedOptionalDependencies.Contains("CollabUtils2"))
        {
            LoadCu2Hook();
        }
    }

    private static void LoadCu2Hook()
    {
        ilHookInGameOverworldHelperUpdateIconRoutine = 
            new ILHook(typeof(InGameOverworldHelper).GetMethod("UpdateIconRoutine", 
                    BindingFlags.NonPublic | BindingFlags.Static)!.GetStateMachineTarget()!,
                InGameOverworldHelper_UpdateIconRoutine);
    }
    
    [OnUnload]
    internal static void Unload()
    {
        IL.Celeste.OuiChapterPanel.Render -= OuiChapterPanelOnRender;
        IL.Celeste.OuiChapterPanel.Option.Render -= OptionOnRender;
        IL.Celeste.OuiChapterPanel.Reset -= OuiChapterPanel_Reset;
        
        IL.Celeste.OuiChapterSelectIcon.SetSelectedPercent -= OuiChapterSelectIconOnSetSelectedPercent;
        IL.Celeste.OuiChapterSelectIcon.Render -= OuiChapterSelectIconOnRender;
        
        ilHookOuiChapterPanelSwapRoutine?.Dispose();
        ilHookOuiChapterPanelSwapRoutine = null;
        
        ilHookOuiChapterPanelOrigDrawCheckpoint?.Dispose();
        ilHookOuiChapterPanelOrigDrawCheckpoint = null;
        
        ilHookOuiChapterSelectIconOrigUpdate?.Dispose();
        ilHookOuiChapterSelectIconOrigUpdate = null;
        
        ilHookInGameOverworldHelperUpdateIconRoutine?.Dispose();
        ilHookInGameOverworldHelperUpdateIconRoutine = null;
        
        TextMaskTarget?.Dispose();
        TextMaskTarget = null;
        
        On.Celeste.MenuOptions.SetWindow -= OnWindowSizeChange;
    }
    
    private static VariableDefinition AddVariable(this MethodBody self, TypeReference type)
    {
        VariableDefinition variable = new VariableDefinition(type);
        self.Variables.Add(variable);
        return variable;
    }
    
    private static void OuiChapterPanel_orig_DrawCheckpoint(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        
        /*
         * IL_0013: ldsfld class Monocle.Atlas Celeste.MTN::Checkpoints
         * IL_0018: ldstr "polaroid"
         */
        
        if (!cursor.TryGotoNext(MoveType.After,
            i => i.MatchLdsfld(typeof(MTN), "Checkpoints"),
            i => i.MatchLdstr("polaroid")))
            throw new HookException(il, "OuiChapterPanel.orig_DrawCheckpoint failed at find polaroid");
        
        cursor.EmitDelegate(ModifyPolaroidPath);
    }
    
    private static void OptionOnRender(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        /*
         * IL_00a7: ldc.r4 1
         * IL_00ac: newobj instance void [FNA]Microsoft.Xna.Framework.Vector2::.ctor(float32, float32)
         * IL_00b1: call valuetype [FNA]Microsoft.Xna.Framework.Vector2 [FNA]Microsoft.Xna.Framework.Vector2::op_Multiply(float32, valuetype [FNA]Microsoft.Xna.Framework.Vector2)
         * callvirt instance void Monocle.MTexture::DrawCentered(valuetype [FNA]Microsoft.Xna.Framework.Vector2, valuetype [FNA]Microsoft.Xna.Framework.Color, valuetype [FNA]Microsoft.Xna.Framework.Vector2)
         */
        if (!cursor.TryGotoNextBestFit(MoveType.After,
            i => i.MatchLdcR4(1),
            i => i.MatchNewobj<Vector2>(".ctor"),
            i => i.MatchCall<Vector2>("op_Multiply"),
            i => i.MatchCallvirt<MTexture>("DrawCentered")))
            throw new HookException(il, "OuiChapterPanel.Option failed at OptionBg");

        cursor.Index--;
        
        cursor.EmitLdstr("ChapterTab");
        cursor.EmitDelegate(StartShaderLayerRender);
        
        cursor.Index++;
        
        cursor.EmitDelegate(EndShaderLayerRender);
    }
    
    private static void OuiChapterPanelOnRender(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        
        // ----- Chapter Card Shader Rendering -----
        
        /*
         * IL_0155: ldc.r4 -32
         * IL_015a: newobj instance void [FNA]Microsoft.Xna.Framework.Vector2::.ctor(float32, float32)
         * IL_015f: call valuetype [FNA]Microsoft.Xna.Framework.Vector2 [FNA]Microsoft.Xna.Framework.Vector2::op_Addition(valuetype [FNA]Microsoft.Xna.Framework.Vector2, valuetype [FNA]Microsoft.Xna.Framework.Vector2)
         * IL_0000: ** ADD HERE
         * IL_0164: callvirt instance void Monocle.MTexture::Draw(valuetype [FNA]Microsoft.Xna.Framework.Vector2)
         */
        if (!cursor.TryGotoNextBestFit(MoveType.After,
            i => i.MatchLdcR4(-32f),
            i => i.MatchNewobj<Vector2>(".ctor"),
            i => i.MatchCall<Vector2>("op_Addition"),
            i => i.MatchCallvirt<MTexture>("Draw")))
            throw new HookException(il, "Unable to find cardtop texture rendering");
        
        cursor.Index--;
        
        cursor.EmitLdstr("ChapterCard");
        cursor.EmitDelegate(StartShaderLayerRender);
        
        /*
         * IL_01d7: conv.r4
         * IL_01d8: newobj instance void [FNA]Microsoft.Xna.Framework.Vector2::.ctor(float32, float32)
         * IL_01dd: call valuetype [FNA]Microsoft.Xna.Framework.Vector2 [FNA]Microsoft.Xna.Framework.Vector2::op_Addition(valuetype [FNA]Microsoft.Xna.Framework.Vector2, valuetype [FNA]Microsoft.Xna.Framework.Vector2)
         * IL_01e2: callvirt instance void Monocle.MTexture::Draw(valuetype [FNA]Microsoft.Xna.Framework.Vector2)
         * IL_0000: ** ADD HERE
         */
        if (!cursor.TryGotoNextBestFit(MoveType.After,
            i => i.MatchConvR4(),
            i => i.MatchNewobj<Vector2>(".ctor"),
            i => i.MatchCall<Vector2>("op_Addition"),
            i => i.MatchCallvirt<MTexture>("Draw")))
            throw new HookException(il, "Unable to find card texture rendering ");
        
        cursor.EmitDelegate(EndShaderLayerRender);
        
        // ----- Overlays Rendering -----
        
        cursor.EmitLdarg0();
        cursor.EmitLdloc2();
        cursor.EmitDelegate(DrawOverlays);
        
        // ----- Option Text Recoloring -----
        
        /*
         * IL_0299: call valuetype [FNA]Microsoft.Xna.Framework.Color [FNA]Microsoft.Xna.Framework.Color::get_Black()
         * IL_029e: ldc.r4 0.8
         * IL_02a3: call valuetype [FNA]Microsoft.Xna.Framework.Color [FNA]Microsoft.Xna.Framework.Color::op_Multiply(valuetype [FNA]Microsoft.Xna.Framework.Color, float32)
         */
        if (!cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchCall<Color>("get_Black"),
            instr => instr.MatchLdcR4(0.8f),
            instr => instr.MatchCall<Color>("op_Multiply")))
            throw new HookException(il, "Unable to find option label rendering to modify.");

        cursor.EmitLdarg0();
        cursor.EmitDelegate(ModifyOptionLabelColor);

        // ----- Title Base Shader Rendering -----

        /*
         * IL_03e6: ldfld class Celeste.AreaData Celeste.OuiChapterPanel::Data
         * IL_03eb: ldfld valuetype [FNA]Microsoft.Xna.Framework.Color Celeste.AreaData::TitleBaseColor
         * IL_0000: ** ADD HERE
         * IL_03f0: callvirt instance void Monocle.MTexture::Draw(valuetype [FNA]Microsoft.Xna.Framework.Vector2, valuetype [FNA]Microsoft.Xna.Framework.Vector2, valuetype [FNA]Microsoft.Xna.Framework.Color)
         * IL_0000: ** ADD HERE
         */
        if (!cursor.TryGotoNext(MoveType.After,
            i => i.MatchLdfld<OuiChapterPanel>("Data"),
            i => i.MatchLdfld<AreaData>("TitleBaseColor"),
            i => i.MatchCallvirt<MTexture>("Draw")))
            throw new HookException(il, "OuiChapterPanel hook failed at Shader Title Base Hooks");
        
        cursor.Index--;
        
        cursor.EmitLdstr("Base");
        cursor.EmitDelegate(StartShaderLayerRender);
        
        cursor.Index++;
        
        cursor.EmitDelegate(EndShaderLayerRender);
        
        // ----- Title Accent Shader Rendering -----
        
        /*
         * IL_0430: ldfld class Celeste.AreaData Celeste.OuiChapterPanel::Data
         * IL_0435: ldfld valuetype [FNA]Microsoft.Xna.Framework.Color Celeste.AreaData::TitleAccentColor
         * IL_0000: ** ADD HERE
         * IL_043a: callvirt instance void Monocle.MTexture::Draw(valuetype [FNA]Microsoft.Xna.Framework.Vector2, valuetype [FNA]Microsoft.Xna.Framework.Vector2, valuetype [FNA]Microsoft.Xna.Framework.Color)
         * IL_0000: ** ADD HERE
         */
        if (!cursor.TryGotoNext(MoveType.After,
            i => i.MatchLdfld<OuiChapterPanel>("Data"),
            i => i.MatchLdfld<AreaData>("TitleAccentColor"),
            i => i.MatchCallvirt<MTexture>("Draw")))
            throw new HookException(il, "OuiChapterPanel hook failed at Shader Accent Hooks");
        
        cursor.Index--;
        
        cursor.EmitLdstr("Accent");
        cursor.EmitDelegate(StartShaderLayerRender);

        cursor.Index++;
        
        cursor.EmitDelegate(EndShaderLayerRender);
        
        // ----- Title Text & Chapter # Text Shader Rendering -----

        cursor.Index = 0;

        /*
         * IL_0000: ** ADD VARS HERE
         * IL_0000: ** STLOC LOCALS HERE
         * IL_0000: ** LDLOC LOCALS HERE
         * IL_0000: ** ADD HERE
         * IL_0000: call void Celeste.ActiveFont::Draw(string, valuetype [FNA]Microsoft.Xna.Framework.Vector2, valuetype [FNA]Microsoft.Xna.Framework.Vector2, valuetype [FNA]Microsoft.Xna.Framework.Vector2, valuetype [FNA]Microsoft.Xna.Framework.Color)
         * IL_0000: ** ADD HERE
         */
        while (cursor.TryGotoNext(MoveType.After,
            i => i.MatchCall(typeof(ActiveFont), "Draw")))
        {
            cursor.Index--;
            
            VariableDefinition text = cursor.Body.AddVariable(il.Import(typeof(string)));
            VariableDefinition position = cursor.Body.AddVariable(il.Import(typeof(Vector2)));
            VariableDefinition justify = cursor.Body.AddVariable(il.Import(typeof(Vector2)));
            VariableDefinition scale = cursor.Body.AddVariable(il.Import(typeof(Vector2)));
            VariableDefinition color = cursor.Body.AddVariable(il.Import(typeof(Color)));
            
            cursor.EmitStloc(color);
            cursor.EmitStloc(scale);
            cursor.EmitStloc(justify);
            cursor.EmitStloc(position);
            cursor.EmitStloc(text);
            
            cursor.EmitLdloc(text);
            cursor.EmitLdloc(position);
            cursor.EmitLdloc(justify);
            cursor.EmitLdloc(scale);
            cursor.EmitLdloc(color);
            
            cursor.EmitLdloc(text);
            cursor.EmitLdloc(position);
            cursor.EmitLdloc(justify);
            cursor.EmitLdloc(scale);
            cursor.EmitLdloc(color);
            cursor.EmitDelegate(StartTextShaderLayerRender);
            
            cursor.Index++;
            
            cursor.EmitLdloc(text);
            cursor.EmitLdloc(position);
            cursor.EmitLdloc(justify);
            cursor.EmitLdloc(scale);
            cursor.EmitLdloc(color);
            cursor.EmitDelegate(EndTextShaderLayerRender);
        }
    }
    
    private static void OuiChapterPanel_Reset(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        
        /*
         * IL_01cc: dup
         * IL_01cd: ldstr "A"
         * IL_01d2: stfld string Celeste.OuiChapterPanel/Option::ID
         * IL_01d7: callvirt instance void class [mscorlib]System.Collections.Generic.List`1<class Celeste.OuiChapterPanel/Option>::Add(!0)
         */
        if (!cursor.TryGotoNextBestFit(MoveType.Before,
            instr => instr.MatchDup(),
            instr => instr.MatchLdstr("A"),
            instr => instr.MatchStfld<OuiChapterPanel.Option>("ID"),
            instr => instr.MatchCallvirt<List<OuiChapterPanel.Option>>("Add")))
            throw new HookException(il, "Unable to find creation of play option to modify.");

        cursor.EmitDup();
        cursor.EmitLdarg0();
        cursor.EmitDelegate(ModifyPlayOptionBgColor);
    }
    
    private static void OuiChapterPanel_SwapRoutine(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        
        /*
         * IL_018f: dup
         * IL_0190: ldstr "eabe26"
         * IL_0195: call valuetype [FNA]Microsoft.Xna.Framework.Color Monocle.Calc::HexToColor(string)
         */
        if (!cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchDup(),
            instr => instr.MatchLdstr("eabe26"),
            instr => instr.MatchCall(typeof(Calc), "HexToColor")))
            throw new HookException(il, "Unable to find assignment to `OuiChapterPanel.Option.BgColor` to modify.");

        cursor.EmitLdloc1();
        cursor.EmitDelegate(ModifyStartOptionBgColor);
        
        /*
         * IL_0279: ldstr "areaselect/checkpoint"
         * IL_027e: call instance string Celeste.OuiChapterPanel::_ModAreaselectTexture(string)
         * IL_0283: callvirt instance class Monocle.MTexture Monocle.Atlas::get_Item(string)
         * IL_0288: stfld class Monocle.MTexture Celeste.OuiChapterPanel/Option::Icon
         */
        if (!cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchLdstr("areaselect/checkpoint"),
            instr => instr.MatchCall<OuiChapterPanel>("_ModAreaselectTexture"),
            instr => instr.MatchCallvirt<Atlas>("get_Item"),
            instr => instr.MatchStfld<OuiChapterPanel.Option>("Icon")))
            throw new HookException("Unable to find creation of checkpoint option to modify.");

        cursor.EmitDup();
        cursor.EmitLdloc1();
        cursor.EmitDelegate(ModifyCheckpointOptionBgColor);
    }

    #region Misc Edits
    
    private static Vector2 ModifyIconOffset(Vector2 orig)
    {
        if (SaveData.Instance == null) return orig;
        AreaData data = AreaData.Get(SaveData.Instance.LastArea_Safe);
        
        if (!HamburgerHelperMetadata.TryGetMetadata(data, out HamburgerHelperMetadata metadata)) 
            return orig;
        
        HamburgerHelperMetadata.ChapterPanelCustomizationSettingsData meta = metadata?.ChapterPanelCustomization;
        if (meta?.CustomIcon == null) return orig;

        return orig + meta.CustomIcon.PositionOffset;
    }
    
    private static void InGameOverworldHelper_UpdateIconRoutine(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        
        /*
         * IL_004f: callvirt instance valuetype [FNA]Microsoft.Xna.Framework.Vector2 [Celeste]Celeste.OuiChapterPanel::get_IconOffset()
         * IL_0054: call valuetype [FNA]Microsoft.Xna.Framework.Vector2 [FNA]Microsoft.Xna.Framework.Vector2::op_Addition(valuetype [FNA]Microsoft.Xna.Framework.Vector2, valuetype [FNA]Microsoft.Xna.Framework.Vector2)
         * IL_0059: stfld valuetype [FNA]Microsoft.Xna.Framework.Vector2 [Celeste]Monocle.Entity::Position
         */
        if (!cursor.TryGotoNext(MoveType.After,
            i => i.MatchCallvirt<OuiChapterPanel>("get_IconOffset"),
            i => i.MatchCall<Vector2>("op_Addition"),
            i => i.MatchStfld<Entity>("Position")))
            throw new HookException(il, "InGameOverworldHelper.UpdateIconRoutine failed to match IconOffset");
        
        cursor.Index--;
        
        cursor.EmitDelegate(ModifyIconOffset);
    }
    
    private static void OuiChapterSelectIconOnSetSelectedPercent(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        
        /*
         * IL_0017: ldloc.0
         * IL_0018: callvirt instance valuetype [FNA]Microsoft.Xna.Framework.Vector2 Celeste.OuiChapterPanel::get_IconOffset()
         * IL_001d: call valuetype [FNA]Microsoft.Xna.Framework.Vector2 [FNA]Microsoft.Xna.Framework.Vector2::op_Addition(valuetype [FNA]Microsoft.Xna.Framework.Vector2, valuetype [FNA]Microsoft.Xna.Framework.Vector2)
         * IL_0022: stloc.1
         */
        if (!cursor.TryGotoNextBestFit(MoveType.After,
            i => i.MatchLdloc0(),
            i => i.MatchCallvirt<OuiChapterPanel>("get_IconOffset"),
            i => i.MatchCall<Vector2>("op_Addition"),
            i => i.MatchStloc1()))
            throw new HookException(il, "OuiChapterSelectIcon.SetSelectedPercent failed to match IconOffset");
        
        cursor.Index--;
        
        cursor.EmitDelegate(ModifyIconOffset);
    }
    
    private static void OuiChapterSelectIcon_orig_Update(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        
        /*
         * IL_00aa: ldloc.0
         * IL_00ab: callvirt instance valuetype [FNA]Microsoft.Xna.Framework.Vector2 Celeste.OuiChapterPanel::get_IconOffset()
         * IL_00b0: call valuetype [FNA]Microsoft.Xna.Framework.Vector2 [FNA]Microsoft.Xna.Framework.Vector2::op_Addition(valuetype [FNA]Microsoft.Xna.Framework.Vector2, valuetype [FNA]Microsoft.Xna.Framework.Vector2)
         * IL_00b5: stfld valuetype [FNA]Microsoft.Xna.Framework.Vector2 Monocle.Entity::Position
         */
        if (!cursor.TryGotoNextBestFit(MoveType.After,
            i => i.MatchLdloc0(),
            i => i.MatchCallvirt<OuiChapterPanel>("get_IconOffset"),
            i => i.MatchCall<Vector2>("op_Addition"),
            i => i.MatchStfld<Entity>("Position")))
        {
            Logger.Log(LogLevel.Info, "DEBUG", "failed");
            return;
        }
        
            // throw new HookException(il, "OuiChapterSelectIcon.SetSelectedPercent failed to match IconOffset");
        
        cursor.Index--;
        
        cursor.EmitDelegate(ModifyIconOffset);
    }
    
    private static void OuiChapterSelectIconOnRender(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        
        /*
         * IL_00b5: ldarg.0
         * IL_00b6: ldfld float32 Celeste.OuiChapterSelectIcon::Rotation
         * IL_00bb: callvirt instance void Monocle.MTexture::DrawCentered(valuetype [FNA]Microsoft.Xna.Framework.Vector2, valuetype [FNA]Microsoft.Xna.Framework.Color, valuetype [FNA]Microsoft.Xna.Framework.Vector2, float32)
         */
        if (!cursor.TryGotoNext(MoveType.After,
            i => i.MatchLdarg0(),
            i => i.MatchLdfld<OuiChapterSelectIcon>("Rotation"),
            i => i.MatchCallvirt<MTexture>("DrawCentered")))
            throw new HookException(il, "OuiChapterSelectIcon.Render failed to match DrawCentered");

        cursor.Index--;

        cursor.EmitLdarg0();
        cursor.EmitDelegate(IconStartShaderLayerRender);

        cursor.Index++;

        cursor.EmitDelegate(EndShaderLayerRender);
    }
    
    private static void IconStartShaderLayerRender(OuiChapterSelectIcon self)
    {
        if (!IconTryLoadEffect(self.Area, out ELD data, out Effect effect))
            return;
        
        HiresRenderer.EndRender();
        
        Matrix transformMatrix = HiresRenderer.DrawToBuffer ? Matrix.Identity : Engine.ScreenMatrix;
        effect = ApplyParameters(effect, data, transformMatrix);
        
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.Default, RasterizerState.CullNone, effect, transformMatrix);
    }
    
    private static bool IconTryLoadEffect(int area, out ELD data, out Effect effect)
    {
        data = null;
        effect = null;
        
        AreaData areaData = AreaData.Get(area);
        if (areaData == null) return false;
        
        if (!HamburgerHelperMetadata.TryGetMetadata(areaData, out HamburgerHelperMetadata metadata))
            return false;
        
        HamburgerHelperMetadata.ChapterPanelCustomizationSettingsData oed = metadata?.ChapterPanelCustomization;

        data = oed?.CustomIcon?.IconEffect;
        
        effect = data?.Effect;
        return effect != null;
    }
    
    private static string ModifyPolaroidPath(string orig)
    {
        if (SaveData.Instance == null) return orig;
        AreaData data = AreaData.Get(SaveData.Instance.LastArea_Safe);
        
        if (!HamburgerHelperMetadata.TryGetMetadata(data, out HamburgerHelperMetadata metadata)) 
            return orig;
        
        HamburgerHelperMetadata.ChapterPanelCustomizationSettingsData meta = metadata?.ChapterPanelCustomization;
        return meta == null ? orig : meta.CustomPolaroidPath;
    }
    
    #endregion
    
    # region Overlays & Recolors
    
    // Overlay rendering was made by aonkeeper4, added + edited with permission of course
    private static void DrawOverlays(OuiChapterPanel panel, MTexture cardTop)
    {
        if (!HamburgerHelperMetadata.TryGetMetadata(panel.Data, out HamburgerHelperMetadata metadata)
            || metadata.ChapterPanelCustomization is not { Overlays: { Count: > 0 } overlays })
            return;
        
        int i = 0;
        
        AreaModeStats stats = panel.RealStats.Modes[(int) panel.Area.Mode];
        foreach (HamburgerHelperMetadata.OverlayData overlayData
            in overlays.Where(overlayData => overlayData.Condition switch { 
                    HamburgerHelperMetadata.OverlayData.Conditions.None => true,
                    HamburgerHelperMetadata.OverlayData.Conditions.Clear => stats.Completed,
                    HamburgerHelperMetadata.OverlayData.Conditions.FullClear => stats.FullClear,
                    HamburgerHelperMetadata.OverlayData.Conditions.Golden => GoldenCollected(panel.Area, stats),
                    HamburgerHelperMetadata.OverlayData.Conditions.Silver => SilverCollected(panel.Area, stats),
                    HamburgerHelperMetadata.OverlayData.Conditions.Rainbow => RainbowCollected(panel.Area, stats),
                    _ => throw new ArgumentOutOfRangeException()
                })
                       .Where(overlayData => overlayData.Texture is not null && GFX.Gui.Has(overlayData.Texture)))
        {
            MTexture texture = GFX.Gui[overlayData.Texture];
            Color color = Calc.HexToColor(overlayData.Color);
            bool renderShaderThisLayer = false;
            
            // should be optimized with StartShaderLayerRender but i need to finish this
            if (TryLoadEffect("Overlay", out ELD data, out Effect effect, index: i))
            {
                renderShaderThisLayer = true;
                
                HiresRenderer.EndRender();
                
                Matrix transformMatrix = HiresRenderer.DrawToBuffer ? Matrix.Identity : Engine.ScreenMatrix;
                effect = ApplyParameters(effect, data, transformMatrix);
                
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.Default, RasterizerState.CullNone, effect, transformMatrix);
            }
            
            switch (overlayData.Anchor)
            {
                case HamburgerHelperMetadata.OverlayData.Anchors.None:
                    texture.Draw(panel.Position + overlayData.PositionOffset, Vector2.Zero, color);
                    break;
                
                case HamburgerHelperMetadata.OverlayData.Anchors.Card:
                    texture.GetSubtexture(0, texture.Height - (int) panel.height, texture.Width, (int) panel.height)
                           .Draw(panel.Position + Vector2.UnitY * (cardTop.Height - 32f) + overlayData.PositionOffset,
                               Vector2.Zero, color);
                    break;
                
                case HamburgerHelperMetadata.OverlayData.Anchors.Stats:
                    if (!panel.selectingMode)
                        break;
                    
                    texture.DrawCentered(panel.Position + panel.contentOffset + Vector2.UnitY * 170f + overlayData.PositionOffset,
                        color);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            // should be optimized with EndShaderLayerRender but i need to finish this
            if (renderShaderThisLayer)
            {
                EndShaderLayerRender();
            }
            
            i++;
        }
    }
    
    private static bool GoldenCollected(AreaKey key, AreaModeStats stats)
        => AreaData.Get(key).Mode[(int) key.Mode].MapData.Goldenberries.Any(berry => stats.Strawberries.Contains(new EntityID(berry.Level.Name, berry.ID)));
    private static bool SilverCollected(AreaKey key, AreaModeStats stats)
        => GoldenCollected(key, stats) && CollabMapDataProcessor.MapsWithSilverBerries.Contains(key.SID);
    private static bool RainbowCollected(AreaKey key, AreaModeStats stats)
        => GoldenCollected(key, stats) && CollabMapDataProcessor.MapsWithRainbowBerries.Contains(key.SID);
    
    // these are all verbatim taken from aonkeeper4's code (with permission, again !!)
    private static void ModifyPlayOptionBgColor(OuiChapterPanel.Option option, OuiChapterPanel panel)
    {
        if (!HamburgerHelperMetadata.TryGetMetadata(panel.Data, out HamburgerHelperMetadata metadata)
            || metadata.ChapterPanelCustomization is not { CustomColors: { PlayOptionBgColor: { } optionBgColor, PlayOptionBgAlpha: var optionBgAlpha } })
            return;
        
        option.BgColor = Calc.HexToColor(optionBgColor) * optionBgAlpha;
    }
    
    private static Color ModifyOptionLabelColor(Color orig, OuiChapterPanel panel)
    {
        if (!HamburgerHelperMetadata.TryGetMetadata(panel.Data, out HamburgerHelperMetadata metadata)
            || metadata.ChapterPanelCustomization is not { CustomColors: { OptionLabelColor: { } optionLabelColor, OptionLabelAlpha: var optionLabelAlpha } })
            return orig;
        
        return Calc.HexToColor(optionLabelColor) * optionLabelAlpha;
    }
    
    private static Color ModifyStartOptionBgColor(Color orig, OuiChapterPanel panel)
    {
        if (!HamburgerHelperMetadata.TryGetMetadata(panel.Data, out HamburgerHelperMetadata metadata)
            || metadata.ChapterPanelCustomization is not { CustomColors: { StartOptionBgColor: { } optionBgColor, StartOptionBgAlpha: var optionBgAlpha } })
            return orig;
        
        return Calc.HexToColor(optionBgColor) * optionBgAlpha;
    }
    
    private static void ModifyCheckpointOptionBgColor(OuiChapterPanel.Option option, OuiChapterPanel panel)
    {
        if (!HamburgerHelperMetadata.TryGetMetadata(panel.Data, out HamburgerHelperMetadata metadata)
            || metadata.ChapterPanelCustomization is not { CustomColors: { CheckpointOptionBgColor: { } optionBgColor, CheckpointOptionBgAlpha: var optionBgAlpha } })
            return;
        
        option.BgColor = Calc.HexToColor(optionBgColor) * optionBgAlpha;
    }
    
    # endregion
    
    #region Chapter Panel Shaders
    
    /// <summary>
    /// Renders given layerType with a shader
    /// </summary>
    /// <param name="layerType"></param>
    private static void StartShaderLayerRender(string layerType)
    {
        if (!TryLoadEffect(layerType, out ELD data, out Effect effect))
            return;
        
        HiresRenderer.EndRender();
        
        Matrix transformMatrix = HiresRenderer.DrawToBuffer ? Matrix.Identity : Engine.ScreenMatrix;
        effect = ApplyParameters(effect, data, transformMatrix);
        
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.Default, RasterizerState.CullNone, effect, transformMatrix);
    }

    /// <summary>
    /// Ends rendering for a given layerType
    /// </summary>
    private static void EndShaderLayerRender()
    {
        HiresRenderer.EndRender();
        HiresRenderer.BeginRender();
    }
    
    /// <summary>
    /// Renders given layerType (text) with options for using a RenderTarget or rendering an outline
    /// </summary>
    /// <param name="text"></param>
    /// <param name="position"></param>
    /// <param name="justify"></param>
    /// <param name="scale"></param>
    /// <param name="color"></param>
    private static void StartTextShaderLayerRender(string text, Vector2 position, 
        Vector2 justify, Vector2 scale, Color color)
    {
        AreaData areaData = GetAreaData();
        if (areaData == null) return;
        
        OuiChapterPanel panel = GetChapterPanel();
        if (panel == null) return;
        
        string layerType = "DefaultText";
        if (color == areaData.TitleAccentColor * 0.8f)
            layerType = "ChapterText";
        if (color == areaData.TitleTextColor * 0.8f)
            layerType = "Text";
        if (position == panel.OptionsRenderPosition + new Vector2(0f, -140f))
            layerType = "PlayText";
        
        if (!TryLoadEffect(layerType, out ELD data, out Effect effect))
            return;
        
        if (data.TextRenderOutline)
        {
            Vector2 outlinePosition = data.TextOffsetOutline ? position + new Vector2(-1f * scale.X, 0f) : position;
            Color outlineColor = data.TextOutlineColor == "FFFFFF" ? Color.White : Calc.HexToColor(data.TextOutlineColor);
            
            ActiveFont.DrawOutline(text, outlinePosition, justify, scale, Color.Transparent, 2f, outlineColor);
        }
        
        HiresRenderer.EndRender();
        
        Matrix transformMatrix = (data.TextRenderToRenderTarget) 
            ? Matrix.Identity : (HiresRenderer.DrawToBuffer)
                ? Matrix.Identity : Engine.ScreenMatrix;
        effect = ApplyParameters(effect, data, transformMatrix);

        GraphicsDevice gd = Engine.Graphics.GraphicsDevice;
        PresentationParameters pp = gd.PresentationParameters;
        
        PreviousTarget = gd.GetRenderTargets();
        
        if (data.TextRenderToRenderTarget)
        {
            pp.RenderTargetUsage = RenderTargetUsage.PreserveContents;
            
            if (TextMaskTarget == null) return;
            
            gd.SetRenderTarget(TextMaskTarget);
            gd.Clear(Color.Transparent);
        }
        
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
            DepthStencilState.Default, RasterizerState.CullNone, effect, transformMatrix);
    }

    /// <summary>
    /// Ends rendering for a given layerType (text) with options for using a RenderTarge
    /// </summary>
    /// <param name="text"></param>
    /// <param name="position"></param>
    /// <param name="justify"></param>
    /// <param name="scale"></param>
    /// <param name="color"></param>
    private static void EndTextShaderLayerRender(string text, Vector2 position, 
        Vector2 justify, Vector2 scale, Color color)
    {
        AreaData areaData = GetAreaData();
        if (areaData == null) return;

        OuiChapterPanel panel = GetChapterPanel();
        if (panel == null) return;
        
        string layerType = "DefaultText";
        if (color == areaData.TitleAccentColor * 0.8f)
            layerType = "ChapterText";
        if (color == areaData.TitleTextColor * 0.8f)
            layerType = "Text";
        if (position == panel.OptionsRenderPosition + new Vector2(0f, -140f))
            layerType = "PlayText";
        
        if (!TryLoadEffect(layerType, out ELD data, out Effect effect))
            return;
        
        HiresRenderer.EndRender();

        GraphicsDevice gd = Engine.Graphics.GraphicsDevice;
        PresentationParameters pp = gd.PresentationParameters;
        
        if (data.TextRenderToRenderTarget)
        {
            SpriteBatch sb = Draw.SpriteBatch;
            
            if (HiresRenderer.DrawToBuffer)
                gd.SetRenderTarget(Celeste.HudTarget);
            else
                gd.SetRenderTargets(PreviousTarget);
            
            Matrix transformMatrix = HiresRenderer.DrawToBuffer ? Matrix.Identity : Engine.ScreenMatrix;
            
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.Default, RasterizerState.CullNone, effect, transformMatrix);
            if (TextMaskTarget != null)
                sb.Draw((RenderTarget2D)TextMaskTarget, Vector2.Zero, Color.White);
            sb.End();
        }
        
        HiresRenderer.BeginRender();
        
        pp.RenderTargetUsage = RenderTargetUsage.DiscardContents;
    }
    
    /// <summary>
    /// Applies parameters to a given Effect
    /// </summary>
    /// <param name="eff">Effect to set parameters on</param>
    /// <param name="data">Data for the current effect layer</param>
    /// <param name="matrix">ViewMatrix for FrostHelper rendering</param>
    /// <returns></returns>
    private static Effect ApplyParameters(Effect eff, ELD data, Matrix matrix)
    {
        // hoping and praying eff isn't null
        eff.Parameters["DeltaTime"]?.SetValue(Engine.DeltaTime);
        
        // surely nobody needs this
        // eff.Parameters["CamPos"]?.SetValue(panel.Overworld.Mountain.Camera.Position);

        VirtualRenderTarget target = HiresRenderer.DrawToBuffer ? Celeste.HudTarget : null;
        float renderWidth = target?.Width ?? Engine.ViewWidth;
        float renderHeight = target?.Height ?? Engine.ViewHeight;
        
        eff.Parameters["Time"]?.SetValue(Engine.Scene.TimeActive * data.TimeMultiplier);
        eff.Parameters["Dimensions"]?.SetValue(new Vector2(renderWidth,
            renderHeight));
        eff.Parameters["Opacity"]?.SetValue(1f);
        
        // originates from frosthelper, i have no clue how transform matrix works here
        // frosthelper shaders use it though even though i don't use them, so i need to support it ig
        // note: everestcore is only on fna, i don't care about supporting xna
        Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;
        Matrix projection = Matrix.CreateOrthographicOffCenter(0, 
            viewport.Width, viewport.Height, 0, 0, 1);

        eff.Parameters["TransformMatrix"]?.SetValue(projection);
            
        eff.Parameters["ViewMatrix"]?.SetValue(matrix);
        eff.Parameters["Photosensitive"]?.SetValue(Settings.Instance.DisableFlashes);
        
        // evil fucked up wall, basically just setting all of the parameter values
        foreach (ELD.BoolParameter param in data.BoolParameters)
        {
            eff.Parameters[param.Name]?.SetValue(param.Value);
        }
        foreach (ELD.NumberParameter param in data.NumberParameters)
        {
            eff.Parameters[param.Name]?.SetValue(param.Value);
        }
        foreach (ELD.Vector2Parameter param in data.Vector2Parameters)
        {
            eff.Parameters[param.Name]?.SetValue(param.VectorValue);
        }
        foreach (ELD.Vector3Parameter param in data.Vector3Parameters)
        {
            eff.Parameters[param.Name]?.SetValue(param.VectorValue);
        }
        foreach (ELD.Vector4Parameter param in data.Vector4Parameters)
        {
            eff.Parameters[param.Name]?.SetValue(param.VectorValue);
        }
        foreach (ELD.SamplerParameter param in data.SamplerParameters)
        {
            Engine.Graphics.GraphicsDevice.Textures[param.Index] = param.Value;
        }
        
        return eff;
    }
    
    /// <summary>
    /// Checks for metadata & an effect on the given metadata layer, outputs many common variables
    /// </summary>
    /// <param name="layerType"></param>
    /// <param name="data">Data for the given layerType</param>
    /// <param name="effect">Shader effect for the given layerType</param>
    /// <param name="index">Index for the Overlay layer</param>
    /// <returns></returns>
    private static bool TryLoadEffect(string layerType, out ELD data, out Effect effect, int index = 0)
    {
        data = null;
        effect = null;

        AreaData areaData = GetAreaData();
        if (areaData == null) return false;
        
        if (!HamburgerHelperMetadata.TryGetMetadata(areaData, out HamburgerHelperMetadata metadata))
            return false;
        
        HamburgerHelperMetadata.ChapterPanelCustomizationSettingsData oed = metadata?.ChapterPanelCustomization;
        
        data = layerType switch {
            "Base" => oed?.TitleBaseEffect,
            "Accent" => oed?.TitleAccentEffect,
            "Text" => oed?.TitleTextEffect,
            "ChapterText" => oed?.ChapterTextEffect,
            "ChapterCard" => oed?.ChapterCardEffect,
            "ChapterTab" => oed?.ChapterTabEffect,
            "PlayText" => oed?.OptionLabelEffect,
            "DefaultText" => oed?.DefaultTextEffect,
            "Overlay" => oed?.Overlays[index].RenderEffect,
            _ => data
        };
        
        effect = data?.Effect;
        return effect != null;
    }

    private static AreaData GetAreaData()
    {
        if (SaveData.Instance == null) return null;
        AreaData areaData = AreaData.Get(SaveData.Instance.LastArea_Safe);
        return areaData;
    }
    
    /// <summary>
    /// Gets the current OuiChapterPanel in the Scene
    /// Split off so any methods using this can also be used within OuiChapterPanel.Option.Render too
    /// </summary>
    /// <returns></returns>
    private static OuiChapterPanel GetChapterPanel()
    {
        OuiChapterPanel panel = Engine.Scene.Entities.FindFirst<OuiChapterPanel>();
        if (panel != null)
            return panel;

        if (!HamburgerHelperModule.LoadedOptionalDependencies.Contains("CollabUtils2")) return null;
        
        // CollabtUtils2 has the OuiChapterPanel in a wrapped scene, so get it from there
        SceneWrappingEntity<Overworld> chapterPanelContainer = Engine.Scene.Entities.FindFirst<SceneWrappingEntity<Overworld>>();
        return chapterPanelContainer?.WrappedScene?.GetUI<OuiChapterPanel>();
    }
    
    # endregion
    
    # region Render Targets

    private static RenderTargetBinding[] PreviousTarget;
    private static VirtualRenderTarget TextMaskTarget;
    
    private static void OnWindowSizeChange(On.Celeste.MenuOptions.orig_SetWindow orig, int scale)
    {
        orig(scale);
        ResizeTarget();
    }
    
    [OnLoadContent]
    internal static void LoadContent(bool firstLoad)
    {
        GetWindowScale(out float scaleX, out float scaleY);
        
        scaleX = Math.Max(scaleX, 1);
        scaleY = Math.Max(scaleY, 1);
        
        float baseWidth = (320 * scaleX);
        float baseHeight = (180 * scaleY);
        
        int width = (int)(baseWidth * Math.Max(Settings.Instance.WindowScale, 6));
        int height = (int)(baseHeight * Math.Max(Settings.Instance.WindowScale, 6));
        
        TextMaskTarget = VirtualContent.CreateRenderTarget("shader-mask-target", width, height);
    }
    
    private static void ResizeTarget()
    {
        GetWindowScale(out float scaleX, out float scaleY);
        
        scaleX = Math.Max(scaleX, 1);
        scaleY = Math.Max(scaleY, 1);
        
        float baseWidth = (320 * scaleX);
        float baseHeight = (180 * scaleY);
        
        int width = (int)(baseWidth * Math.Max(Settings.Instance.WindowScale, 6));
        int height = (int)(baseHeight * Math.Max(Settings.Instance.WindowScale, 6));

        TextMaskTarget.Width = width;
        TextMaskTarget.Height = height;
        TextMaskTarget.Reload();
    }
    
    private static void GetWindowScale(out float scaleX, out float scaleY)
    {
        Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;
    
        int referenceX = 320 * Math.Max(Settings.Instance.WindowScale, 6);
        int referenceY = 180 * Math.Max(Settings.Instance.WindowScale, 6);
        
        scaleX = (float)viewport.Width / referenceX;
        scaleY = (float)viewport.Height / referenceY;
    }
    
    # endregion
}