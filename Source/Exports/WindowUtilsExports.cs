using MonoMod.ModInterop;

namespace Celeste.Mod.HamburgerHelper.Exports;

[ModExportName("HamburgerHelper.WindowUtils")]
public class WindowUtilsExports
{
    public static int WindowModInteropVersion => 1;
    
    public static void SetWindowIconFromMTexture(nint window, MTexture texture) => 
        WindowUtils.SetWindowIconFromMTexture(window, texture, false);
    public static void SetWindowTitle(string title) =>
        WindowUtils.SetWindowTitle(title, false);
}
