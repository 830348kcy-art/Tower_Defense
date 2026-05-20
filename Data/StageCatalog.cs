using System;
using System.Collections.Generic;
using KingdomRushClone.Models;

namespace KingdomRushClone.Data;

public static class StageCatalog
{
    public const double MapWidth  = 1100;
    public const double MapHeight = 620;

    // ─── Stage theme ───────────────────────────────────────────────────
    private static StageTheme ThemeForStage(int n) => n switch
    {
        <= 5  => StageTheme.Grassland,
        <= 10 => StageTheme.Forest,
        <= 15 => StageTheme.Desert,
        <= 20 => StageTheme.Volcano,
        <= 25 => StageTheme.Snow,
        _     => StageTheme.Castle
    };

    // ─── Flavored stage names (must come BEFORE Stages = Build()) ──────
    private static readonly string[] StageNames =
    {
        /* 01 */ "초원의 관문",
        /* 02 */ "뱀의 협곡",
        /* 03 */ "강변 요새",
        /* 04 */ "갈림길",
        /* 05 */ "초원의 마지막 함성",   // mid-boss
        /* 06 */ "흑림의 입구",
        /* 07 */ "나무꾼의 길",
        /* 08 */ "안개 지대",
        /* 09 */ "어둠의 계곡",
        /* 10 */ "숲의 군주",            // boss
        /* 11 */ "사막의 시작",
        /* 12 */ "모래 폭풍",
        /* 13 */ "오아시스 방어",
        /* 14 */ "낙타 대상의 길",
        /* 15 */ "사막 황제",            // mid-boss
        /* 16 */ "용암 지대",
        /* 17 */ "화산의 분노",
        /* 18 */ "불타는 협곡",
        /* 19 */ "재의 도시",
        /* 20 */ "화염 군주",            // boss
        /* 21 */ "설원의 입구",
        /* 22 */ "눈보라 고개",
        /* 23 */ "얼음 궁전",
        /* 24 */ "동결된 강",
        /* 25 */ "설원의 지배자",        // mid-boss
        /* 26 */ "마지막 성벽",
        /* 27 */ "왕도의 방어",
        /* 28 */ "요새 공성",
        /* 29 */ "왕좌의 방",
        /* 30 */ "암흑 황제",            // final boss
    };

    public static readonly List<StageDef> Stages = Build();

    // ─── Path layouts (6 shapes, one per 5-stage block) ─────────────────
    private static List<List<Vec2>> PathFor(int n)
    {
        if (n <= 5)
            // Simple straight path
            return new() { new() { new(40, 310), new(560, 310), new(1060, 310) } };

        if (n <= 10)
            // S-curve
            return new() { new()
            {
                new(40, 100), new(300, 100), new(300, 310),
                new(800, 310), new(800, 520), new(1060, 520)
            }};

        if (n <= 15)
            // Two converging paths
            return new()
            {
                new() { new(40, 150), new(400, 150), new(550, 310), new(900, 310), new(1060, 310) },
                new() { new(40, 470), new(400, 470), new(550, 310), new(900, 310), new(1060, 310) },
            };

        if (n <= 20)
            // Zigzag
            return new() { new()
            {
                new(40, 80),  new(250, 80),  new(250, 540),
                new(600, 540), new(600, 120), new(900, 120),
                new(900, 540), new(1060, 540)
            }};

        if (n <= 25)
            // Three paths merging to one exit
            return new()
            {
                new() { new(40, 100), new(550, 100), new(550, 310), new(1060, 310) },
                new() { new(40, 310), new(1060, 310) },
                new() { new(40, 520), new(550, 520), new(550, 310), new(1060, 310) },
            };

        // Four-path cross (stages 26-30)
        return new()
        {
            new() { new(40, 80),  new(400, 80),  new(400, 310), new(700, 310), new(700, 540), new(1060, 540) },
            new() { new(40, 540), new(400, 540), new(400, 310), new(700, 310), new(700, 80),  new(1060, 80)  },
            new() { new(40, 200), new(900, 200), new(900, 420), new(1060, 420) },
            new() { new(40, 420), new(900, 420), new(900, 200), new(1060, 200) },
        };
    }

