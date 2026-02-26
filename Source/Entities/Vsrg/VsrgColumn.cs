using Celeste.Mod.HamburgerHelper.Entities.Vsrg.HitObjects;

namespace Celeste.Mod.HamburgerHelper.Entities.Vsrg;

public class VsrgColumn : Entity
{
    private readonly int Column;
    private readonly ButtonBinding InputKey;

    private readonly Sprite ColumnSprite;
    private readonly VsrgManager Manager;
    
    public List<HitObject> PastHitObjects = [];
    public PriorityQueue<HitObject, float> HitObjects = new PriorityQueue<HitObject, float>();

    private HitObject NextHitObject => HitObjects.Count > 0 ? HitObjects.Peek() : null;
    private LongNote LastLongNote;
    
    public VsrgColumn(Vector2 position, int column, VsrgManager manager)
        : base(position)
    {
        Column = column;
        Manager = manager;
        
        ColumnSprite = new Sprite(GFX.Game, "objects/hamburger/vsrg/column/");
        ColumnSprite.Add("fadeIn", "fadeIn", 0.0333f, "hold");
        ColumnSprite.AddLoop("hold", "hold", 0.0333f);
        ColumnSprite.Add("fadeOut", "fadeOut", 0.0333f, "idle");
        ColumnSprite.AddLoop("idle", "idle", 0.0333f);
        ColumnSprite.Play("idle");
        
        Position.X = GetXFromColumn(Column);

        InputKey = column switch {
            1 => HamburgerHelperModule.Settings.VsrgColumn1,
            2 => HamburgerHelperModule.Settings.VsrgColumn2,
            3 => HamburgerHelperModule.Settings.VsrgColumn3,
            4 => HamburgerHelperModule.Settings.VsrgColumn4,
            _ => null
        };
    }
    
    public override void Update()
    {
        base.Update();

        if (NextHitObject == null) return;
        
        double offset = NextHitObject.TimingMs - Manager.Timing.SongPosition;
        if (VsrgJudgements.MissedNote(offset))
        {
            HitObject nextObj = HitObjects.Dequeue();
            nextObj.IsHit = true;
            
            Manager.Judgements.Add(Judgement.Miss);
            if (nextObj is LongNote ln)
            {
                ln.IsMissed = true;
                ln.IsReleased = true;
                ln.EarlyReleased = false;
                
                Manager.Judgements.Add(Judgement.Miss);
            }
        }
        
        ColumnSprite.Update();
        
        if (InputKey.Pressed)
        {
            ColumnSprite.Play("fadeIn");
            TryHitNote();
        }

        if (InputKey.Check)
        {
        }

        if (InputKey.Released)
        {
            ColumnSprite.Play("fadeOut");
            TryReleaseNote();
        }
    }

    private void TryHitNote()
    {
        double offset = NextHitObject.TimingMs - Manager.Timing.SongPosition;

        if (!VsrgJudgements.CanHitNote(offset, NextHitObject.NoteType))
            return;
        
        if (HitObjects.Count > 0)
        {
            HitObject hitObj = HitObjects.Dequeue();
            hitObj.IsHit = true;
            
            if (hitObj is LongNote ln)
            {
                ln.IsHeld = true;
                LastLongNote = ln;
            }
            
            Judgement judgement = VsrgJudgements.JudgementAtOffset(offset, NextHitObject.NoteType);
            Manager.Judgements.Add(judgement);
        }
    }

    private void TryReleaseNote()
    {
        if (LastLongNote == null) return;
        
        double endingOffset = LastLongNote.EndingMs - Manager.Timing.SongPosition;
        if (!VsrgJudgements.CanHitNote(endingOffset, NoteType.LongNote))
        {
            LastLongNote.IsReleased = true;
            LastLongNote.EarlyReleased = true;
            Manager.Judgements.Add(Judgement.Great);
        }
        else
        {
            LastLongNote.IsReleased = true;
            LastLongNote.EarlyReleased = false;
            Manager.Judgements.Add(Judgement.Marvelous);
        }

        LastLongNote = null;
    }
    
    public void Draw()
    {
        Color spriteColor = GetColumnColor() * 0.5f * Manager.GameFade;
        spriteColor.A = 0;
        
        ColumnSprite?.GetFrame(ColumnSprite.CurrentAnimationID, ColumnSprite.CurrentAnimationFrame)
            .Draw(Position, Vector2.Zero, spriteColor);
    }
    
    private int GetXFromColumn(int column)
    {
        return column switch {
            1 => (int)Position.X + 116,
            2 => (int)Position.X + 139,
            3 => (int)Position.X + 162,
            4 => (int)Position.X + 185,
            _ => (int)Position.X,
        };
    }
    
    private Color GetColumnColor()
    {
        return Column switch {
            1 or 2 => Calc.HexToColor("001523"),
            3 or 4 => Calc.HexToColor("270017"),
            _ => Color.White
        };
    }
}
