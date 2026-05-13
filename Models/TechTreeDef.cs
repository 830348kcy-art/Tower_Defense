using System.Collections.Generic;

namespace KingdomRushClone.Models;

public enum TechId
{
    TowerAttack,
    TowerSpeed,
    TowerRange,
    TowerCostReduction,
    EnemyHpReduction,
    EnemySpeedReduction,
    WaveGoldBonus,
    KillGoldBonus,
    MeteorCooldown,
    ReinforcementDuration
}

public class TechNode
{
    public TechId Id;
    public string Name = "";
    public string Description = "";
    public int MaxLevel = 5;
    public int CostPerLevel = 1;
    public double EffectPerLevel;
    public string Category = "";
}

public static class TechTreeCatalog
{
    public static readonly List<TechNode> Nodes = new()
    {
        new() { Id = TechId.TowerAttack, Name = "타워 공격력", Description = "모든 타워 공격력 +5%/Lv", MaxLevel = 5, CostPerLevel = 2, EffectPerLevel = 0.05, Category = "타워" },
        new() { Id = TechId.TowerSpeed,  Name = "타워 공격속도", Description = "모든 타워 공격속도 +4%/Lv", MaxLevel = 5, CostPerLevel = 2, EffectPerLevel = 0.04, Category = "타워" },
        new() { Id = TechId.TowerRange,  Name = "타워 사거리", Description = "모든 타워 사거리 +8%/Lv", MaxLevel = 3, CostPerLevel = 3, EffectPerLevel = 0.08, Category = "타워" },
        new() { Id = TechId.TowerCostReduction, Name = "건설 비용 절감", Description = "건설 비용 -5%/Lv", MaxLevel = 3, CostPerLevel = 2, EffectPerLevel = 0.05, Category = "타워" },
        new() { Id = TechId.EnemyHpReduction, Name = "적 HP 감소", Description = "모든 적 HP -3%/Lv", MaxLevel = 3, CostPerLevel = 3, EffectPerLevel = 0.03, Category = "적 약화" },
        new() { Id = TechId.EnemySpeedReduction, Name = "적 이동속도 감소", Description = "모든 적 이속 -3%/Lv", MaxLevel = 3, CostPerLevel = 2, EffectPerLevel = 0.03, Category = "적 약화" },
        new() { Id = TechId.WaveGoldBonus, Name = "웨이브 골드 보너스", Description = "웨이브 클리어 +10G/Lv", MaxLevel = 5, CostPerLevel = 1, EffectPerLevel = 10, Category = "경제" },
        new() { Id = TechId.KillGoldBonus, Name = "처치 골드 보너스", Description = "적 처치 +1G/Lv", MaxLevel = 3, CostPerLevel = 2, EffectPerLevel = 1, Category = "경제" },
        new() { Id = TechId.MeteorCooldown, Name = "화포 쿨감", Description = "화포 쿨다운 -10초/Lv", MaxLevel = 3, CostPerLevel = 2, EffectPerLevel = 10, Category = "스킬" },
        new() { Id = TechId.ReinforcementDuration, Name = "지원군 지속시간", Description = "지원군 지속 +5초/Lv", MaxLevel = 3, CostPerLevel = 2, EffectPerLevel = 5, Category = "스킬" },
    };
}
