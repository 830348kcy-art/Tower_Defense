using TowerDefense.Core;
using TowerDefense.Data;
using TowerDefense.Enemies;
using TowerDefense.Sandbox;
using TowerDefense.Towers;
using TowerDefense.UI;
using System.Windows.Media;

var tests = new (string Name, Action Test)[]
{
    ("GameSession applies chapter multiplier from stage", GameSessionAppliesChapterMultiplierFromStage),
    ("EnemyFactory creates enemies with documented stats", EnemyFactoryCreatesEnemiesWithDocumentedStats),
    ("Damage rules respect immunity and true damage", DamageRulesRespectImmunityAndTrueDamage),
    ("WavePlan emits boss wave for boss stages", WavePlanEmitsBossWaveForBossStages),
    ("WaveManager spawns and removes enemies", WaveManagerSpawnsAndRemovesEnemies),
    ("StageIntroBuilder separates new and returning enemies", StageIntroBuilderSeparatesNewAndReturningEnemies),
    ("StageIntroViewModel selects first enemy and requests close", StageIntroViewModelSelectsFirstEnemyAndRequestsClose),
    ("MainViewModel requests intro before starting stage", MainViewModelRequestsIntroBeforeStartingStage),
    ("Slow effect changes speed and expires", SlowEffectChangesSpeedAndExpires),
    ("Slow effect keeps stronger factor and longer duration", SlowEffectKeepsStrongerFactorAndLongerDuration),
    ("Invincible enemies ignore slow", InvincibleEnemiesIgnoreSlow),
    ("SlowTower applies slow on hit", SlowTowerAppliesSlowOnHit),
    ("SandboxGame spawns enemies and towers attack", SandboxGameSpawnsEnemiesAndTowersAttack),
    ("SandboxGame slow tower reduces enemy speed", SandboxGameSlowTowerReducesEnemySpeed),
    ("SandboxGame slow tower prioritizes fast enemies", SandboxGameSlowTowerPrioritizesFastEnemies),
    ("SandboxGame reports slowed enemy count", SandboxGameReportsSlowedEnemyCount),
    ("SandboxGame split wave includes chapter three split enemies", SandboxGameSplitWaveIncludesChapterThreeSplitEnemies),
    ("SandboxGame spawns split children after death", SandboxGameSpawnsSplitChildrenAfterDeath),
    ("SandboxGame exposes boss health while boss exists", SandboxGameExposesBossHealthWhileBossExists),
    ("SandboxGame clears boss health after boss dies", SandboxGameClearsBossHealthAfterBossDies),
    ("Split boss spawns split mini bosses", SplitBossSpawnsSplitMiniBosses),
    ("Split mini boss spawns normal split bodies", SplitMiniBossSpawnsNormalSplitBodies),
    ("Enemy preview factory creates image source", EnemyPreviewFactoryCreatesImageSource),
    ("Enemy preview factory uses dedicated fallback colors", EnemyPreviewFactoryUsesDedicatedFallbackColors),
    ("Enemy preview factory creates fallback for every known enemy", EnemyPreviewFactoryCreatesFallbackForEveryKnownEnemy),
    ("Enemy preview factory reuses fallback images", EnemyPreviewFactoryReusesFallbackImages),
    ("Enemy preview factory creates transparent battlefield sprites", EnemyPreviewFactoryCreatesTransparentBattlefieldSprites),
};

foreach (var test in tests)
{
    GameSession.ResetForTests();
    WaveManager.ResetForTests();
    test.Test();
    Console.WriteLine($"PASS {test.Name}");
}

static void GameSessionAppliesChapterMultiplierFromStage()
{
    var session = GameSession.Instance;
    session.OnStageChanged(6);

    AssertEqual(6, session.CurrentStage, "stage");
    AssertEqual(2, session.CurrentChapter, "chapter");
    AssertNearly(1.2f, session.WaveMultiplier, "chapter 2 multiplier");

    session.OnStageChanged(16);
    AssertEqual(4, session.CurrentChapter, "chapter");
    AssertNearly(1.728f, session.WaveMultiplier, "chapter 4 multiplier");
}

