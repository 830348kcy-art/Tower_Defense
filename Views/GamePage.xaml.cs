using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using KingdomRushClone.Data;
using KingdomRushClone.Game;
using KingdomRushClone.Managers;
using KingdomRushClone.Models;

namespace KingdomRushClone.Views;

public partial class GamePage : Page
{
    private readonly StageDef _stage;
    private readonly GameEngine _engine;
    private readonly DispatcherTimer _timer;
    private DateTime _lastTick;

    // 타일 시스템
    private const double TileSize = 40;
    private readonly HashSet<(int c, int r)> _buildableTiles = new();
    private readonly Dictionary<(int c, int r), Rectangle> _tileRects = new();
    private (int c, int r) _hoverTile = (-1, -1);

    private readonly Dictionary<TowerInstance, FrameworkElement> _towerShapes = new();
    private readonly Dictionary<EnemyInstance, (Ellipse body, Rectangle hpBg, Rectangle hpFg)> _enemyShapes = new();
    private readonly Dictionary<Projectile, Shape> _projShapes = new();
    private readonly Dictionary<HitEffect, Ellipse> _fxShapes = new();
    private readonly Dictionary<Soldier, (Rectangle body, Rectangle hp)> _soldierShapes = new();

    private TowerInstance? _selectedTower;
    private Ellipse? _rangeIndicator;
    private SkillMode _skillMode = SkillMode.None;
    private Ellipse? _skillIndicator;
    private bool _resultShown;
    private bool _tutorialActive;

    private enum SkillMode { None, Meteor, Reinforce }

    public GamePage(StageDef stage)
    {
        InitializeComponent();
        _stage = stage;
        _engine = new GameEngine(stage);

        StageText.Text = $"Stage {stage.Number} — {stage.Theme}";
        DrawMap();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += OnFrame;
        _lastTick = DateTime.UtcNow;
        _timer.Start();

        Loaded += (_, _) => Focus();
        Unloaded += (_, _) => _timer.Stop();

        if (stage.Number == 1 && !SaveManager.Current.TutorialDone) ShowTutorial();
        UpdateHud();
    }

    private void OnFrame(object? sender, EventArgs e)
    {
        var now = DateTime.UtcNow;
        double dt = (now - _lastTick).TotalSeconds;
        _lastTick = now;
        dt = Math.Min(dt, 0.05);
        _engine.Tick(dt);
        Render();
        UpdateHud();
        if (_engine.Result != GameResult.Playing && !_resultShown)
        {
            _resultShown = true;
            ShowResult();
        }
    }

    // ---------------- DRAW MAP ----------------

    private void DrawMap()
    {
        int cols = (int)(StageCatalog.MapWidth  / TileSize);
        int rows = (int)(StageCatalog.MapHeight / TileSize);

        var pathBrush    = _stage.Effects.Contains(EnvEffect.IcePath)
                           ? new SolidColorBrush(Color.FromRgb(180, 210, 240))
                           : new SolidColorBrush(Color.FromRgb(180, 145, 90));
        var groundBrush  = ThemeGroundBrush(_stage.Theme);
        var buildBrush   = ThemeBuildBrush(_stage.Theme);

        // 1. 타일 그리드 (경로 판별 → 색 분리)
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                double cx = c * TileSize + TileSize / 2;
                double cy = r * TileSize + TileSize / 2;
                bool isPath = IsNearPath(new Vec2(cx, cy), 22);

                var rect = new Rectangle
                {
                    Width  = TileSize - 1,
                    Height = TileSize - 1,
                    Fill   = isPath ? pathBrush : buildBrush,
                    Stroke = new SolidColorBrush(Color.FromArgb(60, 0, 0, 0)),
                    StrokeThickness = 0.5,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(rect, c * TileSize);
                Canvas.SetTop(rect,  r * TileSize);
                GameCanvas.Children.Add(rect);

                if (!isPath)
                {
                    _buildableTiles.Add((c, r));
                    _tileRects[(c, r)] = rect;
                }
            }
        }

        // 2. 환경 효과 오버레이
        foreach (var fx in _stage.Effects)
        {
            if (fx == EnvEffect.NightVision)
                GameCanvas.Children.Add(new Rectangle
                {
                    Width = StageCatalog.MapWidth, Height = StageCatalog.MapHeight,
                    Fill = Brushes.Black, Opacity = 0.30, IsHitTestVisible = false
                });
        }

