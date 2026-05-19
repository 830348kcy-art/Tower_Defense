namespace TowerDefense.UI;

public sealed class StageIntroData
{
    public int StageNumber { get; init; }

    public int ChapterNumber { get; init; }

    public float HpMultiplier { get; init; }

    public int TotalWaves { get; init; } = 8;

    public List<EnemyDisplayInfo> NewEnemies { get; init; } = [];

    public List<EnemyDisplayInfo> ReturningEnemies { get; init; } = [];
}
