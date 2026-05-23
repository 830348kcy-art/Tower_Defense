using System.Collections.Generic;
using System.Linq;
using KingdomRushClone.Models;

namespace KingdomRushClone.Data;

public sealed record StageIntroEnemyInfo(
    EnemyKind Kind,
    string Name,
    string CodeName,
    string HpText,
    string SpeedText,
    string AbilityText,
    bool IsNewAppearance);

public static class StageIntroEnemyInfoBuilder
{
    public static IReadOnlyList<StageIntroEnemyInfo> Build(StageDef stage)
    {
        var previousKinds = StageCatalog.Stages
            .Where(previous => previous.Number < stage.Number)
            .SelectMany(previous => previous.Waves)
            .SelectMany(wave => wave.Entries)
            .Select(entry => entry.Enemy)
            .ToHashSet();

        var result = new List<StageIntroEnemyInfo>();
        var seen = new HashSet<EnemyKind>();
        foreach (var entry in stage.Waves.SelectMany(wave => wave.Entries))
        {
            if (!seen.Add(entry.Enemy)) continue;

            var def = EnemyCatalog.Enemies[entry.Enemy];
            result.Add(new StageIntroEnemyInfo(
                entry.Enemy,
                def.Name,
                CodeNameFor(entry.Enemy),
                HpTextFor(def),
                SpeedTextFor(def),
                EnemyAbilityTextBuilder.Describe(def),
                !previousKinds.Contains(entry.Enemy)));
        }

        return result;
    }

    public static IReadOnlyList<StageIntroEnemyInfo> BuildNew(StageDef stage)
        => Build(stage).Where(entry => entry.IsNewAppearance).ToList();

    public static IReadOnlyList<StageIntroEnemyInfo> BuildReturning(StageDef stage)
        => Build(stage).Where(entry => !entry.IsNewAppearance).ToList();

    public static string StageSubtitle(StageDef stage)
        => $"Chapter {ChapterFor(stage)} / HP x{stage.EnemyHpScale:0.##}";

    private static int ChapterFor(StageDef stage)
        => ((stage.Number - 1) / 5) + 1;

    private static string HpTextFor(EnemyDef def)
    {
        double normalHp = EnemyCatalog.Enemies[EnemyKind.Normal].MaxHp;
        return $"HP {def.MaxHp / normalHp * 100:0}%";
    }

    private static string SpeedTextFor(EnemyDef def)
    {
        double normalSpeed = EnemyCatalog.Enemies[EnemyKind.Normal].Speed;
        return $"이동속도 x{def.Speed / normalSpeed:0.#}";
    }

    private static string CodeNameFor(EnemyKind kind) => kind switch
    {
        EnemyKind.Normal => "enemy_normal",
        EnemyKind.Fast => "enemy_fast",
        EnemyKind.SplitBody => "enemy_split_body",
        EnemyKind.SplitSmall => "enemy_split_small",
        EnemyKind.Elite => "enemy_elite",
        EnemyKind.EliteCharge => "enemy_elite_charge",
        EnemyKind.EliteRegenerator => "enemy_elite_regenerator",
        EnemyKind.EliteWyvern => "enemy_elite_wyvern",
        EnemyKind.MidBossNormal => "enemy_mid_boss_normal",
        EnemyKind.MidBossCharge => "enemy_mid_boss_charge",
        EnemyKind.MidBossSplit => "enemy_mid_boss_split",
        EnemyKind.MidBossSpeed => "enemy_mid_boss_speed",
        EnemyKind.BossNormal => "enemy_boss_normal",
        EnemyKind.BossCharge => "enemy_boss_charge",
        EnemyKind.BossSplit => "enemy_boss_split",
        EnemyKind.BossSpeed => "enemy_boss_speed",
        _ => $"enemy_{kind}"
    };
}
