using KingdomRushClone.Data;
using KingdomRushClone.Game;
using KingdomRushClone.Managers;
using KingdomRushClone.Models;
using KingdomRushClone.Views;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace KingdomRushClone.Tests;

internal static class Program
{
    private static readonly EnemyKind[] ExpectedEnemyKinds =
    {
        EnemyKind.Normal,
        EnemyKind.Fast,
        EnemyKind.SplitBody,
        EnemyKind.SplitSmall,
        EnemyKind.Elite,
        EnemyKind.EliteCharge,
        EnemyKind.EliteRegenerator,
        EnemyKind.EliteWyvern,
        EnemyKind.MidBossNormal,
        EnemyKind.MidBossCharge,
        EnemyKind.MidBossSplit,
        EnemyKind.MidBossSpeed,
        EnemyKind.BossNormal,
        EnemyKind.BossCharge,
        EnemyKind.BossSplit,
        EnemyKind.BossSpeed
    };

    private sealed record ExpectedStageComposition(
        int Stage,
        int Normal,
        int Fast,
        int SplitBody,
        int Elite,
        int EliteCharge,
        EnemyKind? ExtraKind = null,
        int ExtraCount = 0,
        EnemyKind? MidBoss = null,
        EnemyKind? Boss = null,
        EnemyKind? ExtraKind2 = null,
        int ExtraCount2 = 0);

    private static readonly ExpectedStageComposition[] ExpectedStageCompositions =
    {
        new(1, 28, 14, 4, 1, 1),
        new(2, 35, 21, 7, 2, 2),
        new(3, 42, 28, 10, 4, 4, MidBoss: EnemyKind.MidBossNormal),
        new(4, 49, 35, 13, 4, 4),
        new(5, 56, 42, 16, 6, 6, Boss: EnemyKind.BossNormal),
        new(6, 35, 21, 7, 2, 1),
        new(7, 42, 28, 10, 4, 2, EnemyKind.EliteRegenerator, 3),
        new(8, 49, 35, 13, 4, 4, EnemyKind.EliteRegenerator, 4, EnemyKind.MidBossCharge),
        new(9, 56, 42, 16, 6, 4, EnemyKind.EliteRegenerator, 4),
        new(10, 63, 49, 19, 6, 6, EnemyKind.EliteRegenerator, 6, Boss: EnemyKind.BossCharge),
        new(11, 35, 21, 10, 2, 2),
        new(12, 42, 28, 10, 4, 3, EnemyKind.EliteWyvern, 3, ExtraKind2: EnemyKind.EliteRegenerator, ExtraCount2: 3),
        new(13, 49, 35, 13, 4, 4, EnemyKind.EliteWyvern, 4, MidBoss: EnemyKind.MidBossSplit, ExtraKind2: EnemyKind.EliteRegenerator, ExtraCount2: 4),
        new(14, 56, 42, 16, 6, 4, EnemyKind.EliteWyvern, 4, ExtraKind2: EnemyKind.EliteRegenerator, ExtraCount2: 4),
        new(15, 63, 49, 19, 6, 6, EnemyKind.EliteWyvern, 6, Boss: EnemyKind.BossSplit, ExtraKind2: EnemyKind.EliteRegenerator, ExtraCount2: 6),
        new(16, 35, 21, 10, 4, 3),
        new(17, 42, 28, 10, 4, 4, EnemyKind.EliteWyvern, 3),
        new(18, 49, 35, 13, 4, 4, EnemyKind.EliteWyvern, 4, MidBoss: EnemyKind.MidBossSpeed),
        new(19, 56, 42, 16, 6, 6, EnemyKind.EliteWyvern, 4),
        new(20, 63, 49, 19, 6, 7, EnemyKind.EliteWyvern, 6, Boss: EnemyKind.BossSpeed)
    };

