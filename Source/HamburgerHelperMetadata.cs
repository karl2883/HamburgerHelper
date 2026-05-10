// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnassignedField.Global

namespace Celeste.Mod.HamburgerHelper;

public class HamburgerHelperMetadata
{
    private static readonly Dictionary<string, HamburgerHelperMetadata> CachedMetadata = [];
    private static readonly HashSet<string> NegativeCache = [];
    
    private class HamburgerHelperYaml
    {
        public HamburgerHelperMetadata HamburgerHelperMetadata { get; set; } = new HamburgerHelperMetadata();
    }
    
    public class EffectDataLayer
    {
        // various parameter types, all loaded separate to avoid annoying parsing
        // (yaml deserializer takes care of this for me!)
        public class BoolParameter
        {
            public string Name;
            public bool Value;
        }
        
        public class NumberParameter
        {
            public string Name;
            public float Value;
        }
        
        public class Vector2Parameter
        {
            public string Name;
            public float[] Value;
            
            public Vector2 VectorValue => new Vector2(Value[0], Value[1]);
        }
        
        public class Vector3Parameter
        {
            public string Name;
            public float[] Value;
            
            public Vector3 VectorValue => new Vector3(Value[0], Value[1], Value[2]);
        }
        
        public class Vector4Parameter
        {
            public string Name;
            public float[] Value;
            
            public Vector4 VectorValue => new Vector4(Value[0], Value[1], Value[2], Value[3]);
        }

        public class SamplerParameter
        {
            public int Index;
            public string Texture;
            public Texture2D Value => GFX.Gui[Texture].Texture.Texture_Safe;
        }
    
        // ReSharper disable once MemberCanBePrivate.Global
        public string EffectPath { get; set; } = "";
        public Effect Effect => HamburgerHelperGFX.LoadEffect("", EffectPath);
        
        public string TextOutlineColor { get; set; } = "FFFFFF";
        public bool TextRenderToRenderTarget { get; set; } = false;
        public bool TextRenderOutline { get; set; } = false;
        public bool TextOffsetOutline { get; set; } = true;
        
        public float TimeMultiplier { get; set; } = 1f;
        
        public List<BoolParameter> BoolParameters = [];
        public List<NumberParameter> NumberParameters = [];
        public List<Vector2Parameter> Vector2Parameters = [];
        public List<Vector3Parameter> Vector3Parameters = [];
        public List<Vector4Parameter> Vector4Parameters = [];
        public List<SamplerParameter> SamplerParameters = [];
    }
    
    public class OverlayData
    {
        public enum Conditions
        {
            None,
            Clear,
            FullClear,
            Golden,
            Silver,
            Rainbow
        }

        public enum Anchors
        {
            None,
            Card,
            Stats
        }
        
        public Conditions Condition { get; set; } = Conditions.None;
        public Anchors Anchor { get; set; } = Anchors.Card;

        public string Color { get; set; } = "FFFFFF";
        public string Texture { get; set; }
        
        // ReSharper disable once MemberCanBePrivate.Global
        public float[] Offset { get; set; } = [0, 0];
        public Vector2 PositionOffset => new Vector2(Offset[0], Offset[1]);
        public EffectDataLayer RenderEffect { get; set; }
    }

    public class CustomColorData
    {
        public string OptionLabelColor { get; set; }
        public float OptionLabelAlpha { get; set; } = 0.8f;
        
        public string PlayOptionBgColor { get; set; }
        public float PlayOptionBgAlpha { get; set; } = 1f;
        public string StartOptionBgColor { get; set; }
        public float StartOptionBgAlpha { get; set; } = 1f;
        public string CheckpointOptionBgColor { get; set; }
        public float CheckpointOptionBgAlpha { get; set; } = 1f;
    }

    public class CustomIconData
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public float[] Offset { get; set; } = [0, 0];
        public Vector2 PositionOffset => new Vector2(Offset[0], Offset[1]);
        
        public EffectDataLayer IconEffect { get; set; }
    }

    public class OverworldCustomizationSettingsData
    {
        public class CustomHiresSnow
        {
            public string TexturePath { get; set; } = "snow";
            public string[] OverworldTexturePaths { get; set; } = ["snow"];
            public float OverworldSnowSize { get; set; } = 1f;
        }
        
        public string CustomOverlayTexture { get; set; } = "overlay";
        public string CustomVignetteTexture { get; set; } = "vignette";
        
        public CustomHiresSnow CustomSnow { get; set; }
    }

    public OverworldCustomizationSettingsData OverworldCustomization { get; set; }
    
    public class ChapterPanelCustomizationSettingsData
    {
        public CustomColorData CustomColors { get; set; }
        public List<OverlayData> Overlays { get; set; }
        public CustomIconData CustomIcon { get; set; }
        public string CustomPolaroidPath { get; set; } = "polaroid";
        
        public EffectDataLayer TitleTextEffect { get; set; }
        public EffectDataLayer TitleBaseEffect { get; set; }
        public EffectDataLayer TitleAccentEffect { get; set; }
        public EffectDataLayer ChapterTextEffect { get; set; }
        public EffectDataLayer ChapterCardEffect { get; set; }
        public EffectDataLayer ChapterTabEffect { get; set; }
        public EffectDataLayer OptionLabelEffect { get; set; }
        public EffectDataLayer DefaultTextEffect { get; set; }
    }
    
    public ChapterPanelCustomizationSettingsData ChapterPanelCustomization { get; set; }
    
    
    public static bool TryGetMetadata(AreaData area, out HamburgerHelperMetadata metadata)
    {
        if (area == null)
        {
            metadata = null;
            return false;
        }
        
        if (CachedMetadata.TryGetValue(area.SID, out metadata)) return true;
        if (NegativeCache.Contains(area.SID)) return false;
        
        string metaFile = $"Maps/{area.SID}.meta";
        if (!Everest.Content.TryGet(metaFile, out ModAsset asset))
        {
            NegativeCache.Add(area.SID);
            return false;
        }

        if (asset is null || !asset.PathVirtual.StartsWith("Maps") || !asset.TryDeserialize(out HamburgerHelperYaml meta))
        {
            NegativeCache.Add(area.SID);
            return false;
        }
        
        metadata = meta?.HamburgerHelperMetadata;
        if (metadata?.ChapterPanelCustomization == null)
        {
            NegativeCache.Add(area.SID);
            return false;
        }
        
        CachedMetadata[area.SID] = metadata;
        return true;
    }
    
    [OnLoad]
    internal static void Load()
    {
        Everest.Content.OnUpdate += ContentOnOnUpdate;
    }
    
    [OnUnload]
    internal static void Unload()
    {
        Everest.Content.OnUpdate -= ContentOnOnUpdate;
    }
    
    private static void ContentOnOnUpdate(ModAsset arg1, ModAsset arg2)
    {
        ClearMetadataCache();
        HamburgerHelperGFX.ClearEffects();
    }
    
    private static void ClearMetadataCache()
    {
        CachedMetadata.Clear();
        NegativeCache.Clear();
    }
}
