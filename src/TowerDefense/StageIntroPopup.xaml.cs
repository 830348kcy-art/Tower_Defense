using System.Windows;
using System.Windows.Input;
using TowerDefense.UI;

namespace TowerDefense;

public partial class StageIntroPopup : Window
{
    private readonly StageIntroViewModel _viewModel;

    public StageIntroPopup(StageIntroData data)
    {
        InitializeComponent();
        _viewModel = new StageIntroViewModel(data);
        _viewModel.RequestClose += CloseAsAccepted;
        DataContext = _viewModel;
    }

    public bool SkipAlways => _viewModel.SkipAlways;

    private void CloseAsAccepted()
    {
        DialogResult = true;
        Close();
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }
}