static void EnemyFactoryCreatesEnemiesWithDocumentedStats()
{
    var normal = EnemyFactory.Create("enemy_normal");
    var fast = EnemyFactory.Create("enemy_fast");
    var boss = EnemyFactory.Create("boss_split");

    AssertEqual(EnemyCategory.Normal, normal.Category, "normal category");
    AssertNearly(1.0f, normal.BaseHpPercent, "normal hp percent");
    AssertNearly(80f, normal.MoveSpeed, "normal move speed");
    AssertNearly(0.5f, fast.BaseHpPercent, "fast hp percent");
    AssertNearly(160f, fast.MoveSpeed, "fast move speed");
    AssertEqual(EnemyCategory.Boss, boss.Category, "boss category");
    AssertNearly(8.0f, boss.BaseHpPercent, "boss split hp percent");
}

static void DamageRulesRespectImmunityAndTrueDamage()
{
    GameSession.Instance.OnStageChanged(1);
    var resist = EnemyFactory.Create("elite_resist");
    resist.Initialize();

    var hp = resist.CurrentHp;
    resist.TakeDamage(40f, DamageType.Aoe);
    AssertNearly(hp, resist.CurrentHp, "aoe immune damage");

    resist.TakeDamage(40f, DamageType.Single);
    AssertNearly(hp - 40f, resist.CurrentHp, "single damage");

    resist.IsInvincible = true;
    resist.TakeDamage(40f, DamageType.Single);
    AssertNearly(hp - 40f, resist.CurrentHp, "invincible blocks single damage");

    resist.TakeDamage(40f, DamageType.True);
    AssertNearly(hp - 80f, resist.CurrentHp, "true damage bypasses invincible");
}

static void WavePlanEmitsBossWaveForBossStages()
{
    AssertTrue(WavePlan.IsBossStage(5), "stage 5 is boss stage");
    AssertFalse(WavePlan.IsMiniBossStage(5), "stage 5 is not mini boss stage");

    var wave = WavePlan.GetWave(5, 8).ToList();
    AssertTrue(wave.Any(entry => entry.EnemyId == "boss_normal"), "chapter 1 boss appears on S5 W8");
}

static void WaveManagerSpawnsAndRemovesEnemies()
{
    var manager = WaveManager.Instance;
    manager.StartStage(1);

    AssertEqual(1, manager.CurrentStage, "current stage");
    AssertEqual(1, manager.CurrentWave, "current wave");
    AssertTrue(manager.ActiveEnemies.Count > 0, "stage starts with active enemies");

    var first = manager.ActiveEnemies[0];
    first.TakeDamage(first.CurrentHp, DamageType.True);
    AssertFalse(manager.ActiveEnemies.Contains(first), "dead enemy removed");
}

static void StageIntroBuilderSeparatesNewAndReturningEnemies()
{
    GameSession.Instance.OnStageChanged(1);
    var firstIntro = StageIntroBuilder.Build(1);

    AssertTrue(firstIntro.NewEnemies.Count > 0, "first stage has new enemies");
    AssertEqual(0, firstIntro.ReturningEnemies.Count, "first visit has no returning enemies");
    AssertNearly(1.0f, firstIntro.HpMultiplier, "stage 1 multiplier");

    var secondIntro = StageIntroBuilder.Build(1);
    AssertEqual(0, secondIntro.NewEnemies.Count, "second visit has no new enemies");
    AssertTrue(secondIntro.ReturningEnemies.Count > 0, "second visit has returning enemies");
}

