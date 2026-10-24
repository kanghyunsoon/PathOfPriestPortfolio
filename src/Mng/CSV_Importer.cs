using System.Collections.Generic;

public class CSV_Importer
{
    public static List<Dictionary<string, object>> Spawn_Design = new List<Dictionary<string, object>>(CSVReader.Read("Spawner"));
    public static List<Dictionary<string, object>> Summon = new List<Dictionary<string, object>>(CSVReader.Read("Summon"));
    public static List<Dictionary<string, object>> Quest = new List<Dictionary<string, object>>(CSVReader.Read("Quest"));
    public static List<Dictionary<string, object>> Localization = new List<Dictionary<string, object>>(CSVReader.Read("Localization"));
    public static List<Dictionary<string, object>> DailyQuest = new List<Dictionary<string, object>>(CSVReader.Read("DailyQuest"));

    // 표의 마지막 스테이지를 넘으면 마지막 행의 출현 설정을 계속 쓴다.
    public static Dictionary<string, object> SpawnDesignRow(int stage)
    {
        int index = stage < 0 ? 0 : stage;
        if (index >= Spawn_Design.Count) index = Spawn_Design.Count - 1;
        return Spawn_Design[index];
    }
}
