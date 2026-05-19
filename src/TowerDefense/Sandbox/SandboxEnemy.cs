using TowerDefense.Enemies;

namespace TowerDefense.Sandbox;

public sealed class SandboxEnemy
{
    public SandboxEnemy(EnemyBase enemy, IReadOnlyList<SandboxPoint> path)
        : this(enemy, path, path[0].X, path[0].Y, 1)
    {
    }

    public SandboxEnemy(EnemyBase enemy, IReadOnlyList<SandboxPoint> path, double x, double y, int targetIndex)
    {
        if (path.Count < 2)
        {
            throw new ArgumentException("Sandbox enemy path must contain at least two points.", nameof(path));
        }

        Enemy = enemy;
        Path = path;
        X = x;
        Y = y;
        _targetIndex = Math.Clamp(targetIndex, 1, path.Count - 1);
        enemy.Initialize();
        enemy.X = X;
        enemy.Y = Y;
    }

    public EnemyBase Enemy { get; }

    public IReadOnlyList<SandboxPoint> Path { get; }

    public double X { get; private set; }

    public double Y { get; private set; }

    public bool ReachedEnd { get; private set; }

    private int _targetIndex = 1;

    public int TargetIndex => _targetIndex;

    public void Tick(double deltaTime)
    {
        Enemy.Update(deltaTime);
        if (!Enemy.IsAlive || ReachedEnd)
        {
            return;
        }

        var remaining = Enemy.ActualMoveSpeed * deltaTime;
        while (remaining > 0 && !ReachedEnd)
        {
            var target = Path[_targetIndex];
            var dx = target.X - X;
            var dy = target.Y - Y;
            var distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance <= remaining)
            {
                X = target.X;
                Y = target.Y;
                remaining -= distance;
                _targetIndex++;
                ReachedEnd = _targetIndex >= Path.Count;
                continue;
            }

            var ratio = remaining / distance;
            X += dx * ratio;
            Y += dy * ratio;
            remaining = 0;
        }

        Enemy.X = X;
        Enemy.Y = Y;
    }

    public double DistanceTo(double x, double y)
    {
        var dx = X - x;
        var dy = Y - y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}
