using System.Windows;
using TowerDefense.UI;

namespace TowerDefense;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        _viewModel.RequestStageIntro += ShowStageIntro;
        DataContext = _viewModel;
    }

    private void ShowStageIntro(StageIntroData data)
    {
        var popup = new StageIntroPopup(data)
        {
            Owner = this
        };

        if (popup.ShowDialog() == true)
        {
            _viewModel.ConfirmStageIntro();
        }
    }
}
