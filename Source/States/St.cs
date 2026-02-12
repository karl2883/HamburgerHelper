namespace Celeste.Mod.HamburgerHelper.States;

public static class St
{
    public static int Cursor { get; private set; } = -1;
    
    internal static void Load()
    {
        Everest.Events.Player.OnRegisterStates += OnRegisterStates;
        
        States.Cursor.Load();
    }

    internal static void Unload()
    {
        States.Cursor.Unload();
    }
    
    private static void OnRegisterStates(Player player)
    {
        Cursor = player.AddState("Cursor", 
            States.Cursor.CursorUpdate, States.Cursor.CursorRoutine, 
            States.Cursor.CursorBegin, States.Cursor.CursorEnd);
    }
}