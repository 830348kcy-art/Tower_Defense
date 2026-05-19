namespace TowerDefense.Enemies;

public interface IEnemySprite
{
    string CurrentState { get; }

    void SetState(string state);
}