    // ─── Wave generation ────────────────────────────────────────────────
    private static List<WaveDef> WavesFor(int stage)
    {
        var rand = new Random(stage * 7919);
        int waveCount = stage switch { <= 5 => 4, <= 10 => 6, <= 15 => 7, <= 20 => 8, <= 25 => 9, _ => 10 };
        int pathCount = PathFor(stage).Count;
        double baseInterval = Math.Max(0.30, 0.90 - stage * 0.012);
        var waves = new List<WaveDef>();

        for (int w = 0; w < waveCount; w++)
        {
            double wavePower = 10 + stage * 2.2 + w * 2.8;
            var wave = new WaveDef { TimeUntilNext = 22 + stage * 0.5 };

            // ── Primary enemy ──
            EnemyKind primary = PickPrimary(stage, rand);
            int primaryCount = (int)Math.Max(3, wavePower / EnemyWeight(primary));
            wave.Entries.Add(new WaveEntry
            {
                Enemy         = primary,
                Count         = primaryCount,
                SpawnInterval = baseInterval,
                SpawnPath     = rand.Next(pathCount),
                InitialDelay  = 0
            });

            // ── Secondary enemy (waves 2+, increasing chance) ──
            if (w >= 1 && stage >= 2 && rand.NextDouble() < 0.40 + w * 0.07)
            {
                EnemyKind secondary = PickSecondary(primary, stage, rand);
                int secCount = (int)Math.Max(2, wavePower * 0.45 / EnemyWeight(secondary));
                wave.Entries.Add(new WaveEntry
                {
                    Enemy         = secondary,
                    Count         = secCount,
                    SpawnInterval = baseInterval * 1.3,
                    InitialDelay  = 3 + rand.Next(3),
                    SpawnPath     = rand.Next(pathCount)
                });
            }

            // ── Tertiary (late stages) ──
            if (stage >= 15 && w >= 3 && rand.NextDouble() < 0.35)
            {
                EnemyKind tertiary = PickPrimary(Math.Max(1, stage - 5), rand);
                if (tertiary != primary)
                {
                    wave.Entries.Add(new WaveEntry
                    {
                        Enemy         = tertiary,
                        Count         = (int)Math.Max(2, wavePower * 0.25 / EnemyWeight(tertiary)),
                        SpawnInterval = 1.1,
                        InitialDelay  = 7 + rand.Next(4),
                        SpawnPath     = rand.Next(pathCount)
                    });
                }
            }

            waves.Add(wave);
        }

        // ── Mid-boss wave ──
        if (HasMidBossFor(stage))
        {
            var bw = new WaveDef { TimeUntilNext = 30 };
            bw.Entries.Add(new WaveEntry { Enemy = EnemyKind.GoblinScout, Count = 8,  SpawnInterval = 0.45, SpawnPath = 0 });
            if (pathCount > 1)
                bw.Entries.Add(new WaveEntry { Enemy = EnemyKind.OrcWarrior, Count = 4, SpawnInterval = 0.9, SpawnPath = 1, InitialDelay = 1 });
            bw.Entries.Add(new WaveEntry { Enemy = MidBossFor(stage), Count = 1, InitialDelay = 4, SpawnPath = 0 });
            waves.Add(bw);
        }

        // ── Boss wave ──
        if (HasBossFor(stage))
        {
            var bw = new WaveDef { TimeUntilNext = 40 };
            bw.Entries.Add(new WaveEntry { Enemy = EnemyKind.OrcWarrior, Count = 10, SpawnInterval = 0.55, SpawnPath = 0 });
            if (stage >= 20)
                bw.Entries.Add(new WaveEntry { Enemy = EnemyKind.DarkKnight, Count = 4, SpawnInterval = 1.0, InitialDelay = 3, SpawnPath = pathCount > 1 ? 1 : 0 });
            bw.Entries.Add(new WaveEntry { Enemy = BossFor(stage), Count = 1, InitialDelay = 6, SpawnPath = 0 });
            waves.Add(bw);
        }

        return waves;
    }

    // ─── Boss scheduling helpers ────────────────────────────────────────
    /// <summary>Stage 13 hosts a SplitMidBoss; other multiples of 5 (except boss-stages) host the regular MidBoss.</summary>
    private static bool HasMidBossFor(int stage) =>
        stage == 13 || (stage % 5 == 0 && stage % 10 != 0 && stage != 15);

    /// <summary>Stage 15 hosts a SplitBoss; every multiple of 10 hosts the regular Boss.</summary>
    private static bool HasBossFor(int stage) =>
        stage == 15 || stage % 10 == 0;

    private static EnemyKind MidBossFor(int stage) =>
        stage == 13 ? EnemyKind.SplitMidBoss : EnemyKind.MidBoss;

    private static EnemyKind BossFor(int stage) =>
        stage == 15 ? EnemyKind.SplitBoss : EnemyKind.Boss;

    // ─── Enemy selection helpers ────────────────────────────────────────
    private static EnemyKind PickPrimary(int stage, Random rand) => stage switch
    {
        <= 2  => EnemyKind.GoblinSoldier,
        <= 4  => rand.Next(2) == 0 ? EnemyKind.GoblinSoldier : EnemyKind.GoblinScout,
        <= 7  => rand.Next(3) switch { 0 => EnemyKind.GoblinSoldier, 1 => EnemyKind.GoblinScout, _ => EnemyKind.OrcWarrior },
        <= 10 => rand.Next(3) switch { 0 => EnemyKind.GoblinScout,   1 => EnemyKind.OrcWarrior,  _ => EnemyKind.Wyvern },
        <= 14 => rand.Next(3) switch { 0 => EnemyKind.OrcWarrior,    1 => EnemyKind.Wyvern,      _ => EnemyKind.TrollShaman },
        <= 18 => rand.Next(3) switch { 0 => EnemyKind.Wyvern,        1 => EnemyKind.TrollShaman, _ => EnemyKind.DarkKnight },
        _     => rand.Next(3) switch { 0 => EnemyKind.TrollShaman,   1 => EnemyKind.DarkKnight,  _ => EnemyKind.OrcWarrior },
    };

