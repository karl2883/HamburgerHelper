using System.Collections;

namespace Celeste.Mod.HamburgerHelper.Components;

public class CursorComponent : Sprite
{
    private bool HoldingPlayer = false;

    public CursorComponent()
        : base(GFX.Game, "characters/player/hamburger/")
    {
        AddLoop("holding", "cursor", 0.08f);
        AddLoop("released", "pointer", 0.08f);
        
        Visible = false;
    }
    
    public void Pickup()
    {
        Visible = true;
        Color = Color.White;
        
        Play("holding");
        HoldingPlayer = true;
    }
    
    public void Release()
    {
        Play("released");
        HoldingPlayer = false;
    }

    public override void Update()
    {
        base.Update();

        const float transitionSpeed = 1024f;
        
        if (!HoldingPlayer)
        {
            Color.A = (byte)Calc.Approach(Color.A, Color.Transparent.A, transitionSpeed * Engine.DeltaTime);
            Color.R = (byte)Calc.Approach(Color.R, Color.Transparent.R, transitionSpeed * Engine.DeltaTime);
            Color.G = (byte)Calc.Approach(Color.G, Color.Transparent.G, transitionSpeed * Engine.DeltaTime);
            Color.B = (byte)Calc.Approach(Color.B, Color.Transparent.B, transitionSpeed * Engine.DeltaTime);   
        }
        
        if (Color.A == Color.Transparent.A)
        {
            Visible = false;
        }
    }
}
