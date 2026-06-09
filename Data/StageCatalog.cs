using System;
using System.Collections.Generic;
using KingdomRushClone.Models;

namespace KingdomRushClone.Data;

public static class StageCatalog
{
    public const double MapWidth  = 1100;
    public const double MapHeight = 620;

    // ─── Stage theme (5 themes × 4 stages = 20 total) ───────────────────
    private static StageTheme ThemeForStage(int n) => n switch
    {
        <= 4  => StageTheme.Grassland,
        <= 8  => StageTheme.Forest,
        <= 12 => StageTheme.Desert,
        <= 16 => StageTheme.Volcano,
        _     => StageTheme.Castle
    };

    // ─── Flavored stage names (must come BEFORE Stages = Build()) ──────
    // Bosses: 5 (mid), 10 (boss), 13 (split-mid), 15 (split-boss), 20 (final)
    private static readonly string[] StageNames =
    {
        /* 01 */ "초원의 관문",
        /* 02 */ "뱀의 협곡",
        /* 03 */ "강변 요새",
        /* 04 */ "갈림길",
        /* 05 */ "초원의 마지막 함성",   // mid-boss
        /* 06 */ "흑림의 입구",
        /* 07 */ "나무꾼의 길",
        /* 08 */ "어둠의 계곡",
        /* 09 */ "사막의 시작",
        /* 10 */ "사막 황제",            // boss
        /* 11 */ "모래 폭풍",
        /* 12 */ "오아시스 방어",
        /* 13 */ "분열의 협곡",          // split mid-boss
        /* 14 */ "용암 지대",
        /* 15 */ "분열 군주",            // split boss
        /* 16 */ "화산의 분노",
        /* 17 */ "마지막 성벽",
        /* 18 */ "왕도의 방어",
        /* 19 */ "왕좌의 방",
        /* 20 */ "암흑 황제",            // final boss
    };

    public static readonly List<StageDef> Stages = Build();

    // ─── Path layouts (Arknights-style: multiple spawns / objectives) ────
    // Each lane: lane[0] = spawn (red box), lane[^1] = objective (blue box).
    private static List<List<Vec2>> PathFor(int n)
    {
        if (n <= 4)
            // 2 spawns (top-left, bottom-left) merge → 1 objective (right)
            return new()
            {
                new() { new(40, 180), new(320, 180), new(560, 310), new(1060, 310) },
                new() { new(40, 440), new(320, 440), new(560, 310), new(1060, 310) },
            };

        if (n <= 8)
            // 2 spawns → 2 separate objectives (crossing lanes)
            return new()
            {
                new() { new(40, 120), new(400, 120), new(400, 300), new(1060, 300) },
                new() { new(40, 500), new(700, 500), new(700, 160), new(1060, 160) },
            };

        if (n <= 12)
            // 3 spawns (top/mid/bottom) converge → 1 objective
            return new()
            {
                new() { new(40, 110), new(520, 110), new(520, 310), new(1060, 310) },
                new() { new(40, 310), new(1060, 310) },
                new() { new(40, 510), new(520, 510), new(520, 310), new(1060, 310) },
            };

        if (n <= 16)
            // 2 spawns, independent zigzag lanes → 2 objectives
            return new()
            {
                new() { new(40, 80), new(260, 80), new(260, 400), new(560, 400),
                        new(560, 120), new(1060, 120) },
                new() { new(40, 540), new(820, 540), new(820, 320), new(1060, 320) },
            };

        // 17-20: 3 spawns merge → 1 final objective (Castle / final block)
        return new()
        {
            new() { new(40, 90),  new(560, 90),  new(560, 310), new(1060, 310) },
            new() { new(40, 310), new(1060, 310) },
            new() { new(40, 530), new(560, 530), new(560, 310), new(1060, 310) },
        };
    }

