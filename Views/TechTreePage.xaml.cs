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

    private void Build()
    {
        NodesPanel.Children.Clear();
        var sd = SaveManager.Current;
        StarsText.Text = $"보유 ★ {sd.AvailableStars}   /   누적 ★ {sd.TotalStars}";

        var grouped = TechTreeCatalog.Nodes.GroupBy(n => n.Category);
        foreach (var g in grouped)
        {
            NodesPanel.Children.Add(new TextBlock
            {
                Text = g.Key, FontSize = 20, FontWeight = FontWeights.Bold,
                Foreground = Brushes.LightGoldenrodYellow, Margin = new Thickness(0, 16, 0, 8)
            });
            foreach (var node in g) NodesPanel.Children.Add(NodeRow(node));
        }
    }

    private UIElement NodeRow(TechNode node)
    {
        int cur = SaveManager.GetTechLevel(node.Id);
        var row = new Grid { Margin = new Thickness(0, 4, 0, 4) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(280) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var name = new TextBlock { Text = node.Name, Foreground = Brushes.White, FontSize = 15, VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(name, 0); row.Children.Add(name);

        var pips = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        for (int i = 0; i < node.MaxLevel; i++)
        {
            pips.Children.Add(new Ellipse
            {
                Width = 16, Height = 16, Margin = new Thickness(2),
                Fill = i < cur ? Brushes.Gold : Brushes.DimGray
            });
        }
        Grid.SetColumn(pips, 1); row.Children.Add(pips);

        var desc = new TextBlock { Text = node.Description, Foreground = Brushes.LightGray, FontSize = 13, VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(desc, 2); row.Children.Add(desc);

        var costTxt = new TextBlock { Text = $"★ {node.CostPerLevel}", Foreground = Brushes.LightGoldenrodYellow, FontSize = 14, VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(costTxt, 3); row.Children.Add(costTxt);

        var up = new Button
        {
            Content = "강화 +", Width = 70, Height = 28, Margin = new Thickness(4),
            Background = Brushes.SeaGreen, Foreground = Brushes.White, BorderThickness = new Thickness(0),
            IsEnabled = cur < node.MaxLevel && SaveManager.Current.AvailableStars >= node.CostPerLevel
        };
        up.Click += (s, e) => { if (SaveManager.TryUpgradeTech(node.Id)) Build(); };
        Grid.SetColumn(up, 4); row.Children.Add(up);

        var down = new Button
        {
            Content = "−", Width = 30, Height = 28, Margin = new Thickness(4),
            Background = Brushes.IndianRed, Foreground = Brushes.White, BorderThickness = new Thickness(0),
            IsEnabled = cur > 0
        };
        down.Click += (s, e) => { if (SaveManager.TryDowngradeTech(node.Id)) Build(); };
        Grid.SetColumn(down, 5); row.Children.Add(down);

        return row;
    }

    private void OnBack(object s, RoutedEventArgs e) => MainWindow.Instance!.NavigateTo(new MainMenuPage());
}
