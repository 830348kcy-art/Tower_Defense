using TowerDefense.Core;

namespace TowerDefense.Data;

public static class WavePlan
{
    private static readonly IReadOnlyDictionary<int, string> ChapterBoss = new Dictionary<int, string>
    {
        [1] = "boss_normal",
        [2] = "boss_charge",
        [3] = "boss_split",
        [4] = "boss_speed",
    };

    private static readonly IReadOnlyDictionary<int, string> ChapterMiniBoss = new Dictionary<int, string>
    {
        [1] = "miniboss_normal",
        [2] = "miniboss_charge",
        [3] = "miniboss_split",
        [4] = "miniboss_speed",
    };

    public static bool IsBossStage(int stage) => stage % 5 == 0;

    public static bool IsMiniBossStage(int stage) => stage % 5 == 3;

    public static int GetChapter(int stage)
    {
        if (stage is < 1 or > 20)
        {
            throw new ArgumentOutOfRangeException(nameof(stage), "Stage must be between 1 and 20.");
        }

        return (stage - 1) / 5 + 1;
    }

    public static IReadOnlyList<WaveEntry> GetWave(int stage, int wave)
    {
        if (wave is < 1 or > 8)
        {
            throw new ArgumentOutOfRangeException(nameof(wave), "Wave must be between 1 and 8.");
        }

        var chapter = GetChapter(stage);
        var elite = chapter switch
        {
            1 => "elite_shield",
            2 => "elite_charge",
            3 => "elite_regen",
            _ => "elite_resist"
        };

        if (wave == 8 && IsBossStage(stage))
        {
            return [new WaveEntry(ChapterBoss[chapter], 1, 1.2)];
        }

        if (wave == 8 && IsMiniBossStage(stage))
        {
            return [new WaveEntry(ChapterMiniBoss[chapter], 1, 1.0)];
        }

        return wave switch
        {
            1 => [new WaveEntry("enemy_normal", 5)],
            2 => [new WaveEntry("enemy_normal", 6), new WaveEntry("enemy_fast", stage >= 2 ? 2 : 1)],
            3 => [new WaveEntry("enemy_fast", 4), new WaveEntry("enemy_normal", 4)],
            4 => [new WaveEntry("enemy_split_body", 2), new WaveEntry("enemy_normal", 4)],
            5 => [new WaveEntry(elite, 1), new WaveEntry("enemy_normal", 5)],
            6 => [new WaveEntry("elite_ghost", 1), new WaveEntry("enemy_fast", 5)],
            7 => [new WaveEntry("elite_regen", 1), new WaveEntry("enemy_split_body", 2)],
            8 => [new WaveEntry(elite, 2), new WaveEntry("enemy_fast", 6)],
            _ => throw new ArgumentOutOfRangeException(nameof(wave))
        };
    }

    public static IReadOnlyList<string> GetEnemyIds(int stage)
    {
        return Enumerable.Range(1, 8)
            .SelectMany(wave => GetWave(stage, wave))
            .Select(entry => entry.EnemyId)
            .Distinct()
            .ToList();
    }
}
