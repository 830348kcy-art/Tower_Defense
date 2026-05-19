namespace TowerDefense.Core;

public sealed record WaveEntry(string EnemyId, int Count, double SpawnIntervalSeconds = 0.4);
