using System.Collections.Generic;
using KingdomRushClone.Models;

namespace KingdomRushClone.Game;

public class EnemyInstance
{
    public EnemyDef Def = null!;
    public Vec2 Pos;
    public List<Vec2> Path = new();
    public int WaypointIndex;
    public double Hp;
    public double MaxHp;
    public double Speed;
    public double ExternalSpeedBonus;
    public double SlowTimer;
    public double SlowFactor;
    public double DotTimer;
    public double DotDps;
    public double HealTimer;
    public double RegenerateTimer;
    public double GlobalSpeedBonusTimer;
    public double GlobalSpeedBonusActiveTimer;
    public int ShieldCharges;
    public bool ChargeTriggered;
    public double ChargeTimer;
    public bool Alive = true;
    public bool ReachedBase;
    public Soldier? EngagedBy;
    public double EngageTimer;
    public int PathIndex;
    public bool IsWyvern => Def.Kind == EnemyKind.EliteWyvern;
    public bool CanCharge => Def.ChargeSpeedMultiplier > 1;
    public double EffectivePhysicalResist => Def.PhysicalResist + (ChargeTriggered ? Def.ChargePhysicalResistBonus : 0.0);

    public void Tick(double dt)
    {
        if (!Alive) return;

        if (DotTimer > 0)
        {
            Hp -= DotDps * dt;
            DotTimer -= dt;
            if (Hp <= 0) { Alive = false; return; }
        }
        UpdateChargeState();

        if (IsWyvern && EngagedBy != null)
        {
            EngagedBy.Target = null;
            EngagedBy = null;
        }

        if (EngagedBy != null && EngagedBy.Alive)
        {
            EngageTimer -= dt;
            if (EngageTimer <= 0)
            {
                EngagedBy.Hp -= Def.IsBoss || Def.IsMidBoss ? 30 : 8;
                EngageTimer = 1.0;
                if (EngagedBy.Hp <= 0) EngagedBy.Alive = false;
            }
            return;
        }
        EngagedBy = null;

        double effSpeed = Speed * (1 + ExternalSpeedBonus);
        UpdateChargeState();

        if (Def.ChargeSpeedPersists && ChargeTriggered)
        {
            effSpeed *= Def.ChargeSpeedMultiplier;
        }
        else if (ChargeTimer > 0)
        {
            effSpeed *= Def.ChargeSpeedMultiplier;
            ChargeTimer -= dt;
        }

        if (SlowTimer > 0)
        {
            effSpeed *= (1 - SlowFactor);
            SlowTimer -= dt;
        }

        if (WaypointIndex >= Path.Count - 1)
        {
            ReachedBase = true;
            Alive = false;
            return;
        }

        var target = Path[WaypointIndex + 1];
        var dir = (target - Pos).Normalized();
        var step = dir * effSpeed * dt;
        var nextPos = Pos + step;
        if (Pos.DistanceTo(target) < effSpeed * dt + 1)
        {
            Pos = target;
            WaypointIndex++;
        }
        else
        {
            Pos = nextPos;
        }
    }

    public void ApplyDamage(double dmg, DamageType type)
    {
        if (dmg <= 0 || !Alive) return;

        if (IsWyvern && type == DamageType.Explosive) return;
        UpdateChargeState();

        if (type != DamageType.True)
        {
            if (ShieldCharges > 0)
            {
                ShieldCharges--;
                return;
            }
        }

        double mult = type switch
        {
            DamageType.Physical => 1.0 - EffectivePhysicalResist,
            DamageType.Magic => 1.0 - Def.MagicResist,
            DamageType.Explosive => 1.0 - EffectivePhysicalResist * 0.5,
            _ => 1.0
        };
        Hp -= dmg * mult;
        if (Hp <= 0) Alive = false;
        else UpdateChargeState();
    }

    public void ApplySlow(double amount, double duration)
    {
        if (Def.SlowImmune) return;
        if (amount > SlowFactor || SlowTimer <= 0)
        {
            SlowFactor = amount;
            SlowTimer = duration;
        }
        else if (SlowTimer < duration) SlowTimer = duration;
    }

    public void ApplyDot(double dps, double duration)
    {
        DotDps = dps;
        DotTimer = duration;
    }

    private void UpdateChargeState()
    {
        if (!CanCharge || ChargeTriggered || Hp > MaxHp * Def.ChargeHpThreshold) return;

        ChargeTriggered = true;
        ChargeTimer = Def.ChargeSpeedPersists ? 0 : Def.ChargeDuration;
    }
}
