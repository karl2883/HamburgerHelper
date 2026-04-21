namespace Celeste.Mod.HamburgerHelper.Entities;

[CustomEntity("HamburgerHelper/MultiTextureDecal")]
public class MultiTextureDecal : Entity
{
    private readonly string[] Textures;
    
    private readonly float ScaleX;
    private readonly float ScaleY;

    private readonly Color Color;
    private readonly float Rotation;

    private readonly bool Foreground;
    
    public MultiTextureDecal(EntityData data, Vector2 offset) 
        : base(data.Position + offset)
    {
        Textures = data.Attr("textures").Split(',');
        
        ScaleX = data.Float("scaleX", 1f);
        ScaleY = data.Float("scaleY", 1f);
        
        Rotation = data.Float("rotation", 0f);
        Color = data.HexColor("color", Color.White);
        
        Foreground = data.Bool("foreground", true);
        Depth = data.Int("depthOffset", 0); 
    }

    public override void Added(Scene scene)
    {
        base.Added(scene);

        if (scene is not Level level) return;

        int levelInt = level.Session.GetHashCode();
        int seed = (int)Math.Floor(X) + (int)Math.Floor(Y) + levelInt;
        Random random = new Random(seed);
        
        int textureIndex = random.Next(0, Textures.Length);
        string texturePath = Textures[textureIndex];
        
        Vector2 scale = new Vector2(ScaleX, ScaleY);

        const int fgDepth = -10500;
        const int bgDepth = 9000;
        int realDepth = Depth + (Foreground ? fgDepth : bgDepth);
        
        Decal decal = new Decal(texturePath, Position, scale, realDepth, Rotation, Color);
        Scene.Add(decal);
        
        RemoveSelf();
    }
}
