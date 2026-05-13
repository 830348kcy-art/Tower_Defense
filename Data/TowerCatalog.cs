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
            ColorHex = "#4CAF50",
            Levels =
            {
                new() { Cost = 70,  Damage = 6,  Range = 110, AttackInterval = 0.9, DamageType = DamageType.Physical },
                new() { Cost = 60,  Damage = 12, Range = 120, AttackInterval = 0.85, DamageType = DamageType.Physical },
                new() { Cost = 90,  Damage = 20, Range = 135, AttackInterval = 0.8, DamageType = DamageType.Physical },
            },
            BranchA = new TowerDef
            {
                Kind = TowerKind.Archer, Branch = TowerBranch.A,
                Name = "사격수 타워", Description = "초고데미지 단일", ColorHex = "#1B5E20",
                Levels = { new() { Cost = 250, Damage = 80, Range = 180, AttackInterval = 1.1, DamageType = DamageType.Physical } }
            },
            BranchB = new TowerDef
            {
                Kind = TowerKind.Archer, Branch = TowerBranch.B,
                Name = "속사 타워", Description = "다중 연사", ColorHex = "#2E7D32",
                Levels = { new() { Cost = 250, Damage = 25, Range = 140, AttackInterval = 0.35, DamageType = DamageType.Physical } }
            },
        };

        var mage = new TowerDef
        {
            Kind = TowerKind.Mage,
            Name = "마법사 타워",
            Description = "마법 피해, 슬로우",
            ColorHex = "#3F51B5",
            Levels =
            {
                new() { Cost = 100, Damage = 14, Range = 100, AttackInterval = 1.4, DamageType = DamageType.Magic, SlowAmount = 0.20, SlowDuration = 1.0 },
                new() { Cost = 90,  Damage = 24, Range = 110, AttackInterval = 1.3, DamageType = DamageType.Magic, SlowAmount = 0.25, SlowDuration = 1.2 },
                new() { Cost = 130, Damage = 42, Range = 125, AttackInterval = 1.2, DamageType = DamageType.Magic, SlowAmount = 0.30, SlowDuration = 1.5 },
            },
            BranchA = new TowerDef
            {
                Kind = TowerKind.Mage, Branch = TowerBranch.A,
                Name = "서리 마법사", Description = "광역 빙결", ColorHex = "#1A237E",
                Levels = { new() { Cost = 300, Damage = 50, Range = 130, AttackInterval = 1.4, DamageType = DamageType.Magic, SplashRadius = 70, SlowAmount = 0.55, SlowDuration = 2.5 } }
            },
            BranchB = new TowerDef
            {
                Kind = TowerKind.Mage, Branch = TowerBranch.B,
                Name = "화염 마법사", Description = "도트 화염 + 광역", ColorHex = "#B71C1C",
                Levels = { new() { Cost = 300, Damage = 60, Range = 130, AttackInterval = 1.3, DamageType = DamageType.Magic, SplashRadius = 60, DotDamage = 15, DotDuration = 3.0 } }
            },
        };

        var bombard = new TowerDef
        {
            Kind = TowerKind.Bombard,
            Name = "폭격 타워",
            Description = "광역 스플래시",
            ColorHex = "#FF9800",
            Levels =
            {
                new() { Cost = 150, Damage = 25, Range = 95,  AttackInterval = 2.2, DamageType = DamageType.Explosive, SplashRadius = 50 },
                new() { Cost = 130, Damage = 45, Range = 105, AttackInterval = 2.0, DamageType = DamageType.Explosive, SplashRadius = 55 },
                new() { Cost = 180, Damage = 75, Range = 115, AttackInterval = 1.8, DamageType = DamageType.Explosive, SplashRadius = 60 },
            },
            BranchA = new TowerDef
            {
                Kind = TowerKind.Bombard, Branch = TowerBranch.A,
                Name = "박격포", Description = "초장거리 초광역", ColorHex = "#E65100",
                Levels = { new() { Cost = 400, Damage = 120, Range = 200, AttackInterval = 2.4, DamageType = DamageType.Explosive, SplashRadius = 85 } }
            },
            BranchB = new TowerDef
            {
                Kind = TowerKind.Bombard, Branch = TowerBranch.B,
                Name = "지뢰 설치", Description = "경로 광역 즉발", ColorHex = "#BF360C",
                Levels = { new() { Cost = 400, Damage = 200, Range = 120, AttackInterval = 3.0, DamageType = DamageType.Explosive, SplashRadius = 70 } }
            },
        };

        var barracks = new TowerDef
        {
            Kind = TowerKind.Barracks,
            Name = "병영",
            Description = "근접 차단 유닛 소환",
            ColorHex = "#795548",
            Levels =
            {
                new() { Cost = 80,  SoldierCount = 2, SoldierHp = 60,  SoldierDamage = 3, SoldierRespawn = 10, Range = 80,  AttackInterval = 1.0, DamageType = DamageType.Physical },
                new() { Cost = 70,  SoldierCount = 2, SoldierHp = 100, SoldierDamage = 6, SoldierRespawn = 9,  Range = 90,  AttackInterval = 0.9, DamageType = DamageType.Physical },
                new() { Cost = 110, SoldierCount = 3, SoldierHp = 160, SoldierDamage = 10, SoldierRespawn = 8, Range = 100, AttackInterval = 0.8, DamageType = DamageType.Physical },
            },
            BranchA = new TowerDef
            {
                Kind = TowerKind.Barracks, Branch = TowerBranch.A,
                Name = "성기사 부대", Description = "고체력 탱커", ColorHex = "#3E2723",
                Levels = { new() { Cost = 280, SoldierCount = 3, SoldierHp = 320, SoldierDamage = 18, SoldierRespawn = 7, Range = 110, AttackInterval = 0.7, DamageType = DamageType.Physical } }
            },
            BranchB = new TowerDef
            {
                Kind = TowerKind.Barracks, Branch = TowerBranch.B,
                Name = "도적 부대", Description = "고DPS 어쌔신", ColorHex = "#4E342E",
                Levels = { new() { Cost = 280, SoldierCount = 3, SoldierHp = 180, SoldierDamage = 32, SoldierRespawn = 6, Range = 110, AttackInterval = 0.5, DamageType = DamageType.Physical } }
            },
        };

        return new()
        {
            { TowerKind.Archer, archer },
            { TowerKind.Mage, mage },
            { TowerKind.Bombard, bombard },
            { TowerKind.Barracks, barracks },
        };
    }
}
