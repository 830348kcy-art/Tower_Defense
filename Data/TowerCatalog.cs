using System.Collections.Generic;
using KingdomRushClone.Models;

namespace KingdomRushClone.Data;

public static class TowerCatalog
{
    public static readonly Dictionary<TowerKind, TowerDef> Towers = Build();

    private static Dictionary<TowerKind, TowerDef> Build()
    {
        var archer = new TowerDef
        {
            Kind = TowerKind.Archer,
            Name = "아처 타워",
            Description = "빠른 공격, 단일 표적",
            GuideColorHex = "#4CAF50",
            TowerColorHex = "#4CAF50",
            ProjectileColorHex = "#2196F3", // 아처 투사체는 파란색
            Levels =
            {
                new() { Cost = 70,  Damage = 6,  Range = 110, AttackInterval = 0.9, DamageType = DamageType.Physical },
                new() { Cost = 60,  Damage = 12, Range = 120, AttackInterval = 0.85, DamageType = DamageType.Physical },
                new() { Cost = 90,  Damage = 20, Range = 135, AttackInterval = 0.8, DamageType = DamageType.Physical },
            },
            BranchA = new TowerDef
            {
                Kind = TowerKind.Archer, Branch = TowerBranch.A,
                Name = "사격수 타워", Description = "초고데미지 단일", 
                GuideColorHex = "#1B5E20", TowerColorHex = "#1B5E20", ProjectileColorHex = "#1B5E20",
                Levels = { new() { Cost = 250, Damage = 80, Range = 180, AttackInterval = 1.1, DamageType = DamageType.Physical } }
            },
            BranchB = new TowerDef
            {
                Kind = TowerKind.Archer, Branch = TowerBranch.B,
                Name = "속사 타워", Description = "다중 연사", 
                GuideColorHex = "#2E7D32", TowerColorHex = "#2E7D32", ProjectileColorHex = "#2E7D32",
                Levels = { new() { Cost = 250, Damage = 25, Range = 140, AttackInterval = 0.35, DamageType = DamageType.Physical } }
            },
        };

        var mage = new TowerDef
        {
            Kind = TowerKind.Mage,
            Name = "마법 타워",
            Description = "마법 피해",
            GuideColorHex = "#3F51B5",      // 도감: 남색
            TowerColorHex = "#5C6BC0",      // 타워: 연한 남색
            ProjectileColorHex = "#9FA8DA", // 투사체: 아주 연한 파랑
            Levels =
            {
                new() { Cost = 100, Damage = 14, Range = 100, AttackInterval = 1.4, DamageType = DamageType.Magic, SlowAmount = 0.20},
                new() { Cost = 90,  Damage = 24, Range = 110, AttackInterval = 1.3, DamageType = DamageType.Magic, SlowAmount = 0.25},
                new() { Cost = 130, Damage = 42, Range = 125, AttackInterval = 1.2, DamageType = DamageType.Magic, SlowAmount = 0.30},
            },
            BranchA = new TowerDef
            {
                Kind = TowerKind.Mage, Branch = TowerBranch.A,
                Name = "서리 마법 타워", Description = "광역 빙결", 
                GuideColorHex = "#1A237E", TowerColorHex = "#3949AB", ProjectileColorHex = "#81D4FA",
                Levels = { new() { Cost = 300, Damage = 50, Range = 130, AttackInterval = 1.4, DamageType = DamageType.Magic, SplashRadius = 70, SlowAmount = 0.55, SlowDuration = 2.5 } }
            },
            BranchB = new TowerDef
            {
                Kind = TowerKind.Mage, Branch = TowerBranch.B,
                Name = "화염 마법 타워", Description = "도트 화염 + 광역", 
                GuideColorHex = "#B71C1C", TowerColorHex = "#E53935", ProjectileColorHex = "#FFAB91",
                Levels = { new() { Cost = 300, Damage = 60, Range = 130, AttackInterval = 1.3, DamageType = DamageType.Magic, SplashRadius = 60, DotDamage = 15, DotDuration = 3.0 } }
            },
        };

        var bombard = new TowerDef
        {
            Kind = TowerKind.Bombard,
            Name = "폭격 타워",
            Description = "광역 스플래시",
            GuideColorHex = "#FF9800",      // 도감: 주황
            TowerColorHex = "#F57C00",      // 타워: 진한 주황
            ProjectileColorHex = "#FFB74D", // 투사체: 연한 주황
            Levels =
            {
                new() { Cost = 150, Damage = 25, Range = 95,  AttackInterval = 2.2, DamageType = DamageType.Explosive, SplashRadius = 50 },
                new() { Cost = 130, Damage = 45, Range = 105, AttackInterval = 2.0, DamageType = DamageType.Explosive, SplashRadius = 55 },
                new() { Cost = 180, Damage = 75, Range = 115, AttackInterval = 1.8, DamageType = DamageType.Explosive, SplashRadius = 60 },
            },
            BranchA = new TowerDef
            {
                Kind = TowerKind.Bombard, Branch = TowerBranch.A,
                Name = "박격포", Description = "초장거리 초광역",
                GuideColorHex = "#E65100", TowerColorHex = "#EF6C00", ProjectileColorHex = "#FFE082",
                Levels = { new() { Cost = 400, Damage = 120, Range = 200, AttackInterval = 2.4, DamageType = DamageType.Explosive, SplashRadius = 85 } }
            },
            BranchB = new TowerDef
            {
                Kind = TowerKind.Bombard, Branch = TowerBranch.B,
                Name = "지뢰 설치", Description = "경로 광역 즉발",
                GuideColorHex = "#BF360C", TowerColorHex = "#D84315", ProjectileColorHex = "#FFCCBC",
                Levels = { new() { Cost = 400, Damage = 200, Range = 120, AttackInterval = 3.0, DamageType = DamageType.Explosive, SplashRadius = 70 } }
            },
        };

        var barracks = new TowerDef
        {
            Kind = TowerKind.Barracks,
            Name = "병영",
            Description = "근접 차단 유닛 소환",
            GuideColorHex = "#795548",      // 도감: 갈색
            TowerColorHex = "#8D6E63",      // 타워: 연한 갈색
            ProjectileColorHex = "#A1887F", // 투사체(집결지): 아주 연한 갈색
            Levels =
            {
                new() { Cost = 80,  SoldierCount = 2, SoldierHp = 60,  SoldierDamage = 3, SoldierRespawn = 10, Range = 80,  AttackInterval = 1.0, DamageType = DamageType.Physical },
                new() { Cost = 70,  SoldierCount = 2, SoldierHp = 100, SoldierDamage = 6, SoldierRespawn = 9,  Range = 90,  AttackInterval = 0.9, DamageType = DamageType.Physical },
                new() { Cost = 110, SoldierCount = 3, SoldierHp = 160, SoldierDamage = 10, SoldierRespawn = 8, Range = 100, AttackInterval = 0.8, DamageType = DamageType.Physical },
            },
            BranchA = new TowerDef
            {
                Kind = TowerKind.Barracks, Branch = TowerBranch.A,
                Name = "성기사 부대", Description = "고체력 탱커",
                GuideColorHex = "#3E2723", TowerColorHex = "#5D4037", ProjectileColorHex = "#D7CCC8",
                Levels = { new() { Cost = 280, SoldierCount = 3, SoldierHp = 320, SoldierDamage = 18, SoldierRespawn = 7, Range = 110, AttackInterval = 0.7, DamageType = DamageType.Physical } }
            },
            BranchB = new TowerDef
            {
                Kind = TowerKind.Barracks, Branch = TowerBranch.B,
                Name = "도적 부대", Description = "고DPS 어쌔신",
                GuideColorHex = "#4E342E", TowerColorHex = "#6D4C41", ProjectileColorHex = "#BCAAA4",
                Levels = { new() { Cost = 280, SoldierCount = 3, SoldierHp = 180, SoldierDamage = 32, SoldierRespawn = 6, Range = 110, AttackInterval = 0.5, DamageType = DamageType.Physical } }
            },
        };

        var slow = new TowerDef
        {
            Kind = TowerKind.Slow,
            Name = "슬로우 타워",
            Description = "냉기 마법으로 적들을 둔화시키고 약한 피해를 입힙니다.",
            GuideColorHex = "#87CEEB",
            TowerColorHex = "#87CEEB",
            ProjectileColorHex = "#ADD8E6", // 투사체는 더 연한 하늘색
            Levels =
            {
                new() { Cost = 80,  Damage = 1, Range = 50, AttackInterval = 2.0, DamageType = DamageType.Magic, SplashRadius = 60, SlowAmount = 0.40, SlowDuration = 2.0 },
                new() { Cost = 70,  Damage = 2, Range = 60, AttackInterval = 1.9, DamageType = DamageType.Magic, SplashRadius = 65, SlowAmount = 0.45, SlowDuration = 2.2 },
                new() { Cost = 110, Damage = 5, Range = 75, AttackInterval = 1.8, DamageType = DamageType.Magic, SplashRadius = 75, SlowAmount = 0.50, SlowDuration = 2.5 },
            },
            // 분기 기능은 기본값(None)으로 유지하거나 추후 확장 가능
            BranchA = null,
            BranchB = null
        };

        return new()
        {
            { TowerKind.Archer, archer },
            { TowerKind.Mage, mage },
            { TowerKind.Bombard, bombard },
            { TowerKind.Barracks, barracks },
            { TowerKind.Slow, slow },
        };
    }
}
