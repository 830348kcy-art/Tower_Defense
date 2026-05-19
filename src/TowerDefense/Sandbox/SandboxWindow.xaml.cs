using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using TowerDefense.UI;

namespace TowerDefense.Sandbox;

public partial class SandboxWindow : Window
{
    private const double TickSeconds = 1.0 / 30.0;

    private readonly SandboxGame _game = new();
    private readonly DispatcherTimer _timer = new();

    public SandboxWindow()
    {
        InitializeComponent();
        _timer.Interval = TimeSpan.FromSeconds(TickSeconds);
        _timer.Tick += (_, _) => Tick();
        Loaded += (_, _) => Draw();
    }

    private void StartWave_Click(object sender, RoutedEventArgs e)
    {
        _game.StartWave();
        _timer.Start();
        Draw();
    }

    private void StartSplitWave_Click(object sender, RoutedEventArgs e)
    {
        _game.StartSplitWave();
        _timer.Start();
        Draw();
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        _timer.Stop();
        _game.Reset();
        Draw();
    }

    private void Tick()
    {
        _game.Tick(TickSeconds);
        if (!_game.IsWaveRunning)
        {
            _timer.Stop();
        }

        Draw();
    }

    private void Draw()
    {
        GameCanvas.Children.Clear();
        DrawPath();
        DrawTowers();
        DrawEnemies();
        DrawBossHealth();
        HudText.Text = $"Gold {_game.Gold}   Lives {_game.Lives}   Wave {_game.Wave}   Enemies {_game.Enemies.Count}   Slowed {_game.SlowedEnemyCount}   Split {_game.SplitEnemyCount}";
        StatusText.Text = _game.StatusText;
    }

    private void DrawPath()
    {
        var path = new Polyline
        {
            Stroke = new SolidColorBrush(Color.FromRgb(92, 125, 150)),
            StrokeThickness = 20,
            StrokeStartLineCap = PenLineCap.Round,
            StrokeEndLineCap = PenLineCap.Round,
            StrokeLineJoin = PenLineJoin.Round
        };

        foreach (var point in _game.Path)
        {
            path.Points.Add(new Point(point.X, point.Y));
        }

        GameCanvas.Children.Add(path);
    }

    private void DrawTowers()
    {
        foreach (var tower in _game.Towers)
        {
            var range = new Ellipse
            {
                Width = tower.Range * 2,
                Height = tower.Range * 2,
                Stroke = new SolidColorBrush(Color.FromArgb(54, 226, 232, 240)),
                Fill = new SolidColorBrush(Color.FromArgb(16, 226, 232, 240)),
                StrokeThickness = 1
            };
            Canvas.SetLeft(range, tower.X - tower.Range);
            Canvas.SetTop(range, tower.Y - tower.Range);
            GameCanvas.Children.Add(range);

            var body = new Ellipse
            {
                Width = 34,
                Height = 34,
                Fill = tower.Kind == SandboxTowerKind.Slow
                    ? new SolidColorBrush(Color.FromRgb(96, 165, 250))
                    : new SolidColorBrush(Color.FromRgb(92, 173, 92)),
                Stroke = Brushes.White,
                StrokeThickness = 2
            };
            Canvas.SetLeft(body, tower.X - 17);
            Canvas.SetTop(body, tower.Y - 17);
            GameCanvas.Children.Add(body);
        }
    }

