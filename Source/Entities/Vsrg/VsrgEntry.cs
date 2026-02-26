using System.Collections;

namespace Celeste.Mod.HamburgerHelper.Entities.Vsrg;

[CustomEntity("HamburgerHelper/VsrgEntry")]
public class VsrgEntry : Entity
{
    private Chart Chart;
    
    private readonly string ChartFolder;
    private readonly TalkComponent Talk;
    
    private Coroutine EntryCoroutine;
    
    public VsrgEntry(EntityData data, Vector2 offset) 
        : base(data.Position + offset)
    {
        Depth = 1000;

        ChartFolder = data.Attr("chartPath", "Data/bunnerfly/dreamer/mononokes/");
        
        Add(Talk = new TalkComponent(
            new Rectangle(0, 0, data.Width, data.Height), 
            new Vector2(data.Width / 2f, -4), OnTalk));
        Talk.PlayerMustBeFacing = false;
    }
    
    private void OnTalk(Player player)
    {
        Chart = Chart.ProcessChartFromFile(ChartFolder, Difficulty.Finale);
        
        player.Add(EntryCoroutine = new Coroutine(CutsceneRoutine(player)));
    }
    
    private IEnumerator CutsceneRoutine(Player player)
    {
        if (Scene is not Level level) yield break;

        Audio.currentMusicEvent.setParameterValue("fade", 0);
        
        player.StateMachine.State = 11;
        player.StateMachine.Locked = true;

        level.PauseLock = true;
        
        yield return player.DummyWalkToExact((int)Position.X + Talk.Bounds.Width / 2);
        
        yield return GameCutscene(player);
        
        player.StateMachine.Locked = false;
        player.StateMachine.State = 0;

        level.PauseLock = false;
        
        Audio.currentMusicEvent.setParameterValue("fade", 1);
    }
    
    private IEnumerator GameCutscene(Player player)
    {
        VsrgManager vsrg = new VsrgManager(Vector2.Zero, Chart, EntryCoroutine);
        Scene.Add(vsrg);
        
        yield return vsrg.GameRunRoutine();

        yield return 1f;
    }
}