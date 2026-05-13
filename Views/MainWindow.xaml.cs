using System.Windows;

namespace KingdomRushClone.Views;

public partial class MainWindow : Window
{
    public static MainWindow? Instance { get; private set; }

    public MainWindow()
    {
        InitializeComponent();
        Instance = this;
        RootFrame.Navigate(new MainMenuPage());
    }

    public void NavigateTo(System.Windows.Controls.Page page) => RootFrame.Navigate(page);
}
