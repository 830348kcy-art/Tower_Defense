using TowerDefense.Enemies;

namespace TowerDefense.Towers;

public abstract class TowerBase
{
    public void Hit(EnemyBase enemy)
    {
        ArgumentNullException.ThrowIfNull(enemy);
        OnHit(enemy);
    }

    protected abstract void OnHit(EnemyBase enemy);
}
