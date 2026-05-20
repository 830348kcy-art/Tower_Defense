using System.Collections.Generic;

namespace KingdomRushClone.Models;

public class TowerLevel
{
    public int    Cost;
    public double Damage;
    public double Range;
    public double AttackInterval;
    public double SplashRadius;
    public DamageType DamageType;
    public double SlowAmount;
    public double SlowDuration;
    public double DotDamage;
    public double DotDuration;
    public int    SoldierCount;
    public double SoldierHp;
    public double SoldierDamage;
    public double SoldierRespawn;
    /// <summary>Short tooltip text shown next to the upgrade button, e.g. "+사거리 +슬로우".</summary>
    public string UpgradeNote = "";
}

public class TowerDef
{
    public TowerKind   Kind;
    public string      Name        = "";
    /// <summary>Emoji icon rendered on the tower tile in-game.</summary>
    public string      Icon        = "?";
    public string      Description = "";
     public string GuideColorHex = "#888888";      // 도감 UI용 색상
    public string TowerColorHex = "#888888";      // 게임 내 타일 위 타워 색상
    public string ProjectileColorHex = "#FFFFFF"; // 발사되는 투사체 색상
    public string ImagePath = ""; 
    public List<TowerLevel> Levels = new();
    public TowerDef?   BranchA;
    public TowerDef?   BranchB;
    public TowerBranch Branch      = TowerBranch.None;

    /// <summary>Returns 70% of total invested gold up to the given level.</summary>
    public int SellValue(int level)
    {
        int total = 0;
        for (int i = 0; i <= level && i < Levels.Count; i++) total += Levels[i].Cost;
        return (int)(total * 0.7);
    }
}
