using System.Text.RegularExpressions;
using Celeste.Mod.Helpers;
using MonoMod.Cil;

namespace Celeste.Mod.HamburgerHelper.Entities;

[Tracked]
[CustomEntity("HamburgerHelper/StickyWalls")]
public class StickyWalls : Entity
{
    private readonly Facings Facing;
    private List<Sprite> Tiles;
    
    private readonly Color Color;
    private readonly string SpritePath;
    
    private static bool StuckToWall = false;
    
    public StickyWalls(EntityData data, Vector2 offset) 
        : base(data.Position + offset)
    {
        bool left = data.Bool("left", false);
        float height = data.Height;
        
        Tag = Tags.TransitionUpdate;
        Depth = 1999;
        
        if (left)
        {
            Facing = Facings.Left;
            Collider = new Hitbox(2f, height);
        }
        else
        {
            Facing = Facings.Right;
            Collider = new Hitbox(2f, height, 6f);
        }
        
        Color = Calc.HexToColor(data.Attr("color", "FF0000"));
        SpritePath = data.Attr("spritePath", "hamburger/stickywall");
        Tiles = BuildSprite(left);
    }
    
    private List<Sprite> BuildSprite(bool left)
    {
        List<Sprite> sprites = [];
        
        for (int i = 0; i < Height; i += 8)
        {
            string id = (i == 0) ? "top" : !((i + 16) > Height) ? "middle" : "bottom";
            Sprite sprite = new Sprite(GFX.Game, $"objects/{SpritePath}/");
            sprite.AddLoop("idle", id, 0.08f);
            sprite.Play("idle");
            
            if (!left)
            {
                sprite.FlipX = true;
            }
            
            sprite.Position = new Vector2(0f, i);
            sprite.Color = Color;
            
            sprites.Add(sprite);
            Add(sprite);
        }
        
        return sprites;
    }
    
    public override void Update()
    {
        base.Update();
        
        if (Scene is not Level level) return;
        
        Player player = level.Tracker.GetEntity<Player>();
        
        List<Entity> collidedWalls = player?.CollideAll<StickyWalls>();
        if (collidedWalls == null) return;
        
        if (collidedWalls.Count > 0)
        {
            StuckToWall = true;
            
            player.Facing = ((StickyWalls)collidedWalls[0]).Facing;
            player.RefillStamina();
            
            if (player.StateMachine.State != Player.StClimb && player.StateMachine.State != Player.StDash)
            {
                player.StateMachine.State = Player.StClimb;   
            }
        }
        else
        {
            StuckToWall = false;
        }
    }

    internal static void Load()
    {
        IL.Celeste.Player.ClimbUpdate += PlayerOnClimbUpdate;
    }

    internal static void Unload()
    {
        IL.Celeste.Player.ClimbUpdate -= PlayerOnClimbUpdate;
    }
    
    private static void PlayerOnClimbUpdate(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);

        ILLabel jumpLabel = null;
        
        /*
         * IL_008d: call bool Celeste.Input::get_GrabCheck()
         * IL_0092: brtrue.s IL_00bf
         */
        if (!cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchCall(typeof(Input), "get_GrabCheck"),
            instr => instr.MatchBrtrue(out jumpLabel)))
            throw new HookUtilities.HookException("Unable to find get_GrabCheck.");
        
        cursor.EmitDelegate(CheckStucktoWall);
        cursor.EmitBrtrue(jumpLabel);
        
        ILLabel jumpLabel2 = null;

        /*
         * IL_0271: br IL_0356
         *
         * IL_0276: ldsfld class Monocle.VirtualIntegerAxis Celeste.Input::MoveY
         * IL_027b: ldfld int32 Monocle.VirtualIntegerAxis::Value
         * IL_0280: ldc.i4.m1
         * IL_0281: bne.un IL_02fd
         */
        if (!cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchBr(out ILLabel uselessLabel),
            instr => instr.MatchLdsfld(typeof(Input), "MoveY"),
            instr => instr.MatchLdfld<VirtualIntegerAxis>("Value"),
            instr => instr.MatchLdcI4(out int uselessInt),
            instr => instr.MatchBneUn(out jumpLabel2)))
            throw new HookUtilities.HookException("Unable to find climbup.");
        
        cursor.EmitDelegate(CheckStucktoWall);
        cursor.EmitBrtrue(jumpLabel2);
        
        ILLabel jumpLabel3 = null;
        
        /*
         * IL_02fd: ldsfld class Monocle.VirtualIntegerAxis Celeste.Input::MoveY
         * IL_0302: ldfld int32 Monocle.VirtualIntegerAxis::Value
         * IL_0307: ldc.i4.1
         * IL_0308: bne.un.s IL_0350
         */
        if (!cursor.TryGotoNextBestFit(MoveType.After,
            instr => instr.MatchLdsfld(typeof(Input), "MoveY"),
            instr => instr.MatchLdfld<VirtualIntegerAxis>("Value"),
            instr => instr.MatchLdcI4(out int uselessValue),
            instr => instr.MatchBneUn(out jumpLabel3)))
            throw new HookUtilities.HookException("Unable to find climbdown.");
        
        cursor.EmitDelegate(CheckStucktoWall);
        cursor.EmitBrtrue(jumpLabel3);
        
        return;
        
        static bool CheckStucktoWall()
        {
            return StuckToWall;
        }
    }
}
