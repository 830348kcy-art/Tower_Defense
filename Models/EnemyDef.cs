using System.Collections.Generic;

namespace KingdomRushClone.Models;

public class EnemyDef
{
    public EnemyKind Kind;
    public string Name = "";
    public double MaxHp;
    public double Speed;
    public ArmorType Armor;
    public double PhysicalResist;
    public double MagicResist;
    public int GoldReward;
    public int LivesCost = 1;
    public bool IsFlying;
    public string ColorHex = "#88AA66";
    public double Radius = 12;
    public bool SlowImmune;
    public bool IsHealer;
    public double HealAmount;
    public double HealInterval;
    public double HealRadius;
    public List<EnemyKind> DeathSpawns = new();
    public bool IsBoss;
    public bool IsMidBoss;
    public int ShieldCharges;
    public double AuraSpeedBonus;
    public double AuraRadius;
    public double GlobalSpeedBonus;
    public double ChargeHpThreshold = 0.5;
    public double ChargeSpeedMultiplier;
    public double ChargeDuration;
    public double RegenerateSelfPercent;
    public double RegenerateAllyPercent;
    public double RegenerateInterval;
    public double RegenerateRadius;
    public double GhostCycle;
    public double GhostDuration;
}
