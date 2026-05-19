using System.Collections.ObjectModel;
using TowerDefense.Core;
using TowerDefense.Enemies;

namespace TowerDefense.Sandbox;

public sealed class SandboxGame
{
    public SandboxGame()
    {
        GameSession.Instance.OnStageChanged(1);
        Towers.Add(new SandboxTower(SandboxTowerKind.Basic, 120, 240));
        Towers.Add(new SandboxTower(SandboxTowerKind.Slow, 60, 340));
    }

    public ObservableCollection<SandboxEnemy> Enemies { get; } = [];

    public ObservableCollection<SandboxTower> Towers { get; } = [];

    public int Gold { get; private set; } = 80;

    public int Lives { get; private set; } = 10;

    public int Wave { get; private set; }

    public bool IsWaveRunning { get; private set; }

    public string StatusText { get; private set; } = "테스트 준비 완료";

    public int SlowedEnemyCount => Enemies.Count(enemy => enemy.Enemy.SlowFactor < 1.0f);

    public int SplitEnemyCount => Enemies.Count(enemy => enemy.Enemy.EnemyId.Contains("split"));

    public bool HasBoss => FindBoss() is not null;

    public float BossHealthRatio
    {
        get
        {
            var boss = FindBoss();
            if (boss is null)
            {
                return 0f;
            }

            return Math.Clamp(boss.Enemy.CurrentHp / boss.Enemy.MaxHp, 0f, 1f);
        }
    }

    public string BossHealthText
    {
        get
        {
            var boss = FindBoss();
            if (boss is null)
            {
                return string.Empty;
            }

            return $"BOSS HP {BossHealthRatio * 100f:0}%";
        }
    }

    public IReadOnlyList<SandboxPoint> Path { get; } =
    [
        new(0, 300),
        new(210, 300),
        new(210, 120),
        new(520, 120),
        new(520, 420),
        new(760, 420)
    ];

    public void StartWave()
    {
        Reset();
        GameSession.Instance.OnStageChanged(1);
        Wave = 1;
        IsWaveRunning = true;
        StatusText = "기본 테스트 웨이브 진행 중";

        Spawn("enemy_normal");
        Spawn("enemy_fast");
        Spawn("enemy_split_body");
    }

    public void StartSplitWave()
    {
        Reset();
        GameSession.Instance.OnStageChanged(13);
        Wave = 3;
        IsWaveRunning = true;
        StatusText = "챕터3 분열 테스트 진행 중";

        Spawn("boss_split");
        Spawn("miniboss_split");
        Spawn("enemy_split_body");
    }

    public void Reset()
    {
        Enemies.Clear();
        Gold = 80;
        Lives = 10;
        Wave = 0;
        IsWaveRunning = false;
        StatusText = "테스트 준비 완료";
    }

    public void Tick(double deltaTime)
    {
        if (!IsWaveRunning)
        {
            return;
        }

        foreach (var enemy in Enemies.ToList())
        {
            enemy.Tick(deltaTime);
            if (!enemy.Enemy.IsAlive)
            {
                Gold += enemy.Enemy.GoldReward;
                Enemies.Remove(enemy);
                SpawnDeathSpawns(enemy);
                continue;
            }

            if (enemy.ReachedEnd)
            {
                Lives = Math.Max(0, Lives - 1);
                Enemies.Remove(enemy);
            }
        }

        foreach (var tower in Towers)
        {
            tower.Tick(deltaTime, Enemies.ToList());
        }

        if (Enemies.Count == 0)
        {
            IsWaveRunning = false;
            StatusText = Lives > 0 ? "테스트 웨이브 클리어" : "테스트 실패";
        }
    }

    private void Spawn(string enemyId)
    {
        var enemy = EnemyFactory.Create(enemyId);
        var offset = Enemies.Count * 28;
        var wrappedPath = Path
            .Select((point, index) => index == 0 ? new SandboxPoint(point.X - offset, point.Y) : point)
            .ToList();
        Enemies.Add(new SandboxEnemy(enemy, wrappedPath));
    }

    private void SpawnDeathSpawns(SandboxEnemy parent)
    {
        foreach (var child in parent.Enemy.CreateDeathSpawns())
        {
            Enemies.Add(new SandboxEnemy(child, parent.Path, child.X, child.Y, parent.TargetIndex));
        }
    }

    private SandboxEnemy? FindBoss()
    {
        return Enemies.FirstOrDefault(enemy => enemy.Enemy.Category == EnemyCategory.Boss && enemy.Enemy.IsAlive);
    }
}
