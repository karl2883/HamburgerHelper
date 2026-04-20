using System.Collections;
using Celeste.Mod.HamburgerHelper.Entities.Indicators;
using Celeste.Mod.Roslyn.ModLifecycleAttributes;

namespace Celeste.Mod.HamburgerHelper.States;

public static class DarkAngel
{
    #region Particles
    
    private static readonly ParticleType WingParticlesRight = new ParticleType()
    {
        Source = GFX.Game["particles/feather"],
        Color = Calc.HexToColor("2b1c47"),
        Color2 = Calc.HexToColor("1b1631"),
        ColorMode = ParticleType.ColorModes.Choose,
        Direction = 0f,
        DirectionRange = MathHelper.Pi / 4f,
        LifeMin = 0.5f,
        LifeMax = 1f,
        FadeMode = ParticleType.FadeModes.Linear,
        Acceleration = Vector2.Zero,
        Friction = 0.6f,
        SpeedMin = 40f,
        SpeedMax = 80f,
        Size = 1f,
        SpinMin = -MathHelper.Pi / 2f,
        SpinMax = MathHelper.Pi / 2f,
    };

    private static readonly ParticleType WingParticlesLeft = new ParticleType(WingParticlesRight) 
    {
        Direction = MathHelper.Pi,
    };
    
    private static readonly ParticleType FeatherParticlesRight = new ParticleType(WingParticlesRight)
    {
        Color = Calc.HexToColor("6a46b0"),
        Color2 = Calc.HexToColor("483e84"),
        SpeedMin = 20f,
        SpeedMax = 40f,
        Size = 1f,
        FadeMode = ParticleType.FadeModes.Late,
    };

    private static readonly ParticleType FeatherParticlesLeft = new ParticleType(WingParticlesLeft) 
    {
        Color = Calc.HexToColor("6a46b0"),
        Color2 = Calc.HexToColor("483e84"),
        SpeedMin = 20f,
        SpeedMax = 40f,
        Size = 1f,
        FadeMode = ParticleType.FadeModes.Late,
    };
    
    #endregion

    private const float MovementSpeed = 180f;
    
    private const float BaseGravityTarget = 180f;
    private const float GravityDecreaseRate = 6f;
    
    private const float ChargeRate = 8f;
    private const float JumpMax = 105f;
    
    private static float ChargeTimer;
    private static float GravityTarget;
    
    private static int NextState = Player.StNormal;

    private static PlayerOverlay Wings;
    private static PlayerOverlay Halo;
    
    private static bool HasDarkAngelWings
    {
        get => HamburgerHelperModule.Session.HasDarkAngelWings; 
        set => HamburgerHelperModule.Session.HasDarkAngelWings = value;
    }
    
    private static bool UsingDarkAngelWings
    {
        get => HamburgerHelperModule.Session.UsingDarkAngelWings; 
        set => HamburgerHelperModule.Session.UsingDarkAngelWings = value;
    }
    
    public static int AngelUpdate(Player player)
    {
        if (player.OnGround(player.Position, 4)) return Player.StNormal;
        
        player.Speed.Y = Calc.Approach(player.Speed.Y, GravityTarget, (900f * Engine.DeltaTime));
        
        const float airControlFactor = 0.85f;
        if (Math.Abs(player.Speed.X) > MovementSpeed && Math.Sign(player.Speed.X) == player.moveX)
        {
            player.Speed.X = Calc.Approach(player.Speed.X, (MovementSpeed * player.moveX), 
                (400f * airControlFactor * Engine.DeltaTime));
        }
        else
        {
            player.Speed.X = Calc.Approach(player.Speed.X, (MovementSpeed * player.moveX), 
                (1000f * airControlFactor * Engine.DeltaTime));
        }
        
        if (Input.Jump.Check)
        {
            UsingDarkAngelWings = true;
            
            GravityTarget -= GravityDecreaseRate;
            ChargeTimer += ChargeRate;
            ChargeTimer = Math.Min(ChargeTimer, JumpMax);

            if (player.Scene.OnInterval(0.1f))
            {
                player.CreateAngelTrail();
            }
        }
        
        // ReSharper disable once InvertIf
        if (Input.Jump.Released || !Input.Jump.Check)
        {
            player.AngelJump();   
        }
        
        return NextState;
    }
    
    public static IEnumerator AngelRoutine(Player player)
    {
        float dur = 0f;
        while (dur < 1f)
        {
            dur += Engine.DeltaTime;
            yield return null;
        }
        
        player.AngelJump();
        player.StateMachine.State = Player.StNormal;
    }
    
