namespace TowerDefense.Enemies;

public static class EnemyFactory
{
    private static readonly IReadOnlyDictionary<string, Func<EnemyBase>> Factories =
        new Dictionary<string, Func<EnemyBase>>
        {
            ["enemy_normal"] = () => new NormalEnemy(),
            ["enemy_fast"] = () => new FastEnemy(),
            ["enemy_split_body"] = () => new SplitBodyEnemy(),
            ["enemy_split_small"] = () => new SplitSmallEnemy(),
            ["elite_shield"] = () => new EliteShieldEnemy(),
            ["elite_charge"] = () => new EliteChargeEnemy(),
            ["elite_regen"] = () => new EliteRegenEnemy(),
            ["elite_resist"] = () => new EliteResistEnemy(),
            ["elite_ghost"] = () => new EliteGhostEnemy(),
            ["miniboss_normal"] = () => new MiniBossNormal(),
            ["miniboss_charge"] = () => new MiniBossCharge(),
            ["miniboss_split"] = () => new MiniBossSplit(),
            ["miniboss_speed"] = () => new MiniBossSpeed(),
            ["boss_normal"] = () => new BossNormal(),
            ["boss_charge"] = () => new BossCharge(),
            ["boss_split"] = () => new BossSplit(),
            ["boss_speed"] = () => new BossSpeed(),
        };

    public static EnemyBase Create(string id)
    {
        if (!Factories.TryGetValue(id, out var create))
        {
            throw new ArgumentException($"Unknown enemy id: {id}", nameof(id));
        }

        return create();
    }
}