    // ─── Wave generation ────────────────────────────────────────────────
    // Wave count policy (per user request):
    //   • Regular stages         → 8 waves total
    //   • Mid-boss / Boss stages → 10 waves total (9 regular + 1 boss wave appended below)
    private static List<WaveDef> WavesFor(int stage)
    {
        var rand = new Random(stage * 7919);
        bool hasBossWave = HasMidBossFor(stage) || HasBossFor(stage);
        int waveCount = hasBossWave ? 9 : 8;
        int pathCount = PathFor(stage).Count;
        double baseInterval = Math.Max(0.30, 0.90 - stage * 0.012);
        var waves = new List<WaveDef>();

        for (int w = 0; w < waveCount; w++)
        {
            double wavePower = 10 + stage * 2.2 + w * 2.8;
            var wave = new WaveDef { TimeUntilNext = 22 + stage * 0.5 };

            // ── Primary enemy — split across multiple spawn lanes ──
            EnemyKind primary = PickPrimary(stage, rand);
            int primaryCount = (int)Math.Max(3, wavePower / EnemyWeight(primary));
            if (pathCount > 1)
            {
                // Spread the batch over two different lanes so enemies pour in
                // from several spawn points simultaneously (Arknights feel).
                int laneA = w % pathCount;
                int laneB = (laneA + 1) % pathCount;
                int half  = Math.Max(2, primaryCount / 2);
                wave.Entries.Add(new WaveEntry
                {
                    Enemy = primary, Count = half,
                    SpawnInterval = baseInterval, SpawnPath = laneA, InitialDelay = 0
                });
                wave.Entries.Add(new WaveEntry
                {
                    Enemy = primary, Count = primaryCount - half,
                    SpawnInterval = baseInterval, SpawnPath = laneB, InitialDelay = 0.6
                });
            }
            else
            {
                wave.Entries.Add(new WaveEntry
                {
                    Enemy = primary, Count = primaryCount,
                    SpawnInterval = baseInterval, SpawnPath = 0, InitialDelay = 0
                });
            }

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

    // ─── Environment effects (aligned to new 4-stage theme blocks) ─────
    private static List<EnvEffect> EffectsFor(int n)
    {
        var fx = new List<EnvEffect>();
        if (n is >= 9  and <= 12) fx.Add(EnvEffect.NarrowCorridor);              // Desert
        if (n is >= 13 and <= 16) { fx.Add(EnvEffect.LavaTiles); fx.Add(EnvEffect.NightVision); } // Volcano
        // Castle (17-20) has no special effect
        return fx;
    }

    // ─── Difficulty curves (rescaled for 20-stage campaign) ─────────────
    /// <summary>
    /// 적 체력 배율 — 1~4는 학습용으로 매우 쉽게(0.55~0.85),
    /// 5는 첫 중간보스 기준선(1.0), 이후 +0.13 씩 상승해 최종 stage 20 ≈ 2.95.
    /// (1: 0.55, 2: 0.65, 3: 0.75, 4: 0.85, 5: 1.00, …, 10: 1.65, 15: 2.30, 20: 2.95)
    /// </summary>
    private static double HpScaleFor(int n)
    {
        if (n <= 4) return 0.55 + (n - 1) * 0.10;   // 0.55, 0.65, 0.75, 0.85
        return 1.00 + (n - 5) * 0.13;               // 5=1.00, 10=1.65, 20=2.95
    }

    /// <summary>적 속도 배율 — 초반 둔하게, 후반은 약간 빠르게 (stage 20 ≈ 1.30).</summary>
    private static double SpeedScaleFor(int n)
    {
        if (n <= 4) return 0.85 + (n - 1) * 0.04;   // 0.85, 0.89, 0.93, 0.97
        return 1.00 + (n - 5) * 0.020;              // 5=1.00, 10=1.10, 20=1.30
    }

    // ─── Build list ─────────────────────────────────────────────────────
    private const int TotalStages = 20;

    private static List<StageDef> Build()
    {
        var list = new List<StageDef>();
        for (int n = 1; n <= TotalStages; n++)
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
