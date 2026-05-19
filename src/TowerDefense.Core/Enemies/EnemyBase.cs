using TowerDefense.Core;

namespace TowerDefense.Enemies;

public abstract class EnemyBase
{
    public abstract string EnemyId { get; }

    public abstract EnemyCategory Category { get; }

    public abstract float BaseHpPercent { get; }

    public float MaxHp => 80f * BaseHpPercent * GameSession.Instance.WaveMultiplier;

    public float CurrentHp { get; protected set; }

    public virtual float MoveSpeed { get; protected set; } = 80f;

    public float SpeedBonus_Aura { get; set; }

    public float SpeedBonus_BossAura { get; set; }

    public float SlowFactor { get; private set; } = 1.0f;

    private float _slowTimer;

    public float ActualMoveSpeed => MoveSpeed * (1f + SpeedBonus_Aura + SpeedBonus_BossAura) * SlowFactor;

    public int GoldReward { get; protected set; } = 1;

    public bool IsAlive => CurrentHp > 0;

    public bool IsInvincible { get; set; }

    public bool IsImmune_Aoe { get; protected set; }

    public bool IsImmune_Single { get; protected set; }

    public double X { get; set; }

    public double Y { get; set; }

    public IEnemySprite Sprite { get; } = new NullEnemySprite();

    public event Action<EnemyBase>? OnDeath;

    public event Action<EnemyBase, float>? OnDamaged;

    public event Action<EnemyBase>? OnReachEnd;

    public virtual void Initialize()
    {
        CurrentHp = MaxHp;
        SlowFactor = 1.0f;
        _slowTimer = 0f;
        Sprite.SetState("walk");
    }

    public virtual void Update(double deltaTime)
    {
        if (_slowTimer <= 0f)
        {
            return;
        }

        _slowTimer -= (float)deltaTime;
        if (_slowTimer <= 0f)
        {
            _slowTimer = 0f;
            SlowFactor = 1.0f;
        }
    }

    public void ApplySlow(float factor, float duration)
    {
        if (!IsAlive || IsInvincible)
        {
            return;
        }

        if (factor <= 0f || factor > 1f)
        {
            throw new ArgumentOutOfRangeException(nameof(factor), "Slow factor must be greater than 0 and less than or equal to 1.");
        }

        if (duration <= 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Slow duration must be greater than 0.");
        }

        SlowFactor = Math.Min(SlowFactor, factor);
        _slowTimer = Math.Max(_slowTimer, duration);
    }

    public virtual void TakeDamage(float amount, DamageType type)
    {
        if (!IsAlive)
        {
            return;
        }

        if (type != DamageType.True)
        {
            if (IsInvincible)
            {
                return;
            }

            if (type == DamageType.Aoe && IsImmune_Aoe)
            {
                return;
            }

            if (type == DamageType.Single && IsImmune_Single)
            {
                return;
            }
        }

        var applied = Math.Max(0f, amount);
        CurrentHp = Math.Max(0f, CurrentHp - applied);
        OnDamaged?.Invoke(this, applied);

        if (CurrentHp <= 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (!IsAlive)
        {
            return;
        }

        CurrentHp = Math.Min(MaxHp, CurrentHp + Math.Max(0f, amount));
    }

    public void ReachEnd()
    {
        OnReachEnd?.Invoke(this);
    }

    public virtual IEnumerable<EnemyBase> CreateDeathSpawns()
    {
        return Array.Empty<EnemyBase>();
    }

    protected virtual void Die()
    {
        Sprite.SetState("death");
        OnDeath?.Invoke(this);
    }
}