static void StageIntroViewModelSelectsFirstEnemyAndRequestsClose()
{
    var data = StageIntroBuilder.Build(1);
    var viewModel = new StageIntroViewModel(data);

    AssertEqual(data.NewEnemies[0], viewModel.SelectedEnemy?.Info, "default selected enemy");

    var secondEnemy = viewModel.NewEnemies[1];
    viewModel.SelectEnemyCommand.Execute(secondEnemy);
    AssertEqual(secondEnemy, viewModel.SelectedEnemy, "selected enemy after command");

    var closeCount = 0;
    viewModel.RequestClose += () => closeCount++;
    viewModel.StartCommand.Execute(null);

    AssertEqual(1, closeCount, "start command close request count");
    AssertFalse(viewModel.SkipAlways, "start command does not enable skip always");

    viewModel.SkipAlwaysCommand.Execute(null);
    AssertTrue(viewModel.SkipAlways, "skip always command enables skip always");
    AssertEqual(2, closeCount, "skip always command close request count");
}

static void MainViewModelRequestsIntroBeforeStartingStage()
{
    var viewModel = new MainViewModel();
    StageIntroData? requestedIntro = null;
    viewModel.RequestStageIntro += data => requestedIntro = data;

    viewModel.StartStageCommand.Execute(null);

    AssertEqual(viewModel.Intro, requestedIntro, "requested intro data");
    AssertEqual(0, WaveManager.Instance.CurrentStage, "stage not started before intro confirmation");

    viewModel.ConfirmStageIntro();

    AssertEqual(1, WaveManager.Instance.CurrentStage, "stage after intro confirmation");
    AssertEqual(1, WaveManager.Instance.CurrentWave, "wave after intro confirmation");
    AssertTrue(viewModel.StatusText.Contains("활성 적"), "status text after stage start");
}

static void SlowEffectChangesSpeedAndExpires()
{
    var enemy = EnemyFactory.Create("enemy_normal");
    enemy.Initialize();
    enemy.SpeedBonus_Aura = 0.20f;
    enemy.SpeedBonus_BossAura = 0.15f;

    enemy.ApplySlow(0.5f, 2.0f);

    AssertNearly(0.5f, enemy.SlowFactor, "slow factor");
    AssertNearly(80f * 1.35f * 0.5f, enemy.ActualMoveSpeed, "slow actual speed");

    enemy.Update(1.0);
    AssertNearly(0.5f, enemy.SlowFactor, "slow factor before expiration");

    enemy.Update(1.0);
    AssertNearly(1.0f, enemy.SlowFactor, "slow factor after expiration");
    AssertNearly(80f * 1.35f, enemy.ActualMoveSpeed, "actual speed after slow expiration");
}

static void SlowEffectKeepsStrongerFactorAndLongerDuration()
{
    var enemy = EnemyFactory.Create("enemy_normal");
    enemy.Initialize();

    enemy.ApplySlow(0.7f, 1.0f);
    enemy.ApplySlow(0.5f, 0.5f);
    enemy.ApplySlow(0.8f, 3.0f);

    AssertNearly(0.5f, enemy.SlowFactor, "stronger slow factor wins");

    enemy.Update(2.5);
    AssertNearly(0.5f, enemy.SlowFactor, "longer duration keeps slow active");

    enemy.Update(0.5);
    AssertNearly(1.0f, enemy.SlowFactor, "longer duration eventually expires");
}

static void InvincibleEnemiesIgnoreSlow()
{
    var enemy = EnemyFactory.Create("elite_ghost");
    enemy.Initialize();
    enemy.IsInvincible = true;

    enemy.ApplySlow(0.5f, 2.0f);

    AssertNearly(1.0f, enemy.SlowFactor, "invincible enemy ignores slow");
    AssertNearly(enemy.MoveSpeed, enemy.ActualMoveSpeed, "invincible enemy speed unchanged");
}

static void SlowTowerAppliesSlowOnHit()
{
    var enemy = EnemyFactory.Create("enemy_fast");
    enemy.Initialize();
    var tower = new SlowTower();

    tower.Hit(enemy);

    AssertNearly(0.5f, enemy.SlowFactor, "slow tower factor");
    AssertNearly(80f, enemy.ActualMoveSpeed, "fast enemy slowed by tower");
}

