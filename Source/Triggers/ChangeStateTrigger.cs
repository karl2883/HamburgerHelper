namespace Celeste.Mod.HamburgerHelper.Triggers;

[CustomEntity("HamburgerHelper/ChangeStateTrigger")]
public class ChangeStateTrigger : Trigger
{
    private readonly bool OnlyOnce;
    private readonly bool TriggerOnce;
    
    private readonly int State;

    private readonly EntityID Id;
    
    public ChangeStateTrigger(EntityData data, Vector2 offset, EntityID id)
        : base(data, offset)
    {
        OnlyOnce = data.Bool("onlyOnce", false);
        TriggerOnce = data.Bool("triggerOnlyOnce", true);
        
        State = data.Int("state", 0);

        Id = id;
    }

    public override void OnEnter(Player player)
    {
        base.OnEnter(player);

        if (Scene is not Level level) return;
        
        player.StateMachine.State = State;

        if (TriggerOnce)
        {
            RemoveSelf();
        }

        // ReSharper disable once InvertIf
        if (OnlyOnce)
        {
            level.Session.SetFlag("DoNotLoad" + Id);
            level.Session.DoNotLoad.Add(Id);
            
            RemoveSelf();
        }
    }
}
