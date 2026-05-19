using TowerDefense.Enemies;
using TowerDefense.Towers;

namespace TowerDefense.Sandbox;

public enum SandboxTowerKind
{
    Basic,
    Slow
}

public sealed class SandboxTower
{
    private readonly SlowTower _slowTower = new();

    public SandboxTower(SandboxTowerKind kind, double x, double y)
    {
        Kind = kind;
        X = x;
        Y = y;
    }

    public SandboxTowerKind Kind { get; }

    public double X { get; }

    public double Y { get; }

    public float Range { get; init; } = 150f;

    public float Damage { get; init; } = 18f;

    public double FireInterval { get; init; } = 0.35;

    private double _cooldown;

    public bool Tick(double deltaTime, IReadOnlyList<SandboxEnemy> enemies)
    {
        _cooldown -= deltaTime;
        if (_cooldown > 0)
        {
            return false;
        }

        var targets = enemies
            .Where(enemy => enemy.Enemy.IsAlive && !enemy.ReachedEnd && enemy.DistanceTo(X, Y) <= Range);

        var target = Kind == SandboxTowerKind.Slow
            ? targets
                .OrderBy(enemy => enemy.Enemy.SlowFactor < 1.0f)
                .ThenByDescending(enemy => enemy.Enemy.ActualMoveSpeed)
                .ThenBy(enemy => enemy.DistanceTo(X, Y))
                .FirstOrDefault()
            : targets
                .OrderBy(enemy => enemy.DistanceTo(X, Y))
                .FirstOrDefault();

        if (target is null)
        {
            return false;
        }

        if (Kind == SandboxTowerKind.Slow)
        {
            _slowTower.Hit(target.Enemy);
        }
        else
        {
            target.Enemy.TakeDamage(Damage, DamageType.Single);
        }

        _cooldown = FireInterval;
        return true;
    }
}