        // 3. 경로선 (시각 강조)
        foreach (var path in _stage.Paths)
        {
            for (int i = 0; i < path.Count - 1; i++)
                GameCanvas.Children.Add(new Line
                {
                    X1 = path[i].X, Y1 = path[i].Y,
                    X2 = path[i+1].X, Y2 = path[i+1].Y,
                    Stroke = pathBrush, StrokeThickness = 34,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap   = PenLineCap.Round,
                    IsHitTestVisible = false, Opacity = 0.5
                });

            // 스폰 마커 (빨간 삼각)
            var spawnPoly = new Polygon
            {
                Fill = Brushes.Crimson, IsHitTestVisible = false,
                Points = new PointCollection
                {
                    new(path[0].X, path[0].Y - 14),
                    new(path[0].X - 12, path[0].Y + 8),
                    new(path[0].X + 12, path[0].Y + 8)
                }
            };
            GameCanvas.Children.Add(spawnPoly);
            GameCanvas.Children.Add(new TextBlock
            {
                Text = "S", Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 11,
                IsHitTestVisible = false
            }.Also(tb => { Canvas.SetLeft(tb, path[0].X - 5); Canvas.SetTop(tb, path[0].Y - 8); }));

            // 기지 마커 (파란 성)
            var basePt = path[^1];
            GameCanvas.Children.Add(new Rectangle
            {
                Width = 32, Height = 32, Fill = Brushes.RoyalBlue,
                Stroke = Brushes.White, StrokeThickness = 2,
                RadiusX = 4, RadiusY = 4, IsHitTestVisible = false
            }.Also(rct => { Canvas.SetLeft(rct, basePt.X - 16); Canvas.SetTop(rct, basePt.Y - 16); }));
            GameCanvas.Children.Add(new TextBlock
            {
                Text = "🏰", FontSize = 16, IsHitTestVisible = false
            }.Also(tb => { Canvas.SetLeft(tb, basePt.X - 12); Canvas.SetTop(tb, basePt.Y - 14); }));
        }

