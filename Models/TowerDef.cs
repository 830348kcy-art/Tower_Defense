using System.Collections.Generic;

namespace KingdomRushClone.Models;

public class TowerLevel
{
    public int Cost;
    public double Damage;
    public double Range;
    public double AttackInterval;
    public double SplashRadius;
    public DamageType DamageType;
    public double SlowAmount;
    public double SlowDuration;
    public double DotDamage;
    public double DotDuration;
    public int SoldierCount;
    public double SoldierHp;
    public double SoldierDamage;
    public double SoldierRespawn;
}

public class TowerDef
{
    public TowerKind Kind;
    public string Name = "";
    public string Description = "";
    public string ColorHex = "#888888";
    public List<TowerLevel> Levels = new();
    public TowerDef? BranchA;
    public TowerDef? BranchB;
    public TowerBranch Branch = TowerBranch.None;

    public int SellValue(int level)
    {
        int total = 0;
        for (int i = 0; i <= level && i < Levels.Count; i++) total += Levels[i].Cost;
        return (int)(total * 0.7);
    }
}
