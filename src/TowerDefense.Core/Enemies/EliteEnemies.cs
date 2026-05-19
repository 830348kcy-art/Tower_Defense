using TowerDefense.Core;

namespace TowerDefense.Enemies;

public sealed class EliteShieldEnemy : EnemyBase
{
    private int _shieldCount = 3;

    public override string EnemyId => "elite_shield";

    public override EnemyCategory Category => EnemyCategory.Elite;

    public override float BaseHpPercent => 5.0f;

    public override void TakeDamage(float amount, DamageType type)
    {
        if (type != DamageType.True && _shieldCount > 0)
        {
            _shieldCount--;
            Sprite.SetState(_shieldCount > 0 ? "shield" : "walk");
            return;
        }

        base.TakeDamage(amount, type);
    }

    public override void Update(double deltaTime)
    {
        base.Update(deltaTime);

        foreach (var enemy in WaveManager.Instance.ActiveEnemies)
        {
            if (ReferenceEquals(enemy, this))
            {
                continue;
            }

            if (DistanceTo(enemy) <= 100f)
            {
                enemy.SpeedBonus_Aura = 0.20f;
            }
        }
    }

    private double DistanceTo(EnemyBase enemy)
    {
        var dx = X - enemy.X;
        var dy = Y - enemy.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}

public sealed class EliteChargeEnemy : EnemyBase
{
    private bool _isDashing;
    private float _dashTimer;

    public override string EnemyId => "elite_charge";

    public override EnemyCategory Category => EnemyCategory.Elite;

    public override float BaseHpPercent => 2.0f;

    public override void Update(double deltaTime)
    {
        base.Update(deltaTime);

        if (!_isDashing && CurrentHp <= MaxHp * 0.5f)
        {
            StartDash();
        }

        if (!_isDashing)
        {
            return;
        }

        _dashTimer -= (float)deltaTime;
        if (_dashTimer <= 0f)
        {
            EndDash();
        }
    }

    private void StartDash()
    {
        _isDashing = true;
        _dashTimer = 3f;
        MoveSpeed *= 2f;
        Sprite.SetState("dash");
    }

    private void EndDash()
    {
        _isDashing = false;
        MoveSpeed /= 2f;
        Sprite.SetState("walk");
    }
}

public sealed class EliteRegenEnemy : EnemyBase
{
    private const float RegenInterval = 3f;
    private const float SelfRegen = 0.05f;
    private const float AllyRegen = 0.02f;
    private const float AllyRadius = 100f;

    private float _regenTimer = RegenInterval;

    public override string EnemyId => "elite_regen";

    public override EnemyCategory Category => EnemyCategory.Elite;

    public override float BaseHpPercent => 4.0f;

    public override void Update(double deltaTime)
    {
        base.Update(deltaTime);

        _regenTimer -= (float)deltaTime;
        if (_regenTimer > 0f)
        {
            return;
        }

        _regenTimer = RegenInterval;
        Sprite.SetState("heal");
        Heal(MaxHp * SelfRegen);

        foreach (var enemy in WaveManager.Instance.ActiveEnemies)
        {
            if (ReferenceEquals(enemy, this))
            {
                continue;
            }

            if (DistanceTo(enemy) <= AllyRadius)
            {
                enemy.Heal(enemy.MaxHp * AllyRegen);
            }
        }
    }

    private double DistanceTo(EnemyBase enemy)
    {
        var dx = X - enemy.X;
        var dy = Y - enemy.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}

public sealed class EliteResistEnemy : EnemyBase
{
    private float _immuneTimer = 5f;
    private bool _isAoeImmune = true;

    public override string EnemyId => "elite_resist";

    public override EnemyCategory Category => EnemyCategory.Elite;

    public override float BaseHpPercent => 3.5f;

    public override void Initialize()
    {
        base.Initialize();
        ApplyMode();
    }

    public override void Update(double deltaTime)
    {
        base.Update(deltaTime);

        _immuneTimer -= (float)deltaTime;
        if (_immuneTimer > 0f)
        {
            return;
        }

        _immuneTimer = 5f;
        _isAoeImmune = !_isAoeImmune;
        ApplyMode();
    }

    private void ApplyMode()
    {
        IsImmune_Aoe = _isAoeImmune;
        IsImmune_Single = !_isAoeImmune;
        Sprite.SetState(_isAoeImmune ? "immune_aoe" : "immune_single");
    }
}

public sealed class EliteGhostEnemy : EnemyBase
{
    private const float InvincibleDuration = 0.5f;

    private float _cycleTimer = 2f;
    private float _invincibleTimer;

    public override string EnemyId => "elite_ghost";

    public override EnemyCategory Category => EnemyCategory.Elite;

    public override float BaseHpPercent => 2.5f;

    public override void Update(double deltaTime)
    {
        base.Update(deltaTime);

        if (IsInvincible)
        {
            _invincibleTimer -= (float)deltaTime;
            if (_invincibleTimer <= 0f)
            {
                IsInvincible = false;
                _cycleTimer = 2f;
                Sprite.SetState("walk");
            }

            return;
        }

        _cycleTimer -= (float)deltaTime;
        if (_cycleTimer <= 0f)
        {
            IsInvincible = true;
            _invincibleTimer = InvincibleDuration;
            Sprite.SetState("invincible");
        }
    }
}
