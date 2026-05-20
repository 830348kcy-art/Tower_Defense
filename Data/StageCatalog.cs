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

    public static readonly List<StageDef> Stages = Build();

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

    private static List<WaveDef> WavesFor(int stage)
    {
        int chapter = ChapterFor(stage);
        int stageInChapter = StageInChapter(stage);
        int pathCount = PathFor(stage).Count;
        int pressure = stage + chapter * 2;
        double interval = Math.Max(0.35, 0.82 - stage * 0.01);
        var waves = new List<WaveDef>();

        AddWave(waves, 22,
            Entry(EnemyKind.Normal, 8 + pressure, interval, 0, 0));

        AddWave(waves, 23,
            Entry(EnemyKind.Normal, 7 + pressure, interval, 0, 0),
            Entry(EnemyKind.Fast, 4 + stageInChapter, interval * 0.75, PathIndex(1, pathCount), 3));

        AddWave(waves, 24,
            Entry(EnemyKind.Fast, 7 + pressure / 2, interval * 0.7, 0, 0),
            Entry(EnemyKind.Normal, 8 + stageInChapter, interval, PathIndex(1, pathCount), 4));

        AddWave(waves, 25,
            Entry(EnemyKind.SplitBody, 2 + chapter, interval * 1.3, 0, 0),
            Entry(EnemyKind.SplitSmall, 6 + pressure, interval * 0.6, PathIndex(1, pathCount), 2));

        AddWave(waves, 26,
            Entry(EliteForChapter(chapter), 1 + stageInChapter / 2, interval * 1.6, 0, 0),
            Entry(EnemyKind.Normal, 8 + pressure, interval, PathIndex(1, pathCount), 3));

        AddWave(waves, 27,
            Entry(EnemyKind.SplitBody, 2 + stageInChapter, interval * 1.2, 0, 0),
            Entry(AdvancedEliteForChapter(chapter), 1 + chapter / 2, interval * 1.8, PathIndex(1, pathCount), 5));

        AddWave(waves, 28,
            Entry(EnemyKind.Fast, 8 + pressure, interval * 0.65, 0, 0),
            Entry(EnemyKind.EliteRegenerator, chapter >= 3 ? 2 : 1, interval * 2.0, PathIndex(1, pathCount), 4),
            Entry(EnemyKind.EliteGhost, chapter >= 4 ? 2 : 1, interval * 2.0, 0, 7));

        if (HasMidBossFor(stage))
        {
            AddWave(waves, 34,
                Entry(EnemyKind.Fast, 8 + pressure, interval * 0.7, 0, 0),
                Entry(EnemyKind.SplitBody, 2 + chapter, interval * 1.2, PathIndex(1, pathCount), 2),
                Entry(MidBossFor(stage), 1, 1.0, 0, 6));
        }
        else if (HasBossFor(stage))
        {
            AddWave(waves, 40,
                Entry(EnemyKind.Normal, 10 + pressure, interval, 0, 0),
                Entry(EnemyKind.Elite, 2 + chapter, interval * 1.7, PathIndex(1, pathCount), 3),
                Entry(BossFor(stage), 1, 1.0, 0, 8));
        }
        else
        {
            AddWave(waves, 30,
                Entry(EnemyKind.SplitBody, 3 + chapter, interval * 1.1, 0, 0),
                Entry(EnemyKind.EliteCharge, 2 + chapter, interval * 1.5, PathIndex(1, pathCount), 3),
                Entry(EnemyKind.EliteGhost, 1 + chapter / 2, interval * 1.8, 0, 6));
        }

        return waves;
    }

    private static EnemyKind EliteForChapter(int chapter) => chapter switch
    {
        1 => EnemyKind.Elite,
        2 => EnemyKind.EliteCharge,
        3 => EnemyKind.EliteRegenerator,
        _ => EnemyKind.EliteGhost
    };

    private static EnemyKind AdvancedEliteForChapter(int chapter) => chapter switch
    {
        1 => EnemyKind.EliteCharge,
        2 => EnemyKind.EliteRegenerator,
        3 => EnemyKind.EliteGhost,
        _ => EnemyKind.Elite
    };

    private static bool HasMidBossFor(int stage) => StageInChapter(stage) == 3;

    private static bool HasBossFor(int stage) => StageInChapter(stage) == 5;

    private static EnemyKind MidBossFor(int stage) => ChapterFor(stage) switch
    {
        1 => EnemyKind.MidBossNormal,
        2 => EnemyKind.MidBossCharge,
        3 => EnemyKind.MidBossSplit,
        _ => EnemyKind.MidBossSpeed
    };

    private static EnemyKind BossFor(int stage) => ChapterFor(stage) switch
    {
        1 => EnemyKind.BossNormal,
        2 => EnemyKind.BossCharge,
        3 => EnemyKind.BossSplit,
        _ => EnemyKind.BossSpeed
    };

    private static WaveEntry Entry(EnemyKind enemy, int count, double interval, int path, double delay) => new()
    {
        Enemy = enemy,
        Count = count,
        SpawnInterval = interval,
        SpawnPath = path,
        InitialDelay = delay
    };

    private static void AddWave(List<WaveDef> waves, double timeUntilNext, params WaveEntry[] entries)
    {
        var wave = new WaveDef { TimeUntilNext = timeUntilNext };
        wave.Entries.AddRange(entries);
        waves.Add(wave);
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