    private void DrawEnemies()
    {
        foreach (var enemy in _game.Enemies)
        {
            var slowed = enemy.Enemy.SlowFactor < 1.0f;
            var radius = GetEnemyRadius(enemy.Enemy.EnemyId);
            var imageSize = Math.Max(38, radius * 2 + 18);
            var imageRadius = imageSize / 2;

            if (slowed)
            {
                var slowRing = new Ellipse
                {
                    Width = imageSize + 10,
                    Height = imageSize + 10,
                    Stroke = new SolidColorBrush(Color.FromRgb(56, 189, 248)),
                    StrokeThickness = 3
                };
                Canvas.SetLeft(slowRing, enemy.X - imageRadius - 5);
                Canvas.SetTop(slowRing, enemy.Y - imageRadius - 5);
                GameCanvas.Children.Add(slowRing);
            }

            var body = new Image
            {
                Source = EnemyPreviewImageFactory.CreateSprite(enemy.Enemy.EnemyId),
                Width = imageSize,
                Height = imageSize,
                Stretch = Stretch.Uniform,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(body, enemy.X - imageRadius);
            Canvas.SetTop(body, enemy.Y - imageRadius);
            GameCanvas.Children.Add(body);

            if (slowed)
            {
                var slowLabel = new TextBlock
                {
                    Text = "SLOW",
                    Foreground = new SolidColorBrush(Color.FromRgb(186, 230, 253)),
                    FontSize = 10,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(slowLabel, enemy.X - 15);
                Canvas.SetTop(slowLabel, enemy.Y + imageRadius + 3);
                GameCanvas.Children.Add(slowLabel);
            }

            var hpRatio = Math.Clamp(enemy.Enemy.CurrentHp / enemy.Enemy.MaxHp, 0f, 1f);
            var hpWidth = Math.Max(34, imageSize - 6);
            var hpBack = new Rectangle
            {
                Width = hpWidth,
                Height = 4,
                Fill = new SolidColorBrush(Color.FromRgb(51, 65, 85))
            };
            Canvas.SetLeft(hpBack, enemy.X - hpWidth / 2);
            Canvas.SetTop(hpBack, enemy.Y - imageRadius - 8);
            GameCanvas.Children.Add(hpBack);

            var hp = new Rectangle
            {
                Width = hpWidth * hpRatio,
                Height = 4,
                Fill = new SolidColorBrush(Color.FromRgb(190, 242, 100))
            };
            Canvas.SetLeft(hp, enemy.X - hpWidth / 2);
            Canvas.SetTop(hp, enemy.Y - imageRadius - 8);
            GameCanvas.Children.Add(hp);

            var label = GetEnemyLabel(enemy.Enemy.EnemyId);
            if (label is null)
            {
                continue;
            }

            var labelBlock = new TextBlock
            {
                Text = label,
                Foreground = new SolidColorBrush(Color.FromRgb(248, 250, 252)),
                FontSize = 10,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(labelBlock, enemy.X - imageRadius - 8);
            Canvas.SetTop(labelBlock, enemy.Y - imageRadius - 24);
            GameCanvas.Children.Add(labelBlock);
        }
    }

    private void DrawBossHealth()
    {
        if (!_game.HasBoss)
        {
            return;
        }

        const double left = 14;
        const double top = 14;
        const double width = 236;
        const double height = 58;
        const double padding = 12;
        const double barHeight = 10;

        var panel = new Rectangle
        {
            Width = width,
            Height = height,
            Fill = new SolidColorBrush(Color.FromArgb(218, 15, 23, 42)),
            Stroke = new SolidColorBrush(Color.FromRgb(248, 113, 113)),
            StrokeThickness = 1.5,
            RadiusX = 8,
            RadiusY = 8
        };
        Canvas.SetLeft(panel, left);
        Canvas.SetTop(panel, top);
        GameCanvas.Children.Add(panel);

        var text = new TextBlock
        {
            Text = _game.BossHealthText,
            Foreground = new SolidColorBrush(Color.FromRgb(254, 226, 226)),
            FontSize = 14,
            FontWeight = FontWeights.Bold
        };
        Canvas.SetLeft(text, left + padding);
        Canvas.SetTop(text, top + 8);
        GameCanvas.Children.Add(text);

        var barBack = new Rectangle
        {
            Width = width - padding * 2,
            Height = barHeight,
            Fill = new SolidColorBrush(Color.FromRgb(51, 65, 85)),
            RadiusX = 5,
            RadiusY = 5
        };
        Canvas.SetLeft(barBack, left + padding);
        Canvas.SetTop(barBack, top + 36);
        GameCanvas.Children.Add(barBack);

        var bar = new Rectangle
        {
            Width = (width - padding * 2) * _game.BossHealthRatio,
            Height = barHeight,
            Fill = new SolidColorBrush(Color.FromRgb(239, 68, 68)),
            RadiusX = 5,
            RadiusY = 5
        };
        Canvas.SetLeft(bar, left + padding);
        Canvas.SetTop(bar, top + 36);
        GameCanvas.Children.Add(bar);
    }

    private static double GetEnemyRadius(string enemyId)
    {
        if (enemyId.Contains("boss_split"))
        {
            return enemyId.Contains("miniboss") ? 18 : 24;
        }

        if (enemyId.Contains("split_body"))
        {
            return 15;
        }

        if (enemyId.Contains("split_small"))
        {
            return 11;
        }

        return 13;
    }

    private static string? GetEnemyLabel(string enemyId)
    {
        return enemyId switch
        {
            "boss_split" => "BOSS SPLIT",
            "miniboss_split" => "MINI SPLIT",
            "enemy_split_body" => "SPLIT",
            "enemy_split_small" => "SMALL",
            _ => null
        };
    }
}