static void SandboxGameSpawnsEnemiesAndTowersAttack()
{
    var game = new SandboxGame();
    game.StartWave();

    AssertTrue(game.Enemies.Count > 0, "sandbox wave spawns enemies");
    AssertTrue(game.Towers.Count >= 2, "sandbox has test towers");

    var first = game.Enemies[0];
    var initialHp = first.Enemy.CurrentHp;

    game.Tick(0.25);

    AssertTrue(first.Enemy.CurrentHp < initialHp, "basic tower damages enemy");
    AssertTrue(first.X > 0, "enemy moves on path");
}

static void SandboxGameSlowTowerReducesEnemySpeed()
{
    var game = new SandboxGame();
    game.StartWave();

    var enemy = game.Enemies.Single(enemy => enemy.Enemy.EnemyId == "enemy_fast");
    var normalSpeed = enemy.Enemy.ActualMoveSpeed;

    game.Tick(0.25);

    AssertTrue(enemy.Enemy.SlowFactor < 1.0f, "slow tower applies slow factor");
    AssertTrue(enemy.Enemy.ActualMoveSpeed < normalSpeed, "slow tower reduces actual speed");
}

static void SandboxGameSlowTowerPrioritizesFastEnemies()
{
    var game = new SandboxGame();
    game.StartWave();

    var fastEnemy = game.Enemies.Single(enemy => enemy.Enemy.EnemyId == "enemy_fast");

    game.Tick(0.25);

    AssertTrue(fastEnemy.Enemy.SlowFactor < 1.0f, "slow tower should prioritize fast enemy in range");
    AssertNearly(80f, fastEnemy.Enemy.ActualMoveSpeed, "fast enemy slowed to normal speed");
}

static void SandboxGameReportsSlowedEnemyCount()
{
    var game = new SandboxGame();
    game.StartWave();

    AssertEqual(0, game.SlowedEnemyCount, "slowed enemy count before tick");

    game.Tick(0.25);

    AssertTrue(game.SlowedEnemyCount > 0, "slowed enemy count after slow tower fires");
}

static void SandboxGameSplitWaveIncludesChapterThreeSplitEnemies()
{
    var game = new SandboxGame();
    game.StartSplitWave();

    AssertTrue(game.Enemies.Any(enemy => enemy.Enemy.EnemyId == "boss_split"), "sandbox split wave includes split boss");
    AssertTrue(game.Enemies.Any(enemy => enemy.Enemy.EnemyId == "miniboss_split"), "sandbox split wave includes split mini boss");
    AssertTrue(game.Enemies.Any(enemy => enemy.Enemy.EnemyId == "enemy_split_body"), "sandbox split wave includes normal split body");
}

static void SandboxGameSpawnsSplitChildrenAfterDeath()
{
    var game = new SandboxGame();
    game.StartSplitWave();

    var boss = game.Enemies.Single(enemy => enemy.Enemy.EnemyId == "boss_split");
    boss.Enemy.TakeDamage(boss.Enemy.CurrentHp, DamageType.True);
    game.Tick(0);

    AssertFalse(game.Enemies.Contains(boss), "dead split boss removed from sandbox");
    AssertTrue(game.Enemies.Count(enemy => enemy.Enemy.EnemyId == "miniboss_split") >= 3, "split boss adds two mini bosses");

    var miniBossCount = game.Enemies.Count(enemy => enemy.Enemy.EnemyId == "miniboss_split");
    var splitBodyCount = game.Enemies.Count(enemy => enemy.Enemy.EnemyId == "enemy_split_body");
    var miniBoss = game.Enemies.First(enemy => enemy.Enemy.EnemyId == "miniboss_split");

    miniBoss.Enemy.TakeDamage(miniBoss.Enemy.CurrentHp, DamageType.True);
    game.Tick(0);

    AssertEqual(miniBossCount - 1, game.Enemies.Count(enemy => enemy.Enemy.EnemyId == "miniboss_split"), "dead split mini boss removed from sandbox");
    AssertEqual(splitBodyCount + 2, game.Enemies.Count(enemy => enemy.Enemy.EnemyId == "enemy_split_body"), "split mini boss adds two split bodies");
}

