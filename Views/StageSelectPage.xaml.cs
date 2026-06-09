using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using KingdomRushClone.Data;
using KingdomRushClone.Managers;

namespace KingdomRushClone.Views;

public partial class StageSelectPage : Page
{
    public StageSelectPage()
    {
        InitializeComponent();
        var sd = SaveManager.Current;
        StarsText.Text = $"{sd.AvailableStars} / {sd.TotalStars}";
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
                Width = 184, Height = 124, Margin = new Thickness(7),
                Background = new SolidColorBrush(Color.FromRgb(0x11, 0x18, 0x27)),
                BorderThickness = new Thickness(0),
                IsEnabled = unlocked,
                Tag = stage.Number,
                Content = BuildContent(stage.Number, stage.Theme, stars, unlocked,
                                       stage.HasMidBoss, stage.HasBoss, stage.Name)
            };
            btn.Click += OnStage;
            StagesPanel.Children.Add(btn);
        }
    }

    // ─── Arknights theme accent colors ──────────────────────────────────
    private static Color ThemeColor(Models.StageTheme theme) => theme switch
    {
        Models.StageTheme.Grassland => Color.FromRgb(0x4A, 0xDE, 0x80), // green
        Models.StageTheme.Forest    => Color.FromRgb(0x22, 0xC5, 0x5E), // deep green
        Models.StageTheme.Desert    => Color.FromRgb(0xF5, 0x9E, 0x0B), // amber
        Models.StageTheme.Volcano   => Color.FromRgb(0xEF, 0x44, 0x44), // red
        Models.StageTheme.Snow      => Color.FromRgb(0x38, 0xBD, 0xF8), // sky
        _                           => Color.FromRgb(0xA7, 0x8B, 0xFA)  // violet (Castle)
    };

    private static string ThemeCode(Models.StageTheme theme) => theme switch
    {
        Models.StageTheme.Grassland => "GRASSLAND",
        Models.StageTheme.Forest    => "FOREST",
        Models.StageTheme.Desert    => "DESERT",
        Models.StageTheme.Volcano   => "VOLCANO",
        Models.StageTheme.Snow      => "SNOW",
        _                           => "CASTLE"
    };

    private static UIElement BuildContent(int n, Models.StageTheme theme, int stars,
        bool unlocked, bool mid, bool boss, string name)
    {
        var accent = ThemeColor(theme);
        var accentBrush = new SolidColorBrush(accent);
        var dim = Color.FromArgb(unlocked ? (byte)255 : (byte)90, accent.R, accent.G, accent.B);

        var root = new Grid();

        // 좌측 테마 컬러 액센트 스트립
        root.Children.Add(new Rectangle
        {
            Width = 4, HorizontalAlignment = HorizontalAlignment.Left,
            Fill = new SolidColorBrush(dim)
        });

        // 우상단 코너 브래킷 (장식)
        var bracket = new Path
        {
            Data = Geometry.Parse("M 0,0 L 20,0 M 20,0 L 20,20"),
            Stroke = new SolidColorBrush(Color.FromArgb(unlocked ? (byte)150 : (byte)50, accent.R, accent.G, accent.B)),
            StrokeThickness = 1.5,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 8, 8, 0)
        };
        root.Children.Add(bracket);

        var sp = new StackPanel
        {
            Margin = new Thickness(16, 12, 10, 10),
            VerticalAlignment = VerticalAlignment.Center
        };

        // 작전 번호 (영문 라벨 + 숫자)
        var opRow = new StackPanel { Orientation = Orientation.Horizontal };
        opRow.Children.Add(new TextBlock
        {
            Text = "OP", FontSize = 10, FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Color.FromRgb(0x4B, 0x55, 0x63)),
            VerticalAlignment = VerticalAlignment.Bottom, Margin = new Thickness(0, 0, 5, 2)
        });
        opRow.Children.Add(new TextBlock
        {
            Text = $"{n:D2}", FontSize = 30, FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(unlocked ? accent : Color.FromRgb(0x4B, 0x55, 0x63)),
            FontFamily = new FontFamily("Consolas")
        });
        // 보스 태그
        if (boss || mid)
        {
            opRow.Children.Add(new Border
            {
                Background = new SolidColorBrush(boss
                    ? Color.FromRgb(0xEF, 0x44, 0x44) : Color.FromRgb(0xF5, 0x9E, 0x0B)),
                Margin = new Thickness(8, 0, 0, 4),
                Padding = new Thickness(5, 1, 5, 1),
                VerticalAlignment = VerticalAlignment.Bottom,
                Child = new TextBlock
                {
                    Text = boss ? "BOSS" : "ELITE",
                    FontSize = 8, FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black
                }
            });
        }
        sp.Children.Add(opRow);

        // 스테이지 이름
        sp.Children.Add(new TextBlock
        {
            Text = unlocked ? name : "─ ─ ─ ─",
            FontSize = 13, FontWeight = FontWeights.SemiBold,
            Foreground = new SolidColorBrush(unlocked
                ? Color.FromRgb(0xE5, 0xE7, 0xEB) : Color.FromRgb(0x4B, 0x55, 0x63)),
            Margin = new Thickness(0, 2, 0, 0),
            TextTrimming = TextTrimming.CharacterEllipsis
        });

        // 테마 코드
        sp.Children.Add(new TextBlock
        {
            Text = ThemeCode(theme),
            FontSize = 9, FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(dim),
            Margin = new Thickness(0, 1, 0, 6)
        });

        // 하단: 별 또는 잠금
        if (unlocked)
        {
            var starRow = new StackPanel { Orientation = Orientation.Horizontal };
            for (int i = 0; i < 3; i++)
                starRow.Children.Add(new TextBlock
                {
                    Text = i < stars ? "★" : "★",
                    FontSize = 15,
                    Foreground = new SolidColorBrush(i < stars
                        ? Color.FromRgb(0xF5, 0x9E, 0x0B) : Color.FromRgb(0x25, 0x30, 0x47)),
                    Margin = new Thickness(0, 0, 3, 0)
                });
            sp.Children.Add(starRow);
        }
        else
        {
            sp.Children.Add(new TextBlock
            {
                Text = "🔒 LOCKED",
                FontSize = 11, FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(0x4B, 0x55, 0x63))
            });
        }

        root.Children.Add(sp);
        return root;
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
