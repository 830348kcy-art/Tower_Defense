using KingdomRushClone.Data;
using KingdomRushClone.Game;
using KingdomRushClone.Managers;
using KingdomRushClone.Models;
using KingdomRushClone.Views;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
            ("enemy visuals resolve chapter sprites for gameplay and popups", EnemyVisualsResolveChapterSpritesForGameplayAndPopups),
            ("boss fallback visuals use distinct silhouettes", BossFallbackVisualsUseDistinctSilhouettes),
            ("enemy hitboxes and visuals are enlarged", EnemyHitboxesAndVisualsAreEnlarged),
            ("enemy gold rewards are doubled", EnemyGoldRewardsAreDoubled),
            ("enemy base hp values match planned ratios", EnemyBaseHpValuesMatchPlannedRatios),
            ("tower effective range uses larger map scale", TowerEffectiveRangeUsesLargerMapScale),
            ("enemy base speed values match balance plan", EnemyBaseSpeedValuesMatchBalancePlan),
            ("enemy physical resist values match balance plan", EnemyPhysicalResistValuesMatchBalancePlan),
            ("charge enemies gain physical resist at half hp", ChargeEnemiesGainPhysicalResistAtHalfHp),
            ("speed bosses apply planned global speed buffs", SpeedBossesApplyPlannedGlobalSpeedBuffs),
            ("regenerator pulse creates visible healing ring", RegeneratorPulseCreatesVisibleHealingRing),
            ("temporary pause stops wave countdown", TemporaryPauseStopsWaveCountdown),
            ("enemy ability summaries describe special mechanics", EnemyAbilitySummariesDescribeSpecialMechanics),
            ("stage intro enemy entries include popup metadata", StageIntroEnemyEntriesIncludePopupMetadata),
            ("asset preview catalog resolves maps, towers, soldiers, and chapter enemies", AssetPreviewCatalogResolvesMapsTowersSoldiersAndChapterEnemies),
            ("asset preview catalog resolves tower slots and gameplay sprite assets", AssetPreviewCatalogResolvesTowerSlotsAndGameplaySpriteAssets),
            ("stage intro enemy tabs separate new and returning enemies", StageIntroEnemyTabsSeparateNewAndReturningEnemies),
            ("tower visual tuning handles asset scale and anchors", TowerVisualTuningHandlesAssetScaleAndAnchors),
            ("asset preview map uses image-aligned paths and tower slots", AssetPreviewMapUsesImageAlignedPathsAndTowerSlots),
            ("stage catalog uses image-aligned map layouts", StageCatalogUsesImageAlignedMapLayouts),
            ("game render layers keep enemies above tower slots", GameRenderLayersKeepEnemiesAboveTowerSlots),
            ("tower render z index sorts lower towers above upper towers", TowerRenderZIndexSortsLowerTowersAboveUpperTowers),
            ("enemy visual animation exposes bob shake and flash", EnemyVisualAnimationExposesBobShakeAndFlash),
            ("map image hides debug path overlay", MapImageHidesDebugPathOverlay),
            ("chapter boss schedule uses four role families", ChapterBossScheduleUsesFourRoleFamilies),
            ("chapter hp scale uses twenty percent steps", ChapterHpScaleUsesTwentyPercentSteps),
            ("split boss creates split mid bosses", SplitBossCreatesSplitMidBosses),
            ("split mid boss creates split bodies", SplitMidBossCreatesSplitBodies),
            ("split body creates split small enemies", SplitBodyCreatesSplitSmallEnemies),
            ("elite has no special ability", EliteHasNoSpecialAbility),
            ("wyvern elite ignores explosive damage", WyvernEliteIgnoresExplosiveDamage),
            ("wyvern elite passes through barracks soldiers", WyvernElitePassesThroughBarracksSoldiers),
            ("boss spawn and death create enhanced effects", BossSpawnAndDeathCreateEnhancedEffects),
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
        var sheetPaths = roots.Select(root => Path.Combine(root, "EnemySpriteSheet.png")).Distinct().ToArray();
        var assetPaths = spritePaths.Concat(sheetPaths).Distinct().ToArray();
        var originals = assetPaths
            .Where(File.Exists)
            .ToDictionary(path => path, File.ReadAllBytes);

        foreach (var path in originals.Keys)
            File.Delete(path);

        WriteSolidPng(sheetPaths[0], 1700, 950);

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

            foreach (var path in assetPaths)
                if (!originals.ContainsKey(path) && File.Exists(path))
                    File.Delete(path);
        }
    }

    private static void EnemyVisualsResolveChapterSpritesForGameplayAndPopups()
    {
        var spriteDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Enemies");
        var chapterPath = Path.Combine(spriteDir, "Chapter2", $"{EnemyKind.Normal}.png");
        var rootPath = Path.Combine(spriteDir, $"{EnemyKind.Normal}.png");
        byte[]? originalChapterBytes = File.Exists(chapterPath) ? File.ReadAllBytes(chapterPath) : null;
        byte[]? originalRootBytes = File.Exists(rootPath) ? File.ReadAllBytes(rootPath) : null;

        Directory.CreateDirectory(Path.GetDirectoryName(chapterPath)!);
        WriteSolidPng(chapterPath, 2, 2);
        if (File.Exists(rootPath)) File.Delete(rootPath);

        try
        {
            var sprite = EnemyFallbackImageFactory.CreateSpriteVisual(2, EnemyKind.Normal, 40);
            var icon = EnemyFallbackImageFactory.CreateIconVisual(2, EnemyKind.Normal, 58);

            if (sprite is not Image spriteImage)
                throw new InvalidOperationException("chapter sprite should be used for in-game visual");
            if (icon is not Image iconImage)
                throw new InvalidOperationException("chapter sprite should be used for popup visual");
            Assert(spriteImage.Source is BitmapImage spriteSource
                   && spriteSource.UriSource.LocalPath.EndsWith(Path.Combine("Chapter2", "Normal.png"), StringComparison.OrdinalIgnoreCase),
                "gameplay visual should load the chapter-specific enemy sprite");
            Assert(iconImage.Source is BitmapImage iconSource
                   && iconSource.UriSource.LocalPath.EndsWith(Path.Combine("Chapter2", "Normal.png"), StringComparison.OrdinalIgnoreCase),
                "popup visual should load the chapter-specific enemy sprite");
            AssertEqual(40.0, sprite.Width, "chapter sprite gameplay width");
            AssertEqual(58.0, icon.Width, "chapter sprite popup width");
        }
        finally
        {
            if (originalChapterBytes != null)
                File.WriteAllBytes(chapterPath, originalChapterBytes);
            else if (File.Exists(chapterPath))
                File.Delete(chapterPath);

            if (originalRootBytes != null)
                File.WriteAllBytes(rootPath, originalRootBytes);
        }
    }

    private static void BossFallbackVisualsUseDistinctSilhouettes()
    {
        var roots = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Assets", "Enemies"),
            Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Enemies")
        }.Distinct().ToArray();
        var bossKinds = new[] { EnemyKind.BossNormal, EnemyKind.BossCharge, EnemyKind.BossSplit, EnemyKind.BossSpeed };
        var assetPaths = roots
            .SelectMany(root => bossKinds.SelectMany(kind => new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" }
                .Select(extension => Path.Combine(root, $"{kind}{extension}")))
                .Append(Path.Combine(root, "EnemySpriteSheet.png")))
            .Distinct()
            .ToArray();
        var originals = assetPaths
            .Where(File.Exists)
            .ToDictionary(path => path, File.ReadAllBytes);

        foreach (var path in originals.Keys)
            File.Delete(path);

        try
        {
            var signatures = bossKinds
                .Select(kind => RenderFallbackSignature(EnemyFallbackImageFactory.CreateSpriteVisual(kind, 96)))
                .ToArray();

            AssertEqual(bossKinds.Length, signatures.Distinct().Count(), "boss fallback visual signature count");

            AssertFallbackPixelVisible(EnemyKind.BossNormal, 14, 25, "normal boss side tower silhouette");
            AssertFallbackPixelVisible(EnemyKind.BossCharge, 73, 40, "charge boss forward horn silhouette");
            AssertFallbackPixelVisible(EnemyKind.BossSplit, 68, 64, "split boss spawned body silhouette");
            AssertFallbackPixelVisible(EnemyKind.BossSpeed, 8, 32, "speed boss trailing cloak silhouette");
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
            [EnemyKind.Elite] = 19,
            [EnemyKind.EliteCharge] = 19,
            [EnemyKind.EliteRegenerator] = 18,
            [EnemyKind.EliteWyvern] = 18,
            [EnemyKind.MidBossNormal] = 26,
            [EnemyKind.MidBossCharge] = 26,
            [EnemyKind.MidBossSplit] = 27,
            [EnemyKind.MidBossSpeed] = 25,
            [EnemyKind.BossNormal] = 35,
            [EnemyKind.BossCharge] = 35,
            [EnemyKind.BossSplit] = 36,
            [EnemyKind.BossSpeed] = 34
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

    private static void TowerEffectiveRangeUsesLargerMapScale()
    {
        var archer = new TowerInstance { Def = TowerCatalog.Towers[TowerKind.Archer], Level = 0 };
        AssertClose(1.35, TowerInstance.MapRangeScale, "tower map range scale");
        AssertClose(115 * 1.35, archer.EffectiveRange, "archer level 1 effective range");

        var sniper = new TowerInstance { Def = TowerCatalog.Towers[TowerKind.Archer], Level = 2, Branch = TowerBranch.A };
        AssertClose(195 * 1.35, sniper.EffectiveRange, "sniper branch effective range");

        var slowLv1 = new TowerInstance { Def = TowerCatalog.Towers[TowerKind.Slow], Level = 0 };
        var slowLv2 = new TowerInstance { Def = TowerCatalog.Towers[TowerKind.Slow], Level = 1 };
        var slowLv3 = new TowerInstance { Def = TowerCatalog.Towers[TowerKind.Slow], Level = 2 };
        AssertClose(90 * 1.35, slowLv1.EffectiveRange, "slow level 1 effective range");
        AssertClose(105 * 1.35, slowLv2.EffectiveRange, "slow level 2 effective range");
        AssertClose(120 * 1.35, slowLv3.EffectiveRange, "slow level 3 effective range");
        AssertClose(80, slowLv1.CurrentLevel.SplashRadius, "slow level 1 splash radius");
        AssertClose(95, slowLv2.CurrentLevel.SplashRadius, "slow level 2 splash radius");
        AssertClose(110, slowLv3.CurrentLevel.SplashRadius, "slow level 3 splash radius");
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

    private static void AssetPreviewCatalogResolvesMapsTowersSoldiersAndChapterEnemies()
    {
        var projectRoot = Path.Combine("C:", "ProjectAssets");
        var externalRoot = Path.Combine("C:", "UserAssets");

        var mapCandidates = AssetPreviewCatalog.MapCandidates(3, new[] { projectRoot }, externalRoot).ToList();
        AssertEqual(Path.Combine(projectRoot, "Maps", "Chapter3.png"), mapCandidates[0], "chapter map project candidate");
        Assert(mapCandidates.Contains(Path.Combine(externalRoot, "\uB9F5", "3.png")), "chapter map external candidate");

        var enemyCandidates = AssetPreviewCatalog.EnemyCandidates(3, EnemyKind.EliteWyvern, new[] { projectRoot }, externalRoot).ToList();
        AssertEqual(Path.Combine(projectRoot, "Enemies", "Chapter3", "EliteWyvern.png"), enemyCandidates[0], "chapter enemy project candidate");
        Assert(enemyCandidates.Contains(Path.Combine(externalRoot, "\uC801", "\uC8013", "3", "08_wyvern.png")),
            "chapter enemy external candidate");

        var tower = AssetPreviewCatalog.TowerAssets.Single(item => item.Key == "Archer-Lv1");
        var towerCandidates = AssetPreviewCatalog.AssetCandidates(tower, new[] { projectRoot }, externalRoot).ToList();
        Assert(towerCandidates.Contains(Path.Combine(projectRoot, "Towers", "Archer", "Archer-Lv1.png")),
            "tower project candidate");
        Assert(towerCandidates.Contains(Path.Combine(externalRoot, "\uD0C0\uC6CC", "Archer", "Archer-Lv1.png")),
            "tower external candidate");

        var soldier = AssetPreviewCatalog.SoldierAssets.Single(item => item.Key == "Soldier-Support");
        var soldierCandidates = AssetPreviewCatalog.AssetCandidates(soldier, new[] { projectRoot }, externalRoot).ToList();
        Assert(soldierCandidates.Contains(Path.Combine(projectRoot, "Soldiers", "Soldier-Support.png")),
            "soldier project candidate");
        Assert(soldierCandidates.Contains(Path.Combine(externalRoot, "\uD0C0\uC6CC", "Barracks", "Soldier-Support.png")),
            "soldier external candidate");

        var mine = AssetPreviewCatalog.TowerAssets.Single(item => item.Key == "Mine Launcher");
        var mineCandidates = AssetPreviewCatalog.AssetCandidates(mine, new[] { projectRoot }, externalRoot).ToList();
        Assert(mineCandidates.Contains(Path.Combine(projectRoot, "Towers", "Bombard", "Mine-Launcher.png")),
            "mine launcher dashed project candidate");
        Assert(mineCandidates.Contains(Path.Combine(externalRoot, "\uD0C0\uC6CC", "Bombard", "Mine-Launcher.png")),
            "mine launcher dashed external candidate");

        AssertEqual(4, AssetPreviewCatalog.MapChapters.Count, "preview map chapter count");
        AssertEqual(ExpectedEnemyKinds.Length, AssetPreviewCatalog.EnemyKinds.Count, "preview enemy kind count");
        AssertEqual(3, AssetPreviewCatalog.ChapterForStage(13), "preview chapter for stage");
        AssertEqual(4, AssetPreviewCatalog.ChapterForStage(20), "preview chapter for final stage");
    }

    private static void AssetPreviewCatalogResolvesTowerSlotsAndGameplaySpriteAssets()
    {
        var projectRoot = Path.Combine("C:", "ProjectAssets");
        var externalRoot = Path.Combine("C:", "UserAssets");

        var slotCandidates = AssetPreviewCatalog.TowerSlotCandidates(4, new[] { projectRoot }, externalRoot).ToList();
        Assert(slotCandidates.Contains(Path.Combine(projectRoot, "TowerSlot", "TowerSlot-Chapter4.png")),
            "chapter tower slot project candidate");
        Assert(slotCandidates.Contains(Path.Combine(projectRoot, "MapSlots", "TowerSlot-Chapter4.png")),
            "chapter tower slot map-slot project candidate");
        Assert(slotCandidates.Contains(Path.Combine(externalRoot, "\uD0C0\uC6CC \uC2AC\uB86F", "TowerSlot-Chapter4.png")),
            "chapter tower slot external candidate");

        AssertEqual("Archer-Lv1", AssetPreviewCatalog.TowerAssetFor(TowerKind.Archer, 0, TowerBranch.None).Key,
            "archer level 1 tower asset");
        AssertEqual("Archer-Lv3", AssetPreviewCatalog.TowerAssetFor(TowerKind.Archer, 8, TowerBranch.None).Key,
            "archer level should clamp to max base asset");
        AssertEqual("Archer-Sniper", AssetPreviewCatalog.TowerAssetFor(TowerKind.Archer, 2, TowerBranch.A).Key,
            "archer branch A tower asset");
        AssertEqual("Archer-Storm", AssetPreviewCatalog.TowerAssetFor(TowerKind.Archer, 2, TowerBranch.B).Key,
            "archer branch B tower asset");
        AssertEqual("Mine Launcher", AssetPreviewCatalog.TowerAssetFor(TowerKind.Bombard, 2, TowerBranch.B).Key,
            "bombard branch B tower asset");
        AssertEqual("Slow-Lv3", AssetPreviewCatalog.TowerAssetFor(TowerKind.Slow, 9, TowerBranch.None).Key,
            "slow level should clamp to max base asset");

        AssertEqual("Soldier-Lv1", AssetPreviewCatalog.SoldierAssetFor(0, TowerBranch.None, isReinforcement: false).Key,
            "level 1 barracks soldier asset");
        AssertEqual("Soldier-Lv3", AssetPreviewCatalog.SoldierAssetFor(9, TowerBranch.None, isReinforcement: false).Key,
            "base barracks soldier level should clamp");
        AssertEqual("Soldier-Paladin", AssetPreviewCatalog.SoldierAssetFor(2, TowerBranch.A, isReinforcement: false).Key,
            "paladin barracks soldier asset");
        AssertEqual("Soldier-Rogue", AssetPreviewCatalog.SoldierAssetFor(2, TowerBranch.B, isReinforcement: false).Key,
            "rogue barracks soldier asset");
        AssertEqual("Soldier-Support", AssetPreviewCatalog.SoldierAssetFor(0, TowerBranch.None, isReinforcement: true).Key,
            "reinforcement soldier asset");
    }
    private static void TowerVisualTuningHandlesAssetScaleAndAnchors()
    {
        Assert(AssetPreviewCatalog.TowerVisualScaleFor(TowerKind.Archer, 2, TowerBranch.A) > 1.0,
            "archer sniper should be enlarged");
        Assert(AssetPreviewCatalog.TowerVisualAnchorFor(TowerKind.Mage, 0, TowerBranch.None) < 0.78,
            "mage level 1 should be lowered from the default anchor");
        Assert(AssetPreviewCatalog.TowerVisualScaleFor(TowerKind.Bombard, 0, TowerBranch.None) < 1.0,
            "base bombard level 1 should be reduced");
        Assert(AssetPreviewCatalog.TowerVisualScaleFor(TowerKind.Bombard, 1, TowerBranch.None) < 1.0,
            "base bombard level 2 should be reduced");
        Assert(AssetPreviewCatalog.TowerVisualScaleFor(TowerKind.Bombard, 2, TowerBranch.None) < 1.0,
            "base bombard level 3 should be reduced");
        AssertClose(1.0, AssetPreviewCatalog.TowerVisualScaleFor(TowerKind.Bombard, 2, TowerBranch.A),
            "mortar should keep default scale");
        Assert(AssetPreviewCatalog.TowerVisualScaleFor(TowerKind.Barracks, 0, TowerBranch.None) < 1.0,
            "barracks should be slightly reduced");
        Assert(AssetPreviewCatalog.TowerVisualScaleFor(TowerKind.Slow, 0, TowerBranch.None) > 1.0,
            "slow level 1 should be enlarged");
        var sniperOffset = AssetPreviewCatalog.TowerVisualOffsetFor(TowerKind.Archer, 2, TowerBranch.A);
        Assert(sniperOffset.X < 0, "archer sniper should move slightly left");
        Assert(sniperOffset.Y < 0, "archer sniper should move slightly up");
        var archerOffset = AssetPreviewCatalog.TowerVisualOffsetFor(TowerKind.Archer, 0, TowerBranch.None);
        AssertClose(0, archerOffset.X, "base archer x offset");
        AssertClose(0, archerOffset.Y, "base archer y offset");
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

    private static void AssetPreviewMapUsesImageAlignedPathsAndTowerSlots()
    {
        foreach (var chapter in AssetPreviewCatalog.MapChapters)
        {
            var map = AssetPreviewCatalog.PreviewMapForChapter(chapter);

            AssertEqual(chapter, map.Chapter, $"chapter {chapter} preview map chapter");
            Assert(map.Paths.Count > 0, $"chapter {chapter} preview should include at least one path");
            Assert(map.TowerSlots.Count >= 8, $"chapter {chapter} preview should include enough tower slots");

            foreach (var path in map.Paths)
            {
                Assert(path.Count >= 2, $"chapter {chapter} preview path should have at least two points");
                foreach (var point in path)
                    AssertPointInsideMap(point, $"chapter {chapter} preview path point");
            }

            foreach (var slot in map.TowerSlots)
                AssertPointInsideMap(slot, $"chapter {chapter} preview tower slot");

            var slotSizeProperty = typeof(AssetPreviewMap).GetProperty("TowerSlotSize");
            Assert(slotSizeProperty != null, "preview map should expose map-specific tower slot size");
            double slotSize = (double)slotSizeProperty!.GetValue(map)!;
            Assert(slotSize >= 48 && slotSize <= 104, $"chapter {chapter} tower slot size should fit map art");
        }

        var chapter1 = AssetPreviewCatalog.PreviewMapForChapter(1);
        Assert(chapter1.Paths[0].Any(point => Math.Abs(point.Y - 310) < 0.0001),
            "chapter 1 preview path should follow the image road");
        Assert(chapter1.TowerSlots.Any(slot => slot.Y < 250) && chapter1.TowerSlots.Any(slot => slot.Y > 370),
            "chapter 1 preview slots should cover both road sides");
    }

    private static void StageCatalogUsesImageAlignedMapLayouts()
    {
        foreach (var stage in StageCatalog.Stages)
        {
            var preview = AssetPreviewCatalog.PreviewMapForChapter(AssetPreviewCatalog.ChapterForStage(stage.Number));

            AssertEqual(preview.Paths.Count, stage.Paths.Count, $"stage {stage.Number} path count");
            for (int pathIndex = 0; pathIndex < preview.Paths.Count; pathIndex++)
            {
                AssertEqual(preview.Paths[pathIndex].Count, stage.Paths[pathIndex].Count, $"stage {stage.Number} path {pathIndex} point count");
                for (int pointIndex = 0; pointIndex < preview.Paths[pathIndex].Count; pointIndex++)
                    AssertSamePoint(preview.Paths[pathIndex][pointIndex], stage.Paths[pathIndex][pointIndex],
                        $"stage {stage.Number} path {pathIndex} point {pointIndex}");
            }

            AssertEqual(preview.TowerSlots.Count, stage.BuildSlots.Count, $"stage {stage.Number} build slot count");
            for (int slotIndex = 0; slotIndex < preview.TowerSlots.Count; slotIndex++)
                AssertSamePoint(preview.TowerSlots[slotIndex], stage.BuildSlots[slotIndex],
                    $"stage {stage.Number} build slot {slotIndex}");

            var towerVisualSizeMethod = typeof(GamePage).GetMethod("TowerVisualSizeForStage");
            Assert(towerVisualSizeMethod != null, "game page should expose stage-specific tower visual size");
            double visualSize = (double)towerVisualSizeMethod!.Invoke(null, new object[] { stage })!;
            Assert(visualSize >= 86 && visualSize <= 140, $"stage {stage.Number} tower visual size should follow slot size");
        }
    }

    private static void GameRenderLayersKeepEnemiesAboveTowerSlots()
    {
        Assert(GamePage.EnemyBodyZIndex > GamePage.TowerSlotZIndex, "enemies should draw above tower slots");
        Assert(GamePage.EnemyHealthBarZIndex > GamePage.EnemyBodyZIndex, "enemy hp bars should draw above enemy bodies");
        Assert(GamePage.TowerZIndex > GamePage.EnemyHealthBarZIndex, "built towers should draw above enemies");
        Assert(GamePage.SoldierBodyZIndex > GamePage.TowerZIndex, "soldiers should draw above towers");
        Assert(GamePage.FloatingTextZIndex > GamePage.SoldierHealthBarZIndex, "floating text should draw above combat actors");
    }

    private static void TowerRenderZIndexSortsLowerTowersAboveUpperTowers()
    {
        Assert(GamePage.TowerZIndexForY(500) > GamePage.TowerZIndexForY(120),
            "lower tower should draw above upper tower");
        Assert(GamePage.TowerZIndexForY(500) < GamePage.SoldierBodyZIndex,
            "tower y sorting should stay below soldier layer");
    }

    private static void EnemyVisualAnimationExposesBobShakeAndFlash()
    {
        AssertClose(0, GamePage.EnemyBobOffsetFor(0.0, EnemyKind.Normal, 0), "normal enemy bobbing should be disabled");
        AssertClose(0, GamePage.EnemyBobOffsetFor(0.5, EnemyKind.SplitBody, 0), "split enemy bobbing should be disabled");
        AssertClose(0, GamePage.EnemyBobOffsetFor(0.5, EnemyKind.EliteRegenerator, 0), "regenerator bobbing should be disabled");
        AssertClose(0, GamePage.EnemyBobOffsetFor(0.5, EnemyKind.BossNormal, 0), "boss bobbing should be disabled");

        AssertBobs(EnemyKind.Fast);
        AssertBobs(EnemyKind.EliteWyvern);
        AssertBobs(EnemyKind.EliteCharge);
        AssertBobs(EnemyKind.MidBossCharge);
        AssertBobs(EnemyKind.BossCharge);

        var shake = GamePage.EnemyHitShakeOffsetFor(0.12, 0.25);
        Assert(Math.Abs(shake.X) > 0.01 || Math.Abs(shake.Y) > 0.01, "enemy hit shake should move the sprite");
        AssertClose(0, GamePage.EnemyHitShakeOffsetFor(0, 0.25).X, "inactive hit shake x");
        AssertClose(0, GamePage.EnemyHitShakeOffsetFor(0, 0.25).Y, "inactive hit shake y");

        Assert(GamePage.EnemyHitFlashOpacityFor(0.12) > 0, "enemy hit flash should become visible");
        AssertClose(0, GamePage.EnemyHitFlashOpacityFor(0), "inactive hit flash opacity");

        static void AssertBobs(EnemyKind kind)
        {
            double bobA = GamePage.EnemyBobOffsetFor(0.0, kind, 0);
            double bobB = GamePage.EnemyBobOffsetFor(0.5, kind, 0);
            Assert(Math.Abs(bobA - bobB) > 0.01, $"{kind} bobbing should vary over time");
        }
    }

    private static void MapImageHidesDebugPathOverlay()
    {
        Assert(!GamePage.ShouldDrawDebugPathOverlay(hasMapImage: true), "map images should hide debug path overlays");
        Assert(GamePage.ShouldDrawDebugPathOverlay(hasMapImage: false), "fallback maps should draw debug path overlays");
    }

    private static void AssertPointInsideMap(Vec2 point, string label)
    {
        Assert(
            point.X >= 0
            && point.X <= StageCatalog.MapWidth
            && point.Y >= 0
            && point.Y <= StageCatalog.MapHeight,
            $"{label}: {point.X}, {point.Y}");
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

    private static void BossSpawnAndDeathCreateEnhancedEffects()
    {
        var game = CreateGame();
        var normal = game.CreateEnemy(EnemyCatalog.Enemies[EnemyKind.Normal], new Vec2(20, 0), game.Stage.Paths[0], 0);
        AssertEqual(0, game.Effects.Count, "normal enemy should not create entrance effects");

        var boss = game.CreateEnemy(EnemyCatalog.Enemies[EnemyKind.BossNormal], new Vec2(40, 0), game.Stage.Paths[0], 0);
        Assert(game.Effects.Count >= 2, "boss entrance should create layered effects");
        Assert(game.Effects.Any(effect => effect.Radius > boss.Def.Radius * 2), "boss entrance should use a large ring");

        game.Effects.Clear();
        game.Enemies.Add(boss);
        boss.ApplyDamage(boss.MaxHp + 1000, DamageType.True);
        game.Tick(0.01);

        Assert(game.Effects.Count >= 3, "boss death should create stronger layered effects");
        Assert(game.Effects.Any(effect => effect.Radius > boss.Def.Radius * 3), "boss death should use a larger shockwave");
        Assert(!game.Enemies.Contains(boss), "dead boss should be removed after death effects are queued");
        _ = normal;
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

    private static void WriteSolidPng(string path, int width, int height)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        int stride = width * 4;
        var pixels = new byte[stride * height];
        for (int i = 0; i < pixels.Length; i += 4)
        {
            pixels[i] = 255;
            pixels[i + 1] = 255;
            pixels[i + 2] = 255;
            pixels[i + 3] = 255;
        }

        var bitmap = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgra32, null, pixels, stride);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var stream = File.Create(path);
        encoder.Save(stream);
    }

    private static string RenderFallbackSignature(FrameworkElement element)
    {
        var pixels = RenderFallbackPixels(element);

        unchecked
        {
            uint hash = 2166136261;
            foreach (byte pixel in pixels)
            {
                hash ^= pixel;
                hash *= 16777619;
            }
            return hash.ToString("X8");
        }
    }

    private static byte[] RenderFallbackPixels(FrameworkElement element)
    {
        const int size = 96;
        element.Width = size;
        element.Height = size;
        element.Measure(new System.Windows.Size(size, size));
        element.Arrange(new System.Windows.Rect(0, 0, size, size));
        element.UpdateLayout();

        var bitmap = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(element);

        int stride = size * 4;
        var pixels = new byte[stride * size];
        bitmap.CopyPixels(pixels, stride, 0);

        return pixels;
    }

    private static void AssertFallbackPixelVisible(EnemyKind kind, int x, int y, string label)
    {
        byte alpha = RenderFallbackAlphaAt(EnemyFallbackImageFactory.CreateSpriteVisual(kind, 96), x, y);
        Assert(alpha > 0, $"{label}: expected visible fallback pixel at {x},{y}");
    }

    private static byte RenderFallbackAlphaAt(FrameworkElement element, int x, int y)
    {
        var pixels = RenderFallbackPixels(element);
        int index = (y * 96 + x) * 4;
        return pixels[index + 3];
    }

    private static void AssertSamePoint(Vec2 expected, Vec2 actual, string label)
    {
        AssertClose(expected.X, actual.X, $"{label} x");
        AssertClose(expected.Y, actual.Y, $"{label} y");
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