    public static void AngelBegin(Player player)
    {
        HasDarkAngelWings = false;
        UsingDarkAngelWings = true;
        
        SpawnParticles(player);
        
        Input.Jump.ConsumeBuffer();
        
        player.dashAttackTimer = 0f;
        player.gliderBoostTimer = 0f;
        player.wallSlideTimer = 1.2f;
        player.wallBoostTimer = 0f;

        int speedDirection = Math.Sign(player.Speed.X);
        float absoluteSpeed = Math.Abs(player.Speed.X);
        
        if (absoluteSpeed <= MovementSpeed || speedDirection != player.moveX)
        {
            player.Speed.X = (MovementSpeed) * player.moveX;
        }
        
        ChargeTimer = 0f;

        GravityTarget = BaseGravityTarget;
        
        NextState = St.DarkAngel;
        
        Audio.Play("event:/HamburgerHelper/sfx/angel_wing_flap", player.Position);
    }
    
    public static void AngelEnd(Player player)
    {
        Wings?.RemoveSelf();
        Halo?.RemoveSelf();
        
        Wings = null;
        Halo = null;
        
        Audio.Play("event:/HamburgerHelper/sfx/angel_wing_flap", player.Position);
        
        SpawnParticles(player);
        
        UsingDarkAngelWings = false;
        ChargeTimer = 0f;
    }

    private static void SpawnParticles(Player player)
    {
        if (player.Scene is not Level level) return;
        level.ParticlesBG.Emit(WingParticlesLeft, 6, player.Center, Vector2.One * 2f);
        level.ParticlesBG.Emit(WingParticlesRight, 6, player.Center, Vector2.One * 2f);
        level.ParticlesBG.Emit(FeatherParticlesLeft, 6, player.Center, Vector2.One * 2f);
        level.ParticlesBG.Emit(FeatherParticlesRight, 6, player.Center, Vector2.One * 2f);
    }

    private static bool AngelWingCheck()
    {
        return HasDarkAngelWings || UsingDarkAngelWings;
    }
    
    [OnLoad]
    internal static void Load()
    {
        On.Celeste.Player.Update += PlayerOnUpdate;
    }
    
    [OnUnload]
    internal static void Unload()
    {
        On.Celeste.Player.Update -= PlayerOnUpdate;
    }
    
    private static void PlayerOnUpdate(On.Celeste.Player.orig_Update orig, Player self)
    {
        orig(self);
        
        if (!self.OnGround(self.Position, 4) && !self.onGround
            && !self.WallJumpCheck(3) && !self.WallJumpCheck(-3) 
            && self.jumpGraceTimer <= 0f && self.varJumpTimer <= 0f
            && self.StateMachine.State == Player.StNormal || self.StateMachine.State == Player.StDash)
        {
            if (Input.Jump.Pressed && HasDarkAngelWings)
            {
                self.StateMachine.State = St.DarkAngel;   
            }
        }

        // ReSharper disable once InvertIf
        if (AngelWingCheck() && Wings == null && Halo == null)
        {
            Wings = new PlayerOverlay(GFX.Game["characters/player/hamburger/darkAngel/wings"], 
                AngelWingCheck, 15);
            Halo = new PlayerOverlay(GFX.Game["characters/player/hamburger/darkAngel/halo"], 
                AngelWingCheck, -15);
        
            self.Scene.Add(Wings);
            self.Scene.Add(Halo);
            
            SpawnParticles(self);
        }
    }

    #region PlayerExt
    
    // ReSharper disable once ConvertToExtensionBlock
    private static void AngelJump(this Player player)
    {
        NextState = Player.StNormal;
        
        player.Speed.Y -= ChargeTimer;
        ChargeTimer = 0f;
        
        SaveData.Instance.TotalJumps++;
    }


    private static readonly Color[] GradientColors =
    [
        Calc.HexToColor("5300c8"),
        Calc.HexToColor("4805bb"),
        Calc.HexToColor("3e09af"),
        Calc.HexToColor("330ba2"),
        Calc.HexToColor("2a0c95"),
        Calc.HexToColor("200c88"),
        Calc.HexToColor("170b7c"),
        Calc.HexToColor("0d0a6f"),
        Calc.HexToColor("170b7c"),
        Calc.HexToColor("200c88"),
        Calc.HexToColor("2a0c95"),
        Calc.HexToColor("330ba2"),
        Calc.HexToColor("3e09af"),
        Calc.HexToColor("4805bb"),
    ];
    
    private static int ColorIndex;
    private static void CreateAngelTrail(this Player player)
    {
        ColorIndex++;
        ColorIndex %= GradientColors.Length;
        
        Vector2 scale = new Vector2(Math.Abs(player.Sprite.Scale.X) * (float)player.Facing, player.Sprite.Scale.Y);
        TrailManager.Add(player, scale, GradientColors[ColorIndex]);
    }
    
    #endregion
}