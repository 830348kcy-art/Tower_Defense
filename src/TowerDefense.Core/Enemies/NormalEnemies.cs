namespace TowerDefense.Enemies;

public sealed class NormalEnemy : EnemyBase
{
    public override string EnemyId => "enemy_normal";

    public override EnemyCategory Category => EnemyCategory.Normal;

    public override float BaseHpPercent => 1.0f;
}

public sealed class FastEnemy : EnemyBase
{
    public FastEnemy()
    {
        MoveSpeed = 160f;
    }

    public override string EnemyId => "enemy_fast";

    public override EnemyCategory Category => EnemyCategory.Normal;

    public override float BaseHpPercent => 0.5f;
}

public sealed class SplitBodyEnemy : EnemyBase
{
    public override string EnemyId => "enemy_split_body";

    public override EnemyCategory Category => EnemyCategory.Normal;

    public override float BaseHpPercent => 2.25f;

    public override IEnumerable<EnemyBase> CreateDeathSpawns()
    {
        for (var i = 0; i < 3; i++)
        {
            yield return new SplitSmallEnemy
            {
                X = X + (i - 1) * 20,
                Y = Y
            };
        }
    }
}

public sealed class SplitSmallEnemy : EnemyBase
{
    public override string EnemyId => "enemy_split_small";

    public override EnemyCategory Category => EnemyCategory.Normal;

    public override float BaseHpPercent => 0.75f;
}
