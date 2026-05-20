using KingdomRushClone.Data;
using KingdomRushClone.Game;
using KingdomRushClone.Models;

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
        EnemyKind.EliteGhost,
        EnemyKind.MidBossNormal,
        EnemyKind.MidBossCharge,
        EnemyKind.MidBossSplit,
        EnemyKind.MidBossSpeed,
        EnemyKind.BossNormal,
        EnemyKind.BossCharge,
        EnemyKind.BossSplit,
        EnemyKind.BossSpeed
    };

    private static int Main()
    {
        var tests = new (string Name, Action Test)[]
        {
            ("enemy catalog matches sixteen asset plan", EnemyCatalogMatchesSixteenAssetPlan),
            ("stage catalog uses twenty stages and eight waves", StageCatalogUsesTwentyStagesAndEightWaves),
            ("chapter boss schedule uses four role families", ChapterBossScheduleUsesFourRoleFamilies),
            ("chapter hp scale uses twenty percent steps", ChapterHpScaleUsesTwentyPercentSteps),
            ("split boss creates split mid bosses", SplitBossCreatesSplitMidBosses),
            ("split mid boss creates split bodies", SplitMidBossCreatesSplitBodies),
            ("split body creates split small enemies", SplitBodyCreatesSplitSmallEnemies),
            ("elite shield absorbs three non-true hits", EliteShieldAbsorbsThreeNonTrueHits),
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

    private static void EliteShieldAbsorbsThreeNonTrueHits()
    {
        var game = CreateGame();
        var elite = CreateEnemy(game, EnemyKind.Elite);

        elite.ApplyDamage(10, DamageType.Physical);
        elite.ApplyDamage(10, DamageType.Magic);
        elite.ApplyDamage(10, DamageType.Explosive);

        AssertEqual(0, elite.ShieldCharges, "shield charges after three hits");
        AssertClose(elite.MaxHp, elite.Hp, "shielded hp");

        elite.ApplyDamage(10, DamageType.True);
        Assert(elite.Hp < elite.MaxHp, "true damage should bypass depleted shield");
    }

    private static void MeteorRewardsKilledEnemyOnce()
    {
        var game = CreateGame();
        var enemy = CreateEnemy(game, EnemyKind.Normal);

        Assert(game.CastMeteor(enemy.Pos), "meteor should cast");
        game.Tick(0.01);

        AssertEqual(EnemyCatalog.Enemies[EnemyKind.Normal].GoldReward, game.Gold, "meteor kill gold");
        AssertEqual(0, game.Enemies.Count, "dead enemy should be removed");
    }

    private static void AssertStageBoss(int stageNumber, EnemyKind expected, bool isMidBoss = false, bool isBoss = false)
    {
        var stage = StageCatalog.Stages.Single(s => s.Number == stageNumber);

        AssertEqual(isMidBoss, stage.HasMidBoss, $"stage {stageNumber} mid boss flag");
        AssertEqual(isBoss, stage.HasBoss, $"stage {stageNumber} boss flag");
        AssertContains(stage, expected, $"stage {stageNumber} wave plan");
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
