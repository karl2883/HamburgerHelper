using System.Collections;
using FMOD.Studio;

namespace Celeste.Mod.HamburgerHelper.Entities.Firework;

[CustomEntity("HamburgerHelper/Firework")]
public class Firework : Entity
{
    private enum Directions
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3,
    }
    
    private bool Launching = false;
    private readonly float LaunchTime;
    private readonly float LaunchSpeed;
    private static Color IndicatorColor => Calc.HexToColor("5d5d5d");

    private readonly Directions Direction;

    private readonly MTexture FireworkTexture;
    private readonly MTexture IndicatorTexture;

    private readonly Vector2 StartPosition;

    private EventInstance LaunchSound;
    
    public Firework(EntityData data, Vector2 offset) 
        : base(data.Position + offset)
    {
        StartPosition = data.Position + offset;
        
        Direction = data.Enum("direction", Directions.Up);
        
        LaunchTime = data.Float("launchTime", 0.5f);
        LaunchSpeed = data.Float("launchSpeed", 2f);
        
        const string launchPath = "objects/hamburger/firework/";
        string fireworkPath = data.Attr("fireworkSprite", "launchFirework");
        FireworkTexture = GFX.Game[launchPath + fireworkPath];
        
        const string indicatorPath = "objects/hamburger/firework/indicator";
        IndicatorTexture = GFX.Game[indicatorPath];

        Depth = Depths.FGTerrain - 25;

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
        
        Vector2 destination = GetIndicatorPosition();
        IndicatorTexture.DrawCentered(destination, IndicatorColor);
        
        FireworkTexture.DrawCentered(Position, Color.White, Vector2.One, angle);
    }

    private void Launch()
    {
        Launching = true;
        
        Add(new Coroutine(LaunchRoutine()));
    }
    
    private IEnumerator LaunchRoutine()
    {
        if (Scene is not Level level) yield break; 
        
        Player player = level.Tracker.GetEntity<Player>();
        if (player == null) yield break;

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
            bool snapUp = Direction is Directions.Up or Directions.Down;
            bool sidesOnly = Direction is Directions.Left or Directions.Right;
            player.ExplodeLaunch(Position, snapUp, sidesOnly);   
        }

        Audio.Play("event:/HamburgerHelper/sfx/firework_explode", Position);
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