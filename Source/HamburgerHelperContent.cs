using System.IO;

namespace Celeste.Mod.HamburgerHelper;

public static class HamburgerHelperContent
{
    public static string[] GetLinesFromPath(string filePath)
    {
        string[] lines;
        
        if (Everest.Content.TryGet(filePath, out ModAsset file))
        {
            using StreamReader reader = new StreamReader(file.Stream);
            lines = reader.ReadToEnd().Split('\n');
        }
        else
        {
            Logger.Log(LogLevel.Info, "DEBUG", $"Failed to find file ({filePath})");
            return null;   
        }
        
        return lines;
    }
}