static void SandboxGameExposesBossHealthWhileBossExists()
{
    var game = new SandboxGame();
    game.StartWave();

    AssertFalse(game.HasBoss, "basic sandbox wave does not expose boss health");
    AssertEqual(string.Empty, game.BossHealthText, "basic sandbox wave has no boss health text");

    game.StartSplitWave();

    AssertTrue(game.HasBoss, "split sandbox wave exposes boss health");
    AssertNearly(1.0f, game.BossHealthRatio, "boss health starts full");
    AssertTrue(game.BossHealthText.Contains("100%"), "boss health text starts at 100 percent");

    var boss = game.Enemies.Single(enemy => enemy.Enemy.Category == EnemyCategory.Boss);
    boss.Enemy.TakeDamage(boss.Enemy.MaxHp * 0.25f, DamageType.True);

    AssertTrue(game.HasBoss, "damaged boss still exposes boss health");
    AssertNearly(0.75f, game.BossHealthRatio, "boss health ratio after damage");
    AssertTrue(game.BossHealthText.Contains("75%"), "boss health text after damage");
}

static void SandboxGameClearsBossHealthAfterBossDies()
{
    var game = new SandboxGame();
    game.StartSplitWave();
    var boss = game.Enemies.Single(enemy => enemy.Enemy.Category == EnemyCategory.Boss);

    boss.Enemy.TakeDamage(boss.Enemy.CurrentHp, DamageType.True);
    game.Tick(0);

    AssertFalse(game.HasBoss, "dead boss clears boss health");
    AssertEqual(0f, game.BossHealthRatio, "dead boss clears boss health ratio");
    AssertEqual(string.Empty, game.BossHealthText, "dead boss clears boss health text");
}

static void SplitBossSpawnsSplitMiniBosses()
{
    var boss = EnemyFactory.Create("boss_split");

    var spawns = boss.CreateDeathSpawns().ToList();

    AssertEqual(2, spawns.Count, "boss split spawn count");
    AssertTrue(spawns.All(enemy => enemy.EnemyId == "miniboss_split"), "boss split spawns split mini bosses");
}

static void SplitMiniBossSpawnsNormalSplitBodies()
{
    var miniBoss = EnemyFactory.Create("miniboss_split");

    var spawns = miniBoss.CreateDeathSpawns().ToList();

    AssertEqual(2, spawns.Count, "mini boss split spawn count");
    AssertTrue(spawns.All(enemy => enemy.EnemyId == "enemy_split_body"), "mini boss split spawns normal split bodies");
}

static void EnemyPreviewFactoryCreatesImageSource()
{
    var preview = EnemyPreviewImageFactory.Create("boss_split");

    AssertTrue(preview is not null, "enemy preview converter returns image source");
}

static void EnemyPreviewFactoryUsesDedicatedFallbackColors()
{
    AssertTrue(
        ImageContainsColor(EnemyPreviewImageFactory.Create("boss_split"), Color.FromRgb(220, 38, 38)),
        "split boss fallback image uses boss red");
    AssertTrue(
        ImageContainsColor(EnemyPreviewImageFactory.Create("miniboss_split"), Color.FromRgb(234, 88, 12)),
        "split mini boss fallback image uses mini boss orange");
    AssertTrue(
        ImageContainsColor(EnemyPreviewImageFactory.Create("enemy_split_body"), Color.FromRgb(132, 204, 22)),
        "split body fallback image uses split green");
}

