using Celeste.Mod.HamburgerHelper.Entities.Vsrg.HitObjects;

namespace Celeste.Mod.HamburgerHelper.Entities.Vsrg;

public enum Difficulty
{
    Opening,
    Middle,
    Finale,
    Encore,
    Backstage,
}

public enum NoteType
{
    Note,
    LongNote,
    Mine,
    Bumper,
    JudgementBumper,
}

public class Chart
{
    public string ArtistName { get; set; }
    public string SongName { get; set; }
    public string MapperName { get; set; }
    public string DifficultyName { get; set; }
    public string DifficultyNumber { get; set; }
    
    public string SongEvent { get; set; }

    public List<HitObject> HitObjects { get; set; } = [];
    
        public static Chart ProcessChartFromFile(string folderPath, Difficulty diff)
    {
        const string metadata = "metadata.vsmd";
        
        const string openingPath = "opening/opening.vsbt";
        const string middlePath = "middle/middle.vsbt";
        const string finalePath = "finale/finale.vsbt";
        const string encorePath = "encore/encore.vsbt";
        const string backstagePath = "backstage/backstage.vsbt";
        
        string metadataPath = folderPath + metadata;
        
        string[] metadataLines = HamburgerHelperContent.GetLinesFromPath(metadataPath);
        
        Chart processedChart = new Chart 
        {
            ArtistName = metadataLines[0],
            SongName = metadataLines[1],
            MapperName = metadataLines[2],
            DifficultyName = metadataLines[3],
            DifficultyNumber = metadataLines[4]
        };

        string difficultyPath = folderPath;
        difficultyPath += diff switch {
            Difficulty.Opening => openingPath,
            Difficulty.Middle => middlePath,
            Difficulty.Finale => finalePath,
            Difficulty.Encore => encorePath,
            Difficulty.Backstage => backstagePath,
            _ => ""
        };
        
        string[] chartLines = HamburgerHelperContent.GetLinesFromPath(difficultyPath);

        processedChart.SongEvent = chartLines[0].Trim();
        
        // two lines are skippped here:
        // 1. the fmod event for the song
        // 2. the [Buttons] label
        chartLines = chartLines.Skip(2).ToArray();
        
        for (int lineCount = 0; lineCount < chartLines.Length; lineCount++)
        {
            string line = chartLines[lineCount];

            string[] noteData = line.Split('|');
            
            if (noteData.Length != 4)
            {
                lineCount++;
                continue;
            }
            
            int startTime = int.Parse(noteData[0].Trim());
            string noteType = noteData[1].Trim();
            int column = int.Parse(noteData[2].Trim());
            int endTime = int.Parse(noteData[3].Trim());

            switch (noteType)
            {
                // buttons
                case "N":
                    processedChart.HitObjects.Add(new Note(startTime, column));
                    break;
                case "LN":
                    processedChart.HitObjects.Add(
                        new LongNote(startTime, column, endTime));
                    break;
                case "MN":
                    processedChart.HitObjects.Add(new Mine(startTime, column));
                    break;
                
                // bumpers
                case "L":
                    processedChart.HitObjects.Add(
                        new HitObjects.Bumper(startTime, BumperPosition.Left));
                    break;
                case "M":
                    processedChart.HitObjects.Add(
                        new HitObjects.Bumper(startTime, BumperPosition.Middle));
                    break;
                case "R":
                    processedChart.HitObjects.Add(
                        new HitObjects.Bumper(startTime, BumperPosition.Right));
                    break;
                case "JL":
                    processedChart.HitObjects.Add(
                        new JudgementBumper(startTime, BumperPosition.Left));
                    break;
                case "JM":
                    processedChart.HitObjects.Add(
                        new JudgementBumper(startTime, BumperPosition.Middle));
                    break;
                case "JR":
                    processedChart.HitObjects.Add(
                        new JudgementBumper(startTime, BumperPosition.Right));
                    break;
            }
        }
        
        return processedChart;
    }
}
