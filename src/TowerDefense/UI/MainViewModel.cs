using System.Windows.Input;
using TowerDefense.Core;
using TowerDefense.Data;

namespace TowerDefense.UI;

public sealed class MainViewModel : ObservableObject
{
    private string _statusText;

    public MainViewModel()
    {
        Intro = StageIntroBuilder.Build(1);
        _statusText = "스테이지 시작 전입니다. 버튼을 누르면 인트로 팝업을 확인합니다.";
        StartStageCommand = new RelayCommand(RequestIntro);
    }

    public StageIntroData Intro { get; }

    public string StageSummary =>
        $"Stage {Intro.StageNumber} / Chapter {Intro.ChapterNumber} / Waves {Intro.TotalWaves} / HP x{Intro.HpMultiplier:0.###}";

    public string StatusText
    {
        get => _statusText;
        private set
        {
            if (_statusText == value)
            {
                return;
            }

            _statusText = value;
            OnPropertyChanged();
        }
    }

    public ICommand StartStageCommand { get; }

    public event Action<StageIntroData>? RequestStageIntro;

    public void ConfirmStageIntro()
    {
        WaveManager.Instance.StartStage(Intro.StageNumber);
        StatusText = $"S{WaveManager.Instance.CurrentStage} W{WaveManager.Instance.CurrentWave} 시작 - 활성 적 {WaveManager.Instance.ActiveEnemies.Count}마리";
    }

    private void RequestIntro()
    {
        RequestStageIntro?.Invoke(Intro);
    }
}
