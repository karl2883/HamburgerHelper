namespace Celeste.Mod.HamburgerHelper.Entities.Vsrg;

public class VsrgMetadata : Entity
{
    private readonly VsrgManager Manager;
    private Chart Chart => Manager.Chart;
    private string ArtistName => Chart.ArtistName;
    private string SongName => Chart.SongName;
    private string MapperName => Chart.MapperName;
    private string DifficultyName => Chart.DifficultyName;
    
    public VsrgMetadata(Vector2 position, VsrgManager manager)
        : base(position)
    {
        Manager = manager;
    }
    
    public void Draw()
    {
        Color spriteColor = Color.Black * Manager.GameFade;
        Vector2 renderPosition = Manager.ScreenPosition + (Vector2.UnitY * 164);
        
        Monocle.Draw.Rect(renderPosition, 320, 16, spriteColor);
    }
}
