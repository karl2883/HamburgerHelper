using System.Collections;
using Celeste.Mod.HamburgerHelper.Entities.Vsrg.HitObjects;
using Celeste.Mod.Helpers;
using MonoMod.Cil;

namespace Celeste.Mod.HamburgerHelper.Entities.Vsrg;

public class VsrgManager : Entity
{
    private VirtualRenderTarget VsrgRenderTarget;

    public VsrgTimer Timing;
    public float SongPosition => Timing.SongPosition;
    public float SmoothSongPosition => Timing.SmoothSongPosition;
    public int SongLengthMs => Timing.SongLengthMs;
    
    public readonly Chart Chart;
    private List<HitObject> HitObjects => Chart.HitObjects;
    public List<Judgement> Judgements = new List<Judgement>();
    
    private VsrgMetadata Metadata;
    
    private readonly Coroutine EntryCoroutine;
    private readonly MTexture StageTexture;
    
    public float GameFade = 0f;
    private float GameFadeTarget = 0f;
    
    private float BackgroundFade = 0f;
    private float BackgroundFadeTarget = 0f;

    private const float BgOpacity = 0.75f;
    private const float PostSongDelay = 2f;
    
    public Vector2 ScreenPosition => ((Level)Scene).Camera.Position;
    private readonly VsrgColumn[] Columns = new VsrgColumn[4];
    
    public VsrgManager(Vector2 position, Chart chart, Coroutine coroutine)
        : base(position)
    {
        Depth = -int.MaxValue;
        Chart = chart;
        EntryCoroutine = coroutine;
        
        Add(new BeforeRenderHook(BeforeRender));
        StageTexture = GFX.Game["objects/hamburger/vsrg/stage/outline"];
    }

    public IEnumerator GameRunRoutine()
    {
        StartScene();
        
        Audio.GetEventDescription(Chart.SongEvent).getLength(out Timing.SongLengthMs);
        float lengthSeconds = Timing.SongLengthMs / 1000f;
        
        yield return lengthSeconds + PostSongDelay;
        
        EndScene();
    }
    
    public override void Update()
    {
        base.Update();
        
        if (Input.ESC.Pressed)
        {
            EntryCoroutine.Jump();
            Input.ESC.ConsumePress();
            EndScene();
        }
        
        GameFade = Calc.Approach(GameFade, GameFadeTarget, 4f * Engine.DeltaTime);
        BackgroundFade = Calc.Approach(BackgroundFade, BackgroundFadeTarget, 4f * Engine.DeltaTime);
    }
    
    private void VsrgRender()
    {
        if (Scene is not Level level) return;
        
        StageTexture?.Draw(ScreenPosition, Vector2.Zero, Color.White * GameFade);

        foreach (VsrgColumn column in Columns)
        {
            column?.Draw();
        }

        foreach (HitObject obj in Chart.HitObjects)
        {
            obj.Draw();
        }
        
        Metadata?.Draw();
    }
    
    private void StartScene()
    {
        GameFadeTarget = 1f;
        BackgroundFadeTarget = BgOpacity;
        
        Scene.Add(Timing = new VsrgTimer(Vector2.Zero, Chart.SongEvent));
        Timing.PlayMusic();
        
        for (int i = 0; i < 4; i++)
        {
            int column = i + 1;
            
            Columns[i] = new VsrgColumn(ScreenPosition, column, this);
            foreach (HitObject obj in HitObjects)
            {
                obj.Manager = this;
                
                switch (obj)
                {
                    case Note note: {
                        if (note.Column == column)
                        {
                            Columns[i].HitObjects.Enqueue(note, note.TimingMs);
                        }
                        break;
                    }
                    case HitObjects.Bumper bmp:
                        switch (bmp.BumperPosition)
                        {
                            case BumperPosition.Left when column is 1 or 2:
                            case BumperPosition.Middle when column is 2 or 3:
                            case BumperPosition.Right when column is 3 or 4:
                                Columns[i].HitObjects.Enqueue(bmp, bmp.TimingMs);
                                break;
                            default:
                                break;
                        }
                        break;
                }
            }
        }
        
        for (int i = 0; i < 4; i++)
        {
            Scene.Add(Columns[i]);
        }
        
        Scene.Add(Metadata = new VsrgMetadata(Vector2.Zero, this));
        
        Add(new Coroutine(StartGameRoutine()));
    }
    
    private void EndScene()
    {
        GameFadeTarget = 0f;
        BackgroundFadeTarget = 0f;
        
        Add(new Coroutine(FadeOutRoutine()));
    }
    
    private IEnumerator StartGameRoutine()
    {
        while (Math.Abs(GameFade - 1f) > 0.1f)
        {
            yield return null;
        }
    }
    
    private IEnumerator FadeOutRoutine()
    {
        while (GameFade > 0)
        {
            yield return null;
        }
        
        Timing.StopMusic();
        Timing.RemoveSelf();
        
        Metadata.RemoveSelf();
        
        for (int i = 0; i < 4; i++)
        {
            Columns[i]?.RemoveSelf();
            Columns[i] = null;
        }
        
        RemoveSelf();
    }
    
    #region Rendering Bullshit
    
    private void BeforeRender()
    {
        VsrgRenderTarget ??= VirtualContent.CreateRenderTarget("dreamer-vsrg-rt", 320, 180);
        
        GraphicsDevice gd = Engine.Graphics.GraphicsDevice;
        SpriteBatch sb = Draw.SpriteBatch;
        
        if (Scene is not Level level) return;
        Camera camera = level.Camera;
        
        gd.SetRenderTarget(VsrgRenderTarget);
        gd.Clear(Color.Black * BackgroundFade);
        
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, 
            DepthStencilState.None, RasterizerState.CullNone, null, camera.Matrix);
        
        VsrgRender();
        
        sb.End();
        
        gd.SetRenderTarget(null);
    }
    
    private void RenderTargets()
    {
        if (Scene is not Level level) return;
        Camera camera = level.Camera;
        
        SpriteBatch sb = Draw.SpriteBatch;
        
        Matrix transform = camera.Matrix * Engine.ScreenMatrix * Matrix.CreateScale(6f);
        
        sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, 
            DepthStencilState.Default, RasterizerState.CullNone, null, transform);
        sb.Draw(VsrgRenderTarget, ScreenPosition, Color.White);
        sb.End();
    }

    internal static void Load()
    {
        IL.Celeste.Level.Render += LevelOnRender;
    }

    internal static void Unload()
    {
        IL.Celeste.Level.Render -= LevelOnRender;
    }
    
    private static void LevelOnRender(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        
        /*
         * IL_0462: ldfld class Celeste.Mod.UI.SubHudRenderer Celeste.Level::SubHudRenderer
         * IL_0468: callvirt instance void Monocle.Renderer::Render(class Monocle.Scene)
         */
        if (!cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchLdarg0(),
            instr => instr.MatchLdfld<Level>("SubHudRenderer"),
            instr => instr.MatchLdarg0(),
            instr => instr.MatchCallvirt<Renderer>("Render")))
            throw new HookUtilities.HookException("womp womp");
        
        cursor.EmitLdarg0();
        cursor.EmitDelegate(DrawVsrg);

        return;

        static void DrawVsrg(Level level)
        {
            foreach (Entity entity in level.Tracker.GetEntitiesTrackIfNeeded<VsrgManager>())
            {
                if (entity is not VsrgManager manager) continue;

                manager.RenderTargets();
            }
        }
    }
    
    #endregion
}