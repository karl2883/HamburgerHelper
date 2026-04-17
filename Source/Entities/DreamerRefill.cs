using System.Collections;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;

namespace Celeste.Mod.HamburgerHelper.Entities;

// ReSharper disable once ClassNeverInstantiated.Global
[CustomEntity("HamburgerHelper/DreamerRefill")]
public class DreamerRefill : Entity
{
    // ReSharper disable once InconsistentNaming
    private static readonly ParticleType P_Glow = new ParticleType(Refill.P_Glow)
    {
        Color = Calc.HexToColor("BEA6FF"),
        Color2 = Calc.HexToColor("DD6CCA"),
    };
    
    // ReSharper disable once InconsistentNaming
    private static readonly ParticleType P_Regen = new ParticleType(Refill.P_Regen)
    {
        SpeedMin = 40f,
        SpeedMax = 60f,
        Color = Calc.HexToColor("BEA6FF"),
        Color2 = Calc.HexToColor("DD6CCA"),
    };
    
    // ReSharper disable once InconsistentNaming
    private static readonly ParticleType P_Shatter = new ParticleType(Refill.P_Shatter)
    {
        Color = Calc.HexToColor("D7D4FF"),
        Color2 = Calc.HexToColor("AB93ED"),
    };
    
    private readonly Sprite Sprite;
    private readonly Sprite Flash;
    private readonly Image Outline;
    
    private readonly Wiggler Wiggler;
    private readonly BloomPoint Bloom;
    private readonly VertexLight Light;
    private readonly SineWave Sine;

    private Level Level;
    
    private readonly float RespawnTime;
    private readonly bool OneUse;
    
    private float RespawnTimer;

    private static bool HasDreamerDash
    {
        get => HamburgerHelperModule.Session.HasDreamerDash; 
        set => HamburgerHelperModule.Session.HasDreamerDash = value;
    }
    
    private static bool UsedDreamerDash
    {
        get => HamburgerHelperModule.Session.UsedDreamerDash; 
        set => HamburgerHelperModule.Session.UsedDreamerDash = value;
    }
    
    public DreamerRefill(EntityData data, Vector2 offset) 
        : base(data.Position + offset)
    {
        RespawnTime = data.Float("respawnTime", 2.5f);
        OneUse = data.Bool("oneUse", true);
        
        Depth = -100;
        
        Collider = new Hitbox(16f, 16f, -8f, -8f);
        Add(new PlayerCollider(OnPlayer));
        
        Outline = new Image(GFX.Game["objects/hamburger/dreamerrefill/outline"]);
        Outline.CenterOrigin();
        Outline.Visible = false;
        Add(Outline);
        
        Sprite = new Sprite(GFX.Game, "objects/hamburger/dreamerrefill/");
        Sprite.AddLoop("idle", "idle", 0.1f);
        Sprite.Play("idle");
        Sprite.CenterOrigin();
        Add(Sprite);
        
        Flash = new Sprite(GFX.Game, "objects/hamburger/dreamerrefill/");
        Flash.Add("flash", "flash", 0.05f);
        Flash.OnFinish = delegate { Flash.Visible = false; };
        Flash.CenterOrigin();
        Add(Flash);
        
        Wiggler = Wiggler.Create(1f, 4f, delegate(float v) 
        {
            Sprite.Scale = (Flash.Scale = Vector2.One * (1f + v * 0.2f));
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
        
        if (Scene.OnInterval(2f) && Sprite.Visible)
        {
            Flash.Play("flash", true);
            Flash.Visible = true;
        }
    }
    
    private void OnPlayer(Player player)
    {
        if (HasDreamerDash) return;

        player.UseRefill(false);
        HasDreamerDash = true;
        Collidable = false;
        
        Audio.Play("event:/HamburgerHelper/sfx/dreamer_diamond_touch", Position);
        Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
        
        Add(new Coroutine(CollectRoutine(player)));
        RespawnTimer = RespawnTime;
    }
    
    private void Respawn()
    {
        if (Collidable) return;
        
        Audio.Play("event:/HamburgerHelper/sfx/dreamer_diamond_return", Position);
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
        Flash.Visible = false;
        
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
        Flash.Y = yPos;
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
        On.Celeste.Player.DashEnd += PlayerOnDashEnd;
        On.Celeste.Player.DashBegin += PlayerOnDashBegin;
        On.Celeste.Player.DreamDashEnd += PlayerOnDreamDashEnd;
        On.Celeste.Player.Die += PlayerOnDie;
        
        On.Celeste.PlayerHair.GetHairColor += PlayerHairOnGetHairColor;
    }

    [OnUnload]
    internal static void Unload()
    {
        On.Celeste.Player.DashEnd -= PlayerOnDashEnd;
        On.Celeste.Player.DashBegin -= PlayerOnDashBegin;
        On.Celeste.Player.DreamDashEnd -= PlayerOnDreamDashEnd;
        On.Celeste.Player.Die -= PlayerOnDie;
        
        On.Celeste.PlayerHair.GetHairColor -= PlayerHairOnGetHairColor;
    }
    
    private static void PlayerOnDashBegin(On.Celeste.Player.orig_DashBegin orig, Player self)
    {
        orig(self);
        
        // ReSharper disable once InvertIf
        if (HasDreamerDash)
        {
            HasDreamerDash = false;
            UsedDreamerDash = true;
        }
        else
        {
            UsedDreamerDash = false;
        }
    }
    
    private static void PlayerOnDashEnd(On.Celeste.Player.orig_DashEnd orig, Player self)
    {
        orig(self);
        
        // ReSharper disable once InvertIf
        if (UsedDreamerDash)
        {
            // this avoids a crash when you use both a dream refill & dreamer refill
            // without having used a dream block before
            if (self.dreamSfxLoop == null)
            {
                self.Add(self.dreamSfxLoop = new SoundSource());
            }
            
            self.StateMachine.State = Player.StDreamDash;
        }
    }
    
    private static void PlayerOnDreamDashEnd(On.Celeste.Player.orig_DreamDashEnd orig, Player self)
    {
        orig(self);

        // ReSharper disable once InvertIf
        if (UsedDreamerDash)
        {
            self.jumpGraceTimer = 0.1f;
            UsedDreamerDash = false;
        }
    }
    
    private static PlayerDeadBody PlayerOnDie(On.Celeste.Player.orig_Die orig, Player self, 
        Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
    {
        HasDreamerDash = false;
        UsedDreamerDash = false;
        
        return orig(self, direction, evenIfInvincible, registerDeathInStats);
    }
    
    private static Color PlayerHairOnGetHairColor(On.Celeste.PlayerHair.orig_GetHairColor orig, 
        PlayerHair self, int index)
    {
        return HasDreamerDash ? Color.MediumPurple : orig(self, index);
    }
}
