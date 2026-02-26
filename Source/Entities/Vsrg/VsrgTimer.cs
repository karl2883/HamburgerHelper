using FMOD.Studio;

namespace Celeste.Mod.HamburgerHelper.Entities.Vsrg;

public class VsrgTimer : Entity
{
    private EventInstance ChartMusic;
    
    private readonly string FmodEvent;
    
    public float SongPosition;
    public float LastSongPosition;
    public float SmoothSongPosition;
    
    public int SongLengthMs;
    
    private const int SongOffset = 80;
    
    public VsrgTimer(Vector2 position, string fmodEvent)
        : base(position)
    {
        FmodEvent = fmodEvent;
    }

    public override void Removed(Scene scene)
    {
        base.Removed(scene);
        
        StopMusic();
    }

    public override void Update()
    {
        base.Update();
        
        LastSongPosition = SongPosition;
        
        ChartMusic.getTimelinePosition(out int position);
        SongPosition = position - SongOffset;
        SmoothSongPosition = Calc.LerpClamp(LastSongPosition, SongPosition, 0.5f);
    }
    
    public void PlayMusic()
    {
        ChartMusic = Audio.Play(FmodEvent);
    }
    
    public void StopMusic()
    {
        ChartMusic?.stop(STOP_MODE.IMMEDIATE);
    }
}