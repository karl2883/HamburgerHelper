using System.Runtime.InteropServices;
using SDL2;

namespace Celeste.Mod.HamburgerHelper.Utils;

public static class WindowUtils
{
    private static MTexture LastWindowIcon;
    private static string LastWindowTitle = "Celeste";
    
    // i originally made this for ssc4, but i think it's quite useful and i'm happy with it, so why not
    /// <summary>
    /// Sets the window icon given an MTexture and a pointer to the Window Handle
    /// </summary>
    /// <param name="window">Engine.Instance.Window.Handle</param>
    /// <param name="texture">MTexture to set the window icon to</param>
    /// <param name="fromDisabledSetting">Whether or not the icon is being changed from an icon reset</param>
    public static void SetWindowIconFromMTexture(nint window, MTexture texture, bool fromDisabledSetting = false)
    {
        Texture2D tex = texture.Texture.Texture_Safe;
        if (tex == null)
            return;
        
        if (!fromDisabledSetting)
            LastWindowIcon = texture;
        
        Color[] pixels = new Color[tex.Height * tex.Width];
        tex.GetData(pixels);
        
        GCHandle handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        
        try
        {
            nint dataPtr = handle.AddrOfPinnedObject();
            nint surface = SDL.SDL_CreateRGBSurfaceFrom(dataPtr, 
                tex.Width, tex.Height, 
                32, tex.Width * 4,
                0x000000FF, 0x0000FF00, 0x00FF0000, 0xFF000000);
            
            if (surface == nint.Zero)
                return;
            
            SDL.SDL_SetWindowIcon(window, surface);
            SDL.SDL_FreeSurface(surface);
        }
        finally
        {
            handle.Free();
        }
    }
    
    public static void SetWindowTitle(string title, bool fromDisabledSetting = false)
    {
        if (!fromDisabledSetting)
            LastWindowTitle = title;
        
        Engine.Instance.Window.Title = title;
    }
    
    // the goal of this is to reset icon & title if a mapper changes that and does not clean up after
    // i could prooooobably make exceptions for if people don't want it to remain
    
    // if this becomes a problem for someone later, i can adjust it
    // probably by marking if a scene changed title / icon, then resetting properly
    private static void ResetWindowIconAndTitle(On.Celeste.Level.orig_End orig, Level self)
    {
        orig(self);
        
        // logging my crimes to make sure if it needs to be found, it can be
        Logger.Log(LogLevel.Verbose, "HamburgerHelper", "Reset window title & icon");
        Logger.Log(LogLevel.Verbose, "HamburgerHelper", "If you're another mod developer, and this is a problem, contact me so I can avoid problems in the future");
        
        ResetWindowIcon();
        ResetWindowTitle();
    }
    
    private static void ResetWindowIcon(bool fromDisabledSetting = false)
    {
        MTexture defaultIcon = GFX.Game["hamburger/icons/strawberrysdl"];
        nint windowHandle = Engine.Instance.Window.Handle;
        
        SetWindowIconFromMTexture(windowHandle, defaultIcon, fromDisabledSetting);
    }
    
    private static void ResetWindowTitle(bool fromDisabledSetting = false)
    {
        SetWindowTitle("Celeste", fromDisabledSetting);
    }
    
    public static void UpdateWindowIconSetting(bool value)
    {
        if (!value)
        {
            nint windowHandle = Engine.Instance.Window.Handle;
            
            SetWindowIconFromMTexture(windowHandle, LastWindowIcon);
            return;
        }
        
        ResetWindowIcon(fromDisabledSetting: true);
    }
    
    public static void UpdateWindowTitleSetting(bool value)
    {
        if (!value)
        {
            SetWindowTitle(LastWindowTitle);
            return;
        }
        
        ResetWindowTitle(fromDisabledSetting: true);
    }
    
    [OnLoad]
    internal static void Load()
    {
        On.Celeste.Level.End += ResetWindowIconAndTitle;
    }
    
    [OnUnload]
    internal static void Unload()
    {
        On.Celeste.Level.End -= ResetWindowIconAndTitle;
    }
}