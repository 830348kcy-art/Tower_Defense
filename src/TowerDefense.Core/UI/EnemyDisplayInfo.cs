using TowerDefense.Enemies;

namespace TowerDefense.UI;

public sealed class EnemyDisplayInfo
{
    public required string EnemyId { get; init; }

    public required string DisplayName { get; init; }

    public float HpPercent { get; init; }

    public required string[] Abilities { get; init; }

    public EnemyCategory Category { get; init; }

    public bool IsNewThisStage { get; init; }

    public string PreviewSpriteKey { get; init; } = "idle";
}
