using TowerDefense.Enemies;

namespace TowerDefense.Towers;

public sealed class SlowTower : TowerBase
{
    public float SlowFactor { get; init; } = 0.5f;

    public float SlowDuration { get; init; } = 2.0f;

    protected override void OnHit(EnemyBase enemy)
    {
        enemy.ApplySlow(SlowFactor, SlowDuration);
    }
}
