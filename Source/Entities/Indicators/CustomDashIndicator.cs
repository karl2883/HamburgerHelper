namespace Celeste.Mod.HamburgerHelper.Entities.Indicators;

public class CustomDashIndicator : Entity
{
    private struct Indicator
    {
        public MTexture Texture;
        public Func<bool> Condition;
    }
    
    private bool Enabled;
    private static bool GrabbyEnabled => Settings.Instance.GrabMode == GrabModes.Toggle && Input.GrabCheck;
    private static float RenderPos => GrabbyEnabled ? 29f : 21f;
    
    private readonly Wiggler Wiggler;
    private readonly List<Indicator> Indicators = [];
    
    public CustomDashIndicator()
        : base()
    {
        Depth = -1000001;
        
        AddTag(Tags.Global);
        AddTag(Tags.PauseUpdate);
        AddTag(Tags.TransitionUpdate);
        
        Add(Wiggler = Wiggler.Create(0.1f, 0.3f));
        
        CreateIndicators();
    }

    private void CreateIndicators()
    {
        Indicators.Add(new Indicator
        {
            Texture = GFX.Game["util/hamburger/moon"],
            Condition = () => HamburgerHelperModule.Session.HasDreamerDash
        });
        
        Indicators.Add(new Indicator
        {
            Texture = GFX.Game["util/hamburger/lighter"],
            Condition = () => HamburgerHelperModule.Session.CanLight
        });
    }
    
    public override void Update()
    {
        base.Update();
        
        bool isVisible = false;
        if (!SceneAs<Level>().InCutscene)
        {
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity is { Dead: false } && HamburgerHelperModule.Settings.ShowCustomDashIndicators)
            {
                isVisible = true;
            }
        }
        // ReSharper disable once InvertIf
        if (isVisible != Enabled)
        {
            Enabled = isVisible;
            Wiggler.Start();
        }
    }

    public override void Render()
    {
        base.Render();

        if (!Enabled) return;

        List<Indicator> activeIndicators = Indicators.Where(i => i.Condition()).ToList();
        if (activeIndicators.Count == 0) return;
        
        Vector2 scale = Vector2.One * (1f + Wiggler.Value * 0.2f);
        
        Player player = Scene.Tracker.GetEntity<Player>();
        if (player == null) return;
        
        const float width = 8f;
        const float spacing = 2f;
        const float step = width + spacing;

        int indicatorsCount = activeIndicators.Count;
        
        float totalWidth = (indicatorsCount * width) + ((indicatorsCount - 1) * spacing);
        Vector2 basePos = new Vector2(player.X, player.Y - RenderPos);
        
        float startX = basePos.X - totalWidth / 2f + width / 2f;
        
        for (int i = 0; i < indicatorsCount; i++)
        {
            float x = startX + i * step;
            activeIndicators[i].Texture.DrawJustified(new Vector2(x, basePos.Y), new Vector2(0.5f, 1f), Color.White, scale);
        }
    }

    [OnLoad]
    internal static void Load()
    {
        On.Celeste.LevelLoader.LoadingThread += LevelLoaderOnLoadingThread;
    }

    [OnUnload]
    internal static void Unload()
    {
        On.Celeste.LevelLoader.LoadingThread -= LevelLoaderOnLoadingThread;
    }
    
    private static void LevelLoaderOnLoadingThread(On.Celeste.LevelLoader.orig_LoadingThread orig, LevelLoader self)
    {
        self.Level.Add(new CustomDashIndicator());
        
        orig(self);
    }
}
