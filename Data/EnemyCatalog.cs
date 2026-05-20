using System.Collections.Generic;
using KingdomRushClone.Models;

namespace KingdomRushClone.Data;

public static class EnemyCatalog
{
    public static readonly Dictionary<EnemyKind, EnemyDef> Enemies = new()
    {
        { EnemyKind.GoblinSoldier, new EnemyDef
        {
            Kind = EnemyKind.GoblinSoldier, Name = "고블린 병사",
            MaxHp = 40, Speed = 55, Armor = ArmorType.Light, PhysicalResist = 0.0, MagicResist = 0.0,
            GoldReward = 6, ColorHex = "#7CB342", Radius = 11
        }},
        { EnemyKind.GoblinScout, new EnemyDef
        {
            Kind = EnemyKind.GoblinScout, Name = "고블린 스카우트",
            MaxHp = 25, Speed = 95, Armor = ArmorType.None, PhysicalResist = 0.0, MagicResist = 0.0,
            GoldReward = 5, ColorHex = "#AED581", Radius = 9, SlowImmune = true
        }},
        { EnemyKind.SplitBody, new EnemyDef
        {
            Kind = EnemyKind.SplitBody, Name = "분열체",
            MaxHp = 110, Speed = 48, Armor = ArmorType.Light, PhysicalResist = 0.1, MagicResist = 0.0,
            GoldReward = 10, ColorHex = "#84CC16", Radius = 14,
            DeathSpawns = { EnemyKind.SplitSmall, EnemyKind.SplitSmall, EnemyKind.SplitSmall }
        }},
        { EnemyKind.SplitSmall, new EnemyDef
        {
            Kind = EnemyKind.SplitSmall, Name = "작은 분열체",
            MaxHp = 35, Speed = 72, Armor = ArmorType.None, PhysicalResist = 0.0, MagicResist = 0.0,
            GoldReward = 3, ColorHex = "#BEF264", Radius = 8
        }},
        { EnemyKind.OrcWarrior, new EnemyDef
        {
            Kind = EnemyKind.OrcWarrior, Name = "오크 전사",
            MaxHp = 160, Speed = 38, Armor = ArmorType.Heavy, PhysicalResist = 0.35, MagicResist = 0.05,
            GoldReward = 14, ColorHex = "#558B2F", Radius = 14
        }},
        { EnemyKind.Wyvern, new EnemyDef
        {
            Kind = EnemyKind.Wyvern, Name = "와이번",
            MaxHp = 80, Speed = 80, Armor = ArmorType.Flying, PhysicalResist = 0.1, MagicResist = 0.0,
            GoldReward = 16, ColorHex = "#90A4AE", Radius = 12, IsFlying = true
        }},
        { EnemyKind.TrollShaman, new EnemyDef
        {
            Kind = EnemyKind.TrollShaman, Name = "트롤 주술사",
            MaxHp = 110, Speed = 50, Armor = ArmorType.Magical, PhysicalResist = 0.0, MagicResist = 0.5,
            GoldReward = 22, ColorHex = "#6A1B9A", Radius = 12,
            IsHealer = true, HealAmount = 12, HealInterval = 2.0, HealRadius = 60
        }},
        { EnemyKind.DarkKnight, new EnemyDef
        {
            Kind = EnemyKind.DarkKnight, Name = "암흑 기사",
            MaxHp = 260, Speed = 45, Armor = ArmorType.Heavy, PhysicalResist = 0.5, MagicResist = 0.15,
            GoldReward = 28, ColorHex = "#263238", Radius = 14, LivesCost = 2
        }},
        { EnemyKind.SplitMidBoss, new EnemyDef
        {
            Kind = EnemyKind.SplitMidBoss, Name = "분열 중간보스",
            MaxHp = 1500, Speed = 30, Armor = ArmorType.Heavy, PhysicalResist = 0.35, MagicResist = 0.15,
            GoldReward = 180, ColorHex = "#F97316", Radius = 22, LivesCost = 5, IsMidBoss = true,
            DeathSpawns = { EnemyKind.SplitBody, EnemyKind.SplitBody }
        }},
        { EnemyKind.SplitBoss, new EnemyDef
        {
            Kind = EnemyKind.SplitBoss, Name = "분열 보스",
            MaxHp = 4000, Speed = 28, Armor = ArmorType.Heavy, PhysicalResist = 0.45, MagicResist = 0.25,
            GoldReward = 500, ColorHex = "#DC2626", Radius = 28, LivesCost = 10, IsBoss = true,
            DeathSpawns = { EnemyKind.SplitMidBoss, EnemyKind.SplitMidBoss }
        }},
        { EnemyKind.MidBoss, new EnemyDef
        {
            Kind = EnemyKind.MidBoss, Name = "중간 보스",
            MaxHp = 1500, Speed = 30, Armor = ArmorType.Heavy, PhysicalResist = 0.4, MagicResist = 0.2,
            GoldReward = 180, ColorHex = "#D84315", Radius = 22, LivesCost = 5, IsMidBoss = true
        }},
        { EnemyKind.Boss, new EnemyDef
        {
            Kind = EnemyKind.Boss, Name = "보스",
            MaxHp = 4000, Speed = 28, Armor = ArmorType.Heavy, PhysicalResist = 0.45, MagicResist = 0.25,
            GoldReward = 500, ColorHex = "#4A148C", Radius = 28, LivesCost = 10, IsBoss = true
        }},
    };
}
