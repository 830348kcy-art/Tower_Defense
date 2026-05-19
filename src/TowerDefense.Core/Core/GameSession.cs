namespace TowerDefense.Core;

public sealed class GameSession
{
    public static GameSession Instance { get; private set; } = new();

    private int _previousChapter;

    private GameSession()
    {
    }

    public int CurrentStage { get; private set; } = 1;

    public int CurrentChapter => (CurrentStage - 1) / 5 + 1;

    public float WaveMultiplier => (float)Math.Pow(1.2, CurrentChapter - 1);

    public HashSet<string> SeenEnemies { get; } = new();

    public event Action<int>? OnChapterChanged;

    public void OnStageChanged(int newStage)
    {
        if (newStage is < 1 or > 20)
        {
            throw new ArgumentOutOfRangeException(nameof(newStage), "Stage must be between 1 and 20.");
        }

        CurrentStage = newStage;
        var chapter = CurrentChapter;
        if (chapter == _previousChapter)
        {
            return;
        }

        _previousChapter = chapter;
        OnChapterChanged?.Invoke(chapter);
    }

    public static void ResetForTests()
    {
        Instance = new GameSession();
    }
}
