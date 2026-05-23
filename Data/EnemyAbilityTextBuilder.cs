using System;
using System.Collections.Generic;
using KingdomRushClone.Models;

namespace KingdomRushClone.Data;

public static class EnemyAbilityTextBuilder
{
    public static string Describe(EnemyDef def)
    {
        var parts = new List<string>();

        if (def.ShieldCharges > 0)
            parts.Add($"보호막 {def.ShieldCharges}회");

        if (def.AuraSpeedBonus > 0)
            parts.Add($"주변 이속 +{Percent(def.AuraSpeedBonus)}");

        if (def.GlobalSpeedBonus > 0)
        {
            parts.Add(def.GlobalSpeedBonusInterval > 0
                ? $"{Number(def.GlobalSpeedBonusInterval)}초마다 전체 이속 +{Percent(def.GlobalSpeedBonus)} ({Number(def.GlobalSpeedBonusDuration)}초 지속)"
                : $"전체 이속 +{Percent(def.GlobalSpeedBonus)}");
        }

        if (def.ChargeSpeedMultiplier > 1)
        {
            string speedText = def.ChargeSpeedPersists
                ? $"이속 x{Number(def.ChargeSpeedMultiplier)} 유지"
                : $"{Number(def.ChargeDuration)}초 동안 이속 x{Number(def.ChargeSpeedMultiplier)}";
            parts.Add($"체력 {Percent(def.ChargeHpThreshold)} 이하 {speedText}, 물리저항 +{Percent(def.ChargePhysicalResistBonus)}");
        }

        if (def.RegenerateInterval > 0)
            parts.Add($"{Number(def.RegenerateInterval)}초마다 자신 HP {Percent(def.RegenerateSelfPercent)} 회복, 주변 아군 HP {Percent(def.RegenerateAllyPercent)} 회복");

        if (def.DeathSpawns.Count > 0)
            parts.Add(DeathSpawnText(def));

        if (def.Kind == EnemyKind.EliteWyvern)
            parts.Add("폭발 면역, 병영 통과");

        return string.Join(" / ", parts);
    }

    private static string DeathSpawnText(EnemyDef def)
    {
        var counts = new Dictionary<EnemyKind, int>();
        foreach (var kind in def.DeathSpawns)
            counts[kind] = counts.TryGetValue(kind, out var count) ? count + 1 : 1;

        var parts = new List<string>();
        foreach (var (kind, count) in counts)
            parts.Add($"{ShortName(kind)} x{count}");

        return "사망 시 " + string.Join(", ", parts);
    }

    private static string ShortName(EnemyKind kind) => kind switch
    {
        EnemyKind.SplitSmall => "작은 분열체",
        EnemyKind.SplitBody => "분열체",
        EnemyKind.MidBossSplit => "분열 중간보스",
        _ => EnemyCatalog.Enemies.TryGetValue(kind, out var def) ? def.Name : kind.ToString()
    };

    private static string Percent(double value)
        => $"{Math.Round(value * 100):0}%";

    private static string Number(double value)
        => $"{value:0.#}";
}
