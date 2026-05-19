using TowerDefense.Core;
using TowerDefense.UI;

namespace TowerDefense.Data;

public static class StageIntroBuilder
{
    public static StageIntroData Build(int stage)
    {
        GameSession.Instance.OnStageChanged(stage);

        var enemyIds = WavePlan.GetEnemyIds(stage);
        var newEnemies = new List<EnemyDisplayInfo>();
        var returningEnemies = new List<EnemyDisplayInfo>();

        foreach (var enemyId in enemyIds)
        {
            var spec = EnemyDatabase.Get(enemyId);
            var isNew = !GameSession.Instance.SeenEnemies.Contains(enemyId);
            var display = new EnemyDisplayInfo
            {
                EnemyId = spec.EnemyId,
                DisplayName = spec.DisplayName,
                HpPercent = spec.BaseHpPercent * 100f,
                Abilities = spec.Abilities,
                Category = spec.Category,
                IsNewThisStage = isNew,
            };

            if (isNew)
            {
                newEnemies.Add(display);
            }
            else
            {
                returningEnemies.Add(display);
            }
        }

        GameSession.Instance.SeenEnemies.UnionWith(enemyIds);

        return new StageIntroData
        {
            StageNumber = stage,
            ChapterNumber = WavePlan.GetChapter(stage),
            HpMultiplier = GameSession.Instance.WaveMultiplier,
            NewEnemies = newEnemies,
            ReturningEnemies = returningEnemies,
        };
    }
}