static void EnemyPreviewFactoryCreatesFallbackForEveryKnownEnemy()
{
    var enemyIds = new[]
    {
        "enemy_normal",
        "enemy_fast",
        "enemy_split_body",
        "enemy_split_small",
        "elite_shield",
        "elite_charge",
        "elite_regen",
        "elite_resist",
        "elite_ghost",
        "miniboss_normal",
        "miniboss_charge",
        "miniboss_split",
        "miniboss_speed",
        "boss_normal",
        "boss_charge",
        "boss_split",
        "boss_speed",
    };

    foreach (var enemyId in enemyIds)
    {
        var preview = EnemyPreviewImageFactory.Create(enemyId);
        AssertTrue(preview is DrawingImage, $"{enemyId} fallback image is drawing image");
        AssertTrue(preview.IsFrozen, $"{enemyId} fallback image is frozen");
        AssertTrue(ImageColorCount(preview) >= 3, $"{enemyId} fallback image has visible colors");
    }
}

static void EnemyPreviewFactoryReusesFallbackImages()
{
    var first = EnemyPreviewImageFactory.Create("boss_split");
    var second = EnemyPreviewImageFactory.Create("boss_split");

    AssertTrue(ReferenceEquals(first, second), "enemy fallback image should be reused for repeated requests");
}

static void EnemyPreviewFactoryCreatesTransparentBattlefieldSprites()
{
    var icon = EnemyPreviewImageFactory.Create("boss_split");
    var sprite = EnemyPreviewImageFactory.CreateSprite("boss_split");

    AssertTrue(ImageContainsColor(icon, Color.FromRgb(239, 246, 255)), "popup icon keeps fallback background");
    AssertFalse(ImageContainsColor(sprite, Color.FromRgb(239, 246, 255)), "battlefield sprite omits fallback background fill");
    AssertFalse(ImageContainsColor(sprite, Color.FromRgb(191, 219, 254)), "battlefield sprite omits fallback background stroke");
    AssertTrue(ImageContainsColor(sprite, Color.FromRgb(220, 38, 38)), "battlefield sprite keeps enemy body color");
    AssertTrue(ReferenceEquals(sprite, EnemyPreviewImageFactory.CreateSprite("boss_split")), "battlefield sprite should be reused");
    AssertFalse(ReferenceEquals(icon, sprite), "popup icon and battlefield sprite use separate cached images");
}

static bool ImageContainsColor(ImageSource source, Color color)
{
    return CollectColors(source).Contains(color);
}

static int ImageColorCount(ImageSource source)
{
    return CollectColors(source).Count;
}

static HashSet<Color> CollectColors(ImageSource source)
{
    var colors = new HashSet<Color>();
    if (source is DrawingImage image)
    {
        CollectDrawingColors(image.Drawing, colors);
    }

    return colors;
}

static void CollectDrawingColors(Drawing drawing, ISet<Color> colors)
{
    if (drawing is DrawingGroup group)
    {
        foreach (var child in group.Children)
        {
            CollectDrawingColors(child, colors);
        }

        return;
    }

    if (drawing is GeometryDrawing geometry)
    {
        AddBrushColor(geometry.Brush, colors);
        AddBrushColor(geometry.Pen?.Brush, colors);
    }
}

static void AddBrushColor(Brush? brush, ISet<Color> colors)
{
    if (brush is SolidColorBrush solid)
    {
        colors.Add(solid.Color);
    }
}

static void AssertEqual<T>(T expected, T actual, string label)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"{label}: expected {expected}, got {actual}");
    }
}

static void AssertNearly(float expected, float actual, string label)
{
    if (Math.Abs(expected - actual) > 0.001f)
    {
        throw new InvalidOperationException($"{label}: expected {expected}, got {actual}");
    }
}

static void AssertTrue(bool condition, string label)
{
    if (!condition)
    {
        throw new InvalidOperationException(label);
    }
}

static void AssertFalse(bool condition, string label)
{
    if (condition)
    {
        throw new InvalidOperationException(label);
    }
}
