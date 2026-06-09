using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using KingdomRushClone.Data;
using KingdomRushClone.Models;

namespace KingdomRushClone.Views;

public partial class AssetPreviewPage : Page
{
    private readonly string? _externalRoot = AssetPreviewCatalog.ExternalAssetRoot();
    private readonly Dictionary<(int Chapter, int Slot), string> _placedTowerPaths = new();
    private AssetPreviewItem _selectedTowerAsset = AssetPreviewCatalog.TowerAssets[0];
    private EnemyKind _selectedEnemyKind = EnemyKind.Normal;

    public AssetPreviewPage()
    {
        InitializeComponent();

        foreach (var chapter in AssetPreviewCatalog.MapChapters)
        {
            ChapterBox.Items.Add(new ComboBoxItem
            {
                Content = $"Chapter {chapter}",
                Tag = chapter
            });
        }

        ChapterBox.SelectedIndex = 0;
        Render();
    }

    private int SelectedChapter =>
        ChapterBox.SelectedItem is ComboBoxItem { Tag: int chapter } ? chapter : 1;

    private void OnBack(object sender, RoutedEventArgs e)
        => MainWindow.Instance!.NavigateTo(new MainMenuPage());

    private void OnChapterChanged(object sender, SelectionChangedEventArgs e)
        => Render();

    private void Render()
    {
        if (PreviewHost == null) return;

        int chapter = SelectedChapter;
        PreviewHost.Children.Clear();
        StatusText.Text = _externalRoot == null
            ? "External assets: not found"
            : $"External assets: {_externalRoot}";

        AddMapSection(chapter);
        AddAssetSection("Towers", AssetPreviewCatalog.TowerAssets);
        AddAssetSection("Barracks Soldiers", AssetPreviewCatalog.SoldierAssets);
        AddEnemySection(chapter);
    }

    private void AddMapSection(int chapter)
    {
        var section = MakeSection($"Chapter {chapter} Map");
        var mapPath = AssetPreviewCatalog.FirstExisting(AssetPreviewCatalog.MapCandidates(chapter));
        var previewMap = AssetPreviewCatalog.PreviewMapForChapter(chapter);

        var canvas = new Canvas
        {
            Width = StageCatalog.MapWidth,
            Height = StageCatalog.MapHeight,
            Background = new SolidColorBrush(Color.FromRgb(15, 23, 42)),
            ClipToBounds = true,
            Margin = new Thickness(0, 8, 0, 8)
        };

        if (mapPath != null && TryLoadBitmap(mapPath) is { } map)
        {
            canvas.Children.Add(new Image
            {
                Source = map,
                Width = StageCatalog.MapWidth,
                Height = StageCatalog.MapHeight,
                Stretch = Stretch.Fill,
                IsHitTestVisible = false
            });
        }
        else
        {
            canvas.Children.Add(new Rectangle
            {
                Width = StageCatalog.MapWidth,
                Height = StageCatalog.MapHeight,
                Fill = new SolidColorBrush(Color.FromRgb(31, 41, 55)),
                IsHitTestVisible = false
            });
            canvas.Children.Add(MakeCanvasText("Map image missing", 24, 24, 24, Brushes.White));
        }

        DrawPreviewPaths(canvas, previewMap.Paths);
        AddEnemyPreview(canvas, chapter, _selectedEnemyKind, previewMap.Paths[0]);
        DrawTowerSlots(canvas, chapter, previewMap);

        section.Children.Add(MakeMapControlPanel(chapter));

        section.Children.Add(new Border
        {
            Child = canvas,
            BorderBrush = new SolidColorBrush(Color.FromRgb(55, 65, 81)),
            BorderThickness = new Thickness(1),
            Background = new SolidColorBrush(Color.FromRgb(15, 23, 42))
        });

        section.Children.Add(MakePathLabel(mapPath, AssetPreviewCatalog.MapCandidates(chapter).FirstOrDefault()));
        PreviewHost.Children.Add(section);
    }

    private void AddAssetSection(string title, System.Collections.Generic.IEnumerable<AssetPreviewItem> items)
    {
        var section = MakeSection(title);
        var wrap = new WrapPanel { Margin = new Thickness(-6, 4, -6, 0) };

        foreach (var item in items)
        {
            var path = AssetPreviewCatalog.FirstExisting(AssetPreviewCatalog.AssetCandidates(item));
            wrap.Children.Add(MakeImageCard(item.Name, item.Group, item.PreviewSize, path));
        }

        section.Children.Add(wrap);
        PreviewHost.Children.Add(section);
    }

