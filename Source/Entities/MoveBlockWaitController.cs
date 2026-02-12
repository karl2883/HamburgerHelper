using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace Celeste.Mod.HamburgerHelper.Entities;

[Tracked]
[CustomEntity("HamburgerHelper/MoveBlockWaitController")]
// ReSharper disable once ClassNeverInstantiated.Global

public class MoveBlockWaitController : Entity
{
    private static ILHook ilMoveBlockController;
    
    private readonly float Delay;
    
    /// <summary>
    /// This entity really is a single IL hook, it only exists for the hook to check if it's real
    /// </summary>
    public MoveBlockWaitController(EntityData data, Vector2 offset) 
        : base(data.Position + offset)
    {
        Delay = data.Float("delay", 0f);
    }
    
    internal static void Load()
    {
        ilMoveBlockController = new ILHook(
            typeof(MoveBlock).GetMethod("Controller", 
                BindingFlags.NonPublic | BindingFlags.Instance)!.GetStateMachineTarget()!,
            ModController);
    }
    
    internal static void Unload()
    {
        ilMoveBlockController.Dispose();
        ilMoveBlockController = null;
    }
    
    private static void ModController(ILContext il)
    {
        ILCursor cursor = new ILCursor(il);
        
        /*
         * IL_00a1: ldarg.0
         * IL_00a2: ldc.r4 0.2
         */
        if (!cursor.TryGotoNext(MoveType.After,
            i => i.MatchLdarg0(),
            i => i.MatchLdcR4(0.2f)))
            throw new HookUtilities.HookException(il, "MoveBlock.Controller ilhook failed at find yield return");
        
        cursor.EmitLdloc1();
        cursor.EmitDelegate(ModStartTime);
    }
    
    private static float ModStartTime(float orig, MoveBlock block)
    {
        MoveBlockWaitController controller = block?.Scene?.Tracker?.GetEntity<MoveBlockWaitController>();
        return controller?.Delay ?? orig;
    }
}