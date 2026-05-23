using System.Collections.Generic;
using KingdomRushClone.Models;

namespace KingdomRushClone.Data;

public static class EnemyCatalog
{
    public static readonly Dictionary<EnemyKind, EnemyDef> Enemies = new()
    {
        { EnemyKind.Normal, new EnemyDef
        {
            Kind = EnemyKind.Normal, Name = "일반",
            MaxHp = 60, Speed = 80, Armor = ArmorType.None, PhysicalResist = 0.15, MagicResist = 0.0,
            GoldReward = 16, ColorHex = "#22C55E", Radius = 11
        }},
        { EnemyKind.Fast, new EnemyDef
        {
            Kind = EnemyKind.Fast, Name = "빠른",
            MaxHp = 45, Speed = 160, Armor = ArmorType.None, PhysicalResist = 0.0, MagicResist = 0.0,
            GoldReward = 14, ColorHex = "#0EA5E9", Radius = 11
        }},
        { EnemyKind.SplitBody, new EnemyDef
        {
            Kind = EnemyKind.SplitBody, Name = "분열체",
            MaxHp = 150, Speed = 80, Armor = ArmorType.Light, PhysicalResist = 0.1, MagicResist = 0.0,
            GoldReward = 28, ColorHex = "#84CC16", Radius = 14,
            DeathSpawns = { EnemyKind.SplitSmall, EnemyKind.SplitSmall, EnemyKind.SplitSmall }
        }},
        { EnemyKind.SplitSmall, new EnemyDef
        {
            Kind = EnemyKind.SplitSmall, Name = "작은 분열체",
            MaxHp = 48, Speed = 80, Armor = ArmorType.None, PhysicalResist = 0.0, MagicResist = 0.0,
            GoldReward = 8, ColorHex = "#BEF264", Radius = 8
        }},
        { EnemyKind.Elite, new EnemyDef
        {
            Kind = EnemyKind.Elite, Name = "엘리트",
            MaxHp = 300, Speed = 80, Armor = ArmorType.Heavy, PhysicalResist = 0.20, MagicResist = 0.05,
            GoldReward = 60, ColorHex = "#64748B", Radius = 15, LivesCost = 2
        }},
        { EnemyKind.EliteCharge, new EnemyDef
        {
            Kind = EnemyKind.EliteCharge, Name = "돌진 엘리트",
            MaxHp = 210, Speed = 110, Armor = ArmorType.Light, PhysicalResist = 0.25, MagicResist = 0.0,
            GoldReward = 48, ColorHex = "#F97316", Radius = 15, LivesCost = 2,
            ChargeHpThreshold = 0.5, ChargeSpeedMultiplier = 1.5, ChargeDuration = 3.0,
            ChargePhysicalResistBonus = 0.10
        }},
        { EnemyKind.EliteRegenerator, new EnemyDef
        {
            Kind = EnemyKind.EliteRegenerator, Name = "재생 엘리트",
            MaxHp = 240, Speed = 80, Armor = ArmorType.Magical, PhysicalResist = 0.0, MagicResist = 0.25,
            GoldReward = 64, ColorHex = "#16A34A", Radius = 14, LivesCost = 2,
            RegenerateSelfPercent = 0.05, RegenerateAllyPercent = 0.02, RegenerateInterval = 3.0, RegenerateRadius = 100
        }},
        { EnemyKind.EliteWyvern, new EnemyDef
        {
            Kind = EnemyKind.EliteWyvern, Name = "와이번 엘리트",
            MaxHp = 240, Speed = 150, Armor = ArmorType.None, PhysicalResist = 0.20, MagicResist = 0.35,
            GoldReward = 56, ColorHex = "#A78BFA", Radius = 13, LivesCost = 2
        }},
        { EnemyKind.MidBossNormal, new EnemyDef
        {
            Kind = EnemyKind.MidBossNormal, Name = "중간보스",
            MaxHp = 420, Speed = 70, Armor = ArmorType.Heavy, PhysicalResist = 0.30, MagicResist = 0.05,
            GoldReward = 200, ColorHex = "#D84315", Radius = 21, LivesCost = 5, IsMidBoss = true
        }},
        { EnemyKind.MidBossCharge, new EnemyDef
        {
            Kind = EnemyKind.MidBossCharge, Name = "돌진 중간보스",
            MaxHp = 330, Speed = 100, Armor = ArmorType.Heavy, PhysicalResist = 0.30, MagicResist = 0.05,
            GoldReward = 220, ColorHex = "#EA580C", Radius = 21, LivesCost = 5, IsMidBoss = true,
            ChargeHpThreshold = 0.5, ChargeSpeedMultiplier = 2.0, ChargeDuration = 5.0,
            ChargePhysicalResistBonus = 0.13
        }},
        { EnemyKind.MidBossSplit, new EnemyDef
        {
            Kind = EnemyKind.MidBossSplit, Name = "분열 중간보스",
            MaxHp = 390, Speed = 70, Armor = ArmorType.Heavy, PhysicalResist = 0.15, MagicResist = 0.05,
            GoldReward = 240, ColorHex = "#F97316", Radius = 22, LivesCost = 5, IsMidBoss = true,
            DeathSpawns = { EnemyKind.SplitBody, EnemyKind.SplitBody }
        }},
        { EnemyKind.MidBossSpeed, new EnemyDef
        {
            Kind = EnemyKind.MidBossSpeed, Name = "속도 중간보스",
            MaxHp = 315, Speed = 140, Armor = ArmorType.Light, PhysicalResist = 0.05, MagicResist = 0.05,
            GoldReward = 220, ColorHex = "#0284C7", Radius = 20, LivesCost = 5, IsMidBoss = true,
            GlobalSpeedBonus = 0.15, GlobalSpeedBonusInterval = 5.0, GlobalSpeedBonusDuration = 3.0
        }},
        { EnemyKind.BossNormal, new EnemyDef
        {
            Kind = EnemyKind.BossNormal, Name = "보스",
            MaxHp = 900, Speed = 60, Armor = ArmorType.Heavy, PhysicalResist = 0.35, MagicResist = 0.10,
            GoldReward = 520, ColorHex = "#4A148C", Radius = 28, LivesCost = 10, IsBoss = true
        }},
        { EnemyKind.BossCharge, new EnemyDef
        {
            Kind = EnemyKind.BossCharge, Name = "돌진 보스",
            MaxHp = 660, Speed = 90, Armor = ArmorType.Heavy, PhysicalResist = 0.35, MagicResist = 0.10,
            GoldReward = 560, ColorHex = "#B45309", Radius = 28, LivesCost = 10, IsBoss = true,
            ChargeHpThreshold = 0.5, ChargeSpeedMultiplier = 2.5, ChargeDuration = 0.0,
            ChargePhysicalResistBonus = 0.15, ChargeSpeedPersists = true
        }},
        { EnemyKind.BossSplit, new EnemyDef
        {
            Kind = EnemyKind.BossSplit, Name = "분열 보스",
            MaxHp = 870, Speed = 60, Armor = ArmorType.Heavy, PhysicalResist = 0.20, MagicResist = 0.10,
            GoldReward = 600, ColorHex = "#DC2626", Radius = 29, LivesCost = 10, IsBoss = true,
            DeathSpawns = { EnemyKind.MidBossSplit, EnemyKind.MidBossSplit }
        }},
        { EnemyKind.BossSpeed, new EnemyDef
        {
            Kind = EnemyKind.BossSpeed, Name = "속도 보스",
            MaxHp = 600, Speed = 130, Armor = ArmorType.Heavy, PhysicalResist = 0.10, MagicResist = 0.10,
            GoldReward = 560, ColorHex = "#0369A1", Radius = 27, LivesCost = 10, IsBoss = true,
            GlobalSpeedBonus = 0.20
        }},
    };
}
