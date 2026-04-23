namespace Celeste.Mod.HamburgerHelper.States;

public static class St
{
    public static int Cursor { get; private set; } = 0;
    public static int DarkAngel { get; private set; } = 0;
    
    [OnLoad]
    internal static void Load()
    {
        Everest.Events.Player.OnRegisterStates += OnRegisterStates;
    }

    [OnUnload]
    internal static void Unload()
    {
    }
    
    private static void OnRegisterStates(Player player)
    {
        Cursor = player.AddState("Cursor", 
            States.Cursor.CursorUpdate, States.Cursor.CursorRoutine, 
            States.Cursor.CursorBegin, States.Cursor.CursorEnd);
        
        DarkAngel = player.AddState("DarkAngel",
            States.DarkAngel.AngelUpdate, States.DarkAngel.AngelRoutine,
            States.DarkAngel.AngelBegin, States.DarkAngel.AngelEnd);
    }
}