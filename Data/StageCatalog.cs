using System;
using System.Collections.Generic;
using KingdomRushClone.Models;

namespace KingdomRushClone.Data;

public static class StageCatalog
{
    public const double MapWidth = 1100;
    public const double MapHeight = 620;

    public static readonly List<StageDef> Stages = Build();

    private static StageTheme ThemeForStage(int n) => n switch
    {
        <= 5 => StageTheme.Grassland,
        <= 10 => StageTheme.Forest,
        <= 15 => StageTheme.Desert,
        <= 20 => StageTheme.Volcano,
        <= 25 => StageTheme.Snow,
        _ => StageTheme.Castle
    };

    private static List<List<Vec2>> PathFor(int n)
    {
        if (n <= 5)
            return new() { new() { new(40, 310), new(560, 310), new(1060, 310) } };
        if (n <= 10)
            return new() { new() { new(40, 100), new(300, 100), new(300, 310), new(800, 310), new(800, 520), new(1060, 520) } };
        if (n <= 15)
            return new()
            {
                new() { new(40, 150), new(400, 150), new(550, 310), new(900, 310), new(1060, 310) },
                new() { new(40, 470), new(400, 470), new(550, 310), new(900, 310), new(1060, 310) },
            };
        if (n <= 20)
            return new() { new() { new(40, 80), new(250, 80), new(250, 540), new(600, 540), new(600, 120), new(900, 120), new(900, 540), new(1060, 540) } };
        if (n <= 25)
            return new()
            {
                new() { new(40, 100), new(550, 100), new(550, 310), new(1060, 310) },
                new() { new(40, 310), new(1060, 310) },
                new() { new(40, 520), new(550, 520), new(550, 310), new(1060, 310) },
            };
        return new()
        {
            new() { new(40, 80),  new(400, 80),  new(400, 310), new(700, 310), new(700, 540), new(1060, 540) },
            new() { new(40, 540), new(400, 540), new(400, 310), new(700, 310), new(700, 80),  new(1060, 80) },
            new() { new(40, 200), new(900, 200), new(900, 420), new(1060, 420) },
            new() { new(40, 420), new(900, 420), new(900, 200), new(1060, 200) },
        };
    }

    private static List<Vec2> BuildSlotsAround(List<List<Vec2>> paths)
    {
        var slots = new HashSet<(int x, int y)>();
        const double gridX = 80, gridY = 80;
        for (double x = 70; x < MapWidth - 30; x += gridX)
        {
            for (double y = 70; y < MapHeight - 30; y += gridY)
            {
                if (NearPath(paths, x, y, 45) && !OnPath(paths, x, y, 30))
                    slots.Add(((int)x, (int)y));
            }
        }
        var list = new List<Vec2>();
        foreach (var (x, y) in slots) list.Add(new(x, y));
        return list;
    }

    private static bool OnPath(List<List<Vec2>> paths, double x, double y, double tolerance)
    {
        foreach (var path in paths)
            for (int i = 0; i < path.Count - 1; i++)
                if (DistPointSeg(new(x, y), path[i], path[i + 1]) < tolerance) return true;
        return false;
    }
    private static bool NearPath(List<List<Vec2>> paths, double x, double y, double tolerance)
    {
        foreach (var path in paths)
            for (int i = 0; i < path.Count - 1; i++)
                if (DistPointSeg(new(x, y), path[i], path[i + 1]) < tolerance) return true;
        return false;
    }
    private static double DistPointSeg(Vec2 p, Vec2 a, Vec2 b)
    {
        var ab = b - a;
        var ap = p - a;
        double t = (ap.X * ab.X + ap.Y * ab.Y) / Math.Max(1e-6, ab.X * ab.X + ab.Y * ab.Y);
        t = Math.Max(0, Math.Min(1, t));
        var proj = new Vec2(a.X + ab.X * t, a.Y + ab.Y * t);
        return p.DistanceTo(proj);
    }