    private static EnemyKind PickSecondary(EnemyKind primary, int stage, Random rand)
    {
        var pool = new List<EnemyKind>();
        if (primary != EnemyKind.GoblinSoldier)                  pool.Add(EnemyKind.GoblinSoldier);
        if (primary != EnemyKind.GoblinScout  && stage >= 2)     pool.Add(EnemyKind.GoblinScout);
        if (primary != EnemyKind.OrcWarrior   && stage >= 4)     pool.Add(EnemyKind.OrcWarrior);
        if (primary != EnemyKind.Wyvern       && stage >= 7)     pool.Add(EnemyKind.Wyvern);
        if (primary != EnemyKind.TrollShaman  && stage >= 9)     pool.Add(EnemyKind.TrollShaman);
        if (primary != EnemyKind.DarkKnight   && stage >= 16)    pool.Add(EnemyKind.DarkKnight);
        return pool.Count == 0 ? EnemyKind.GoblinSoldier : pool[rand.Next(pool.Count)];
    }

    /// <summary>Used to scale enemy count based on relative threat value.</summary>
    private static double EnemyWeight(EnemyKind k) => k switch
    {
        EnemyKind.GoblinSoldier => 1.0,
        EnemyKind.GoblinScout   => 1.3,
        EnemyKind.OrcWarrior    => 2.8,
        EnemyKind.Wyvern        => 3.2,
        EnemyKind.TrollShaman   => 3.8,
        EnemyKind.DarkKnight    => 5.5,
        _                       => 2.0
    };

    // ─── Allowed towers ─────────────────────────────────────────────────
    private static List<TowerKind> AllowedTowersFor(int n)
    {
        var list = new List<TowerKind> { TowerKind.Archer, TowerKind.Slow };
        if (n >= 2) list.Add(TowerKind.Barracks);
        if (n >= 4) list.Add(TowerKind.Mage);
        if (n >= 6) list.Add(TowerKind.Bombard);
        return list;
    }

    // ─── Environment effects ────────────────────────────────────────────
    private static List<EnvEffect> EffectsFor(int n)
    {
        var fx = new List<EnvEffect>();
        if (n is >= 16 and <= 20) { fx.Add(EnvEffect.LavaTiles); fx.Add(EnvEffect.NightVision); }
        if (n is >= 21 and <= 25)   fx.Add(EnvEffect.IcePath);
        if (n is >= 11 and <= 15)   fx.Add(EnvEffect.NarrowCorridor);
        return fx;
    }

    // ─── Difficulty curves ──────────────────────────────────────────────
    /// <summary>
    /// 적 체력 배율 — 1~4는 학습용으로 매우 쉽게(0.55~0.85),
    /// 5는 첫 중간보스 기준선(1.0), 6~30은 매 스테이지 +0.10 으로 점진 상승.
    /// (1: 0.55, 2: 0.65, 3: 0.75, 4: 0.85, 5: 1.00, 6: 1.00, …, 30: 3.40)
    /// </summary>
    private static double HpScaleFor(int n)
    {
        if (n <= 4) return 0.55 + (n - 1) * 0.10;   // 0.55, 0.65, 0.75, 0.85
        if (n == 5) return 1.00;                    // 중간보스 기준선
        return 1.00 + (n - 5) * 0.10;               // 6=1.00, 7=1.10, …, 30=3.50
    }

    /// <summary>적 속도 배율 — 초반 둔하게, 후반은 원래 페이스 유지.</summary>
    private static double SpeedScaleFor(int n)
    {
        if (n <= 4) return 0.85 + (n - 1) * 0.04;   // 0.85, 0.89, 0.93, 0.97
        return 1.00 + (n - 5) * 0.015;              // 5=1.00, …, 30=1.375
    }

    // ─── Build list ─────────────────────────────────────────────────────
    private static List<StageDef> Build()
    {
        var list = new List<StageDef>();
        for (int n = 1; n <= 30; n++)
        {
            var paths = PathFor(n);
            list.Add(new StageDef
            {
                Number          = n,
                Name            = StageNames[n - 1],
                Theme           = ThemeForStage(n),
                StartingGold    = 200 + (n - 1) * 12,
                StartingLives   = 20,
                Paths           = paths,
                BuildSlots      = new(),
                Waves           = WavesFor(n),
                AllowedTowers   = AllowedTowersFor(n),
                HasMidBoss      = HasMidBossFor(n),
                HasBoss         = HasBossFor(n),
                EnemyHpScale    = HpScaleFor(n),
                EnemySpeedScale = SpeedScaleFor(n),
                Effects         = EffectsFor(n),
            });
        }
        return list;
    }
}