    [STAThread]
    private static int Main()
    {
        var tests = new (string Name, Action Test)[]
        {
            ("enemy catalog matches sixteen asset plan", EnemyCatalogMatchesSixteenAssetPlan),
            ("stage catalog uses twenty stages and eight waves", StageCatalogUsesTwentyStagesAndEightWaves),
            ("stage wave totals match composition table", StageWaveTotalsMatchCompositionTable),
            ("enemy visuals fallback to code drawn controls", EnemyVisualsFallbackToCodeDrawnControls),
            ("enemy visuals prefer sprite files when present", EnemyVisualsPreferSpriteFilesWhenPresent),
            ("enemy visuals use sprite sheet when individual sprites are missing", EnemyVisualsUseSpriteSheetWhenIndividualSpritesAreMissing),
            ("enemy hitboxes and visuals are enlarged", EnemyHitboxesAndVisualsAreEnlarged),
            ("enemy gold rewards are doubled", EnemyGoldRewardsAreDoubled),
            ("enemy base hp values match planned ratios", EnemyBaseHpValuesMatchPlannedRatios),
            ("enemy base speed values match balance plan", EnemyBaseSpeedValuesMatchBalancePlan),
            ("enemy physical resist values match balance plan", EnemyPhysicalResistValuesMatchBalancePlan),
            ("charge enemies gain physical resist at half hp", ChargeEnemiesGainPhysicalResistAtHalfHp),
            ("speed bosses apply planned global speed buffs", SpeedBossesApplyPlannedGlobalSpeedBuffs),
            ("regenerator pulse creates visible healing ring", RegeneratorPulseCreatesVisibleHealingRing),
            ("temporary pause stops wave countdown", TemporaryPauseStopsWaveCountdown),
            ("enemy ability summaries describe special mechanics", EnemyAbilitySummariesDescribeSpecialMechanics),
            ("stage intro enemy entries include popup metadata", StageIntroEnemyEntriesIncludePopupMetadata),
            ("stage intro enemy tabs separate new and returning enemies", StageIntroEnemyTabsSeparateNewAndReturningEnemies),
            ("chapter boss schedule uses four role families", ChapterBossScheduleUsesFourRoleFamilies),
            ("chapter hp scale uses twenty percent steps", ChapterHpScaleUsesTwentyPercentSteps),
            ("split boss creates split mid bosses", SplitBossCreatesSplitMidBosses),
            ("split mid boss creates split bodies", SplitMidBossCreatesSplitBodies),
            ("split body creates split small enemies", SplitBodyCreatesSplitSmallEnemies),
            ("elite has no special ability", EliteHasNoSpecialAbility),
            ("wyvern elite ignores explosive damage", WyvernEliteIgnoresExplosiveDamage),
            ("wyvern elite passes through barracks soldiers", WyvernElitePassesThroughBarracksSoldiers),
            ("meteor rewards killed enemy once", MeteorRewardsKilledEnemyOnce)
        };

        var failures = 0;
        foreach (var (name, test) in tests)
        {
            try
            {
                test();
                Console.WriteLine($"PASS  {name}");
            }
            catch (Exception ex)
            {
                failures++;
                Console.WriteLine($"FAIL  {name}: {ex.Message}");
            }
        }

        Console.WriteLine();
        Console.WriteLine(failures == 0
            ? $"All {tests.Length} tests passed."
            : $"{failures}/{tests.Length} tests failed.");

        return failures == 0 ? 0 : 1;
    }

    private static void EnemyCatalogMatchesSixteenAssetPlan()
    {
        var expected = ExpectedEnemyKinds.ToHashSet();

        AssertEqual(16, Enum.GetValues<EnemyKind>().Length, "enemy enum count");
        AssertEqual(16, EnemyCatalog.Enemies.Count, "enemy catalog count");
        AssertEqual(4, ExpectedEnemyKinds.Count(k => k.ToString().StartsWith("Elite", StringComparison.Ordinal)), "elite kind count");
        Assert(!Enum.GetNames<EnemyKind>().Any(name => name.Contains("Resist", StringComparison.OrdinalIgnoreCase)),
            "elite resist kind should not exist");

        foreach (var kind in ExpectedEnemyKinds)
            Assert(EnemyCatalog.Enemies.ContainsKey(kind), $"catalog should include {kind}");

        foreach (var kind in EnemyCatalog.Enemies.Keys)
            Assert(expected.Contains(kind), $"catalog should not include old or chapter variant kind {kind}");
    }

    private static void StageCatalogUsesTwentyStagesAndEightWaves()
    {
        var expected = ExpectedEnemyKinds.ToHashSet();

        AssertEqual(20, StageCatalog.Stages.Count, "stage count");
        foreach (var stage in StageCatalog.Stages)
        {
            AssertEqual(8, stage.Waves.Count, $"stage {stage.Number} wave count");
            foreach (var entry in stage.Waves.SelectMany(wave => wave.Entries))
                Assert(expected.Contains(entry.Enemy), $"stage {stage.Number} should only use planned enemy kinds");
        }
    }

    private static void StageWaveTotalsMatchCompositionTable()
    {
        foreach (var expected in ExpectedStageCompositions)
        {
            var stage = StageCatalog.Stages.Single(s => s.Number == expected.Stage);
            var counts = stage.Waves
                .SelectMany(wave => wave.Entries)
                .GroupBy(entry => entry.Enemy)
                .ToDictionary(group => group.Key, group => group.Sum(entry => entry.Count));

            AssertEnemyCount(counts, expected.Stage, EnemyKind.Normal, expected.Normal);
            AssertEnemyCount(counts, expected.Stage, EnemyKind.Fast, expected.Fast);
            AssertEnemyCount(counts, expected.Stage, EnemyKind.SplitBody, expected.SplitBody);
            AssertEnemyCount(counts, expected.Stage, EnemyKind.Elite, expected.Elite);
            AssertEnemyCount(counts, expected.Stage, EnemyKind.EliteCharge, expected.EliteCharge);

            if (expected.ExtraKind != null)
                AssertEnemyCount(counts, expected.Stage, expected.ExtraKind.Value, expected.ExtraCount);

            if (expected.ExtraKind2 != null)
                AssertEnemyCount(counts, expected.Stage, expected.ExtraKind2.Value, expected.ExtraCount2);

            if (expected.MidBoss != null)
                AssertEnemyCount(counts, expected.Stage, expected.MidBoss.Value, 1);

            if (expected.Boss != null)
                AssertEnemyCount(counts, expected.Stage, expected.Boss.Value, 1);

            var expectedKinds = ExpectedKindsFor(expected).ToHashSet();
            foreach (var (kind, count) in counts)
                if (count > 0)
                    Assert(expectedKinds.Contains(kind), $"stage {expected.Stage} should not spawn {kind}");
        }
    }

