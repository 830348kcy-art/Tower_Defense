using TowerDefense.Enemies;

namespace TowerDefense.Data;

public sealed record EnemySpec(
    string EnemyId,
    string DisplayName,
    EnemyCategory Category,
    float BaseHpPercent,
    float MoveSpeed,
    string[] Abilities);
