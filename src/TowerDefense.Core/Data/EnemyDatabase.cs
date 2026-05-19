using TowerDefense.Enemies;

namespace TowerDefense.Data;

public static class EnemyDatabase
{
    private static readonly IReadOnlyDictionary<string, EnemySpec> Specs =
        new List<EnemySpec>
        {
            new("enemy_normal", "일반", EnemyCategory.Normal, 1.00f, 80f, []),
            new("enemy_fast", "빠른", EnemyCategory.Normal, 0.50f, 160f, ["이동속도 x2"]),
            new("enemy_split_body", "분열체 원본", EnemyCategory.Normal, 2.25f, 80f, ["처치 시 소체 3마리 생성"]),
            new("enemy_split_small", "분열 소체", EnemyCategory.Normal, 0.75f, 80f, ["추가 분열 없음"]),
            new("elite_shield", "엘리트", EnemyCategory.Elite, 5.00f, 80f, ["보호막 3회", "주변 이동속도 +20%"]),
            new("elite_charge", "엘리트 돌격", EnemyCategory.Elite, 2.00f, 80f, ["HP 50% 이하 돌진"]),
            new("elite_regen", "엘리트 재생자", EnemyCategory.Elite, 4.00f, 80f, ["3초마다 회복"]),
            new("elite_resist", "엘리트 저항자", EnemyCategory.Elite, 3.50f, 80f, ["광역/단일 면역 교대"]),
            new("elite_ghost", "엘리트 유령", EnemyCategory.Elite, 2.50f, 80f, ["2초 주기 0.5초 무적"]),
            new("miniboss_normal", "중간보스 일반", EnemyCategory.MiniBoss, 3.00f, 60f, ["탱커형"]),
            new("miniboss_charge", "중간보스 돌격", EnemyCategory.MiniBoss, 2.50f, 60f, ["HP 50% 이하 돌진"]),
            new("miniboss_split", "중간보스 분열", EnemyCategory.MiniBoss, 3.50f, 60f, ["처치 시 소체 2마리"]),
            new("miniboss_speed", "중간보스 스피드", EnemyCategory.MiniBoss, 2.00f, 60f, ["전체 이동속도 +15%"]),
            new("boss_normal", "보스 일반", EnemyCategory.Boss, 7.50f, 50f, ["탱커형"]),
            new("boss_charge", "보스 돌격", EnemyCategory.Boss, 6.00f, 50f, ["HP 50% 이하 돌진"]),
            new("boss_split", "보스 분열", EnemyCategory.Boss, 8.00f, 50f, ["처치 시 중간보스급 소체 2마리"]),
            new("boss_speed", "보스 스피드", EnemyCategory.Boss, 4.00f, 50f, ["전체 이동속도 +15%"]),
        }.ToDictionary(spec => spec.EnemyId);

    public static EnemySpec Get(string id)
    {
        if (!Specs.TryGetValue(id, out var spec))
        {
            throw new ArgumentException($"Unknown enemy id: {id}", nameof(id));
        }

        return spec;
    }
}