        // 마우스 이동 시 건설 가능 타일 하이라이트
        GameCanvas.MouseMove += OnCanvasMouseMove;
    }

    private bool IsNearPath(Vec2 point, double tolerance)
    {
        foreach (var path in _stage.Paths)
            for (int i = 0; i < path.Count - 1; i++)
                if (DistPointSeg(point, path[i], path[i + 1]) < tolerance) return true;
        return false;
    }

    private static double DistPointSeg(Vec2 p, Vec2 a, Vec2 b)
    {
        var ab = b - a; var ap = p - a;
        double t = Math.Max(0, Math.Min(1, (ap.X * ab.X + ap.Y * ab.Y) / Math.Max(1e-9, ab.X * ab.X + ab.Y * ab.Y)));
        return p.DistanceTo(new Vec2(a.X + ab.X * t, a.Y + ab.Y * t));
    }

    private (int c, int r) TileAt(Point p)
    {
        int c = (int)(p.X / TileSize);
        int r = (int)(p.Y / TileSize);
        return (c, r);
    }

    private Vec2 TileCenter(int c, int r) => new(c * TileSize + TileSize / 2, r * TileSize + TileSize / 2);

    private void OnCanvasMouseMove(object sender, MouseEventArgs e)
    {
        if (Overlay.Visibility == Visibility.Visible) return;
        var tile = TileAt(e.GetPosition(GameCanvas));
        if (tile == _hoverTile) return;

        // 이전 hover 복원
        if (_tileRects.TryGetValue(_hoverTile, out var prev))
            prev.Opacity = 1.0;

        _hoverTile = tile;
        if (_buildableTiles.Contains(tile) && _tileRects.TryGetValue(tile, out var cur))
        {
            bool occupied = IsTileOccupied(tile);
            cur.Opacity = occupied ? 0.7 : 0.55;
        }
    }

    private bool IsTileOccupied((int c, int r) tile)
    {
        var center = TileCenter(tile.c, tile.r);
        foreach (var t in _engine.Towers)
            if (t.Pos.DistanceTo(center) < TileSize / 2) return true;
        return false;
    }

    private static SolidColorBrush ThemeGroundBrush(StageTheme t) => t switch
    {
        StageTheme.Grassland => new SolidColorBrush(Color.FromRgb(60, 100, 50)),
        StageTheme.Forest    => new SolidColorBrush(Color.FromRgb(30, 70, 30)),
        StageTheme.Desert    => new SolidColorBrush(Color.FromRgb(180, 150, 80)),
        StageTheme.Volcano   => new SolidColorBrush(Color.FromRgb(80, 30, 20)),
        StageTheme.Snow      => new SolidColorBrush(Color.FromRgb(200, 220, 240)),
        _                    => new SolidColorBrush(Color.FromRgb(50, 20, 70)),
    };

    private static SolidColorBrush ThemeBuildBrush(StageTheme t) => t switch
    {
        StageTheme.Grassland => new SolidColorBrush(Color.FromRgb(72, 120, 60)),
        StageTheme.Forest    => new SolidColorBrush(Color.FromRgb(40, 90, 40)),
        StageTheme.Desert    => new SolidColorBrush(Color.FromRgb(200, 170, 90)),
        StageTheme.Volcano   => new SolidColorBrush(Color.FromRgb(100, 40, 25)),
        StageTheme.Snow      => new SolidColorBrush(Color.FromRgb(215, 230, 248)),
        _                    => new SolidColorBrush(Color.FromRgb(65, 30, 90)),
    };

    // ---------------- RENDER ----------------

    private void Render()
    {
        // Towers
        foreach (var t in _engine.Towers)
        {
            if (!_towerShapes.ContainsKey(t)) AddTowerVisual(t);
            else UpdateTowerVisual(t);
        }

        // Enemies
        var alive = new HashSet<EnemyInstance>(_engine.Enemies);
        var remove = new List<EnemyInstance>();
        foreach (var kv in _enemyShapes) if (!alive.Contains(kv.Key)) remove.Add(kv.Key);
        foreach (var r in remove)
        {
            GameCanvas.Children.Remove(_enemyShapes[r].body);
            GameCanvas.Children.Remove(_enemyShapes[r].hpBg);
            GameCanvas.Children.Remove(_enemyShapes[r].hpFg);
            _enemyShapes.Remove(r);
        }
        foreach (var e in _engine.Enemies)
        {
            if (!_enemyShapes.ContainsKey(e))
            {
                var body = new Ellipse
                {
                    Width = e.Def.Radius * 2, Height = e.Def.Radius * 2,
                    Fill = (Brush)new BrushConverter().ConvertFromString(e.Def.ColorHex)!,
                    Stroke = e.Def.IsBoss ? Brushes.Yellow : e.Def.IsFlying ? Brushes.White : Brushes.Black,
                    StrokeThickness = e.Def.IsBoss ? 3 : 1, IsHitTestVisible = false
                };
                var bg = new Rectangle { Width = e.Def.Radius * 2, Height = 4, Fill = Brushes.Black, IsHitTestVisible = false };
                var fg = new Rectangle { Width = e.Def.Radius * 2, Height = 4, Fill = Brushes.LimeGreen, IsHitTestVisible = false };
                _enemyShapes[e] = (body, bg, fg);
                GameCanvas.Children.Add(body);
                GameCanvas.Children.Add(bg);
                GameCanvas.Children.Add(fg);
            }
            var sh = _enemyShapes[e];
            Canvas.SetLeft(sh.body, e.Pos.X - e.Def.Radius);
            Canvas.SetTop(sh.body, e.Pos.Y - e.Def.Radius);
            Canvas.SetLeft(sh.hpBg, e.Pos.X - e.Def.Radius);
            Canvas.SetTop(sh.hpBg, e.Pos.Y - e.Def.Radius - 8);
            Canvas.SetLeft(sh.hpFg, e.Pos.X - e.Def.Radius);
            Canvas.SetTop(sh.hpFg, e.Pos.Y - e.Def.Radius - 8);
            sh.hpFg.Width = (e.Def.Radius * 2) * Math.Max(0, e.Hp / e.MaxHp);
            sh.body.Opacity = e.SlowTimer > 0 ? 0.6 : 1.0;
        }

        // Projectiles
        var pAlive = new HashSet<Projectile>(_engine.Projectiles);
        var pRem = new List<Projectile>();
        foreach (var kv in _projShapes) if (!pAlive.Contains(kv.Key)) pRem.Add(kv.Key);
        foreach (var r in pRem) { GameCanvas.Children.Remove(_projShapes[r]); _projShapes.Remove(r); }
        foreach (var p in _engine.Projectiles)
        {
            if (!_projShapes.ContainsKey(p))
            {
                var shape = new Ellipse
                {
                    Width = p.SplashRadius > 0 ? 8 : 6, Height = p.SplashRadius > 0 ? 8 : 6,
                    Fill = (Brush)new BrushConverter().ConvertFromString(p.ColorHex)!,
                    IsHitTestVisible = false
                };
                _projShapes[p] = shape;
                GameCanvas.Children.Add(shape);
            }
            Canvas.SetLeft(_projShapes[p], p.Pos.X - 4);
            Canvas.SetTop(_projShapes[p], p.Pos.Y - 4);
        }

        // Effects
        var fxAlive = new HashSet<HitEffect>(_engine.Effects);
        var fxRem = new List<HitEffect>();
        foreach (var kv in _fxShapes) if (!fxAlive.Contains(kv.Key)) fxRem.Add(kv.Key);
        foreach (var r in fxRem) { GameCanvas.Children.Remove(_fxShapes[r]); _fxShapes.Remove(r); }
        foreach (var fx in _engine.Effects)
        {
            if (!_fxShapes.ContainsKey(fx))
            {
                var shape = new Ellipse
                {
                    Width = fx.Radius * 2, Height = fx.Radius * 2,
                    Fill = (Brush)new BrushConverter().ConvertFromString(fx.ColorHex)!,
                    IsHitTestVisible = false, Opacity = 0.6
                };
                _fxShapes[fx] = shape;
                GameCanvas.Children.Add(shape);
            }
            var s = _fxShapes[fx];
            Canvas.SetLeft(s, fx.Pos.X - fx.Radius);
            Canvas.SetTop(s, fx.Pos.Y - fx.Radius);
            s.Opacity = 0.6 * Math.Max(0, fx.TimeLeft / fx.TotalTime);
        }

        // Soldiers (barracks + reinforcements)
        var allSoldiers = new List<Soldier>();
        foreach (var t in _engine.Towers) if (t.IsBarracks) allSoldiers.AddRange(t.Soldiers);
        allSoldiers.AddRange(_engine.Reinforcements);
        var sAlive = new HashSet<Soldier>(allSoldiers);
        var sRem = new List<Soldier>();
        foreach (var kv in _soldierShapes) if (!sAlive.Contains(kv.Key)) sRem.Add(kv.Key);
        foreach (var r in sRem)
        {
            GameCanvas.Children.Remove(_soldierShapes[r].body);
            GameCanvas.Children.Remove(_soldierShapes[r].hp);
            _soldierShapes.Remove(r);
        }
        foreach (var s in allSoldiers)
        {
            if (!_soldierShapes.ContainsKey(s))
            {
                var body = new Rectangle
                {
                    Width = 12, Height = 12, Fill = Brushes.SteelBlue,
                    Stroke = Brushes.White, StrokeThickness = 1, IsHitTestVisible = false
                };
                var hp = new Rectangle { Width = 12, Height = 3, Fill = Brushes.LimeGreen, IsHitTestVisible = false };
                _soldierShapes[s] = (body, hp);
                GameCanvas.Children.Add(body);
                GameCanvas.Children.Add(hp);
            }
            var sh = _soldierShapes[s];
            sh.body.Visibility = s.Alive ? Visibility.Visible : Visibility.Collapsed;
            sh.hp.Visibility = s.Alive ? Visibility.Visible : Visibility.Collapsed;
            Canvas.SetLeft(sh.body, s.Pos.X - 6);
            Canvas.SetTop(sh.body, s.Pos.Y - 6);
            Canvas.SetLeft(sh.hp, s.Pos.X - 6);
            Canvas.SetTop(sh.hp, s.Pos.Y - 12);
            sh.hp.Width = 12 * Math.Max(0, s.Hp / Math.Max(1, s.MaxHp));
        }
    }

    private void AddTowerVisual(TowerInstance t)
    {
        var g = new Grid { Width = 36, Height = 36 };
        var rect = new Rectangle
        {
            Width = 36, Height = 36,
            Fill = (Brush)new BrushConverter().ConvertFromString(t.CurrentColorHex)!,
            Stroke = Brushes.Black, StrokeThickness = 1.5,
            RadiusX = 6, RadiusY = 6
        };
        g.Children.Add(rect);
        var label = new TextBlock
        {
            Text = TowerLabel(t.Def.Kind),
            Foreground = Brushes.White, FontWeight = FontWeights.Bold, FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        g.Children.Add(label);
        g.Tag = t;
        g.IsHitTestVisible = false;
        Canvas.SetLeft(g, t.Pos.X - 18);
        Canvas.SetTop(g, t.Pos.Y - 18);
        GameCanvas.Children.Add(g);
        _towerShapes[t] = g;
    }

    private void UpdateTowerVisual(TowerInstance t)
    {
        if (_towerShapes[t] is Grid g && g.Children[0] is Rectangle r)
        {
            r.Fill = (Brush)new BrushConverter().ConvertFromString(t.CurrentColorHex)!;
            if (g.Children[1] is TextBlock tb) tb.Text = TowerLabel(t.Def.Kind) + (t.Level > 0 ? (t.Level + 1).ToString() : "");
        }
    }

    private static string TowerLabel(TowerKind k) => k switch
    {
        TowerKind.Archer => "A",
        TowerKind.Mage => "M",
        TowerKind.Bombard => "B",
        TowerKind.Barracks => "S",
        _ => "?"
    };

    // ---------------- HUD ----------------

    private void UpdateHud()
    {
        GoldText.Text = $"♦ {_engine.Gold}G";
        LivesText.Text = $"❤ {_engine.Lives}/{_engine.LivesMax}";
        WaveText.Text = $"웨이브 {_engine.Spawner.CurrentWave}/{_engine.Spawner.TotalWaves}";
        SpeedBtn.Content = $"속도: {(_engine.Speed == GameSpeed.Fast ? "2x" : "1x")}";
        PauseBtn.Content = _engine.Speed == GameSpeed.Paused ? "재개" : "일시정지";
        NextWaveTimer.Text = _engine.Spawner.AllWavesStarted ? "마지막 웨이브" : $"다음 웨이브: {_engine.Spawner.NextWaveCountdown:F1}s";
        MeteorBtn.IsEnabled = _engine.MeteorCooldown <= 0;
        MeteorBtn.Content = _engine.MeteorCooldown > 0 ? $"🔥 {_engine.MeteorCooldown:F0}s" : "🔥 화포 [1]";
        ReinforceBtn.IsEnabled = _engine.ReinforcementCooldown <= 0;
        ReinforceBtn.Content = _engine.ReinforcementCooldown > 0 ? $"🛡 {_engine.ReinforcementCooldown:F0}s" : "🛡 지원군 [2]";
        HintText.Text = _skillMode switch
        {
            SkillMode.Meteor => "맵을 클릭하여 화포 시전",
            SkillMode.Reinforce => "맵을 클릭하여 지원군 소환",
            _ => ""
        };
    }

    // ---------------- INTERACTION ----------------

    private void OnCanvasClick(object sender, MouseButtonEventArgs e)
    {
        var pt  = e.GetPosition(GameCanvas);
        var pos = new Vec2(pt.X, pt.Y);

        // ── 스킬 모드 ──
        if (_skillMode == SkillMode.Meteor)
        {
            if (_engine.CastMeteor(pos)) { _skillMode = SkillMode.None; ClearSkillIndicator(); }
            return;
        }
        if (_skillMode == SkillMode.Reinforce)
        {
            if (_engine.CastReinforcements(pos)) { _skillMode = SkillMode.None; ClearSkillIndicator(); }
            return;
        }

        // 오버레이 열려있으면 닫기
        if (Overlay.Visibility == Visibility.Visible)
        {
            ClearSelection();
            return;
        }

        // ── 타일 판별 ──
        var tile = TileAt(pt);
        var center = TileCenter(tile.c, tile.r);

        // 배치된 타워 클릭?
        foreach (var t in _engine.Towers)
        {
            if (t.Pos.DistanceTo(center) < TileSize / 2)
            {
                SelectTower(t);
                return;
            }
        }

        // 건설 가능 타일?
        if (_buildableTiles.Contains(tile))
        {
            ShowBuildMenu(center);
            return;
        }

        ClearSelection();
    }

    private void ShowBuildMenu(Vec2 pos)
    {
        ClearSelection();
        var panel = new StackPanel { Background = (Brush)new BrushConverter().ConvertFromString("#222831")!, Width = 240 };
        panel.Children.Add(new TextBlock { Text = "타워 건설", FontSize = 18, Foreground = Brushes.Gold, FontWeight = FontWeights.Bold, Margin = new Thickness(8) });
        foreach (var kind in _stage.AllowedTowers)
        {
            var def = TowerCatalog.Towers[kind];
            int cost = (int)(def.Levels[0].Cost * (1 - SaveManager.TechEffect(TechId.TowerCostReduction)));
            var btn = new Button
            {
                Width = 220, Height = 44, Margin = new Thickness(8, 4, 8, 4),
                Background = (Brush)new BrushConverter().ConvertFromString(def.ColorHex)!,
                Foreground = Brushes.White, BorderThickness = new Thickness(0),
                Content = $"{def.Name}  —  {cost}G",
                IsEnabled = _engine.Gold >= cost
            };
            btn.Click += (s, e) =>
            {
                if (_engine.TryBuild(pos, kind)) ClearSelection();
            };
            panel.Children.Add(btn);
        }
        var close = new Button { Content = "취소", Width = 80, Height = 30, Margin = new Thickness(8), Background = Brushes.Gray, Foreground = Brushes.White, BorderThickness = new Thickness(0) };
        close.Click += (s, e) => ClearSelection();
        panel.Children.Add(close);

        ShowFloatingPanel(pos, panel);
    }

    private void SelectTower(TowerInstance t)
    {
        ClearSelection();
        _selectedTower = t;
        _rangeIndicator = new Ellipse
        {
            Width = t.EffectiveRange * 2, Height = t.EffectiveRange * 2,
            Stroke = Brushes.Yellow, StrokeThickness = 1.5,
            Fill = new SolidColorBrush(Color.FromArgb(40, 255, 255, 0)),
            IsHitTestVisible = false
        };
        Canvas.SetLeft(_rangeIndicator, t.Pos.X - t.EffectiveRange);
        Canvas.SetTop(_rangeIndicator, t.Pos.Y - t.EffectiveRange);
        GameCanvas.Children.Add(_rangeIndicator);

        var panel = new StackPanel { Background = (Brush)new BrushConverter().ConvertFromString("#222831")!, Width = 260 };
        panel.Children.Add(new TextBlock { Text = t.DisplayName, FontSize = 16, Foreground = Brushes.Gold, FontWeight = FontWeights.Bold, Margin = new Thickness(8) });
        var lvl = t.CurrentLevel;
        if (!t.IsBarracks)
            panel.Children.Add(new TextBlock { Text = $"공격력 {t.EffectiveDamage:F0}  사거리 {t.EffectiveRange:F0}  공속 {1 / t.EffectiveAttackInterval:F2}/s", Foreground = Brushes.LightGray, Margin = new Thickness(8, 0, 8, 8) });
        else
            panel.Children.Add(new TextBlock { Text = $"병력 {lvl.SoldierCount}  HP {lvl.SoldierHp:F0}  대미지 {lvl.SoldierDamage:F0}", Foreground = Brushes.LightGray, Margin = new Thickness(8, 0, 8, 8) });

        if (t.CanUpgrade)
        {
            var up = new Button
            {
                Content = $"업그레이드 → Lv{t.Level + 2}  ({t.UpgradeCost}G)",
                Width = 240, Height = 38, Margin = new Thickness(8, 4, 8, 4),
                Background = Brushes.SeaGreen, Foreground = Brushes.White, BorderThickness = new Thickness(0),
                IsEnabled = _engine.Gold >= t.UpgradeCost
            };
            up.Click += (s, e) => { _engine.TryUpgrade(t); SelectTower(t); };
            panel.Children.Add(up);
        }
        else if (t.CanChooseBranch)
        {
            if (t.Def.BranchA != null)
            {
                var a = new Button
                {
                    Content = $"[A] {t.Def.BranchA.Name}  ({t.BranchACost}G)",
                    Width = 240, Height = 38, Margin = new Thickness(8, 4, 8, 4),
                    Background = (Brush)new BrushConverter().ConvertFromString(t.Def.BranchA.ColorHex)!,
                    Foreground = Brushes.White, BorderThickness = new Thickness(0),
                    IsEnabled = _engine.Gold >= t.BranchACost
                };
                a.Click += (s, e) => { _engine.TryBranch(t, TowerBranch.A); SelectTower(t); };
                panel.Children.Add(a);
            }
            if (t.Def.BranchB != null)
            {
                var b = new Button
                {
                    Content = $"[B] {t.Def.BranchB.Name}  ({t.BranchBCost}G)",
                    Width = 240, Height = 38, Margin = new Thickness(8, 4, 8, 4),
                    Background = (Brush)new BrushConverter().ConvertFromString(t.Def.BranchB.ColorHex)!,
                    Foreground = Brushes.White, BorderThickness = new Thickness(0),
                    IsEnabled = _engine.Gold >= t.BranchBCost
                };
                b.Click += (s, e) => { _engine.TryBranch(t, TowerBranch.B); SelectTower(t); };
                panel.Children.Add(b);
            }
        }

        var sell = new Button
        {
            Content = $"판매  (+{t.SellValue()}G)",
            Width = 240, Height = 32, Margin = new Thickness(8, 4, 8, 4),
            Background = Brushes.IndianRed, Foreground = Brushes.White, BorderThickness = new Thickness(0)
        };
        sell.Click += (s, e) => { _engine.Sell(t); RemoveTowerVisual(t); ClearSelection(); };
        panel.Children.Add(sell);

        var close = new Button { Content = "닫기", Width = 80, Height = 28, Margin = new Thickness(8), Background = Brushes.Gray, Foreground = Brushes.White, BorderThickness = new Thickness(0) };
        close.Click += (s, e) => ClearSelection();
        panel.Children.Add(close);

        ShowFloatingPanel(t.Pos, panel);
    }

    private void RemoveTowerVisual(TowerInstance t)
    {
        if (_towerShapes.TryGetValue(t, out var v)) { GameCanvas.Children.Remove(v); _towerShapes.Remove(t); }
    }

    private void ShowFloatingPanel(Vec2 anchor, FrameworkElement panel)
    {
        Overlay.Visibility = Visibility.Visible;
        OverlayContent.Content = panel;
    }

    private void ClearSelection()
    {
        _selectedTower = null;
        if (_rangeIndicator != null) { GameCanvas.Children.Remove(_rangeIndicator); _rangeIndicator = null; }
        Overlay.Visibility = Visibility.Collapsed;
        OverlayContent.Content = null;
    }

    private void ClearSkillIndicator()
    {
        if (_skillIndicator != null) { GameCanvas.Children.Remove(_skillIndicator); _skillIndicator = null; }
    }

    // ---------------- BUTTONS ----------------

    private void OnSpeed(object s, RoutedEventArgs e)
    {
        _engine.Speed = _engine.Speed == GameSpeed.Fast ? GameSpeed.Normal : GameSpeed.Fast;
    }
    private void OnPause(object s, RoutedEventArgs e)
    {
        _engine.Speed = _engine.Speed == GameSpeed.Paused ? GameSpeed.Normal : GameSpeed.Paused;
    }
    private void OnCallWave(object s, RoutedEventArgs e)
    {
        int bonus = _engine.CallNextWaveEarly();
        if (bonus > 0) HintText.Text = $"+{bonus}G 조기 호출 보너스";
    }
    private void OnMeteor(object s, RoutedEventArgs e) => _skillMode = _engine.MeteorCooldown <= 0 ? SkillMode.Meteor : SkillMode.None;
    private void OnReinforce(object s, RoutedEventArgs e) => _skillMode = _engine.ReinforcementCooldown <= 0 ? SkillMode.Reinforce : SkillMode.None;
    private void OnExit(object s, RoutedEventArgs e) { _timer.Stop(); MainWindow.Instance!.NavigateTo(new StageSelectPage()); }

    private void OnKey(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Space: OnPause(this, new RoutedEventArgs()); break;
            case Key.F: OnSpeed(this, new RoutedEventArgs()); break;
            case Key.N: OnCallWave(this, new RoutedEventArgs()); break;
            case Key.D1: OnMeteor(this, new RoutedEventArgs()); break;
            case Key.D2: OnReinforce(this, new RoutedEventArgs()); break;
            case Key.Escape: ClearSelection(); _skillMode = SkillMode.None; ClearSkillIndicator(); break;
        }
    }

    // ---------------- TUTORIAL ----------------

    private void ShowTutorial()
    {
        _tutorialActive = true;
        var msgs = new[]
        {
            "튜토리얼 — 1/5\n\n노란색 둥근 슬롯을 클릭하면 타워를 건설할 수 있습니다.\n좌하단 [다음 웨이브 N] 버튼으로 적이 출현합니다.",
            "튜토리얼 — 2/5\n\n타워는 아처(빠름)·마법사(슬로우)·폭격(광역)·병영(차단) 4종입니다.\n적의 종류에 따라 타워 조합을 바꿔보세요.",
            "튜토리얼 — 3/5\n\n적 처치 시 골드를 얻고, 적이 본진(파란 사각형)에 도달하면 라이프가 줄어듭니다.\n라이프 0이면 패배.",
            "튜토리얼 — 4/5\n\n[1] 화포 지원과 [2] 지원군 소환은 강력한 스킬입니다.\n쿨다운이 있으니 위기 상황에 사용하세요.",
            "튜토리얼 — 5/5\n\n다음 웨이브를 일찍 호출(N)하면 잔여시간 비례 골드 보너스를 받습니다.\n방어선이 안정되면 적극 활용하세요. 행운을 빕니다!"
        };
        int idx = 0;
        StackPanel panel = null!;
        Action show = null!;
        show = () =>
        {
            if (idx >= msgs.Length)
            {
                SaveManager.Current.TutorialDone = true;
                _engine.Gold += 200;
                SaveManager.Save();
                _tutorialActive = false;
                ClearSelection();
                return;
            }
            panel = new StackPanel { Background = Brushes.Black, Width = 480 };
            panel.Children.Add(new TextBlock { Text = msgs[idx], FontSize = 16, Foreground = Brushes.White, Margin = new Thickness(20), TextWrapping = TextWrapping.Wrap });
            var next = new Button { Content = idx == msgs.Length - 1 ? "시작" : "다음 ▶", Width = 120, Height = 36, Margin = new Thickness(20), Background = Brushes.SeaGreen, Foreground = Brushes.White, BorderThickness = new Thickness(0) };
            next.Click += (s, e) => { idx++; show(); };
            var skip = new Button { Content = "건너뛰기", Width = 120, Height = 28, Margin = new Thickness(20, 0, 20, 20), Background = Brushes.Gray, Foreground = Brushes.White, BorderThickness = new Thickness(0) };
            skip.Click += (s, e) => { idx = msgs.Length; show(); };
            panel.Children.Add(next);
            panel.Children.Add(skip);
            Overlay.Visibility = Visibility.Visible;
            OverlayContent.Content = panel;
        };
        show();
    }

    // ---------------- RESULT ----------------

    private void ShowResult()
    {
        _timer.Stop();
        int stars = _engine.ComputeStars();
        if (_engine.Result == GameResult.Won)
            SaveManager.RecordStageStars(_stage.Number, stars);

        var panel = new StackPanel { Background = Brushes.Black, Width = 480 };
        panel.Children.Add(new TextBlock
        {
            Text = _engine.Result == GameResult.Won ? "✦ 승리! ✦" : "✗ 패배 ✗",
            FontSize = 36, FontWeight = FontWeights.Bold,
            Foreground = _engine.Result == GameResult.Won ? Brushes.Gold : Brushes.IndianRed,
            HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(20)
        });
        if (_engine.Result == GameResult.Won)
        {
            var starsTb = new TextBlock { FontSize = 40, HorizontalAlignment = HorizontalAlignment.Center, Foreground = Brushes.Gold };
            for (int i = 0; i < 3; i++) starsTb.Text += i < stars ? "★" : "☆";
            panel.Children.Add(starsTb);
            panel.Children.Add(new TextBlock { Text = $"남은 라이프: {_engine.Lives}/{_engine.LivesMax}", Foreground = Brushes.White, FontSize = 16, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 8, 0, 0) });
            panel.Children.Add(new TextBlock { Text = $"획득 골드: {_engine.Gold}G", Foreground = Brushes.LightGoldenrodYellow, FontSize = 14, HorizontalAlignment = HorizontalAlignment.Center });
        }
        var sp = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 20, 0, 20) };
        var retry = new Button { Content = "재도전", Width = 110, Height = 38, Margin = new Thickness(6), Background = Brushes.SeaGreen, Foreground = Brushes.White, BorderThickness = new Thickness(0) };
        retry.Click += (s, e) => MainWindow.Instance!.NavigateTo(new GamePage(_stage));
        var stageSel = new Button { Content = "스테이지", Width = 110, Height = 38, Margin = new Thickness(6), Background = Brushes.SteelBlue, Foreground = Brushes.White, BorderThickness = new Thickness(0) };
        stageSel.Click += (s, e) => MainWindow.Instance!.NavigateTo(new StageSelectPage());
        var tech = new Button { Content = "테크트리", Width = 110, Height = 38, Margin = new Thickness(6), Background = Brushes.MediumPurple, Foreground = Brushes.White, BorderThickness = new Thickness(0) };
        tech.Click += (s, e) => MainWindow.Instance!.NavigateTo(new TechTreePage());
        sp.Children.Add(retry); sp.Children.Add(stageSel); sp.Children.Add(tech);
        panel.Children.Add(sp);
        Overlay.Visibility = Visibility.Visible;
        OverlayContent.Content = panel;
    }
}
