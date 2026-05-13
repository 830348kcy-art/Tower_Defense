using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using KingdomRushClone.Models;

namespace KingdomRushClone.Managers;

public class SaveData
{
    public Dictionary<int, int> StageStars { get; set; } = new();
    public Dictionary<TechId, int> TechLevels { get; set; } = new();
    public bool TutorialDone { get; set; }

    public int TotalStars
    {
        get
        {
            int s = 0;
            foreach (var v in StageStars.Values) s += v;
            return s;
        }
    }

    public int SpentStars
    {
        get
        {
            int s = 0;
            foreach (var kv in TechLevels)
            {
                var node = TechTreeCatalog.Nodes.Find(n => n.Id == kv.Key);
                if (node != null) s += node.CostPerLevel * kv.Value;
            }
            return s;
        }
    }

    public int AvailableStars => TotalStars - SpentStars;
}

public static class SaveManager
{
    private static readonly string Path = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "KingdomRushClone", "save.json");

    public static SaveData Current { get; private set; } = Load();

    public static SaveData Load()
    {
        try
        {
            if (File.Exists(Path))
                return JsonSerializer.Deserialize<SaveData>(File.ReadAllText(Path)) ?? new SaveData();
        }
        catch { }
        return new SaveData();
    }

    public static void Save()
    {
        try
        {
            var dir = System.IO.Path.GetDirectoryName(Path)!;
            Directory.CreateDirectory(dir);
            File.WriteAllText(Path, JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }

    public static void RecordStageStars(int stage, int stars)
    {
        if (!Current.StageStars.TryGetValue(stage, out var existing) || stars > existing)
            Current.StageStars[stage] = stars;
        Save();
    }

    public static int GetTechLevel(TechId id) => Current.TechLevels.TryGetValue(id, out var v) ? v : 0;

    public static bool TryUpgradeTech(TechId id)
    {
        var node = TechTreeCatalog.Nodes.Find(n => n.Id == id);
        if (node == null) return false;
        int cur = GetTechLevel(id);
        if (cur >= node.MaxLevel) return false;
        if (Current.AvailableStars < node.CostPerLevel) return false;
        Current.TechLevels[id] = cur + 1;
        Save();
        return true;
    }

    public static bool TryDowngradeTech(TechId id)
    {
        int cur = GetTechLevel(id);
        if (cur <= 0) return false;
        Current.TechLevels[id] = cur - 1;
        if (Current.TechLevels[id] == 0) Current.TechLevels.Remove(id);
        Save();
        return true;
    }

    public static double TechEffect(TechId id)
    {
        var node = TechTreeCatalog.Nodes.Find(n => n.Id == id);
        if (node == null) return 0;
        return node.EffectPerLevel * GetTechLevel(id);
    }
}
