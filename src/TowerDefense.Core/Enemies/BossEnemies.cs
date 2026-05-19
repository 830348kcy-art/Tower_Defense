using TowerDefense.Core;

namespace TowerDefense.Enemies;

public abstract class MiniBossBase : EnemyBase
{
    protected MiniBossBase()
    {
        MoveSpeed = 60f;
        GoldReward = 10;
    }

    public override EnemyCategory Category => EnemyCategory.MiniBoss;
}

public sealed class MiniBossNormal : MiniBossBase
{
    public override string EnemyId => "miniboss_normal";

    public override float BaseHpPercent => 3.0f;
}

public sealed class MiniBossCharge : MiniBossBase
{
    private bool _isDashing;

    public override string EnemyId => "miniboss_charge";

    public override float BaseHpPercent => 2.5f;

    public override void Update(double deltaTime)
    {
        base.Update(deltaTime);

        if (_isDashing || CurrentHp > MaxHp * 0.5f)
        {
            return;
        }

        _isDashing = true;
        MoveSpeed *= 2f;
        Sprite.SetState("dash");
    }
}

public sealed class MiniBossSplit : MiniBossBase
{
    public override string EnemyId => "miniboss_split";

    public override float BaseHpPercent => 3.5f;

    public override IEnumerable<EnemyBase> CreateDeathSpawns()
    {
        for (var i = 0; i < 2; i++)
        {
            yield return new SplitBodyEnemy
            {
                X = X + (i == 0 ? -24 : 24),
                Y = Y
            };
        }
    }
}

public sealed class MiniBossSpeed : MiniBossBase
{
    public const float FieldSpeedBonus = 0.15f;

    public override string EnemyId => "miniboss_speed";

    public override float BaseHpPercent => 2.0f;

    public override void Update(double deltaTime)
    {
        base.Update(deltaTime);

        foreach (var enemy in WaveManager.Instance.ActiveEnemies)
        {
            if (!ReferenceEquals(enemy, this))
            {
                enemy.SpeedBonus_BossAura = FieldSpeedBonus;
            }
        }
    }
}

public abstract class BossBase : EnemyBase
{
    protected BossBase()
    {
        MoveSpeed = 50f;
        GoldReward = 25;
    }

    public override EnemyCategory Category => EnemyCategory.Boss;
}

public sealed class BossNormal : BossBase
{
    public override string EnemyId => "boss_normal";

    public override float BaseHpPercent => 7.5f;
}

public sealed class BossCharge : BossBase
{
    private bool _isDashing;

    public override string EnemyId => "boss_charge";

    public override float BaseHpPercent => 6.0f;

    public override void Update(double deltaTime)
    {
        base.Update(deltaTime);

        if (_isDashing || CurrentHp > MaxHp * 0.5f)
        {
            return;
        }

        _isDashing = true;
        MoveSpeed *= 2f;
        Sprite.SetState("dash");
    }
}

public sealed class BossSplit : BossBase
{
    public override string EnemyId => "boss_split";

    public override float BaseHpPercent => 8.0f;

    public override IEnumerable<EnemyBase> CreateDeathSpawns()
    {
        for (var i = 0; i < 2; i++)
        {
            yield return new MiniBossSplit
            {
                X = X + (i == 0 ? -32 : 32),
                Y = Y
            };
        }
    }
}

public sealed class BossSpeed : BossBase
{
    public const float FieldSpeedBonus = 0.15f;

    public override string EnemyId => "boss_speed";

    public override float BaseHpPercent => 4.0f;

    public override void Update(double deltaTime)
    {
        base.Update(deltaTime);

        Sprite.SetState("buff");
        foreach (var enemy in WaveManager.Instance.ActiveEnemies)
        {
            if (!ReferenceEquals(enemy, this))
            {
                enemy.SpeedBonus_BossAura = FieldSpeedBonus;
            }
        }
    }
}
