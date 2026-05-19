using System.Windows.Input;

namespace TowerDefense.UI;

public sealed class StageIntroViewModel : ObservableObject
{
    private StageIntroEnemyViewModel? _selectedEnemy;
    private bool _skipAlways;

    public StageIntroViewModel(StageIntroData data)
    {
        Data = data;
        NewEnemies = data.NewEnemies.Select(enemy => new StageIntroEnemyViewModel(enemy)).ToList();
        ReturningEnemies = data.ReturningEnemies.Select(enemy => new StageIntroEnemyViewModel(enemy)).ToList();
        _selectedEnemy = NewEnemies.FirstOrDefault() ?? ReturningEnemies.FirstOrDefault();
        SelectEnemyCommand = new RelayCommand<StageIntroEnemyViewModel>(SelectEnemy);
        StartCommand = new RelayCommand(Close);
        SkipAlwaysCommand = new RelayCommand(SkipAlwaysAndClose);
    }

    public StageIntroData Data { get; }

    public IReadOnlyList<StageIntroEnemyViewModel> NewEnemies { get; }

    public IReadOnlyList<StageIntroEnemyViewModel> ReturningEnemies { get; }

    public StageIntroEnemyViewModel? SelectedEnemy
    {
        get => _selectedEnemy;
        private set
        {
            if (ReferenceEquals(_selectedEnemy, value))
            {
                return;
            }

            _selectedEnemy = value;
            OnPropertyChanged();
        }
    }

    public bool SkipAlways
    {
        get => _skipAlways;
        private set
        {
            if (_skipAlways == value)
            {
                return;
            }

            _skipAlways = value;
            OnPropertyChanged();
        }
    }

    public string StageTitle => $"Stage {Data.StageNumber}";

    public string ChapterText => $"Chapter {Data.ChapterNumber} / HP x{Data.HpMultiplier:0.###}";

    public string AlertText
    {
        get
        {
            if (IsBossStage)
            {
                return "보스 스테이지";
            }

            if (IsMiniBossStage)
            {
                return "중간보스 스테이지";
            }

            return IsChapterStart ? "챕터 시작" : string.Empty;
        }
    }

    public string BannerText
    {
        get
        {
            if (IsBossStage)
            {
                return "W8에 보스가 등장합니다. 적 체력 배율과 특수 능력을 확인하세요.";
            }

            if (IsMiniBossStage)
            {
                return "W8에 중간보스가 등장합니다. 초반 웨이브에서 골드 흐름을 안정화하세요.";
            }

            if (IsChapterStart)
            {
                return "새 챕터 체력 배율이 적용됩니다. HP 배율은 챕터 동안 유지됩니다.";
            }

            return "이번 스테이지에 등장하는 적 정보를 확인하세요.";
        }
    }

    public bool HasNewEnemies => Data.NewEnemies.Count > 0;

    public bool HasReturningEnemies => Data.ReturningEnemies.Count > 0;

    public bool IsBossStage => Data.StageNumber % 5 == 0;

    public bool IsMiniBossStage => Data.StageNumber % 5 == 3;

    public bool IsChapterStart => Data.StageNumber == 1 || (Data.StageNumber - 1) % 5 == 0;

    public ICommand SelectEnemyCommand { get; }

    public ICommand StartCommand { get; }

    public ICommand SkipAlwaysCommand { get; }

    public event Action? RequestClose;

    private void SelectEnemy(StageIntroEnemyViewModel enemy)
    {
        SelectedEnemy = enemy;
    }

    private void Close()
    {
        RequestClose?.Invoke();
    }

    private void SkipAlwaysAndClose()
    {
        SkipAlways = true;
        Close();
    }
}