    private static void EnemyVisualsFallbackToCodeDrawnControls()
    {
        var spritePaths = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Assets", "Enemies", $"{EnemyKind.Fast}.png"),
            Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Enemies", $"{EnemyKind.Fast}.png"),
            Path.Combine(AppContext.BaseDirectory, "Assets", "Enemies", "EnemySpriteSheet.png"),
            Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Enemies", "EnemySpriteSheet.png")
        }.Distinct().ToArray();
        var originals = spritePaths
            .Where(File.Exists)
            .ToDictionary(path => path, File.ReadAllBytes);

        foreach (var path in originals.Keys)
            File.Delete(path);

        try
        {
            var sprite = EnemyFallbackImageFactory.CreateSpriteVisual(EnemyKind.Fast, 40);
            var icon = EnemyFallbackImageFactory.CreateIconVisual(EnemyKind.Fast, 58);

            Assert(sprite is not Image, "missing sprite should use code-drawn in-game visual");
            Assert(icon is not Image, "missing sprite should use matching code-drawn popup visual");
            AssertEqual(40.0, sprite.Width, "sprite fallback width");
            AssertEqual(40.0, sprite.Height, "sprite fallback height");
            AssertEqual(58.0, icon.Width, "icon fallback width");
            AssertEqual(58.0, icon.Height, "icon fallback height");
        }
        finally
        {
            foreach (var (path, bytes) in originals)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllBytes(path, bytes);
            }
        }
    }

    private static void EnemyVisualsPreferSpriteFilesWhenPresent()
    {
        var spriteDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Enemies");
        var spritePath = Path.Combine(spriteDir, $"{EnemyKind.Normal}.png");
        byte[]? originalBytes = File.Exists(spritePath) ? File.ReadAllBytes(spritePath) : null;

        Directory.CreateDirectory(spriteDir);
        File.WriteAllBytes(spritePath, Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+/p9sAAAAASUVORK5CYII="));

        try
        {
            var sprite = EnemyFallbackImageFactory.CreateSpriteVisual(EnemyKind.Normal, 40);
            var icon = EnemyFallbackImageFactory.CreateIconVisual(EnemyKind.Normal, 58);

            Assert(sprite is Image, "sprite file should be used for in-game visual");
            Assert(icon is Image, "sprite file should be used for popup visual");
            AssertEqual(40.0, sprite.Width, "sprite file visual width");
            AssertEqual(58.0, icon.Width, "sprite file icon width");
        }
        finally
        {
            if (originalBytes != null)
                File.WriteAllBytes(spritePath, originalBytes);
            else if (File.Exists(spritePath))
                File.Delete(spritePath);
        }
    }

    private static void EnemyVisualsUseSpriteSheetWhenIndividualSpritesAreMissing()
    {
        var roots = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Assets", "Enemies"),
            Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Enemies")
        }.Distinct().ToArray();
        var spritePaths = roots
            .SelectMany(root => new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" }
                .Select(extension => Path.Combine(root, $"{EnemyKind.EliteWyvern}{extension}")))
            .Distinct()
            .ToArray();
        var originals = spritePaths
            .Where(File.Exists)
            .ToDictionary(path => path, File.ReadAllBytes);

        foreach (var path in originals.Keys)
            File.Delete(path);

        try
        {
            var sprite = EnemyFallbackImageFactory.CreateSpriteVisual(EnemyKind.EliteWyvern, 44);
            var icon = EnemyFallbackImageFactory.CreateIconVisual(EnemyKind.EliteWyvern, 58);

            if (sprite is not Image spriteImage)
                throw new InvalidOperationException("missing individual sprite should use sprite sheet in-game visual");
            if (icon is not Image iconImage)
                throw new InvalidOperationException("missing individual sprite should use sprite sheet popup visual");
            Assert(spriteImage.Source is CroppedBitmap, "sprite sheet visual should be cropped");
            Assert(iconImage.Source is CroppedBitmap, "sprite sheet icon should be cropped");
            AssertEqual(44.0, sprite.Width, "sprite sheet visual width");
            AssertEqual(58.0, icon.Width, "sprite sheet icon width");
        }
        finally
        {
            foreach (var (path, bytes) in originals)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllBytes(path, bytes);
            }
        }
    }

    private static void EnemyHitboxesAndVisualsAreEnlarged()
    {
        var expected = new Dictionary<EnemyKind, double>
        {
            [EnemyKind.Normal] = 11,
            [EnemyKind.Fast] = 11,
            [EnemyKind.SplitBody] = 14,
            [EnemyKind.SplitSmall] = 8,
            [EnemyKind.Elite] = 15,
            [EnemyKind.EliteCharge] = 15,
            [EnemyKind.EliteRegenerator] = 14,
            [EnemyKind.EliteWyvern] = 13,
            [EnemyKind.MidBossNormal] = 21,
            [EnemyKind.MidBossCharge] = 21,
            [EnemyKind.MidBossSplit] = 22,
            [EnemyKind.MidBossSpeed] = 20,
            [EnemyKind.BossNormal] = 28,
            [EnemyKind.BossCharge] = 28,
            [EnemyKind.BossSplit] = 29,
            [EnemyKind.BossSpeed] = 27
        };

        foreach (var (kind, radius) in expected)
            AssertClose(radius, EnemyCatalog.Enemies[kind].Radius, $"{kind} collision radius");

        var normal = EnemyCatalog.Enemies[EnemyKind.Normal];
        AssertClose(normal.Radius * 2.4 * 1.5, GamePage.EnemySpriteSizeFor(normal), "normal sprite display size");
        AssertClose(normal.Radius * 2.0 * 1.5, GamePage.EnemyHealthBarWidthFor(normal), "normal hp bar display width");
    }

    private static void EnemyGoldRewardsAreDoubled()
    {
        var expected = new Dictionary<EnemyKind, int>
        {
            [EnemyKind.Normal] = 16,
            [EnemyKind.Fast] = 14,
            [EnemyKind.SplitBody] = 28,
            [EnemyKind.SplitSmall] = 8,
            [EnemyKind.Elite] = 60,
            [EnemyKind.EliteCharge] = 48,
            [EnemyKind.EliteRegenerator] = 64,
            [EnemyKind.EliteWyvern] = 56,
            [EnemyKind.MidBossNormal] = 200,
            [EnemyKind.MidBossCharge] = 220,
            [EnemyKind.MidBossSplit] = 240,
            [EnemyKind.MidBossSpeed] = 220,
            [EnemyKind.BossNormal] = 520,
            [EnemyKind.BossCharge] = 560,
            [EnemyKind.BossSplit] = 600,
            [EnemyKind.BossSpeed] = 560
        };

        foreach (var (kind, reward) in expected)
            AssertEqual(reward, EnemyCatalog.Enemies[kind].GoldReward, $"{kind} gold reward");
    }

    private static void EnemyBaseHpValuesMatchPlannedRatios()
    {
        var expected = new Dictionary<EnemyKind, double>
        {
            [EnemyKind.Normal] = 60,
            [EnemyKind.Fast] = 45,
            [EnemyKind.SplitBody] = 150,
            [EnemyKind.SplitSmall] = 48,
            [EnemyKind.Elite] = 300,
            [EnemyKind.EliteCharge] = 210,
            [EnemyKind.EliteRegenerator] = 240,
            [EnemyKind.EliteWyvern] = 240,
            [EnemyKind.MidBossNormal] = 420,
            [EnemyKind.MidBossCharge] = 330,
            [EnemyKind.MidBossSplit] = 390,
            [EnemyKind.MidBossSpeed] = 315,
            [EnemyKind.BossNormal] = 900,
            [EnemyKind.BossCharge] = 660,
            [EnemyKind.BossSplit] = 870,
            [EnemyKind.BossSpeed] = 600
        };

        foreach (var (kind, hp) in expected)
            AssertClose(hp, EnemyCatalog.Enemies[kind].MaxHp, $"{kind} base hp");
    }

    private static void EnemyBaseSpeedValuesMatchBalancePlan()
    {
        var expected = new Dictionary<EnemyKind, double>
        {
            [EnemyKind.Normal] = 80,
            [EnemyKind.Fast] = 160,
            [EnemyKind.SplitBody] = 80,
            [EnemyKind.SplitSmall] = 80,
            [EnemyKind.Elite] = 80,
            [EnemyKind.EliteCharge] = 110,
            [EnemyKind.EliteRegenerator] = 80,
            [EnemyKind.EliteWyvern] = 150,
            [EnemyKind.MidBossNormal] = 70,
            [EnemyKind.MidBossCharge] = 100,
            [EnemyKind.MidBossSplit] = 70,
            [EnemyKind.MidBossSpeed] = 140,
            [EnemyKind.BossNormal] = 60,
            [EnemyKind.BossCharge] = 90,
            [EnemyKind.BossSplit] = 60,
            [EnemyKind.BossSpeed] = 130
        };

        foreach (var (kind, speed) in expected)
            AssertClose(speed, EnemyCatalog.Enemies[kind].Speed, $"{kind} base speed");
    }

    private static void EnemyPhysicalResistValuesMatchBalancePlan()
    {
        var expected = new Dictionary<EnemyKind, double>
        {
            [EnemyKind.Normal] = 0.15,
            [EnemyKind.Elite] = 0.20,
            [EnemyKind.EliteCharge] = 0.25,
            [EnemyKind.EliteWyvern] = 0.20,
            [EnemyKind.MidBossNormal] = 0.30,
            [EnemyKind.MidBossCharge] = 0.30,
            [EnemyKind.BossNormal] = 0.35,
            [EnemyKind.BossCharge] = 0.35
        };

        foreach (var (kind, resist) in expected)
            AssertClose(resist, EnemyCatalog.Enemies[kind].PhysicalResist, $"{kind} physical resist");
    }

    private static void ChargeEnemiesGainPhysicalResistAtHalfHp()
    {
        var game = CreateGame();
        var expected = new Dictionary<EnemyKind, (double Multiplier, double Duration, double ResistBonus, bool Persistent)>
        {
            [EnemyKind.EliteCharge] = (1.5, 3.0, 0.10, false),
            [EnemyKind.MidBossCharge] = (2.0, 5.0, 0.13, false),
            [EnemyKind.BossCharge] = (2.5, 0.0, 0.15, true)
        };

        foreach (var (kind, charge) in expected)
        {
            var enemy = CreateEnemy(game, kind);
            var baseResist = enemy.Def.PhysicalResist;

            AssertClose(baseResist, enemy.EffectivePhysicalResist, $"{kind} physical resist before charge");
            AssertClose(charge.Multiplier, enemy.Def.ChargeSpeedMultiplier, $"{kind} charge speed multiplier");
            AssertClose(charge.Duration, enemy.Def.ChargeDuration, $"{kind} charge duration");
            AssertClose(charge.ResistBonus, enemy.Def.ChargePhysicalResistBonus, $"{kind} charge physical resist bonus");
            AssertEqual(charge.Persistent, enemy.Def.ChargeSpeedPersists, $"{kind} charge persistence");

            enemy.ApplyDamage(enemy.MaxHp * 0.5, DamageType.True);

            Assert(enemy.ChargeTriggered, $"{kind} charge should trigger at half hp");
            AssertClose(baseResist + charge.ResistBonus, enemy.EffectivePhysicalResist, $"{kind} physical resist after charge");

            var hp = enemy.Hp;
            enemy.ApplyDamage(10, DamageType.Physical);
            AssertClose(hp - 10 * (1.0 - enemy.EffectivePhysicalResist), enemy.Hp, $"{kind} physical damage after charge");
        }

        var normal = CreateEnemy(game, EnemyKind.Normal);
        normal.ApplyDamage(normal.MaxHp * 0.5, DamageType.True);

        Assert(!normal.ChargeTriggered, "normal enemy should not trigger charge");
        AssertClose(normal.Def.PhysicalResist, normal.EffectivePhysicalResist, "normal physical resist after half hp");
    }

    private static void SpeedBossesApplyPlannedGlobalSpeedBuffs()
    {
        var game = CreateGame();
        var speedMidBoss = CreateEnemy(game, EnemyKind.MidBossSpeed);
        var ally = CreateEnemy(game, EnemyKind.Normal);
        speedMidBoss.Speed = 0;
        ally.Speed = 0;

        game.Tick(4.9);
        AssertClose(0, ally.ExternalSpeedBonus, "speed mid boss buff before interval");

        game.Tick(0.11);
        AssertClose(0.15, ally.ExternalSpeedBonus, "speed mid boss buff after interval");

        game.Tick(2.9);
        AssertClose(0.15, ally.ExternalSpeedBonus, "speed mid boss buff during duration");

        game.Tick(0.11);
        AssertClose(0, ally.ExternalSpeedBonus, "speed mid boss buff after duration");

        var bossGame = CreateGame();
        var speedBoss = CreateEnemy(bossGame, EnemyKind.BossSpeed);
        var bossAlly = CreateEnemy(bossGame, EnemyKind.Normal);
        speedBoss.Speed = 0;
        bossAlly.Speed = 0;

        bossGame.Tick(0.01);

        AssertClose(0.20, bossAlly.ExternalSpeedBonus, "speed boss global speed buff");
        AssertClose(0, speedBoss.ExternalSpeedBonus, "speed boss should not buff itself");
    }

    private static void RegeneratorPulseCreatesVisibleHealingRing()
    {
        var game = CreateGame();
        var regenerator = CreateEnemy(game, EnemyKind.EliteRegenerator);
        regenerator.Hp = regenerator.MaxHp * 0.5;
        regenerator.RegenerateTimer = 0;

        game.Tick(0.01);

        AssertEqual(1, game.Effects.Count, "regenerator healing effect count");
        var effect = game.Effects[0];
        AssertClose(regenerator.Pos.X, effect.Pos.X, "regenerator healing ring x");
        AssertClose(regenerator.Pos.Y, effect.Pos.Y, "regenerator healing ring y");
        AssertClose(regenerator.Def.RegenerateRadius, effect.Radius, "regenerator healing ring radius");
        AssertEqual("#22C55E", effect.ColorHex, "regenerator healing ring color");
        Assert(effect.TimeLeft > 0, "regenerator healing ring should stay visible after tick");
    }

    private static void TemporaryPauseStopsWaveCountdown()
    {
        var game = CreateGame();
        game.Stage.Waves.Add(new WaveDef { TimeUntilNext = 10 });
        var startingCountdown = game.Spawner.NextWaveCountdown;

        game.BeginTemporaryPause();
        game.Tick(1.0);

        AssertClose(startingCountdown, game.Spawner.NextWaveCountdown, "wave countdown while temporarily paused");

        game.EndTemporaryPause();
        game.Tick(1.0);

        AssertClose(startingCountdown - 1.0, game.Spawner.NextWaveCountdown, "wave countdown after temporary pause ends");
    }

    private static void EnemyAbilitySummariesDescribeSpecialMechanics()
    {
        AssertEqual("", EnemyAbilityTextBuilder.Describe(EnemyCatalog.Enemies[EnemyKind.Normal]), "normal ability summary");
        AssertEqual("", EnemyAbilityTextBuilder.Describe(EnemyCatalog.Enemies[EnemyKind.Fast]), "fast ability summary");
        AssertEqual("", EnemyAbilityTextBuilder.Describe(EnemyCatalog.Enemies[EnemyKind.SplitSmall]), "split small ability summary");

        AssertTextContains(EnemyAbilityTextBuilder.Describe(EnemyCatalog.Enemies[EnemyKind.SplitBody]), "분열체 x3", "split body ability summary");
        AssertEqual("", EnemyAbilityTextBuilder.Describe(EnemyCatalog.Enemies[EnemyKind.Elite]), "elite ability summary");
        AssertTextContains(EnemyAbilityTextBuilder.Describe(EnemyCatalog.Enemies[EnemyKind.EliteCharge]), "이속 x1.5", "elite charge speed summary");
        AssertTextContains(EnemyAbilityTextBuilder.Describe(EnemyCatalog.Enemies[EnemyKind.EliteCharge]), "물리저항 +10%", "elite charge resist summary");
        AssertTextContains(EnemyAbilityTextBuilder.Describe(EnemyCatalog.Enemies[EnemyKind.MidBossCharge]), "5초 동안 이속 x2", "mid boss charge speed summary");
        AssertTextContains(EnemyAbilityTextBuilder.Describe(EnemyCatalog.Enemies[EnemyKind.MidBossCharge]), "물리저항 +13%", "mid boss charge resist summary");
        AssertTextContains(EnemyAbilityTextBuilder.Describe(EnemyCatalog.Enemies[EnemyKind.BossCharge]), "이속 x2.5 유지", "boss charge speed summary");
        AssertTextContains(EnemyAbilityTextBuilder.Describe(EnemyCatalog.Enemies[EnemyKind.BossCharge]), "물리저항 +15%", "boss charge resist summary");
        AssertTextContains(EnemyAbilityTextBuilder.Describe(EnemyCatalog.Enemies[EnemyKind.EliteRegenerator]), "3초마다 자신 HP 5% 회복, 주변 아군 HP 2% 회복", "regenerator ability summary");
        AssertTextContains(EnemyAbilityTextBuilder.Describe(EnemyCatalog.Enemies[EnemyKind.EliteWyvern]), "폭발 면역", "wyvern explosive summary");
        AssertTextContains(EnemyAbilityTextBuilder.Describe(EnemyCatalog.Enemies[EnemyKind.EliteWyvern]), "병영 통과", "wyvern barracks summary");
        AssertTextContains(EnemyAbilityTextBuilder.Describe(EnemyCatalog.Enemies[EnemyKind.MidBossSpeed]), "5초마다 전체 이속 +15%", "speed mid boss ability summary");
        AssertTextContains(EnemyAbilityTextBuilder.Describe(EnemyCatalog.Enemies[EnemyKind.BossSpeed]), "전체 이속 +20%", "speed boss ability summary");
    }

    private static void StageIntroEnemyEntriesIncludePopupMetadata()
    {
        var stage2 = StageCatalog.Stages.Single(stage => stage.Number == 2);
        var stage2Entries = StageIntroEnemyInfoBuilder.Build(stage2).ToList();
        var normal = stage2Entries.Single(entry => entry.Kind == EnemyKind.Normal);
        var fast = stage2Entries.Single(entry => entry.Kind == EnemyKind.Fast);

        Assert(!normal.IsNewAppearance, "stage 2 normal should be returning");
        Assert(!fast.IsNewAppearance, "stage 2 fast should be returning");
        AssertEqual("enemy_fast", fast.CodeName, "fast code name");
        AssertEqual("HP 75%", fast.HpText, "fast hp text");
        AssertEqual("이동속도 x2", fast.SpeedText, "fast speed text");
        AssertEqual("Chapter 1 / HP x1", StageIntroEnemyInfoBuilder.StageSubtitle(stage2), "stage 2 subtitle");

        var stage3 = StageCatalog.Stages.Single(stage => stage.Number == 3);
        var split = StageIntroEnemyInfoBuilder.Build(stage3).Single(entry => entry.Kind == EnemyKind.SplitBody);

        Assert(!split.IsNewAppearance, "stage 3 split body should be returning");
        AssertEqual("enemy_split_body", split.CodeName, "split body code name");
        AssertEqual("HP 250%", split.HpText, "split body hp text");
        var midBoss = StageIntroEnemyInfoBuilder.Build(stage3).Single(entry => entry.Kind == EnemyKind.MidBossNormal);
        Assert(midBoss.IsNewAppearance, "stage 3 normal mid boss should be new");
        AssertEqual("Chapter 1 / HP x1", StageIntroEnemyInfoBuilder.StageSubtitle(stage3), "stage 3 subtitle");

        var stage6 = StageCatalog.Stages.Single(stage => stage.Number == 6);
        AssertEqual("Chapter 2 / HP x1.2", StageIntroEnemyInfoBuilder.StageSubtitle(stage6), "stage 6 subtitle");
    }

    private static void StageIntroEnemyTabsSeparateNewAndReturningEnemies()
    {
        var stage3 = StageCatalog.Stages.Single(stage => stage.Number == 3);
        var newEnemies = StageIntroEnemyInfoBuilder.BuildNew(stage3).Select(entry => entry.Kind).ToHashSet();
        var returningEnemies = StageIntroEnemyInfoBuilder.BuildReturning(stage3).Select(entry => entry.Kind).ToHashSet();

        Assert(!newEnemies.Contains(EnemyKind.SplitBody), "stage 3 new tab should not include split body");
        Assert(!newEnemies.Contains(EnemyKind.Elite), "stage 3 new tab should not include elite");
        Assert(newEnemies.Contains(EnemyKind.MidBossNormal), "stage 3 new tab should include normal mid boss");
        Assert(!newEnemies.Contains(EnemyKind.Normal), "stage 3 new tab should not include normal");
        Assert(!newEnemies.Contains(EnemyKind.Fast), "stage 3 new tab should not include fast");

        Assert(returningEnemies.Contains(EnemyKind.Normal), "stage 3 returning tab should include normal");
        Assert(returningEnemies.Contains(EnemyKind.Fast), "stage 3 returning tab should include fast");
        Assert(returningEnemies.Contains(EnemyKind.SplitBody), "stage 3 returning tab should include split body");
        Assert(returningEnemies.Contains(EnemyKind.Elite), "stage 3 returning tab should include elite");
        Assert(!newEnemies.Overlaps(returningEnemies), "new and returning tabs should not overlap");

        var stage1 = StageCatalog.Stages.Single(stage => stage.Number == 1);
        Assert(StageIntroEnemyInfoBuilder.BuildReturning(stage1).Count == 0, "stage 1 returning tab should be empty");
    }

    private static void ChapterBossScheduleUsesFourRoleFamilies()
    {
        AssertStageBoss(3,  EnemyKind.MidBossNormal, isMidBoss: true);
        AssertStageBoss(5,  EnemyKind.BossNormal,    isBoss: true);
        AssertStageBoss(8,  EnemyKind.MidBossCharge, isMidBoss: true);
        AssertStageBoss(10, EnemyKind.BossCharge,    isBoss: true);
        AssertStageBoss(13, EnemyKind.MidBossSplit,  isMidBoss: true);
        AssertStageBoss(15, EnemyKind.BossSplit,     isBoss: true);
        AssertStageBoss(18, EnemyKind.MidBossSpeed,  isMidBoss: true);
        AssertStageBoss(20, EnemyKind.BossSpeed,     isBoss: true);
    }

    private static void ChapterHpScaleUsesTwentyPercentSteps()
    {
        AssertClose(1.0,   StageCatalog.Stages.Single(s => s.Number == 1).EnemyHpScale,  "chapter 1 hp scale");
        AssertClose(1.2,   StageCatalog.Stages.Single(s => s.Number == 6).EnemyHpScale,  "chapter 2 hp scale");
        AssertClose(1.44,  StageCatalog.Stages.Single(s => s.Number == 11).EnemyHpScale, "chapter 3 hp scale");
        AssertClose(1.728, StageCatalog.Stages.Single(s => s.Number == 16).EnemyHpScale, "chapter 4 hp scale");
    }

    private static void SplitBossCreatesSplitMidBosses()
    {
        var game = CreateGame();
        var boss = CreateEnemy(game, EnemyKind.BossSplit);
        KillAndTick(game, boss);

        AssertEqual(2, game.Enemies.Count, "split boss child count");
        AssertAllKind(game, EnemyKind.MidBossSplit);
    }

    private static void SplitMidBossCreatesSplitBodies()
    {
        var game = CreateGame();
        var midBoss = CreateEnemy(game, EnemyKind.MidBossSplit);
        KillAndTick(game, midBoss);

        AssertEqual(2, game.Enemies.Count, "split mid boss child count");
        AssertAllKind(game, EnemyKind.SplitBody);
    }

    private static void SplitBodyCreatesSplitSmallEnemies()
    {
        var game = CreateGame();
        var splitBody = CreateEnemy(game, EnemyKind.SplitBody);
        KillAndTick(game, splitBody);

        AssertEqual(3, game.Enemies.Count, "split body child count");
        AssertAllKind(game, EnemyKind.SplitSmall);
    }

    private static void EliteHasNoSpecialAbility()
    {
        var game = CreateGame();
        var elite = CreateEnemy(game, EnemyKind.Elite);
        var normal = CreateEnemy(game, EnemyKind.Normal);
        normal.Pos = elite.Pos;

        AssertEqual(0, elite.ShieldCharges, "elite shield charges");
        AssertClose(0, elite.Def.AuraSpeedBonus, "elite aura speed bonus");
        AssertClose(0, elite.Def.AuraRadius, "elite aura radius");

        var hp = elite.Hp;
        elite.ApplyDamage(10, DamageType.Physical);
        AssertClose(hp - 10 * (1.0 - elite.Def.PhysicalResist), elite.Hp, "elite should take physical damage immediately");

        game.Tick(0.01);

        AssertClose(0, normal.ExternalSpeedBonus, "elite should not speed up nearby enemies");
    }

    private static void WyvernEliteIgnoresExplosiveDamage()
    {
        var game = CreateGame();
        var wyvern = CreateEnemy(game, EnemyKind.EliteWyvern);
        var hp = wyvern.Hp;

        wyvern.ApplyDamage(30, DamageType.Explosive);
        AssertClose(hp, wyvern.Hp, "wyvern hp after explosive damage");

        wyvern.ApplyDamage(10, DamageType.Physical);
        AssertClose(hp - 10 * (1.0 - wyvern.Def.PhysicalResist), wyvern.Hp, "wyvern hp after physical damage");

        hp = wyvern.Hp;
        wyvern.ApplyDamage(100, DamageType.Magic);
        AssertClose(hp - 100 * (1.0 - wyvern.Def.MagicResist), wyvern.Hp, "wyvern hp after magic damage");

        hp = wyvern.Hp;
        wyvern.ApplyDamage(10, DamageType.True);
        AssertClose(hp - 10, wyvern.Hp, "wyvern hp after true damage");
    }

    private static void WyvernElitePassesThroughBarracksSoldiers()
    {
        var game = CreateGame();
        var wyvern = CreateEnemy(game, EnemyKind.EliteWyvern);
        var soldier = new Soldier
        {
            Pos = wyvern.Pos,
            RallyPos = wyvern.Pos,
            Hp = 50,
            MaxHp = 50,
            Damage = 10,
            AttackInterval = 1.0,
            EngageRadius = 30,
            Alive = true
        };

        soldier.Tick(0.1, game);

        Assert(soldier.Target == null, "soldier should not target wyvern elite");
        Assert(wyvern.EngagedBy == null, "wyvern elite should not be engaged by soldier");

        soldier.Target = wyvern;
        wyvern.EngagedBy = soldier;
        wyvern.EngageTimer = 0.5;
        var x = wyvern.Pos.X;

        wyvern.Tick(0.1);

        Assert(soldier.Target == null, "wyvern elite should clear soldier target when already engaged");
        Assert(wyvern.EngagedBy == null, "wyvern elite should clear existing engagement");
        Assert(wyvern.Pos.X > x, "wyvern elite should keep moving through barracks");
    }

    private static void MeteorRewardsKilledEnemyOnce()
    {
        var game = CreateGame();
        var enemy = CreateEnemy(game, EnemyKind.Normal);

        Assert(game.CastMeteor(enemy.Pos), "meteor should cast");
        game.Tick(0.01);

        int expectedGold = EnemyCatalog.Enemies[EnemyKind.Normal].GoldReward
            + (int)SaveManager.TechEffect(TechId.KillGoldBonus);
        AssertEqual(expectedGold, game.Gold, "meteor kill gold");
        AssertEqual(0, game.Enemies.Count, "dead enemy should be removed");
    }

    private static void AssertStageBoss(int stageNumber, EnemyKind expected, bool isMidBoss = false, bool isBoss = false)
    {
        var stage = StageCatalog.Stages.Single(s => s.Number == stageNumber);

        AssertEqual(isMidBoss, stage.HasMidBoss, $"stage {stageNumber} mid boss flag");
        AssertEqual(isBoss, stage.HasBoss, $"stage {stageNumber} boss flag");
        AssertContains(stage, expected, $"stage {stageNumber} wave plan");
    }

    private static IEnumerable<EnemyKind> ExpectedKindsFor(ExpectedStageComposition expected)
    {
        if (expected.Normal > 0) yield return EnemyKind.Normal;
        if (expected.Fast > 0) yield return EnemyKind.Fast;
        if (expected.SplitBody > 0) yield return EnemyKind.SplitBody;
        if (expected.Elite > 0) yield return EnemyKind.Elite;
        if (expected.EliteCharge > 0) yield return EnemyKind.EliteCharge;
        if (expected.ExtraKind != null && expected.ExtraCount > 0) yield return expected.ExtraKind.Value;
        if (expected.ExtraKind2 != null && expected.ExtraCount2 > 0) yield return expected.ExtraKind2.Value;
        if (expected.MidBoss != null) yield return expected.MidBoss.Value;
        if (expected.Boss != null) yield return expected.Boss.Value;
    }

    private static void AssertEnemyCount(
        IReadOnlyDictionary<EnemyKind, int> counts,
        int stage,
        EnemyKind kind,
        int expected)
    {
        counts.TryGetValue(kind, out var actual);
        AssertEqual(expected, actual, $"stage {stage} {kind} count");
    }

    private static GameEngine CreateGame()
    {
        var stage = new StageDef
        {
            Number = 99,
            Name = "Test",
            StartingGold = 0,
            StartingLives = 20,
            Paths = { new List<Vec2> { new(0, 0), new(200, 0) } },
            EnemyHpScale = 1.0,
            EnemySpeedScale = 1.0
        };

        return new GameEngine(stage);
    }

    private static EnemyInstance CreateEnemy(GameEngine game, EnemyKind kind)
    {
        var def = EnemyCatalog.Enemies[kind];
        var enemy = game.CreateEnemy(def, new Vec2(40, 0), game.Stage.Paths[0], 0);
        game.Enemies.Add(enemy);
        return enemy;
    }

    private static void KillAndTick(GameEngine game, EnemyInstance enemy)
    {
        enemy.ApplyDamage(enemy.MaxHp + 1000, DamageType.True);
        game.Tick(0.01);
    }

    private static void AssertAllKind(GameEngine game, EnemyKind kind)
    {
        foreach (var enemy in game.Enemies)
        {
            AssertEqual(kind, enemy.Def.Kind, "child kind");
            Assert(enemy.Alive, "child should be alive");
            AssertEqual(game.Stage.Paths[0], enemy.Path, "child path");
        }
    }

    private static void AssertContains(StageDef stage, EnemyKind kind, string label)
    {
        var contains = stage.Waves
            .SelectMany(wave => wave.Entries)
            .Any(entry => entry.Enemy == kind);
        Assert(contains, $"{label} should include {kind}");
    }

    private static void AssertTextContains(string actual, string expected, string label)
    {
        if (!actual.Contains(expected, StringComparison.Ordinal))
            throw new InvalidOperationException($"{label}: expected '{actual}' to contain '{expected}'");
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }

    private static void AssertClose(double expected, double actual, string label)
    {
        if (Math.Abs(expected - actual) > 0.0001)
            throw new InvalidOperationException($"{label}: expected {expected}, got {actual}");
    }

    private static void AssertEqual<T>(T expected, T actual, string label)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new InvalidOperationException($"{label}: expected {expected}, got {actual}");
    }
}