    private void AddEnemySection(int chapter)
    {
        var section = MakeSection($"Chapter {chapter} Enemies");
        var wrap = new WrapPanel { Margin = new Thickness(-6, 4, -6, 0) };

        foreach (var kind in AssetPreviewCatalog.EnemyKinds)
        {
            var def = EnemyCatalog.Enemies[kind];
            var path = AssetPreviewCatalog.FirstExisting(AssetPreviewCatalog.EnemyCandidates(chapter, kind));
            var size = Math.Clamp(GamePage.EnemySpriteSizeFor(def), 34, 112);
            wrap.Children.Add(MakeImageCard(kind.ToString(), def.Name, size, path));
        }

        section.Children.Add(wrap);
        PreviewHost.Children.Add(section);
    }

    private static StackPanel MakeSection(string title)
    {
        var section = new StackPanel { Margin = new Thickness(0, 0, 0, 22) };
        section.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = new SolidColorBrush(Color.FromRgb(249, 250, 251)),
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 8)
        });
        return section;
    }

    private static Border MakeImageCard(string title, string subtitle, double previewSize, string? path)
    {
        var imageHost = new Grid
        {
            Width = 132,
            Height = 112,
            HorizontalAlignment = HorizontalAlignment.Center,
            Background = new SolidColorBrush(Color.FromRgb(17, 24, 39))
        };

        if (path != null && TryLoadBitmap(path) is { } image)
        {
            imageHost.Children.Add(new Image
            {
                Source = image,
                Width = previewSize,
                Height = previewSize,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                ToolTip = path
            });
        }
        else
        {
            imageHost.Children.Add(new Border
            {
                Width = 92,
                Height = 70,
                Background = new SolidColorBrush(Color.FromRgb(127, 29, 29)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(248, 113, 113)),
                BorderThickness = new Thickness(1),
                Child = new TextBlock
                {
                    Text = "Missing",
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            });
        }

        var stack = new StackPanel();
        stack.Children.Add(imageHost);
        stack.Children.Add(new TextBlock
        {
            Text = title,
            Foreground = Brushes.White,
            FontSize = 13,
            FontWeight = FontWeights.Bold,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(0, 8, 0, 0)
        });
        stack.Children.Add(new TextBlock
        {
            Text = subtitle,
            Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)),
            FontSize = 11,
            TextTrimming = TextTrimming.CharacterEllipsis,
            Margin = new Thickness(0, 2, 0, 0)
        });
        stack.Children.Add(new TextBlock
        {
            Text = path == null ? "No file" : System.IO.Path.GetFileName(path),
            Foreground = new SolidColorBrush(path == null ? Color.FromRgb(252, 165, 165) : Color.FromRgb(134, 239, 172)),
            FontSize = 11,
            TextTrimming = TextTrimming.CharacterEllipsis,
            ToolTip = path ?? "",
            Margin = new Thickness(0, 6, 0, 0)
        });

        return new Border
        {
            Child = stack,
            Width = 164,
            MinHeight = 188,
            Margin = new Thickness(6),
            Padding = new Thickness(10),
            Background = new SolidColorBrush(Color.FromRgb(31, 41, 55)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(55, 65, 81)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6)
        };
    }

    private static TextBlock MakePathLabel(string? existingPath, string? firstCandidate)
    {
        return new TextBlock
        {
            Text = existingPath == null
                ? $"Missing. First candidate: {firstCandidate ?? "(none)"}"
                : $"Loaded: {existingPath}",
            Foreground = new SolidColorBrush(existingPath == null ? Color.FromRgb(252, 165, 165) : Color.FromRgb(187, 247, 208)),
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 4, 0, 0)
        };
    }

    private StackPanel MakeMapControlPanel(int chapter)
    {
        var panel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 0, 0, 8)
        };

        panel.Children.Add(new TextBlock
        {
            Text = "Tower",
            Foreground = Brushes.White,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0)
        });

        var towerBox = new ComboBox
        {
            Width = 190,
            Height = 30,
            Margin = new Thickness(0, 0, 14, 0)
        };
        foreach (var item in AssetPreviewCatalog.TowerAssets)
        {
            towerBox.Items.Add(new ComboBoxItem
            {
                Content = item.Name,
                Tag = item
            });
        }
        towerBox.SelectedIndex = SelectedTowerIndex();
        towerBox.SelectionChanged += (_, _) =>
        {
            if (towerBox.SelectedItem is ComboBoxItem { Tag: AssetPreviewItem item })
            {
                _selectedTowerAsset = item;
                Render();
            }
        };
        panel.Children.Add(towerBox);

        panel.Children.Add(new TextBlock
        {
            Text = "Enemy",
            Foreground = Brushes.White,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0)
        });

        var enemyBox = new ComboBox
        {
            Width = 230,
            Height = 30,
            Margin = new Thickness(0, 0, 14, 0)
        };
        foreach (var kind in AssetPreviewCatalog.EnemyKinds)
        {
            var def = EnemyCatalog.Enemies[kind];
            enemyBox.Items.Add(new ComboBoxItem
            {
                Content = $"{kind} / {def.Name}",
                Tag = kind
            });
        }
        enemyBox.SelectedIndex = SelectedEnemyIndex();
        enemyBox.SelectionChanged += (_, _) =>
        {
            if (enemyBox.SelectedItem is ComboBoxItem { Tag: EnemyKind kind })
            {
                _selectedEnemyKind = kind;
                Render();
            }
        };
        panel.Children.Add(enemyBox);

        var clearButton = new Button
        {
            Content = "Clear Slots",
            Width = 96,
            Height = 30,
            Margin = new Thickness(0, 0, 14, 0),
            Background = new SolidColorBrush(Color.FromRgb(55, 65, 81)),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0)
        };
        clearButton.Click += (_, _) =>
        {
            foreach (var key in _placedTowerPaths.Keys.Where(key => key.Chapter == chapter).ToArray())
                _placedTowerPaths.Remove(key);
            Render();
        };
        panel.Children.Add(clearButton);

        panel.Children.Add(new TextBlock
        {
            Text = "Click a slot on the map to place the selected tower.",
            Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225)),
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center
        });

        return panel;
    }

    private int SelectedTowerIndex()
    {
        for (int i = 0; i < AssetPreviewCatalog.TowerAssets.Count; i++)
            if (AssetPreviewCatalog.TowerAssets[i].Key == _selectedTowerAsset.Key)
                return i;
        return 0;
    }

    private int SelectedEnemyIndex()
    {
        for (int i = 0; i < AssetPreviewCatalog.EnemyKinds.Count; i++)
            if (AssetPreviewCatalog.EnemyKinds[i] == _selectedEnemyKind)
                return i;
        return 0;
    }

    private void DrawTowerSlots(Canvas canvas, int chapter, AssetPreviewMap map)
    {
        for (int i = 0; i < map.TowerSlots.Count; i++)
            DrawTowerSlot(canvas, chapter, i, map.TowerSlots[i], map.TowerSlotSize);
    }

    private void DrawTowerSlot(Canvas canvas, int chapter, int slotIndex, Vec2 slot, double slotSize)
    {
        var baseRing = CreateTowerSlotVisual(chapter, slotSize);
        double slotWidth = baseRing.Width;
        double slotHeight = baseRing.Height;
        Canvas.SetLeft(baseRing, slot.X - slotWidth / 2);
        Canvas.SetTop(baseRing, slot.Y - slotHeight / 2);
        canvas.Children.Add(baseRing);

        if (_placedTowerPaths.TryGetValue((chapter, slotIndex), out var placedPath)
            && TryLoadBitmap(placedPath) is { } towerImage)
        {
            double size = AssetPreviewCatalog.TowerVisualSizeForChapter(chapter);
            var image = new Image
            {
                Source = towerImage,
                Width = size,
                Height = size,
                Stretch = Stretch.Uniform,
                ToolTip = placedPath,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(image, slot.X - size / 2);
            Canvas.SetTop(image, slot.Y - size * 0.78);
            canvas.Children.Add(image);
        }
        else
        {
            canvas.Children.Add(MakeCanvasText("+", slot.X - 7, slot.Y - slotHeight * 0.38, 28, Brushes.White));
        }

        double hitWidth = slotWidth * 1.18;
        double hitHeight = slotHeight * 1.36;
        var hitArea = new Border
        {
            Width = hitWidth,
            Height = hitHeight,
            Background = Brushes.Transparent,
            ToolTip = $"Place {_selectedTowerAsset.Name}"
        };
        hitArea.MouseLeftButtonDown += (_, _) => PlaceTower(chapter, slotIndex);
        Canvas.SetLeft(hitArea, slot.X - hitWidth / 2);
        Canvas.SetTop(hitArea, slot.Y - hitHeight / 2);
        canvas.Children.Add(hitArea);
    }

    private static FrameworkElement CreateTowerSlotVisual(int chapter, double slotSize)
    {
        double slotWidth = slotSize * 1.32;
        double slotHeight = slotSize;
        var slotPath = AssetPreviewCatalog.FirstExisting(AssetPreviewCatalog.TowerSlotCandidates(chapter));
        if (slotPath != null && TryLoadBitmap(slotPath) is { } slotImage)
        {
            var image = new Image
            {
                Source = slotImage,
                Width = slotWidth,
                Height = slotHeight,
                Stretch = Stretch.Uniform,
                ToolTip = slotPath,
                IsHitTestVisible = false
            };
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);
            return image;
        }

        return new Ellipse
        {
            Width = slotSize,
            Height = slotSize * 0.68,
            Fill = new SolidColorBrush(Color.FromArgb(100, 15, 23, 42)),
            Stroke = new SolidColorBrush(Color.FromArgb(220, 250, 204, 21)),
            StrokeThickness = 3,
            IsHitTestVisible = false
        };
    }

    private void PlaceTower(int chapter, int slotIndex)
    {
        var path = AssetPreviewCatalog.FirstExisting(AssetPreviewCatalog.AssetCandidates(_selectedTowerAsset));
        if (path == null) return;

        _placedTowerPaths[(chapter, slotIndex)] = path;
        Render();
    }

    private static void AddEnemyPreview(Canvas canvas, int chapter, EnemyKind kind, IReadOnlyList<Vec2> path)
    {
        var point = PointOnPath(path, 0.45);
        var def = EnemyCatalog.Enemies[kind];
        double size = Math.Clamp(GamePage.EnemySpriteSizeFor(def), 42, 128);
        var imagePath = AssetPreviewCatalog.FirstExisting(AssetPreviewCatalog.EnemyCandidates(chapter, kind));

        FrameworkElement visual;
        if (imagePath != null && TryLoadBitmap(imagePath) is { } image)
        {
            visual = new Image
            {
                Source = image,
                Width = size,
                Height = size,
                Stretch = Stretch.Uniform,
                ToolTip = imagePath,
                IsHitTestVisible = false
            };
        }
        else
        {
            visual = EnemyFallbackImageFactory.CreateSpriteVisual(kind, size);
            visual.IsHitTestVisible = false;
        }

        Canvas.SetLeft(visual, point.X - size / 2);
        Canvas.SetTop(visual, point.Y - size * 0.78);
        canvas.Children.Add(visual);
        DrawMarker(canvas, point, "E", Color.FromRgb(234, 88, 12));
    }

    private static void DrawPreviewPaths(Canvas canvas, IReadOnlyList<IReadOnlyList<Vec2>> paths)
    {
        foreach (var path in paths)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                canvas.Children.Add(new Line
                {
                    X1 = path[i].X,
                    Y1 = path[i].Y,
                    X2 = path[i + 1].X,
                    Y2 = path[i + 1].Y,
                    Stroke = new SolidColorBrush(Color.FromArgb(220, 34, 197, 94)),
                    StrokeThickness = 8,
                    StrokeDashArray = new DoubleCollection { 6, 5 },
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round,
                    IsHitTestVisible = false
                });
            }

            DrawMarker(canvas, path[0], "S", Color.FromRgb(220, 38, 38));
            DrawMarker(canvas, path[^1], "B", Color.FromRgb(37, 99, 235));
        }
    }

    private static Vec2 PointOnPath(IReadOnlyList<Vec2> path, double progress)
    {
        if (path.Count == 0) return new Vec2(0, 0);
        if (path.Count == 1) return path[0];

        var lengths = new double[path.Count - 1];
        double total = 0;
        for (int i = 0; i < path.Count - 1; i++)
        {
            lengths[i] = Distance(path[i], path[i + 1]);
            total += lengths[i];
        }

        if (total <= 0) return path[0];

        double target = Math.Clamp(progress, 0, 1) * total;
        for (int i = 0; i < lengths.Length; i++)
        {
            if (target > lengths[i])
            {
                target -= lengths[i];
                continue;
            }

            double t = lengths[i] <= 0 ? 0 : target / lengths[i];
            return new Vec2(
                path[i].X + (path[i + 1].X - path[i].X) * t,
                path[i].Y + (path[i + 1].Y - path[i].Y) * t);
        }

        return path[^1];
    }

    private static void DrawMarker(Canvas canvas, Vec2 point, string text, Color color)
    {
        var marker = new Border
        {
            Width = 28,
            Height = 28,
            CornerRadius = new CornerRadius(14),
            Background = new SolidColorBrush(color),
            BorderBrush = Brushes.White,
            BorderThickness = new Thickness(2),
            Child = new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            },
            IsHitTestVisible = false
        };
        Canvas.SetLeft(marker, point.X - 14);
        Canvas.SetTop(marker, point.Y - 14);
        canvas.Children.Add(marker);
    }

    private static double Distance(Vec2 a, Vec2 b)
    {
        double dx = b.X - a.X;
        double dy = b.Y - a.Y;

        return Math.Sqrt(dx * dx + dy * dy);
    }

    private static TextBlock MakeCanvasText(string text, double x, double y, double fontSize, Brush brush)
    {
        var block = new TextBlock
        {
            Text = text,
            Foreground = brush,
            FontSize = fontSize,
            FontWeight = FontWeights.Bold,
            IsHitTestVisible = false
        };
        Canvas.SetLeft(block, x);
        Canvas.SetTop(block, y);
        return block;
    }

    private static BitmapImage? TryLoadBitmap(string path)
    {
        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.EndInit();
            image.Freeze();
            return image;
        }
        catch
        {
            return null;
        }
    }
}
