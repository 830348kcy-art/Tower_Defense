using System;
using System.Collections.Generic;
using KingdomRushClone.Models;

namespace KingdomRushClone.Data;

public static class StageCatalog
{
    public const double MapWidth = 1100;
    public const double MapHeight = 620;

    private static readonly string[] StageNames =
    {
        "1구역 관문",
        "1구역 들길",
        "1구역 경계",
        "1구역 외곽",
        "1구역 지휘관",
        "2구역 입구",
        "2구역 굽은 길",
        "2구역 돌진 지점",
        "2구역 강변",
        "2구역 지휘관",
        "3구역 통로",
        "3구역 모래길",
        "3구역 분열 지점",
        "3구역 능선",
        "3구역 지휘관",
        "4구역 잿길",
        "4구역 교차로",
        "4구역 속도 지점",
        "4구역 성벽",
        "4구역 최종 지휘관"
    };

    private static int ChapterFor(int stage) => ((stage - 1) / 5) + 1;

    private static int StageInChapter(int stage) => ((stage - 1) % 5) + 1;

    private static StageTheme ThemeForStage(int stage) => ChapterFor(stage) switch
    {
        1 => StageTheme.Grassland,
        2 => StageTheme.Forest,
        3 => StageTheme.Desert,
        _ => StageTheme.Volcano
    };

    private static List<List<Vec2>> PathFor(int stage)
    {
        return ChapterFor(stage) switch
        {
            1 => new() { new() { new(40, 310), new(560, 310), new(1060, 310) } },
            2 => new() { new()
            {
                new(40, 100), new(300, 100), new(300, 310),
                new(800, 310), new(800, 520), new(1060, 520)
            }},
            3 => new()
            {
                new() { new(40, 150), new(400, 150), new(550, 310), new(900, 310), new(1060, 310) },
                new() { new(40, 470), new(400, 470), new(550, 310), new(900, 310), new(1060, 310) },
            },
            _ => new() { new()
            {
                new(40, 80), new(250, 80), new(250, 540),
                new(600, 540), new(600, 120), new(900, 120),
                new(900, 540), new(1060, 540)
            }}
        };
    }

    private sealed record StageComposition(
        int Normal,
        int Fast,
        int SplitBody,
        int Elite,
        int EliteCharge,
        EnemyKind? ExtraKind = null,
        int ExtraCount = 0,
        EnemyKind? MidBoss = null,
        EnemyKind? Boss = null);

    private static readonly StageComposition[] StageCompositions =
    {
        new(4, 0, 0, 0, 0),
        new(5, 3, 0, 0, 0),
        new(6, 4, 2, 0, 0, MidBoss: EnemyKind.MidBossNormal),
        new(7, 5, 2, 1, 0),
        new(8, 6, 3, 1, 0, Boss: EnemyKind.BossNormal),
        new(5, 3, 0, 0, 0),
        new(6, 4, 2, 0, 0),
        new(7, 5, 3, 1, 1, MidBoss: EnemyKind.MidBossCharge),
        new(8, 6, 3, 2, 1),
        new(9, 7, 4, 2, 2, Boss: EnemyKind.BossCharge),
        new(5, 3, 0, 0, 0),
        new(6, 4, 2, 1, 0, EnemyKind.EliteRegenerator, 1),
        new(8, 5, 3, 1, 1, EnemyKind.EliteRegenerator, 1, EnemyKind.MidBossSplit),
        new(9, 6, 4, 2, 1, EnemyKind.EliteRegenerator, 2),
        new(10, 7, 4, 2, 2, EnemyKind.EliteRegenerator, 2, Boss: EnemyKind.BossSplit),
        new(5, 3, 0, 0, 0),
        new(7, 4, 3, 1, 1, EnemyKind.EliteGhost, 1),
        new(9, 5, 4, 2, 2, EnemyKind.EliteGhost, 1, EnemyKind.MidBossSpeed),
        new(10, 6, 5, 2, 2, EnemyKind.EliteGhost, 2),
        new(12, 8, 5, 3, 3, EnemyKind.EliteGhost, 2, Boss: EnemyKind.BossSpeed)
    };

    private static readonly double[] WaveTimes = { 22, 23, 24, 25, 26, 27, 28, 30 };
    private static readonly int[] NormalWaves = { 0, 1, 2, 7 };
    private static readonly int[] FastWaves = { 1, 2, 5, 7 };
    private static readonly int[] SplitWaves = { 2, 3, 7 };
    private static readonly int[] EliteWaves = { 3, 6, 7 };
    private static readonly int[] EliteChargeWaves = { 4, 6, 7 };
    private static readonly int[] ExtraWaves = { 5, 7 };

    public static readonly List<StageDef> Stages = Build();