    private static List<WaveDef> WavesFor(int stage)
    {
        var rand = new Random(stage * 7919);
        int waveCount = stage switch { <= 5 => 4, <= 10 => 6, <= 15 => 7, <= 20 => 8, <= 25 => 9, _ => 10 };
        var waves = new List<WaveDef>();
        bool hasMid = HasMidBossFor(stage);
        bool hasBoss = HasBossFor(stage);
        int pathCount = PathFor(stage).Count;

        for (int w = 0; w < waveCount; w++)
        {
            var wave = new WaveDef { TimeUntilNext = 20 + stage * 0.4 };
            int wavePower = (int)(8 + stage * 1.5 + w * 2.0);
            var entry = new WaveEntry { Enemy = EnemyKind.GoblinSoldier, Count = Math.Max(4, wavePower), SpawnInterval = Math.Max(0.35, 0.8 - stage * 0.01), SpawnPath = rand.Next(pathCount) };

            if (stage >= 2 && rand.NextDouble() < 0.6)
                entry.Enemy = EnemyKind.GoblinScout;
            if (stage >= 4 && rand.NextDouble() < 0.4)
                entry.Enemy = EnemyKind.OrcWarrior;
            if (stage >= 7 && rand.NextDouble() < 0.35)
                entry.Enemy = EnemyKind.Wyvern;
            if (stage >= 9 && rand.NextDouble() < 0.3)
                entry.Enemy = EnemyKind.TrollShaman;
            if (stage >= 16 && rand.NextDouble() < 0.4)
                entry.Enemy = EnemyKind.DarkKnight;

            wave.Entries.Add(entry);

            if (stage >= 3 && w >= 1 && rand.NextDouble() < 0.6)
            {
                wave.Entries.Add(new WaveEntry
                {
                    Enemy = (EnemyKind)rand.Next(0, Math.Min(5, 1 + stage / 3)),
                    Count = Math.Max(2, wavePower / 3),
                    SpawnInterval = 1.0,
                    InitialDelay = 4,
                    SpawnPath = rand.Next(pathCount)
                });
            }
            waves.Add(wave);
        }

        if (hasMid)
        {
            var bossWave = new WaveDef { TimeUntilNext = 30 };
            bossWave.Entries.Add(new WaveEntry { Enemy = EnemyKind.GoblinSoldier, Count = 4, SpawnInterval = 0.6, SpawnPath = 0 });
            bossWave.Entries.Add(new WaveEntry { Enemy = MidBossFor(stage), Count = 1, InitialDelay = 3, SpawnPath = 0 });
            waves.Add(bossWave);
        }
        if (hasBoss)
        {
            var bossWave = new WaveDef { TimeUntilNext = 30 };
            bossWave.Entries.Add(new WaveEntry { Enemy = EnemyKind.OrcWarrior, Count = 6, SpawnInterval = 0.7, SpawnPath = 0 });
            bossWave.Entries.Add(new WaveEntry { Enemy = BossFor(stage), Count = 1, InitialDelay = 4, SpawnPath = 0 });
            waves.Add(bossWave);
        }
        return waves;
    }

    private static bool HasMidBossFor(int stage) => stage == 13 || (stage % 5 == 0 && stage % 10 != 0 && stage != 15);

    private static bool HasBossFor(int stage) => stage == 15 || stage % 10 == 0;

    private static EnemyKind MidBossFor(int stage) => stage == 13 ? EnemyKind.SplitMidBoss : EnemyKind.MidBoss;

    private static EnemyKind BossFor(int stage) => stage == 15 ? EnemyKind.SplitBoss : EnemyKind.Boss;

    private static List<StageDef> Build()
    {
        var list = new List<StageDef>();
        for (int n = 1; n <= 30; n++)
        {
            var paths = PathFor(n);
            var s = new StageDef
            {
                Number = n,
                Name = $"스테이지 {n}",
                Theme = ThemeForStage(n),
                StartingGold = 200 + (n - 1) * 10,
                StartingLives = 20,
                Paths = paths,
                BuildSlots = BuildSlotsAround(paths),
                Waves = WavesFor(n),
                AllowedTowers = AllowedTowersFor(n),
                HasMidBoss = HasMidBossFor(n),
                HasBoss = HasBossFor(n),
                EnemyHpScale = 1.0 + (n - 1) * 0.10,
                EnemySpeedScale = 1.0 + (n - 1) * 0.015,
                Effects = EffectsFor(n),
            };
            list.Add(s);
        }
        return list;
    }

    private static List<TowerKind> AllowedTowersFor(int n)
    {
        var list = new List<TowerKind> { TowerKind.Archer, TowerKind.Slow };
        if (n >= 2) list.Add(TowerKind.Barracks);
        if (n >= 4) list.Add(TowerKind.Mage);
        if (n >= 6) list.Add(TowerKind.Bombard);
        return list;
    }

    private static List<EnvEffect> EffectsFor(int n)
    {
        var fx = new List<EnvEffect>();
        if (n is >= 16 and <= 20) { fx.Add(EnvEffect.LavaTiles); fx.Add(EnvEffect.NightVision); }
        if (n is >= 21 and <= 25) fx.Add(EnvEffect.IcePath);
        if (n is >= 11 and <= 15) fx.Add(EnvEffect.NarrowCorridor);
        return fx;
    }
}
