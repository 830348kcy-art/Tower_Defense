using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using KingdomRushClone.Data;
using KingdomRushClone.Managers;

namespace KingdomRushClone.Views;

public partial class StageSelectPage : Page
{
    public StageSelectPage()
    {
        InitializeComponent();
        var sd = SaveManager.Current;
        StarsText.Text = $"보유 별: ★ {sd.AvailableStars} / 누적 {sd.TotalStars}";
        Build();
    }

    private void Build()
    {
        StagesPanel.Children.Clear();
        var sd = SaveManager.Current;
        int maxUnlocked = 1;
        foreach (var kv in sd.StageStars) if (kv.Value > 0 && kv.Key + 1 > maxUnlocked) maxUnlocked = kv.Key + 1;

        foreach (var stage in StageCatalog.Stages)
        {
            bool unlocked = stage.Number <= maxUnlocked;
            int stars = sd.StageStars.TryGetValue(stage.Number, out var st) ? st : 0;
            var btn = new Button
            {
                Width = 170, Height = 110, Margin = new Thickness(8),
                Background = unlocked ? new SolidColorBrush(ThemeColor(stage.Theme)) : Brushes.DimGray,
                Foreground = Brushes.White, BorderThickness = new Thickness(0),
                IsEnabled = unlocked,
                Tag = stage.Number,
                Content = BuildContent(stage.Number, stage.Theme, stars, unlocked, stage.HasMidBoss, stage.HasBoss)
            };
            btn.Click += OnStage;
            StagesPanel.Children.Add(btn);
        }
    }

    private static Color ThemeColor(Models.StageTheme theme) => theme switch
    {
        Models.StageTheme.Grassland => Color.FromRgb(76, 175, 80),
        Models.StageTheme.Forest => Color.FromRgb(46, 125, 50),
        Models.StageTheme.Desert => Color.FromRgb(255, 152, 0),
        Models.StageTheme.Volcano => Color.FromRgb(191, 54, 12),
        Models.StageTheme.Snow => Color.FromRgb(100, 181, 246),
        _ => Color.FromRgb(74, 20, 140)
    };

    private static UIElement BuildContent(int n, Models.StageTheme theme, int stars, bool unlocked, bool mid, bool boss)
    {
        var sp = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        sp.Children.Add(new TextBlock { Text = $"Stage {n}", FontSize = 18, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center });
        sp.Children.Add(new TextBlock { Text = theme.ToString() + (boss ? " · BOSS" : mid ? " · 중간보스" : ""), FontSize = 11, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 4, 0, 6) });
        if (unlocked)
        {
            var starsTxt = new TextBlock { FontSize = 18, HorizontalAlignment = HorizontalAlignment.Center };
            for (int i = 0; i < 3; i++) starsTxt.Text += i < stars ? "★" : "☆";
            sp.Children.Add(starsTxt);
        }
        else
            sp.Children.Add(new TextBlock { Text = "🔒", FontSize = 18, HorizontalAlignment = HorizontalAlignment.Center });
        return sp;
    }

    private void OnStage(object s, RoutedEventArgs e)
    {
        int n = (int)((Button)s).Tag;
        var stage = StageCatalog.Stages.Find(x => x.Number == n);
        if (stage == null) return;
        MainWindow.Instance!.NavigateTo(new GamePage(stage));
    }

    private void OnBack(object s, RoutedEventArgs e) => MainWindow.Instance!.NavigateTo(new MainMenuPage());
}
