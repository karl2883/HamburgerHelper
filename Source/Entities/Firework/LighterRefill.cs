using System.Collections;

namespace Celeste.Mod.HamburgerHelper.Entities.Firework;

// ReSharper disable once ClassNeverInstantiated.Global
[CustomEntity("HamburgerHelper/LighterRefill")]
public class LighterRefill : Entity
{
    // ReSharper disable once InconsistentNaming
    private static readonly ParticleType P_Glow = new ParticleType(Refill.P_Glow)
    {
        Color = Calc.HexToColor("999999"),
        Color2 = Calc.HexToColor("dbdbdb"),
    };
    
    // ReSharper disable once InconsistentNaming
    private static readonly ParticleType P_Regen = new ParticleType(Refill.P_Regen)
    {
        SpeedMin = 40f,
        SpeedMax = 60f,
        Color = Calc.HexToColor("999999"),
        Color2 = Calc.HexToColor("dbdbdb"),
    };
    
    // ReSharper disable once InconsistentNaming
    private static readonly ParticleType P_Shatter = new ParticleType(Refill.P_Shatter)
    {
        Color = Calc.HexToColor("a8a8a8"),
        Color2 = Calc.HexToColor("ebebeb"),
    };
    
    private readonly Sprite Sprite;
    private readonly Image Outline;
    
    private readonly Wiggler Wiggler;
    private readonly BloomPoint Bloom;
    private readonly VertexLight Light;
    private readonly SineWave Sine;

    private Level Level;
    
    private readonly float RespawnTime;
    private readonly bool OneUse;
    
    private float RespawnTimer;

    private static bool CanLight
    {
        get => HamburgerHelperModule.Session.CanLight; 
        set => HamburgerHelperModule.Session.CanLight = value;
    }
    
    public LighterRefill(EntityData data, Vector2 offset) 
        : base(data.Position + offset)
    {
        RespawnTime = data.Float("respawnTime", 2.5f);
        OneUse = data.Bool("oneUse", true);
        
        Depth = -100;
        
        Collider = new Hitbox(16f, 16f, -8f, -8f);
        Add(new PlayerCollider(OnPlayer));
        
        Outline = new Image(GFX.Game["objects/hamburger/lighter/outline00"]);
        Outline.CenterOrigin();
        Outline.Visible = false;
        Add(Outline);
        
        Sprite = new Sprite(GFX.Game, "objects/hamburger/lighter/");
        Sprite.AddLoop("idle", "idle", 0.1f);
        Sprite.Play("idle");
        Sprite.CenterOrigin();
        Add(Sprite);
        
        Wiggler = Wiggler.Create(1f, 4f, delegate(float v) 
        {
            Sprite.Scale = Vector2.One * (1f + v * 0.2f);
        });
        Add(Wiggler);
        
        Add(Bloom = new BloomPoint(0.8f, 16f));
        Add(Light = new VertexLight(Color.White, 1f, 16, 48));
        Add(Sine = new SineWave(0.6f, 0f));
        Sine.Randomize();
        
        Add(new MirrorReflection());
        UpdateY();
    }

    public override void Update()
    {
        base.Update();
        
        if (RespawnTimer > 0f)
        {
            RespawnTimer -= Engine.DeltaTime;
            if (RespawnTimer <= 0f)
            {
                Respawn();
            }
        }
        else if (Scene.OnInterval(0.1f))
        { 
            Level.ParticlesFG.Emit(P_Glow, 1, Position, Vector2.One * 5f);
        }
        
        UpdateY();
        
        Light.Alpha = Calc.Approach(Light.Alpha, 
            Sprite.Visible ? 1f : 0f, 4f * Engine.DeltaTime);
        Bloom.Alpha = Light.Alpha * 0.8f;
    }
    
    private void OnPlayer(Player player)
    {
        player.UseRefill(false);
        CanLight = true;
        
        Collidable = false;
        
        Audio.Play("event:/HamburgerHelper/sfx/lighter_touch", Position);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        
        Add(new Coroutine(CollectRoutine(player)));
        RespawnTimer = RespawnTime;
    }
    
    private void Respawn()
    {
        if (Collidable) return;
        
        Audio.Play("event:/HamburgerHelper/sfx/lighter_return", Position);
        Level.ParticlesFG.Emit(P_Regen, 16, Position, Vector2.One * 2f);
        
        Collidable = true;
        Depth = -100;
        
        Sprite.Visible = true;
        Outline.Visible = false;
        
        Wiggler.Start();
    }
    
    private IEnumerator CollectRoutine(Player player)
    {
        Celeste.Freeze(0.05f);
        yield return null;
        
        Level.Shake();
        
        Sprite.Visible = false;
        
        Depth = 8999;
        if (!OneUse)
        {
            Outline.Visible = true;
        }
        
        yield return 0.05f;
        
        float playerAngle = player.Speed.Angle();
        
        Level.ParticlesFG.Emit(P_Shatter, 5, Position, 
            Vector2.One * 4f, playerAngle - MathF.PI / 2f);
        Level.ParticlesFG.Emit(P_Shatter, 5, Position, 
            Vector2.One * 4f, playerAngle + MathF.PI / 2f);
        
        SlashFx.Burst(Position, playerAngle);
        
        if (OneUse)
        {
            RemoveSelf();
        }
    }
    
    private void UpdateY()
    {
        float yPos = Sine.Value * 2f;
        
        Sprite.Y = yPos;
        Bloom.Y = yPos;
    }
    
    public override void Render()
    {
        if (Sprite.Visible)
        {
            Sprite.DrawOutline();
        }
        base.Render();
    }
    
    public override void Added(Scene scene)
    {
        base.Added(scene);
        Level = scene as Level;
    }
    
    [OnLoad]
    internal static void Load()
    {
        On.Celeste.Player.Die += PlayerOnDie;
        On.Celeste.Level.TransitionRoutine += LevelOnTransitionRoutine;
    }

    [OnUnload]
    internal static void Unload()
    {
        On.Celeste.Player.Die -= PlayerOnDie;
        On.Celeste.Level.TransitionRoutine -= LevelOnTransitionRoutine;
    }
    
    private static PlayerDeadBody PlayerOnDie(On.Celeste.Player.orig_Die orig, Player self, 
        Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
    {
        CanLight = false;
        
        return orig(self, direction, evenIfInvincible, registerDeathInStats);
    }
    
    private static IEnumerator LevelOnTransitionRoutine(On.Celeste.Level.orig_TransitionRoutine orig, Level self, LevelData next, Vector2 direction)
    {
        CanLight = false;
        
        yield return new SwapImmediately(orig(self, next, direction));
    }
}
