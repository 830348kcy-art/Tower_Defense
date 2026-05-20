using System.Collections.Generic;
using KingdomRushClone.Models;

namespace KingdomRushClone.Data;

/// <summary>
/// Defines all four base tower types and their upgrade/branch data.
///
/// DPS reference (Lv1 → Lv3 → Branch):
///   Archer   : 8.9 → 30.0 → BranchA 100 single / BranchB 3×28 multishot
///   Mage     : 10.7→ 37.5 → BranchA 50+AoE freeze / BranchB 65+burn DoT
///   Bombard  : 12.5→ 43.3 → BranchA 52 mega AoE / BranchB instant 250 trap
///   Barracks : — blocking — 2×60hp → 3×180hp → Paladin 3×380hp / Rogue 3×200hp
/// </summary>
public static class TowerCatalog
{
    public static readonly Dictionary<TowerKind, TowerDef> Towers = Build();

    private static Dictionary<TowerKind, TowerDef> Build()
    {
        // ─────────────────────────────────────────────
        //  🏹 ARCHER  —  빠른 공속, 단일 표적, 크리티컬 12 %
        // ─────────────────────────────────────────────
        var archer = new TowerDef
        {
            Kind = TowerKind.Archer,
            Name = "아처 타워",
            Icon = "🏹",
            Description = "빠른 공속과 높은 단일 피해. 12% 확률로 치명타 2.2배.",
            GuideColorHex = "#4CAF50",
            TowerColorHex = "#4CAF50",
            ProjectileColorHex = "#2196F3", // 아처 투사체는 파란색
            Levels =
            {
                new() { Cost = 70,  Damage = 8,  Range = 115, AttackInterval = 0.90,
                        DamageType = DamageType.Physical,
                        UpgradeNote = "" },
                new() { Cost = 65,  Damage = 15, Range = 125, AttackInterval = 0.85,
                        DamageType = DamageType.Physical,
                        UpgradeNote = "+피해 +사거리" },
                new() { Cost = 95,  Damage = 25, Range = 140, AttackInterval = 0.80,
                        DamageType = DamageType.Physical,
                        UpgradeNote = "+피해 +사거리  →  분기 선택 가능" },
            },
            BranchA = new TowerDef   // 정밀 사격
            {
                Kind = TowerKind.Archer,
                Branch = TowerBranch.A,
                Name = "정밀 사수",
                Icon = "🎯",
                Description = "초고피해 단일. 치명타 확률 25%, 방어구 관통.",
                GuideColorHex = "#1B5E20",
                TowerColorHex = "#1B5E20",
                ProjectileColorHex = "#1B5E20",
                Levels = { new() { Cost = 280, Damage = 95, Range = 195, AttackInterval = 1.10,
                                    DamageType = DamageType.Physical,
                                    UpgradeNote = "치명타 25% / 관통" } }
            },
            BranchB = new TowerDef   // 속사
            {
                Kind = TowerKind.Archer,
                Branch = TowerBranch.B,
                Name = "폭풍 사수",
                Icon = "⚡",
                Description = "3개 표적 동시 연사. 빠른 공속 유지.",
                GuideColorHex = "#2E7D32",
                TowerColorHex = "#2E7D32",
                ProjectileColorHex = "#2E7D32",
                Levels = { new() { Cost = 280, Damage = 28, Range = 145, AttackInterval = 0.38,
                                    DamageType = DamageType.Physical,
                                    UpgradeNote = "3중 연사" } }
            },
        };

        // ─────────────────────────────────────────────
        //  🔮 MAGE  —  마법 피해, 슬로우, AoE 분기
        // ─────────────────────────────────────────────
        var mage = new TowerDef
        {
            Kind = TowerKind.Mage,
            Name = "마법 타워",
            Icon = "🔮",
            Description = "마법 피해 + 물리 방어를 무시.",
            GuideColorHex = "#3F51B5",      // 도감: 남색
            TowerColorHex = "#5C6BC0",      // 타워: 연한 남색
            ProjectileColorHex = "#9FA8DA",
            Levels =
            {
               new() { Cost = 100, Damage = 14, Range = 100, AttackInterval = 1.4, DamageType = DamageType.Magic, SlowAmount = 0.20},
                new() { Cost = 90,  Damage = 24, Range = 110, AttackInterval = 1.3, DamageType = DamageType.Magic, SlowAmount = 0.25},
                new() { Cost = 130, Damage = 42, Range = 125, AttackInterval = 1.2, DamageType = DamageType.Magic, SlowAmount = 0.30},
            },
            BranchA = new TowerDef   // 서리 마법사
            {
                Kind = TowerKind.Mage,
                Branch = TowerBranch.A,
                Name = "서리 마법 타워",
                Icon = "❄",
                Description = "광역 냉기 폭발. 55% 슬로우 2.5초.",
                GuideColorHex = "#1A237E",
                TowerColorHex = "#3949AB",
                ProjectileColorHex = "#81D4FA",
                Levels = { new() { Cost = 320, Damage = 55, Range = 135, AttackInterval = 1.40,
                                    DamageType = DamageType.Magic,
                                    SplashRadius = 75, SlowAmount = 0.55, SlowDuration = 2.5,
                                    UpgradeNote = "광역 냉기" } }
            },
            BranchB = new TowerDef   // 화염 마법사
            {
                Kind = TowerKind.Mage,
                Branch = TowerBranch.B,
                Name = "화염 마법 타워",
                Icon = "🔥",
                Description = "광역 화염 폭발 + 3초 화상 도트 15dps.",
                GuideColorHex = "#B71C1C",
                TowerColorHex = "#E53935",
                ProjectileColorHex = "#FFAB91",
                Levels = { new() { Cost = 320, Damage = 65, Range = 135, AttackInterval = 1.30,
                                    DamageType = DamageType.Magic,
                                    SplashRadius = 65, DotDamage = 15, DotDuration = 3.0,
                                    UpgradeNote = "광역 화염 + 화상" } }
            },
        };

        // ─────────────────────────────────────────────
        //  💣 BOMBARD  —  탄도 폭탄, 광역 폭발, 비행 불가
        // ─────────────────────────────────────────────
        var bombard = new TowerDef
        {
            Kind = TowerKind.Bombard,
            Name = "폭격 타워",
            Icon = "💣",
            Description = "포물선 탄도 광역 폭발. 비행 적에게 무효.",
            GuideColorHex = "#F57C00",
            TowerColorHex = "#F57C00",
            ProjectileColorHex = "#FFB74D",
            Levels =
            {
                new() { Cost = 150, Damage = 28, Range = 100, AttackInterval = 2.20,
                        DamageType = DamageType.Explosive, SplashRadius = 52,
                        UpgradeNote = "" },
                new() { Cost = 135, Damage = 50, Range = 110, AttackInterval = 2.00,
                        DamageType = DamageType.Explosive, SplashRadius = 58,
                        UpgradeNote = "+피해 +범위" },
                new() { Cost = 190, Damage = 80, Range = 120, AttackInterval = 1.80,
                        DamageType = DamageType.Explosive, SplashRadius = 65,
                        UpgradeNote = "+피해 +범위  →  분기 선택 가능" },
            },
            BranchA = new TowerDef   // 박격포
            {
                Kind = TowerKind.Bombard,
                Branch = TowerBranch.A,
                Name = "박격포",
                Icon = "🎆",
                Description = "초장거리 초광역 포격. 거대 폭발 반경.",
                GuideColorHex = "#E65100",
                TowerColorHex = "#EF6C00",
                ProjectileColorHex = "#FFE082",
                Levels = { new() { Cost = 420, Damage = 130, Range = 210, AttackInterval = 2.50,
                                    DamageType = DamageType.Explosive, SplashRadius = 92,
                                    UpgradeNote = "초장거리·초광역" } }
            },
            BranchB = new TowerDef   // 지뢰
            {
                Kind = TowerKind.Bombard,
                Branch = TowerBranch.B,
                Name = "지뢰 포대",
                Icon = "💥",
                Description = "쿨타임마다 경로에 즉발 대폭발. 단 폭발력은 최대.",
                GuideColorHex = "#BF360C",
                TowerColorHex = "#D84315",
                ProjectileColorHex = "#FFCCBC",
                Levels = { new() { Cost = 420, Damage = 220, Range = 125, AttackInterval = 3.20,
                                    DamageType = DamageType.Explosive, SplashRadius = 75,
                                    UpgradeNote = "즉발 대폭발" } }
            },
        };

        // ─────────────────────────────────────────────
        //  ⚔️ BARRACKS  —  근접 병사 소환, 경로 차단
        // ─────────────────────────────────────────────
        var barracks = new TowerDef
        {
            Kind = TowerKind.Barracks,
            Name = "병영",
            Icon = "⚔",
            Description = "근접 병사를 소환해 적의 진격을 차단.",
            GuideColorHex = "#795548",      // 도감: 갈색
            TowerColorHex = "#8D6E63",      // 타워: 연한 갈색
            ProjectileColorHex = "#A1887F", // 투사체(집결지): 아주 연한 갈색
            Levels =
            {
                new() { Cost = 80,  SoldierCount = 2, SoldierHp = 65,  SoldierDamage = 4,
                        SoldierRespawn = 10, Range = 85,  AttackInterval = 1.0,
                        DamageType = DamageType.Physical,
                        UpgradeNote = "" },
                new() { Cost = 75,  SoldierCount = 2, SoldierHp = 110, SoldierDamage = 7,
                        SoldierRespawn = 9,  Range = 95,  AttackInterval = 0.9,
                        DamageType = DamageType.Physical,
                        UpgradeNote = "+HP +피해" },
                new() { Cost = 120, SoldierCount = 3, SoldierHp = 175, SoldierDamage = 12,
                        SoldierRespawn = 8,  Range = 105, AttackInterval = 0.8,
                        DamageType = DamageType.Physical,
                        UpgradeNote = "+병사 1 +HP  →  분기 선택 가능" },
            },
            BranchA = new TowerDef   // 성기사
            {
                Kind = TowerKind.Barracks,
                Branch = TowerBranch.A,
                Name = "성기사 부대",
                Icon = "🛡",
                Description = "고체력 탱커. 보스전 차단 특화.",
                GuideColorHex = "#3E2723",
                TowerColorHex = "#5D4037",
                ProjectileColorHex = "#D7CCC8",
                Levels = { new() { Cost = 300, SoldierCount = 3,
                                    SoldierHp = 380, SoldierDamage = 20,
                                    SoldierRespawn = 7, Range = 115, AttackInterval = 0.7,
                                    DamageType = DamageType.Physical,
                                    UpgradeNote = "고HP 탱커" } }
            },
            BranchB = new TowerDef   // 도적
            {
                Kind = TowerKind.Barracks,
                Branch = TowerBranch.B,
                Name = "도적 부대",
                Icon = "🗡",
                Description = "고DPS 어쌔신. 빠른 공속으로 연속 타격.",
                GuideColorHex = "#4E342E",
                TowerColorHex = "#6D4C41",
                ProjectileColorHex = "#BCAAA4",
                Levels = { new() { Cost = 300, SoldierCount = 3,
                                    SoldierHp = 200, SoldierDamage = 35,
                                    SoldierRespawn = 6, Range = 115, AttackInterval = 0.45,
                                    DamageType = DamageType.Physical,
                                    UpgradeNote = "고DPS 어쌔신" } }
            },
        };
        var slow = new TowerDef
        {
            Kind = TowerKind.Slow,
            Name = "슬로우 타워",
            Icon = "❄",
            Description = "냉기 마법으로 적들을 둔화시키고 약한 피해를 입힙니다.",
            GuideColorHex = "#87CEEB",
            TowerColorHex = "#00BFFF",
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
            { TowerKind.Archer,   archer   },
            { TowerKind.Mage,     mage     },
            { TowerKind.Bombard,  bombard  },
            { TowerKind.Barracks, barracks },
            { TowerKind.Slow, slow },
        };
    }
}
