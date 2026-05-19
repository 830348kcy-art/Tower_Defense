using TowerDefense.Data;
using TowerDefense.Enemies;

namespace TowerDefense.Core;

public sealed class WaveManager
{
    public static WaveManager Instance { get; private set; } = new();

    private WaveManager()
    {
    }

    public int CurrentStage { get; private set; }

    public int CurrentWave { get; private set; }

    public List<EnemyBase> ActiveEnemies { get; } = new();

    public event Action? WaveCompleted;

    public void StartStage(int stage)
    {
        CurrentStage = stage;
        GameSession.Instance.OnStageChanged(stage);
        CurrentWave = 0;
        ActiveEnemies.Clear();
        StartNextWave();
    }

    public void StartNextWave()
    {
        if (CurrentStage == 0)
        {
            throw new InvalidOperationException("StartStage must be called before StartNextWave.");
        }

        if (CurrentWave >= 8)
        {
            return;
        }

        CurrentWave++;
        foreach (var entry in WavePlan.GetWave(CurrentStage, CurrentWave))
        {
            for (var i = 0; i < entry.Count; i++)
            {
                SpawnEnemy(EnemyFactory.Create(entry.EnemyId));
            }
        }
    }

    public void SpawnEnemy(EnemyBase enemy)
    {
        ActiveEnemies.Add(enemy);
        enemy.OnDeath += HandleEnemyDeath;
        enemy.OnReachEnd += HandleEnemyReachEnd;
        enemy.Initialize();
    }

    private void HandleEnemyDeath(EnemyBase enemy)
    {
        RemoveEnemy(enemy);
        foreach (var child in enemy.CreateDeathSpawns())
        {
            SpawnEnemy(child);
        }

        CheckWaveComplete();
    }

    private void HandleEnemyReachEnd(EnemyBase enemy)
    {
        RemoveEnemy(enemy);
        CheckWaveComplete();
    }

    private void RemoveEnemy(EnemyBase enemy)
    {
        enemy.OnDeath -= HandleEnemyDeath;
        enemy.OnReachEnd -= HandleEnemyReachEnd;
        ActiveEnemies.Remove(enemy);
    }

    private void CheckWaveComplete()
    {
        if (ActiveEnemies.Count == 0)
        {
            WaveCompleted?.Invoke();
        }
    }

    public static void ResetForTests()
    {
        Instance = new WaveManager();
    }
}
