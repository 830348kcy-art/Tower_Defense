using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using KingdomRushClone.Managers;
using KingdomRushClone.Models;

namespace KingdomRushClone.Views;

public partial class TechTreePage : Page
{
    public TechTreePage()
    {
        InitializeComponent();
        Build();
    }

    // ─── Arknights palette ──────────────────────────────────────────────
    private static readonly Color Cyan   = Color.FromRgb(0x00, 0xD4, 0xFF);
    private static readonly Color Amber  = Color.FromRgb(0xF5, 0x9E, 0x0B);
    private static readonly Color PanelBg = Color.FromRgb(0x1C, 0x23, 0x33);
    private static readonly Color PanelBg2 = Color.FromRgb(0x11, 0x18, 0x27);
    private static readonly Color TextMuted = Color.FromRgb(0x4B, 0x55, 0x63);
    private static readonly Color TextSec   = Color.FromRgb(0x9C, 0xA3, 0xAF);

    private void Build()
    {
        NodesPanel.Children.Clear();
        var sd = SaveManager.Current;
        StarsText.Text = $"{sd.AvailableStars}";

        var grouped = TechTreeCatalog.Nodes.GroupBy(n => n.Category);
        foreach (var g in grouped)
        {
            // 카테고리 헤더 (좌측 앰버 바 + 영문 라벨 느낌)
            var header = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 18, 0, 8)
            };
            header.Children.Add(new Rectangle
            {
                Width = 3, Height = 18, Fill = new SolidColorBrush(Amber),
                Margin = new Thickness(0, 0, 10, 0), VerticalAlignment = VerticalAlignment.Center
            });
            header.Children.Add(new TextBlock
            {
                Text = g.Key, FontSize = 17, FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Amber), VerticalAlignment = VerticalAlignment.Center
            });
            NodesPanel.Children.Add(header);
            foreach (var node in g) NodesPanel.Children.Add(NodeRow(node));
        }
    }

    private UIElement NodeRow(TechNode node)
    {
        int cur = SaveManager.GetTechLevel(node.Id);
        bool maxed = cur >= node.MaxLevel;

        var card = new Border
        {
            Background = new SolidColorBrush(PanelBg2),
            BorderBrush = new SolidColorBrush(cur > 0 ? Cyan : Color.FromRgb(0x16, 0x4E, 0x63)),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 3, 0, 3)
        };

        var outer = new Grid();
        // 좌측 진행도 액센트 (강화됨 = 시안, 미강화 = 흐림)
        outer.Children.Add(new Rectangle
        {
            Width = 4, HorizontalAlignment = HorizontalAlignment.Left,
            Fill = new SolidColorBrush(cur > 0 ? Cyan : Color.FromRgb(0x25, 0x30, 0x47))
        });

        var row = new Grid { Margin = new Thickness(16, 10, 12, 10) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(210) }); // name + desc
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) }); // pips
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // desc
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });  // cost
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });     // buttons

        // 이름 + 레벨 표기
        var nameSp = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        nameSp.Children.Add(new TextBlock
        {
            Text = node.Name, Foreground = new SolidColorBrush(Color.FromRgb(0xE5, 0xE7, 0xEB)),
            FontSize = 15, FontWeight = FontWeights.SemiBold
        });
        nameSp.Children.Add(new TextBlock
        {
            Text = $"LV {cur} / {node.MaxLevel}",
            Foreground = new SolidColorBrush(maxed ? Amber : TextMuted),
            FontSize = 10, FontWeight = FontWeights.Bold, FontFamily = new FontFamily("Consolas")
        });
        Grid.SetColumn(nameSp, 0); row.Children.Add(nameSp);

        // 사각 핍 (Arknights segment style)
        var pips = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        for (int i = 0; i < node.MaxLevel; i++)
        {
            pips.Children.Add(new Rectangle
            {
                Width = 18, Height = 8, Margin = new Thickness(0, 0, 3, 0),
                Fill = i < cur ? new SolidColorBrush(Cyan) : new SolidColorBrush(Color.FromRgb(0x25, 0x30, 0x47))
            });
        }
        Grid.SetColumn(pips, 1); row.Children.Add(pips);

        var desc = new TextBlock
        {
            Text = node.Description, Foreground = new SolidColorBrush(TextSec),
            FontSize = 12, VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap, Margin = new Thickness(4, 0, 8, 0)
        };
        Grid.SetColumn(desc, 2); row.Children.Add(desc);

        // 비용
        var costSp = new StackPanel { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
        if (!maxed)
        {
            costSp.Children.Add(new TextBlock
            {
                Text = "COST", FontSize = 8, FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(TextMuted), HorizontalAlignment = HorizontalAlignment.Center
            });
            costSp.Children.Add(new TextBlock
            {
                Text = $"★ {node.CostPerLevel}", Foreground = new SolidColorBrush(Amber),
                FontSize = 14, FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center
            });
        }
        else
        {
            costSp.Children.Add(new TextBlock
            {
                Text = "MAX", FontSize = 13, FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Amber), HorizontalAlignment = HorizontalAlignment.Center
            });
        }
        Grid.SetColumn(costSp, 3); row.Children.Add(costSp);

        // 버튼들
        var btns = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        var down = new Button
        {
            Content = "−", Width = 34, Height = 32, Margin = new Thickness(0, 0, 4, 0),
            Background = new SolidColorBrush(Color.FromRgb(0x2D, 0x0D, 0x05)),
            FontSize = 16, FontWeight = FontWeights.Bold,
            IsEnabled = cur > 0
        };
        down.Click += (s, e) => { if (SaveManager.TryDowngradeTech(node.Id)) Build(); };
        btns.Children.Add(down);

        var up = new Button
        {
            Content = "강화 +", Width = 70, Height = 32,
            Background = new SolidColorBrush(Color.FromRgb(0x0D, 0x2E, 0x0E)),
            FontSize = 13, FontWeight = FontWeights.Bold,
            IsEnabled = !maxed && SaveManager.Current.AvailableStars >= node.CostPerLevel
        };
        up.Click += (s, e) => { if (SaveManager.TryUpgradeTech(node.Id)) Build(); };
        btns.Children.Add(up);
        Grid.SetColumn(btns, 4); row.Children.Add(btns);

        outer.Children.Add(row);
        card.Child = outer;
        return card;
    }

    private void OnBack(object s, RoutedEventArgs e) => MainWindow.Instance!.NavigateTo(new MainMenuPage());
}
