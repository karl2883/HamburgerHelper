using System.Collections;
using FMOD.Studio;

namespace Celeste.Mod.HamburgerHelper.Entities.Firework;

[CustomEntity("HamburgerHelper/Fuse")]
// ReSharper disable once ClassNeverInstantiated.Global
public class Fuse : Entity
{
    private readonly List<Vector2> Nodes;
    private Vector2 FuseHead => Nodes[0];
    
    private readonly Sprite FuseSprite;
    private readonly Color FuseColor;
    private static Color FuseTailColor => Calc.HexToColor("a63333");
    
    private readonly float FuseSpeed;
    private readonly bool PlayerIgnite;
    
    public Vector2? SparkPosition;
    public bool IsLit = false;
    private bool LitByOther = false;

    private const float BufferTime = 4 / 60f;
    private float BufferTimer;
    private bool Buffering => BufferTimer > 0f;

    private static EventInstance FuseAmbience;
    
    private static bool CanLight
    {
        get => HamburgerHelperModule.Session.CanLight; 
        set => HamburgerHelperModule.Session.CanLight = value;
    }
    
    public Fuse(EntityData data, Vector2 offset) 
        : base(data.Position + offset)
    {
        Nodes = [.. data.NodesWithPosition(offset)];
        
        FuseSpeed = data.Float("fuseSpeed", 3f);
        PlayerIgnite = data.Bool("playerIgnite", true);
        
        FuseColor = data.HexColor("fuseColor", Calc.HexToColor("a65959"));
        
        const string fusePath = "objects/hamburger/firework/";
        FuseSprite = new Sprite(GFX.Game, fusePath);
        FuseSprite.AddLoop("idle", "fuseEnd", 50);
        FuseSprite.AddLoop("invis", "invis", 50);
        FuseSprite.AddLoop("lit", "fuseLit", 0.032f);
        
        Vector2 fuseOrigin = new Vector2(4, 4);
        FuseSprite.Origin = fuseOrigin;
        FuseSprite.Color = FuseTailColor;
        
        string animation = PlayerIgnite ? "idle" : "invis";
        FuseSprite.Play(animation);
        FuseSprite.Visible = false;
        
        Add(FuseSprite);
        
        Depth = Depths.FGTerrain - 3000;
        
        const float colliderWidth = 32f;
        const float colliderHeight = 32f;
        
        const float offsetX = -(colliderWidth / 2f);
        const float offsetY = -(colliderHeight / 2f);
        Collider = new Hitbox(colliderWidth, colliderHeight, offsetX, offsetY);
    }

    public override void Update()
    {
        base.Update();
        
        if (IsLit) return;
        if (Scene is not Level level) return;
        
        Player player = level.Tracker.GetEntity<Player>();
        if (player == null) return;

        if (BufferTimer > 0f) BufferTimer -= Engine.DeltaTime;
        
        if (Input.Grab.Pressed)
        {
            BufferTimer = BufferTime;
        }
        
        switch (IsLit)
        {
            case false when PlayerIgnite: {
                if (CollideCheck<Player>() && Buffering && CanLight)
                {
                    Audio.Play("event:/HamburgerHelper/sfx/lighter_light", Center);
                    Add(new Coroutine(PlayAudioRoutine()));
                    
                    Ignite();
                }
                break;
            }
            case false when !PlayerIgnite: {
                List<Entity> fuses = Scene.Tracker.GetEntitiesTrackIfNeeded<Fuse>();
                foreach (Entity entity in fuses)
                {
                    if (entity is not Fuse fuse) continue;
                    if (!fuse.IsLit) continue;

                    Vector2? sparkPosition = fuse.SparkPosition;
                    if (!sparkPosition.HasValue) continue;
                
                    float sparkDistance = Vector2.Distance(FuseHead, sparkPosition.Value);
                    if (!(sparkDistance <= 2f))
                        continue;

                    LitByOther = true;
                    Ignite();
                }
                break;
            }
        }
    }

    public override void Render()
    {
        base.Render();
        
        for (int i = 0; i < Nodes.Count - 1; i++)
        {
            Vector2 currentNode = Nodes[i];
            Vector2 nextNode = Nodes[i + 1];

            Vector2[] outlineOffsets = [
                new Vector2(0, 1),
                new Vector2(0, -1),
                new Vector2(1, 0),
                new Vector2(-1, 0),
            ];

            foreach (Vector2 offset in outlineOffsets)
            {
                Draw.Line(currentNode + offset, nextNode + offset, Color.Black);
            }
            
            Draw.Line(currentNode, nextNode, FuseColor);
        }

        if (Nodes.Count < 2)
            return;

        if (!PlayerIgnite && !IsLit)
            return;
        
        Vector2 startNode = Nodes[0];
        Vector2 afterStartNode = Nodes[1];
        float fuseAngle = Calc.Angle(afterStartNode, startNode);
        
        FuseSprite.Rotation = fuseAngle;
            
        float offsetX = FuseHead.X - Position.X;
        float offsetY = FuseHead.Y - Position.Y;
        FuseSprite.Position = new Vector2(offsetX, offsetY);
        
        // rendering is done here to place it over the fuse line
        FuseSprite.Render();
    }

    private void Ignite()
    {
        if (IsLit) return;
        if (!CanLight && !LitByOther) return;
        
        Collider = null;

        if (!LitByOther) CanLight = false;
        
        IsLit = true;
        FuseSprite.Color = Color.White;
        
        Add(new Coroutine(FuseRoutine()));
    }

    private IEnumerator PlayAudioRoutine()
    {
        yield return 0.8f;

        if (FuseAmbience == null)
        {
            FuseAmbience = Audio.Play("event:/HamburgerHelper/sfx/fuse_sparkler", Center);   
        }
    }
    
    private IEnumerator FuseRoutine()
    {
        FuseSprite.Play("lit");
        
        while (Nodes.Count > 1)
        {
            Vector2 segmentStart = Nodes[0];
            Vector2 segmentEnd = Nodes[1];
        
            float segmentLength = Vector2.Distance(segmentStart, segmentEnd);
            float segmentTime = segmentLength / (FuseSpeed * 60f);
            float elapsed = 0f;
        
            while (elapsed < segmentTime)
            {
                elapsed += Engine.DeltaTime;
                float t = Calc.Clamp(elapsed / segmentTime, 0f, 1f);
            
                SparkPosition = Vector2.Lerp(segmentStart, segmentEnd, t);
                Nodes[0] = SparkPosition.Value;
            
                yield return null;
            }
        
            Nodes.RemoveAt(0);
        }
    
        Audio.Stop(FuseAmbience);
        FuseAmbience = null;
        
        SparkPosition = null;
        
        RemoveSelf();
    }
}
