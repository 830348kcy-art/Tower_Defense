namespace TowerDefense.Enemies;

public sealed class NullEnemySprite : IEnemySprite
{
    public string CurrentState { get; private set; } = "idle";

    public void SetState(string state)
    {
        CurrentState = state;
    }
}