    private static List<WaveDef> WavesFor(int stage)
    {
        int pathCount = PathFor(stage).Count;
        double interval = Math.Max(0.35, 0.82 - stage * 0.01);
        var plan = StageCompositions[stage - 1];
        var waves = CreateEmptyWaves(plan);

        Distribute(waves, EnemyKind.Normal, plan.Normal, NormalWaves, interval, pathCount);
        Distribute(waves, EnemyKind.Fast, plan.Fast, FastWaves, interval * 0.75, pathCount);
        Distribute(waves, EnemyKind.SplitBody, plan.SplitBody, SplitWaves, interval * 1.2, pathCount);
        Distribute(waves, EnemyKind.Elite, plan.Elite, EliteWaves, interval * 1.6, pathCount);
        Distribute(waves, EnemyKind.EliteCharge, plan.EliteCharge, EliteChargeWaves, interval * 1.5, pathCount);

        if (plan.ExtraKind != null)
            Distribute(waves, plan.ExtraKind.Value, plan.ExtraCount, ExtraWaves, interval * 1.8, pathCount);

        if (plan.MidBoss != null)
            AddCount(waves[7], plan.MidBoss.Value, 1, 1.0, 0, 6);

        if (plan.Boss != null)
            AddCount(waves[7], plan.Boss.Value, 1, 1.0, 0, 8);

        return waves;
    }

    private static List<WaveDef> CreateEmptyWaves(StageComposition plan)
    {
        var waves = new List<WaveDef>();
        for (int i = 0; i < WaveTimes.Length; i++)
            waves.Add(new WaveDef { TimeUntilNext = WaveTimes[i] });

        if (plan.MidBoss != null) waves[7].TimeUntilNext = 34;
        if (plan.Boss != null) waves[7].TimeUntilNext = 40;
        return waves;
    }

    private static bool HasMidBossFor(int stage) => StageInChapter(stage) == 3;

    private static bool HasBossFor(int stage) => StageInChapter(stage) == 5;

    private static void Distribute(
        List<WaveDef> waves,
        EnemyKind enemy,
        int totalCount,
        int[] targetWaves,
        double interval,
        int pathCount)
    {
        if (totalCount <= 0) return;

        int baseCount = totalCount / targetWaves.Length;
        int remainder = totalCount % targetWaves.Length;
        for (int i = 0; i < targetWaves.Length; i++)
        {
            int count = baseCount + (i < remainder ? 1 : 0);
            if (count <= 0) continue;
            AddCount(waves[targetWaves[i]], enemy, count, interval, PathIndex(i, pathCount), i * 0.5);
        }
    }

    private static void AddCount(
        WaveDef wave,
        EnemyKind enemy,
        int count,
        double interval,
        int path,
        double delay)
    {
        if (count <= 0) return;

        foreach (var entry in wave.Entries)
        {
            if (entry.Enemy != enemy || entry.SpawnPath != path) continue;
            entry.Count += count;
            return;
        }

        wave.Entries.Add(new WaveEntry
        {
            Enemy = enemy,
            Count = count,
            SpawnInterval = interval,
            SpawnPath = path,
            InitialDelay = delay
        });
    }

    private static int PathIndex(int requested, int pathCount) => requested % Math.Max(1, pathCount);

    private static List<TowerKind> AllowedTowersFor(int stage)
    {
        var list = new List<TowerKind> { TowerKind.Archer, TowerKind.Slow };
        if (stage >= 2) list.Add(TowerKind.Barracks);
        if (stage >= 4) list.Add(TowerKind.Mage);
        if (stage >= 6) list.Add(TowerKind.Bombard);
        return list;
    }

    private static List<EnvEffect> EffectsFor(int stage)
    {
        var effects = new List<EnvEffect>();
        if (stage is >= 11 and <= 15) effects.Add(EnvEffect.NarrowCorridor);
        if (stage is >= 16 and <= 20)
        {
            effects.Add(EnvEffect.LavaTiles);
            effects.Add(EnvEffect.NightVision);
        }
        return effects;
    }

    private static double HpScaleFor(int stage) => Math.Pow(1.2, ChapterFor(stage) - 1);

    private static double SpeedScaleFor(int stage) => 1.0 + (ChapterFor(stage) - 1) * 0.03;

    private static List<StageDef> Build()
    {
        var list = new List<StageDef>();
        for (int stage = 1; stage <= 20; stage++)
        {
            var paths = PathFor(stage);
            list.Add(new StageDef
            {
                Number = stage,
                Name = StageNames[stage - 1],
                Theme = ThemeForStage(stage),
                StartingGold = 220 + (stage - 1) * 12,
                StartingLives = 20,
                Paths = paths,
                BuildSlots = new(),
                Waves = WavesFor(stage),
                AllowedTowers = AllowedTowersFor(stage),
                HasMidBoss = HasMidBossFor(stage),
                HasBoss = HasBossFor(stage),
                EnemyHpScale = HpScaleFor(stage),
                EnemySpeedScale = SpeedScaleFor(stage),
                Effects = EffectsFor(stage),
            });
        }
        return list;
    }
}
