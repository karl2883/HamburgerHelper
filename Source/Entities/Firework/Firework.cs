using System.Collections;
using FMOD.Studio;

namespace Celeste.Mod.HamburgerHelper.Entities.Firework;

[CustomEntity("HamburgerHelper/Firework")]
public class Firework : Entity
{
    private class FireworkIndicator : Entity
    {
        private readonly MTexture IndicatorTexture;

        public FireworkIndicator(Vector2 position) 
            : base(position)
        {
            const string indicatorPath = "objects/hamburger/firework/indicator";
            IndicatorTexture = GFX.Game[indicatorPath];

            Collider = new Hitbox(1f, 1f);
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);

            if (CollideCheck<Solid>())
            {
                Depth = Depths.FGTerrain - 3000;
            }
            else
            {
                Depth = Depths.BGDecals - 1;   
            }
        }

        public override void Render()
        {
            base.Render();
            
            IndicatorTexture.DrawCentered(Position, IndicatorColor);
        }
    }
    
    private enum Directions
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3,
    }

    private static readonly ParticleType TrailParticles = new ParticleType() 
    {
        Size = 1f,
        LifeMin = 0.25f,
        LifeMax = 0.5f,
        Color = Calc.HexToColor("#4A4A4A"),
    };
    
    private bool Launching = false;
    private readonly float LaunchTime;
    private readonly float LaunchSpeed;
    private static Color IndicatorColor => Calc.HexToColor("5d5d5d");

    private readonly Directions Direction;
    private readonly MTexture FireworkTexture;
    private readonly Vector2 StartPosition;

    private readonly bool SidesOnly;
    private readonly bool SnapUp;

    private EventInstance LaunchSound;
    private FireworkIndicator Indicator;
    
    public Firework(EntityData data, Vector2 offset) 
        : base(data.Position + offset)
    {
        StartPosition = data.Position + offset;
        
        Direction = data.Enum("direction", Directions.Up);
        
        LaunchTime = data.Float("launchTime", 0.5f);
        LaunchSpeed = data.Float("launchSpeed", 2f);

        SidesOnly = data.Bool("sidesOnly", false);
        SnapUp = data.Bool("snapUp", false);
        
        const string launchPath = "objects/hamburger/firework/";
        string fireworkPath = data.Attr("fireworkSprite", "launchFirework");
        FireworkTexture = GFX.Game[launchPath + fireworkPath];

        Depth = Depths.FGTerrain - 3000;

        // this collider is used exclusively to get the center position
        SetColliderForDirection();
    }

    public override void Update()
    {
        base.Update();

        if (Launching) return;
        
        List<Entity> fuses = Scene.Tracker.GetEntitiesTrackIfNeeded<Fuse>();
        foreach (Entity entity in fuses)
        {
            if (entity is not Fuse fuse) continue;
            if (!fuse.IsLit) continue;

            Vector2? sparkPosition = fuse.SparkPosition;
            if (!sparkPosition.HasValue) continue;

            if (CollidePoint(sparkPosition.Value))
            {
                Launch();
            }
        }
    }

    public override void Render()
    {
        base.Render();
        
        float angle = Direction switch {
            Directions.Up => MathHelper.ToRadians(-90f),
            Directions.Down => MathHelper.ToRadians(90f),
            Directions.Left => MathHelper.ToRadians(180f),
            Directions.Right => 0f,
            _ => 0f
        };
        
        FireworkTexture.DrawCentered(Position, Color.White, Vector2.One, angle);
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);
        
        Indicator = new FireworkIndicator(GetIndicatorPosition());
        scene.Add(Indicator);
    }
    
    private void Launch()
    {
        Launching = true;
        
        Add(new Coroutine(LaunchRoutine()));
    }
    
    private IEnumerator LaunchRoutine()
    {
        if (Scene is not Level level) yield break; 

        LaunchSound = Audio.Play("event:/HamburgerHelper/sfx/firework_launch", Center);
        
        float dur = 0f;
        while (dur < LaunchTime)
        {
            dur += Engine.DeltaTime;

            Vector2 movementOffset = Direction switch {
                Directions.Up => -Vector2.UnitY,
                Directions.Down => Vector2.UnitY,
                Directions.Left => -Vector2.UnitX,
                Directions.Right => Vector2.UnitX,
                _ => Vector2.UnitX
            };
            
            movementOffset *= LaunchSpeed * 60f * Engine.DeltaTime;
            Position += movementOffset;

            if (Scene.OnInterval(0.025f))
            {
                level.ParticlesBG.Emit(TrailParticles, 1, Center, Vector2.One * 2f);
            }
            
            yield return null;
        }

        if (Audio.IsPlaying(LaunchSound))
        {
            Audio.Stop(LaunchSound);
            LaunchSound = null;
        }
        
        Explode();
    }

    private void Explode()
    {
        if (Scene is not Level level) return; 
        
        Player player = level.Tracker.GetEntity<Player>();
        if (player == null) return;

        Collider = null;
        Collider = new Circle(48f);
        
        foreach (Entity entity in Scene.Tracker.GetEntities<TempleCrackedBlock>())
        {
            TempleCrackedBlock tcb = (TempleCrackedBlock)entity;
            if (CollideCheck(tcb))
            {
                tcb.Break(Position);
            }
        }
        foreach (Entity entity in Scene.Tracker.GetEntitiesTrackIfNeeded<DashBlock>())
        {
            DashBlock db = (DashBlock)entity;
            if (CollideCheck(db))
            {
                db.Break(db.Center, db.Center, true, true);
            }
        }
        foreach (Entity entity in Scene.Tracker.GetEntities<TouchSwitch>())
        {
            TouchSwitch ts = (TouchSwitch)entity;
            if (CollideCheck(ts))
            {
                ts.TurnOn();
            }
        }
        foreach (Entity entity in Scene.Tracker.GetEntities<FloatingDebris>())
        {
            FloatingDebris fd = (FloatingDebris)entity;
            if (CollideCheck(fd))
            {
                fd.OnExplode(Position);
            }
        }

        Collider = null;

        // this was ripped from Celeste.Puffer
        level.Shake();
        level.Displacement.AddBurst(Position, 0.4f, 12f, 36f, 0.5f);
        level.Displacement.AddBurst(Position, 0.4f, 24f, 48f, 0.5f);
        level.Displacement.AddBurst(Position, 0.4f, 36f, 60f, 0.5f);
        for (float num = 0f; num < MathF.PI * 2f; num += 0.17453292f)
        {
            Vector2 position = Center + Calc.AngleToVector(num + Calc.Random.Range(-MathF.PI / 90f, MathF.PI / 90f), Calc.Random.Range(12, 18));
            level.Particles.Emit(Seeker.P_Regen, position, num);
        }
        
        float playerDistance = Vector2.Distance(player.Center, Center);
        if (playerDistance <= 48f)
        {
            player.ExplodeLaunch(Position, SnapUp, SidesOnly);   
        }

        Audio.Play("event:/HamburgerHelper/sfx/firework_explode", Position);
        Indicator.RemoveSelf();
        RemoveSelf();
    }
    
    private Vector2 GetIndicatorPosition()
    {
        Vector2 movementOffset = Direction switch {
            Directions.Up => -Vector2.UnitY,
            Directions.Down => Vector2.UnitY,
            Directions.Left => -Vector2.UnitX,
            Directions.Right => Vector2.UnitX,
            _ => Vector2.UnitX
        };

        return StartPosition + movementOffset * LaunchSpeed * LaunchTime * 60f;
    }
    
    private void SetColliderForDirection()
    {
        Collider = Direction switch {
            Directions.Up => new Hitbox(8, 8, -4, 0),
            Directions.Down => new Hitbox(8, 8, -3, -8),
            Directions.Left => new Hitbox(8, 8, 0, -4),
            Directions.Right => new Hitbox(8, 8, -8, -4),
            _ => Collider
        };
    }
}