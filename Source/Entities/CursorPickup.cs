using System.Collections;
using AsmResolver.PE.DotNet.ReadyToRun;
using Celeste.Mod.HamburgerHelper.States;

namespace Celeste.Mod.HamburgerHelper.Entities;

[CustomEntity("HamburgerHelper/CursorPickup")]
public class CursorPickup : Entity
{
    private class WindowContentRenderer : Entity
    {
        private VirtualRenderTarget WindowRenderTarget;
        private const int WindowWidth = 39;
        private const int WindowHeight = 29;
        
        private readonly MTexture MouseTexture;
        
        public Vector2 CursorPosition = new Vector2(16f, 8f);
        
        public WindowContentRenderer(Vector2 position)
            : base(position)
        {
            Depth = -100;
            
            MouseTexture = GFX.Game["objects/hamburger/cursorpickup/mouse"];
            
            Add(new BeforeRenderHook(BeforeRender));
        }
        
        public override void Added(Scene scene)
        {
            base.Added(scene);
            
            ClearTarget();
            
            WindowRenderTarget = VirtualContent.CreateRenderTarget($"window-{X}{Y}", WindowWidth, WindowHeight);
        }
    
        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            
            ClearTarget();
        }

        private void ClearTarget()
        {
            if (WindowRenderTarget == null) return;
            
            WindowRenderTarget.Dispose();
            WindowRenderTarget = null;
        }

        private void BeforeRender()
        {
            if (WindowRenderTarget == null) return;
        
            GraphicsDevice gd = Engine.Graphics.GraphicsDevice;
        
            gd.SetRenderTarget(WindowRenderTarget);
            gd.Clear(Color.Transparent);
        
            GameplayRenderer.Begin();
        
            if (Collidable)
            {
                if (Scene is not Level level) return;
                
                MouseTexture.Draw(CursorPosition + level.Camera.Position);
            }
        
            GameplayRenderer.End();
        
            gd.SetRenderTarget(null);
        }
    
        public override void Render()
        {
            base.Render();
        
            SpriteBatch sb = Draw.SpriteBatch;
            
            Vector2 windowContentOffset = new Vector2(4f, 14f);
            sb.Draw(WindowRenderTarget, Position + windowContentOffset, Color.White);
        }
    }

    private WindowContentRenderer WindowRenderer;
    
    private readonly float RespawnTime;
    private readonly float CursorTime;
    private readonly float CursorSpeed;

    public CursorPickup(EntityData data, Vector2 offset) 
        : base(data.Position + offset)
    {
        Depth = 1000;

        RespawnTime = data.Float("respawnTime", 3f);
        CursorTime = data.Float("cursorTime", 2f);
        CursorSpeed = data.Float("cursorSpeed", 3f);
        
        string windowTexture = data.Attr("windowTexture", "objects/hamburger/cursorpickup/window");
        
        Image windowSprite = new(GFX.Game[windowTexture]);
        windowSprite.CenterOrigin();
        Add(windowSprite);
        
        Collider = new Hitbox(16, 16, -8f, -4f);
        
        Add(new PlayerCollider(OnPlayer));
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        
        Vector2 windowQuadrant = new Vector2(24f, 24f);
        WindowRenderer = new WindowContentRenderer(Position - windowQuadrant);
        
        Scene.Add(WindowRenderer);
    }
    
    private void OnPlayer(Player player)
    {
        if (player.StateMachine.State == St.Cursor)
        {
            player.Play("event:/HamburgerHelper/sfx/mouse_down");
        }
        
        player.StateMachine.State = St.Cursor;
        
        Cursor.CursorStateTime = CursorTime;
        Cursor.CursorStateSpeed = CursorSpeed;
        
        Add(new Coroutine(ResetRoutine()));
    }

    private IEnumerator ResetRoutine()
    {
        if (WindowRenderer == null) yield break;
        
        Collidable = false;
        
        Vector2 onscreenPos = new Vector2(16f, 8f);
        
        float randomAngle = Calc.Random.NextAngle();
        Vector2 offscreenPos = Calc.AngleToVector(randomAngle, 32) + onscreenPos;
        
        WindowRenderer.CursorPosition = offscreenPos;
        WindowRenderer.Depth = Depth;
        
        if (RespawnTime < 0)
        {
            yield break;
        }
        
        yield return RespawnTime - 0.5f;

        const float cursorMoveTime = 0.5f;
        for (float i = 0; i < cursorMoveTime; i += Engine.DeltaTime)
        {
            WindowRenderer.CursorPosition = Vector2.Lerp(offscreenPos, onscreenPos, Ease.BackOut(i / cursorMoveTime));
            
            yield return null;
        }

        Collidable = true;
        
        WindowRenderer.CursorPosition = onscreenPos;
        WindowRenderer.Depth = -100;
    }
}
