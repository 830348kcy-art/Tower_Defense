using System;
using System.Collections.Generic;
using System.Linq;
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
    // ─── Engine / timer ─────────────────────────────────────────────────
    private readonly StageDef    _stage;
    private readonly GameEngine  _engine;
    private readonly DispatcherTimer _timer;
    private DateTime _lastTick;
    private double   _lastRawDt;

    // ─── Tile system ────────────────────────────────────────────────────
    private const double TileSize = 40;
    private readonly HashSet<(int c, int r)>             _buildableTiles = new();
    private readonly Dictionary<(int c, int r), Rectangle> _tileRects   = new();
    private (int c, int r) _hoverTile = (-1, -1);

    // ─── Shape caches ────────────────────────────────────────────────────
    private readonly Dictionary<TowerInstance,  FrameworkElement>                              _towerShapes   = new();
    private readonly Dictionary<EnemyInstance,  (Image body, Rectangle hpBg, Rectangle hpFg)> _enemyShapes = new();
    private readonly Dictionary<Projectile,     Shape>                                          _projShapes   = new();
    private readonly Dictionary<HitEffect,      Ellipse>                                        _fxShapes     = new();
    private readonly Dictionary<Soldier,        (Rectangle body, Rectangle hp)>                 _soldierShapes= new();

    // ─── Floating damage numbers ─────────────────────────────────────────
    private readonly List<(TextBlock tb, double life)> _damageLabels = new();

    // ─── Selection / skill mode ──────────────────────────────────────────
    private TowerInstance? _selectedTower;
    private Ellipse?       _rangeIndicator;
    private SkillMode      _skillMode = SkillMode.None;
    private Ellipse?       _skillIndicator;
    private bool           _resultShown;

    // ─── HUD state ───────────────────────────────────────────────────────
    private int    _lastLives;
    private int    _lastDisplayedWave;
    private double _livesFlashTimer;
    private double _announceTimer;

    private enum SkillMode { None, Meteor, Reinforce }

    // ─── Constructor ─────────────────────────────────────────────────────
    public GamePage(StageDef stage)
    {
        InitializeComponent();
        _stage  = stage;
        _engine = new GameEngine(stage);

        StageText.Text = $"Stage {stage.Number} — {stage.Name}";
        DrawMap();

        _lastLives        = stage.StartingLives;
        _lastDisplayedWave = 0;

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += OnFrame;
        _lastTick = DateTime.UtcNow;
        _timer.Start();

        Loaded   += (_, _) => Focus();
        Unloaded += (_, _) => _timer.Stop();

        UpdateHud();

        if (stage.Number == 1 && !SaveManager.Current.TutorialDone)
            ShowTutorial();
        else
            ShowStageIntro();
    }

    // ─── Game loop ───────────────────────────────────────────────────────
    private void OnFrame(object? sender, EventArgs e)
    {
        var    now   = DateTime.UtcNow;
        double rawDt = Math.Min((now - _lastTick).TotalSeconds, 0.05);
        _lastTick   = now;
        _lastRawDt  = rawDt;

        // Wave announcement (runs on real-time, not game-time)
        int curWave = _engine.Spawner.CurrentWave;
        if (curWave > _lastDisplayedWave)
        {
            _lastDisplayedWave = curWave;
            TriggerWaveAnnounce(curWave);
        }
        if (_announceTimer > 0)
        {
            _announceTimer -= rawDt;
            AnnounceOverlay.Opacity = _announceTimer > 2.0 ? 1.0 : _announceTimer / 0.5;
            if (_announceTimer <= 0) AnnounceOverlay.Visibility = Visibility.Collapsed;
        }

        // Lives flash
        if (_engine.Lives < _lastLives)
        {
            _lastLives      = _engine.Lives;
            _livesFlashTimer = 0.55;
        }
        if (_livesFlashTimer > 0)
        {
            _livesFlashTimer -= rawDt;
            LivesFlashBorder.Visibility = Visibility.Visible;
            LivesFlashBorder.Opacity    = Math.Max(0, _livesFlashTimer / 0.55);
        }
        else
        {
            LivesFlashBorder.Visibility = Visibility.Collapsed;
        }

        // Game tick
        _engine.Tick(rawDt);

        // Drain damage events → floating labels
        foreach (var ev in _engine.DamageEvents)
            SpawnDamageLabel(ev.Pos, ev.Amount, ev.Type, ev.IsCrit);

        Render();
        UpdateHud();

        if (_engine.Result != GameResult.Playing && !_resultShown)
        {
            _resultShown = true;
            ShowResult();
        }
    }

    // ─── Wave announce ───────────────────────────────────────────────────
    private void TriggerWaveAnnounce(int wave)
    {
        bool isLast = wave == _engine.Spawner.TotalWaves;
        AnnounceText.Text = isLast
            ? $"⚠  최후의 웨이브!  ({wave} / {_engine.Spawner.TotalWaves})"
            : $"웨이브  {wave} / {_engine.Spawner.TotalWaves}";
        AnnounceOverlay.Visibility = Visibility.Visible;
        _announceTimer = 2.5;
    }

    // ─── Floating damage numbers ─────────────────────────────────────────
    private void SpawnDamageLabel(Vec2 pos, double amount, DamageType type, bool crit)
    {
        var color = type switch
        {
            DamageType.Magic     => Color.FromRgb(170, 130, 255),
            DamageType.Explosive => Color.FromRgb(255, 130,  30),
            DamageType.True      => Color.FromRgb(255, 220,   0),
            _                    => Color.FromRgb(255, 255, 255),
        };
        string text = crit ? $"⚡{amount:F0}!" : $"{amount:F0}";
        var tb = new TextBlock
        {
            Text       = text,
            Foreground = new SolidColorBrush(color),
            FontSize   = crit ? 18 : 12,
            FontWeight = crit ? FontWeights.Bold : FontWeights.Normal,
            IsHitTestVisible = false
        };
        Canvas.SetLeft(tb, pos.X - (crit ? 18 : 10));
        Canvas.SetTop(tb,  pos.Y - 22);
        Canvas.SetZIndex(tb, 200);
        GameCanvas.Children.Add(tb);
        _damageLabels.Add((tb, 0.80));
    }

    private void UpdateDamageLabels()
    {
        double dt = _lastRawDt;
        for (int i = _damageLabels.Count - 1; i >= 0; i--)
        {
            var (tb, life) = _damageLabels[i];
            double newLife = life - dt;
            if (newLife <= 0)
            {
                GameCanvas.Children.Remove(tb);
                _damageLabels.RemoveAt(i);
            }
            else
            {
                Canvas.SetTop(tb, Canvas.GetTop(tb) - 45 * dt);
                tb.Opacity = newLife / 0.80;
                _damageLabels[i] = (tb, newLife);
            }
        }
    }

    // ─── MAP DRAW ────────────────────────────────────────────────────────
    private void DrawMap()
    {
        int cols = (int)(StageCatalog.MapWidth  / TileSize);
        int rows = (int)(StageCatalog.MapHeight / TileSize);

        bool isIce = _stage.Effects.Contains(EnvEffect.IcePath);
        var pathBrush  = isIce
            ? new SolidColorBrush(Color.FromRgb(170, 210, 245))
            : new SolidColorBrush(Color.FromRgb(175, 140, 88));
        var buildBrush = ThemeBuildBrush(_stage.Theme);

        // Tile grid
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                double cx = c * TileSize + TileSize / 2;
                double cy = r * TileSize + TileSize / 2;
                bool   isPath = IsNearPath(new Vec2(cx, cy), 22);

                var rect = new Rectangle
                {
                    Width  = TileSize - 1,
                    Height = TileSize - 1,
                    Fill   = isPath ? pathBrush : buildBrush,
                    Stroke = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0)),
                    StrokeThickness  = 0.5,
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

        // Night-vision overlay
        if (_stage.Effects.Contains(EnvEffect.NightVision))
            GameCanvas.Children.Add(new Rectangle
            {
                Width  = StageCatalog.MapWidth,
                Height = StageCatalog.MapHeight,
                Fill   = Brushes.Black, Opacity = 0.28,
                IsHitTestVisible = false
            });

        // Path lines + markers
        foreach (var path in _stage.Paths)
        {
            for (int i = 0; i < path.Count - 1; i++)
                GameCanvas.Children.Add(new Line
                {
                    X1 = path[i].X,   Y1 = path[i].Y,
                    X2 = path[i+1].X, Y2 = path[i+1].Y,
                    Stroke = pathBrush, StrokeThickness = 34,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap   = PenLineCap.Round,
                    IsHitTestVisible   = false, Opacity = 0.50
                });

            // Spawn marker
            GameCanvas.Children.Add(new Polygon
            {
                Fill   = Brushes.Crimson, IsHitTestVisible = false,
                Points = new PointCollection
                {
                    new(path[0].X, path[0].Y - 14),
                    new(path[0].X - 12, path[0].Y + 8),
                    new(path[0].X + 12, path[0].Y + 8)
                }
            });
            GameCanvas.Children.Add(new TextBlock
            {
                Text = "S", Foreground = Brushes.White,
                FontWeight = FontWeights.Bold, FontSize = 11,
                IsHitTestVisible = false
            }.Also(tb => { Canvas.SetLeft(tb, path[0].X - 5); Canvas.SetTop(tb, path[0].Y - 8); }));

            // Base marker
            var bp = path[^1];
            GameCanvas.Children.Add(new Rectangle
            {
                Width = 34, Height = 34, Fill = Brushes.RoyalBlue,
                Stroke = Brushes.White, StrokeThickness = 2,
                RadiusX = 5, RadiusY = 5, IsHitTestVisible = false
            }.Also(rc => { Canvas.SetLeft(rc, bp.X - 17); Canvas.SetTop(rc, bp.Y - 17); }));
            GameCanvas.Children.Add(new TextBlock
            {
                Text = "🏰", FontSize = 17, IsHitTestVisible = false
            }.Also(tb => { Canvas.SetLeft(tb, bp.X - 13); Canvas.SetTop(tb, bp.Y - 14); }));
        }

        GameCanvas.MouseMove += OnCanvasMouseMove;
    }

    private bool IsNearPath(Vec2 pt, double tol)
    {
        foreach (var path in _stage.Paths)
            for (int i = 0; i < path.Count - 1; i++)
                if (DistPointSeg(pt, path[i], path[i + 1]) < tol) return true;
        return false;
    }

    private static double DistPointSeg(Vec2 p, Vec2 a, Vec2 b)
    {
        var    ab = b - a; var ap = p - a;
        double t  = Math.Max(0, Math.Min(1,
            (ap.X * ab.X + ap.Y * ab.Y) / Math.Max(1e-9, ab.X * ab.X + ab.Y * ab.Y)));
        return p.DistanceTo(new Vec2(a.X + ab.X * t, a.Y + ab.Y * t));
    }

    private (int c, int r) TileAt(Point p) =>
        ((int)(p.X / TileSize), (int)(p.Y / TileSize));

    private Vec2 TileCenter(int c, int r) =>
        new(c * TileSize + TileSize / 2, r * TileSize + TileSize / 2);

    private void OnCanvasMouseMove(object sender, MouseEventArgs e)
    {
        if (Overlay.Visibility == Visibility.Visible) return;
        var tile = TileAt(e.GetPosition(GameCanvas));
        if (tile == _hoverTile) return;

        if (_tileRects.TryGetValue(_hoverTile, out var prev)) prev.Opacity = 1.0;
        _hoverTile = tile;
        if (_buildableTiles.Contains(tile) && _tileRects.TryGetValue(tile, out var cur))
            cur.Opacity = IsTileOccupied(tile) ? 0.7 : 0.52;
    }

    private bool IsTileOccupied((int c, int r) tile)
    {
        var center = TileCenter(tile.c, tile.r);
        foreach (var t in _engine.Towers)
            if (t.Pos.DistanceTo(center) < TileSize / 2) return true;
        return false;
    }

    // ─── Theme brushes ───────────────────────────────────────────────────
    private static SolidColorBrush ThemeBuildBrush(StageTheme t) => t switch
    {
        StageTheme.Grassland => new(Color.FromRgb(70, 118, 58)),
        StageTheme.Forest    => new(Color.FromRgb(38,  88, 38)),
        StageTheme.Desert    => new(Color.FromRgb(198, 168, 88)),
        StageTheme.Volcano   => new(Color.FromRgb(98,  38, 22)),
        StageTheme.Snow      => new(Color.FromRgb(212, 228, 248)),
        _                    => new(Color.FromRgb(62,  28, 88)),
    };

    // ─── RENDER ──────────────────────────────────────────────────────────
    private void Render()
    {
        RenderTowers();
        RenderEnemies();
        RenderProjectiles();
        RenderEffects();
        RenderSoldiers();
        UpdateDamageLabels();
        UpdateBossHpBar();
    }

    // ── Towers ──────────────────────────────────────────────────────────
    private void RenderTowers()
    {
        foreach (var t in _engine.Towers)
        {
            if (!_towerShapes.ContainsKey(t)) AddTowerVisual(t);
            else                               UpdateTowerVisual(t);
        }
    }

    private void AddTowerVisual(TowerInstance t)
    {
        var g    = new Grid { Width = 38, Height = 38 };
        var rect = new Rectangle
        {
            Width = 38, Height = 38,
            Fill  = HexBrush(t.CurrentColorHex),
            Stroke = new SolidColorBrush(Color.FromRgb(20, 20, 20)),
            StrokeThickness = 1.5, RadiusX = 7, RadiusY = 7
        };
        g.Children.Add(rect);

        // Big icon
        var icon = new TextBlock
        {
            Text     = t.CurrentIcon,
            FontSize = 18,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment   = VerticalAlignment.Center,
            IsHitTestVisible = false
        };
        g.Children.Add(icon);

        // Level indicator dots
        var pips = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment   = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 0, 2),
            IsHitTestVisible = false
        };
        g.Children.Add(pips);

        g.Tag = t;
        g.IsHitTestVisible = false;
        Canvas.SetLeft(g, t.Pos.X - 19);
        Canvas.SetTop(g,  t.Pos.Y - 19);
        GameCanvas.Children.Add(g);
        _towerShapes[t] = g;

        UpdateTowerVisual(t);
    }

    private void UpdateTowerVisual(TowerInstance t)
    {
        if (_towerShapes[t] is not Grid g) return;

        // Update color
        if (g.Children[0] is Rectangle rect)
            rect.Fill = HexBrush(t.CurrentColorHex);

        // Update icon text
        if (g.Children[1] is TextBlock icon)
            icon.Text = t.CurrentIcon;

        // Update level pips
        if (g.Children[2] is StackPanel pips)
        {
            pips.Children.Clear();
            if (!t.IsBranched)
            {
                int maxLvl = t.Def.Levels.Count;   // 3
                for (int i = 0; i < maxLvl; i++)
                {
                    pips.Children.Add(new Ellipse
                    {
                        Width = 5, Height = 5, Margin = new Thickness(1.5, 0, 1.5, 0),
                        Fill = i <= t.Level
                            ? Brushes.Gold
                            : new SolidColorBrush(Color.FromArgb(120, 60, 60, 60))
                    });
                }
            }
            else
            {
                // Branch badge
                pips.Children.Add(new TextBlock
                {
                    Text     = t.Branch == TowerBranch.A ? "A" : "B",
                    Foreground = Brushes.White, FontSize = 8, FontWeight = FontWeights.Bold
                });
            }
        }
    }

    private void RemoveTowerVisual(TowerInstance t)
    {
        if (_towerShapes.TryGetValue(t, out var v))
        {
            GameCanvas.Children.Remove(v);
            _towerShapes.Remove(t);
        }
    }

    // ── Enemies ─────────────────────────────────────────────────────────
    private void RenderEnemies()
    {
        // Remove dead enemies
        var alive = new HashSet<EnemyInstance>(_engine.Enemies);
        foreach (var key in _enemyShapes.Keys.Where(k => !alive.Contains(k)).ToList())
        {
            GameCanvas.Children.Remove(_enemyShapes[key].body);
            GameCanvas.Children.Remove(_enemyShapes[key].hpBg);
            GameCanvas.Children.Remove(_enemyShapes[key].hpFg);
            _enemyShapes.Remove(key);
        }

        foreach (var e in _engine.Enemies)
        {
            if (!_enemyShapes.ContainsKey(e))
            {
                double spriteSize = e.Def.Radius * 2.4;
                var body = new Image
                {
                    Width            = spriteSize,
                    Height           = spriteSize,
                    Source           = EnemyFallbackImageFactory.CreateSprite(e.Def.Kind),
                    Stretch          = Stretch.Uniform,
                    IsHitTestVisible = false
                };
                double barWidth = e.Def.Radius * 2;
                var hpBg = new Rectangle
                {
                    Width = barWidth, Height = 5,
                    Fill  = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                    IsHitTestVisible = false
                };
                var hpFg = new Rectangle
                {
                    Width = barWidth, Height = 5,
                    Fill  = Brushes.LimeGreen,
                    IsHitTestVisible = false
                };
                _enemyShapes[e] = (body, hpBg, hpFg);
                GameCanvas.Children.Add(hpBg);
                GameCanvas.Children.Add(hpFg);
                GameCanvas.Children.Add(body);
            }

            var sh = _enemyShapes[e];
            double r  = e.Def.Radius;
            double bx = e.Pos.X - r;
            double by = e.Pos.Y - r;

            Canvas.SetLeft(sh.body, e.Pos.X - sh.body.Width / 2);
            Canvas.SetTop (sh.body, e.Pos.Y - sh.body.Height / 2);
            Canvas.SetLeft(sh.hpBg, bx); Canvas.SetTop(sh.hpBg, by - 9);
            Canvas.SetLeft(sh.hpFg, bx); Canvas.SetTop(sh.hpFg, by - 9);

            // HP bar width + color gradient
            double ratio = Math.Max(0, e.Hp / e.MaxHp);
            sh.hpFg.Width = e.Def.Radius * 2 * ratio;
            sh.hpFg.Fill  = HpBarBrush(ratio);

            // Status tint (Image has no Stroke; convey via Opacity)
            if (e.SlowTimer > 0)      sh.body.Opacity = 0.65;   // frozen / slowed
            else if (e.DotTimer > 0)  sh.body.Opacity = 0.85;   // burning
            else                      sh.body.Opacity = 1.0;
        }
    }

    private static SolidColorBrush HpBarBrush(double ratio)
    {
        byte r = ratio > 0.5 ? (byte)(255 * (1 - ratio) * 2) : (byte)220;
        byte g = ratio < 0.5 ? (byte)(200 * ratio * 2)       : (byte)190;
        return new SolidColorBrush(Color.FromRgb(r, g, 0));
    }

    // ── Projectiles ─────────────────────────────────────────────────────
    private void RenderProjectiles()
    {
        var pAlive = new HashSet<Projectile>(_engine.Projectiles);
        foreach (var key in _projShapes.Keys.Where(k => !pAlive.Contains(k)).ToList())
        {
            GameCanvas.Children.Remove(_projShapes[key]);
            _projShapes.Remove(key);
        }
        foreach (var p in _engine.Projectiles)
        {
            if (!_projShapes.ContainsKey(p))
            {
                double sz = p.SplashRadius > 0 ? 10 : 7;
                var shape = new Ellipse
                {
                    Width = sz, Height = sz,
                    Fill  = HexBrush(p.ColorHex),
                    IsHitTestVisible = false
                };
                _projShapes[p] = shape;
                GameCanvas.Children.Add(shape);
            }
            var rp = p.GetRenderPos();
            double hs = p.SplashRadius > 0 ? 5 : 3.5;
            Canvas.SetLeft(_projShapes[p], rp.X - hs);
            Canvas.SetTop(_projShapes[p],  rp.Y - hs);
        }
    }

    // ── Effects ─────────────────────────────────────────────────────────
    private void RenderEffects()
    {
        var fxAlive = new HashSet<HitEffect>(_engine.Effects);
        foreach (var key in _fxShapes.Keys.Where(k => !fxAlive.Contains(k)).ToList())
        {
            GameCanvas.Children.Remove(_fxShapes[key]);
            _fxShapes.Remove(key);
        }
        foreach (var fx in _engine.Effects)
        {
            if (!_fxShapes.ContainsKey(fx))
            {
                var shape = new Ellipse
                {
                    Width  = fx.Radius * 2, Height = fx.Radius * 2,
                    Fill   = HexBrush(fx.ColorHex),
                    IsHitTestVisible = false, Opacity = 0.65
                };
                _fxShapes[fx] = shape;
                GameCanvas.Children.Add(shape);
            }
            var s = _fxShapes[fx];
            Canvas.SetLeft(s, fx.Pos.X - fx.Radius);
            Canvas.SetTop(s,  fx.Pos.Y - fx.Radius);
            s.Opacity = 0.65 * Math.Max(0, fx.TimeLeft / fx.TotalTime);
        }
    }

    // ── Soldiers ────────────────────────────────────────────────────────
    private void RenderSoldiers()
    {
        var all = new List<Soldier>();
        foreach (var t in _engine.Towers)
            if (t.IsBarracks) all.AddRange(t.Soldiers);
        all.AddRange(_engine.Reinforcements);

        var sAlive = new HashSet<Soldier>(all);
        foreach (var key in _soldierShapes.Keys.Where(k => !sAlive.Contains(k)).ToList())
        {
            GameCanvas.Children.Remove(_soldierShapes[key].body);
            GameCanvas.Children.Remove(_soldierShapes[key].hp);
            _soldierShapes.Remove(key);
        }
        foreach (var s in all)
        {
            if (!_soldierShapes.ContainsKey(s))
            {
                bool isRein = s.Owner == null;
                var body = new Rectangle
                {
                    Width = 13, Height = 13,
                    Fill  = isRein ? Brushes.Goldenrod : Brushes.SteelBlue,
                    Stroke = Brushes.White, StrokeThickness = 1,
                    IsHitTestVisible = false
                };
                var hp = new Rectangle
                {
                    Width = 13, Height = 3,
                    Fill  = Brushes.LimeGreen,
                    IsHitTestVisible = false
                };
                _soldierShapes[s] = (body, hp);
                GameCanvas.Children.Add(body);
                GameCanvas.Children.Add(hp);
            }
            var sh = _soldierShapes[s];
            sh.body.Visibility = s.Alive ? Visibility.Visible : Visibility.Collapsed;
            sh.hp.Visibility   = s.Alive ? Visibility.Visible : Visibility.Collapsed;
            Canvas.SetLeft(sh.body, s.Pos.X - 6.5);
            Canvas.SetTop(sh.body,  s.Pos.Y - 6.5);
            Canvas.SetLeft(sh.hp,   s.Pos.X - 6.5);
            Canvas.SetTop(sh.hp,    s.Pos.Y - 14);
            sh.hp.Width = 13 * Math.Max(0, s.Hp / Math.Max(1, s.MaxHp));
        }
    }

    // ── Boss HP bar ─────────────────────────────────────────────────────
    private void UpdateBossHpBar()
    {
        EnemyInstance? boss = null;
        foreach (var e in _engine.Enemies)
            if (e.Alive && (e.Def.IsBoss || e.Def.IsMidBoss)) { boss = e; break; }

        if (boss == null)
        {
            BossHpPanel.Visibility = Visibility.Collapsed;
        }
        else
        {
            BossHpPanel.Visibility = Visibility.Visible;
            BossNameText.Text      = $"  {boss.Def.Name}";
            BossHpBar.Value        = Math.Max(0, boss.Hp / boss.MaxHp * 100);
        }
    }

    // ─── HUD UPDATE ──────────────────────────────────────────────────────
    private void UpdateHud()
    {
        GoldText.Text   = $"♦ {_engine.Gold}G";
        LivesText.Text  = $"❤ {_engine.Lives} / {_engine.LivesMax}";
        WaveText.Text   = $"웨이브  {_engine.Spawner.CurrentWave} / {_engine.Spawner.TotalWaves}";
        SpeedBtn.Content = _engine.Speed == GameSpeed.Fast ? "속도: 2x" : "속도: 1x";
        PauseBtn.Content = _engine.Speed == GameSpeed.Paused ? "재개" : "일시정지";

        NextWaveTimer.Text = _engine.Spawner.AllWavesStarted
            ? "마지막 웨이브"
            : $"다음 웨이브: {_engine.Spawner.NextWaveCountdown:F1}s";

        // Wave preview
        if (!_engine.Spawner.AllWavesStarted)
        {
            var peek = _engine.Spawner.PeekNextWaveEnemies();
            NextWavePreview.Text = peek.Count > 0
                ? "다음: " + string.Join(" ", peek.Select(EnemyIcon))
                : "";
        }
        else
        {
            NextWavePreview.Text = "";
        }

        // Skill buttons
        MeteorBtn.IsEnabled    = _engine.MeteorCooldown <= 0;
        MeteorBtn.Content      = _engine.MeteorCooldown > 0
            ? $"🔥 {_engine.MeteorCooldown:F0}s"  : "🔥 화포 [1]";
        ReinforceBtn.IsEnabled = _engine.ReinforcementCooldown <= 0;
        ReinforceBtn.Content   = _engine.ReinforcementCooldown > 0
            ? $"🛡 {_engine.ReinforcementCooldown:F0}s" : "🛡 지원군 [2]";

        HintText.Text = _skillMode switch
        {
            SkillMode.Meteor   => "맵 클릭 → 화포 시전",
            SkillMode.Reinforce => "맵 클릭 → 지원군 소환",
            _ => ""
        };
    }

    private static string EnemyIcon(EnemyKind k) => k switch
    {
        EnemyKind.Normal           => "N",
        EnemyKind.Fast             => "F",
        EnemyKind.SplitBody        => "S",
        EnemyKind.SplitSmall       => "s",
        EnemyKind.Elite            => "E",
        EnemyKind.EliteCharge      => "EC",
        EnemyKind.EliteRegenerator => "ER",
        EnemyKind.EliteGhost       => "EG",
        EnemyKind.MidBossNormal    => "MN",
        EnemyKind.MidBossCharge    => "MC",
        EnemyKind.MidBossSplit     => "MS",
        EnemyKind.MidBossSpeed     => "MV",
        EnemyKind.BossNormal       => "BN",
        EnemyKind.BossCharge       => "BC",
        EnemyKind.BossSplit        => "BS",
        EnemyKind.BossSpeed        => "BV",
        _                          => "?"
    };

    // ─── INTERACTION ─────────────────────────────────────────────────────
    private void OnCanvasClick(object sender, MouseButtonEventArgs e)
    {
        var pt  = e.GetPosition(GameCanvas);
        var pos = new Vec2(pt.X, pt.Y);

        // Skill mode
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

        // Close overlay on background click
        if (Overlay.Visibility == Visibility.Visible)
        {
            ClearSelection();
            return;
        }

        var tile   = TileAt(pt);
        var center = TileCenter(tile.c, tile.r);

        // Existing tower?
        foreach (var t in _engine.Towers)
        {
            if (t.Pos.DistanceTo(center) < TileSize / 2)
            {
                SelectTower(t);
                return;
            }
        }

        // Buildable tile?
        if (_buildableTiles.Contains(tile))
        {
            ShowBuildMenu(center);
            return;
        }

        ClearSelection();
    }

    // ── Build menu ───────────────────────────────────────────────────────
    private void ShowBuildMenu(Vec2 pos)
    {
        ClearSelection();
        var panel = BuildPanel("#222831", 260);
        AddPanelTitle(panel, "타워 건설", "#FFD369");

        foreach (var kind in _stage.AllowedTowers)
        {
            var def   = TowerCatalog.Towers[kind];
            int cost  = (int)(def.Levels[0].Cost * (1 - SaveManager.TechEffect(TechId.TowerCostReduction)));
            double dps = def.Levels[0].Damage / def.Levels[0].AttackInterval;

            var btn = MakeButton(
                $"{def.Icon} {def.Name}  —  {cost}G   (DPS {dps:F1})",
                240, 44, def.TowerColorHex);
            btn.IsEnabled = _engine.Gold >= cost;
            btn.ToolTip   = def.Description;
            btn.Click += (s, e) => { if (_engine.TryBuild(pos, kind)) ClearSelection(); };
            panel.Children.Add(btn);
        }

        panel.Children.Add(MakeCancelBtn("취소", () => ClearSelection()));
        ShowFloatingPanel(pos, panel);
    }

    // ── Tower panel ──────────────────────────────────────────────────────
    private void SelectTower(TowerInstance t)
    {
        ClearSelection();
        _selectedTower = t;

        // Range ring
        _rangeIndicator = new Ellipse
        {
            Width  = t.EffectiveRange * 2,
            Height = t.EffectiveRange * 2,
            Stroke = new SolidColorBrush(Color.FromArgb(200, 255, 220, 0)),
            StrokeThickness = 1.5,
            Fill   = new SolidColorBrush(Color.FromArgb(25, 255, 255, 0)),
            IsHitTestVisible = false
        };
        Canvas.SetLeft(_rangeIndicator, t.Pos.X - t.EffectiveRange);
        Canvas.SetTop(_rangeIndicator,  t.Pos.Y - t.EffectiveRange);
        GameCanvas.Children.Add(_rangeIndicator);

        var panel = BuildPanel("#222831", 272);
        AddPanelTitle(panel, t.DisplayName, "#FFD369");

        // Stats row
        var lvl = t.CurrentLevel;
        string stats = t.IsBarracks
            ? $"병력 {lvl.SoldierCount}  HP {lvl.SoldierHp:F0}  대미지 {lvl.SoldierDamage:F0}"
            : $"공격력 {t.EffectiveDamage:F0}  사거리 {t.EffectiveRange:F0}  공속 {1/t.EffectiveAttackInterval:F2}/s";
        panel.Children.Add(new TextBlock
        {
            Text = stats, Foreground = Brushes.LightGray,
            FontSize = 12, Margin = new Thickness(10, 0, 10, 8),
            TextWrapping = TextWrapping.Wrap
        });

        // Targeting mode button (not for barracks)
        if (!t.IsBarracks)
        {
            var tBtn = MakeButton($"🎯 타겟: {TargetModeName(t.TargetMode)}", 252, 30, "#3A4A5A");
            tBtn.FontSize = 12;
            tBtn.Click += (s, e) =>
            {
                t.TargetMode = (TargetMode)(((int)t.TargetMode + 1) % 5);
                SelectTower(t);
            };
            panel.Children.Add(tBtn);
        }

        // Upgrade buttons
        if (t.CanUpgrade)
        {
            string note = t.Def.Levels[t.Level + 1].UpgradeNote;
            string label = $"업그레이드 → Lv{t.Level + 2}  ({t.UpgradeCost}G)"
                         + (note.Length > 0 ? $"  [{note}]" : "");
            var up = MakeButton(label, 252, 38, "#1B6B2A");
            up.IsEnabled = _engine.Gold >= t.UpgradeCost;
            up.Click += (s, e) => { _engine.TryUpgrade(t); SelectTower(t); };
            panel.Children.Add(up);
        }
        else if (t.CanChooseBranch)
        {
            panel.Children.Add(new TextBlock
            {
                Text = "⬆ 분기 선택", Foreground = Brushes.Gold,
                FontSize = 12, FontWeight = FontWeights.Bold,
                Margin = new Thickness(10, 4, 10, 2)
            });
            if (t.Def.BranchA != null)
            {
                string note = t.Def.BranchA.Levels[0].UpgradeNote;
                var a = MakeButton($"[A] {t.Def.BranchA.Icon} {t.Def.BranchA.Name}  ({t.BranchACost}G)"
                                 + (note.Length > 0 ? $"  — {note}" : ""), 252, 38, t.Def.BranchA.TowerColorHex);
                a.ToolTip   = t.Def.BranchA.Description;
                a.IsEnabled = _engine.Gold >= t.BranchACost;
                a.Click += (s, e) => { _engine.TryBranch(t, TowerBranch.A); SelectTower(t); };
                panel.Children.Add(a);
            }
            if (t.Def.BranchB != null)
            {
                string note = t.Def.BranchB.Levels[0].UpgradeNote;
                var b = MakeButton($"[B] {t.Def.BranchB.Icon} {t.Def.BranchB.Name}  ({t.BranchBCost}G)"
                                 + (note.Length > 0 ? $"  — {note}" : ""), 252, 38, t.Def.BranchB.TowerColorHex);
                b.ToolTip   = t.Def.BranchB.Description;
                b.IsEnabled = _engine.Gold >= t.BranchBCost;
                b.Click += (s, e) => { _engine.TryBranch(t, TowerBranch.B); SelectTower(t); };
                panel.Children.Add(b);
            }
        }

        // Sell
        var sell = MakeButton($"판매  (+{t.SellValue()}G)", 252, 32, "#8B1A1A");
        sell.Click += (s, e) => { _engine.Sell(t); RemoveTowerVisual(t); ClearSelection(); };
        panel.Children.Add(sell);

        panel.Children.Add(MakeCancelBtn("닫기", () => ClearSelection()));
        ShowFloatingPanel(t.Pos, panel);
    }

    private static string TargetModeName(TargetMode m) => m switch
    {
        TargetMode.First    => "선두 우선",
        TargetMode.Last     => "후미 우선",
        TargetMode.Strongest => "고체력 우선",
        TargetMode.Weakest  => "저체력 우선",
        TargetMode.Flying   => "비행 우선",
        _                   => "?"
    };

    // ─── Panel helpers ───────────────────────────────────────────────────
    private static StackPanel BuildPanel(string bgHex, double width)
        => new() { Background = HexBrush(bgHex), Width = width };

    private static void AddPanelTitle(StackPanel p, string text, string colorHex)
        => p.Children.Add(new TextBlock
        {
            Text = text, FontSize = 16, FontWeight = FontWeights.Bold,
            Foreground = HexBrush(colorHex), Margin = new Thickness(10, 10, 10, 6)
        });

    private static Button MakeButton(string label, double w, double h, string colorHex)
        => new()
        {
            Content = label, Width = w, Height = h,
            Margin  = new Thickness(10, 3, 10, 3),
            Background = HexBrush(colorHex),
            Foreground = Brushes.White,
            BorderThickness = new Thickness(0),
            FontSize = 13, HorizontalContentAlignment = HorizontalAlignment.Left,
            Padding = new Thickness(6, 0, 6, 0)
        };

    private static Button MakeCancelBtn(string label, Action onClick)
    {
        var b = new Button
        {
            Content = label, Width = 80, Height = 28,
            Margin  = new Thickness(10, 6, 10, 10),
            Background = HexBrush("#546E7A"),
            Foreground = Brushes.White, BorderThickness = new Thickness(0), FontSize = 12
        };
        b.Click += (_, _) => onClick();
        return b;
    }

    private void ShowFloatingPanel(Vec2 anchor, FrameworkElement panel)
    {
        Overlay.Visibility  = Visibility.Visible;
        OverlayContent.Content = panel;
    }

    private void ClearSelection()
    {
        _selectedTower = null;
        if (_rangeIndicator != null)
        {
            GameCanvas.Children.Remove(_rangeIndicator);
            _rangeIndicator = null;
        }
        Overlay.Visibility     = Visibility.Collapsed;
        OverlayContent.Content = null;
    }

    private void ClearSkillIndicator()
    {
        if (_skillIndicator != null)
        {
            GameCanvas.Children.Remove(_skillIndicator);
            _skillIndicator = null;
        }
    }

    // ─── BUTTON EVENTS ───────────────────────────────────────────────────
    private void OnSpeed(object s, RoutedEventArgs e)
        => _engine.Speed = _engine.Speed == GameSpeed.Fast ? GameSpeed.Normal : GameSpeed.Fast;

    private void OnPause(object s, RoutedEventArgs e)
        => _engine.Speed = _engine.Speed == GameSpeed.Paused ? GameSpeed.Normal : GameSpeed.Paused;

    private void OnCallWave(object s, RoutedEventArgs e)
    {
        int bonus = _engine.CallNextWaveEarly();
        if (bonus > 0) HintText.Text = $"+{bonus}G  조기 호출 보너스!";
    }

    private void OnMeteor(object s, RoutedEventArgs e)
        => _skillMode = _engine.MeteorCooldown <= 0 ? SkillMode.Meteor : SkillMode.None;

    private void OnReinforce(object s, RoutedEventArgs e)
        => _skillMode = _engine.ReinforcementCooldown <= 0 ? SkillMode.Reinforce : SkillMode.None;

    private void OnExit(object s, RoutedEventArgs e)
    {
        _timer.Stop();
        MainWindow.Instance!.NavigateTo(new StageSelectPage());
    }

    private void OnKey(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Space:  OnPause(this, new RoutedEventArgs()); break;
            case Key.F:      OnSpeed(this, new RoutedEventArgs()); break;
            case Key.N:      OnCallWave(this, new RoutedEventArgs()); break;
            case Key.D1:     OnMeteor(this, new RoutedEventArgs()); break;
            case Key.D2:     OnReinforce(this, new RoutedEventArgs()); break;
            case Key.Escape: ClearSelection(); _skillMode = SkillMode.None; ClearSkillIndicator(); break;
        }
    }

    // ─── TUTORIAL ────────────────────────────────────────────────────────
    private void ShowTutorial()
    {
        var msgs = new[]
        {
            "튜토리얼 — 1/5\n\n녹색 타일을 클릭하면 타워를 건설할 수 있습니다.\n하단 [다음 웨이브 N] 버튼으로 적을 출현시키세요.",
            "튜토리얼 — 2/5\n\n타워는 아처 🏹·마법사 🔮·폭격 💣·병영 ⚔ 4종입니다.\n타워를 클릭하면 업그레이드·분기·판매가 가능합니다.",
            "튜토리얼 — 3/5\n\n🎯 버튼으로 각 타워의 타겟 우선순위를 변경할 수 있습니다.\n선두 / 후미 / 고체력 / 저체력 / 비행 중 선택하세요.",
            "튜토리얼 — 4/5\n\n[1] 화포 지원과 [2] 지원군 소환은 강력한 스킬입니다.\n쿨다운이 있으니 위기 상황에만 사용하세요.",
            "튜토리얼 — 5/5\n\n다음 웨이브를 일찍 호출(N)하면 잔여시간 비례 골드 보너스!\n아처는 12% 확률로 치명타 2.2배 피해를 줍니다.\n\n행운을 빕니다! (+200G 지급)"
        };
        int idx = 0;
        Action show = null!;
        show = () =>
        {
            if (idx >= msgs.Length)
            {
                SaveManager.Current.TutorialDone = true;
                _engine.Gold += 200;
                SaveManager.Save();
                ClearSelection();
                return;
            }
            var panel = new StackPanel
            {
                Background = new SolidColorBrush(Color.FromRgb(20, 24, 32)),
                Width = 500
            };
            var border = new Border
            {
                Child = panel, BorderBrush = new SolidColorBrush(Color.FromRgb(80, 100, 130)),
                BorderThickness = new Thickness(1.5), CornerRadius = new CornerRadius(8)
            };
            panel.Children.Add(new TextBlock
            {
                Text = msgs[idx], FontSize = 15, Foreground = Brushes.White,
                Margin = new Thickness(24, 20, 24, 12), TextWrapping = TextWrapping.Wrap,
                LineHeight = 22
            });
            var btns = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(24, 0, 24, 20) };
            var next = MakeButton(idx == msgs.Length - 1 ? "시작! ▶" : "다음 ▶", 130, 36, "#1B6B2A");
            next.Click += (s, e) => { idx++; show(); };
            var skip = MakeButton("건너뛰기", 100, 30, "#546E7A");
            skip.Margin = new Thickness(8, 3, 8, 3);
            skip.Click += (s, e) => { idx = msgs.Length; show(); };
            btns.Children.Add(next);
            btns.Children.Add(skip);
            panel.Children.Add(btns);
            Overlay.Visibility     = Visibility.Visible;
            OverlayContent.Content = border;
        };
        show();
    }

    // ─── STAGE INTRO ─────────────────────────────────────────────────────
    /// <summary>
    /// Shown once at the start of every stage (except Stage 1 first play).
    /// Lists every distinct enemy kind that appears in the upcoming wave plan.
    /// </summary>
    private void ShowStageIntro()
    {
        var panel = new StackPanel
        {
            Background = new SolidColorBrush(Color.FromRgb(18, 22, 30)),
            Width = 560
        };
        var border = new Border
        {
            Child = panel,
            BorderBrush = new SolidColorBrush(Color.FromRgb(200, 160, 0)),
            BorderThickness = new Thickness(1.5),
            CornerRadius = new CornerRadius(8)
        };

        panel.Children.Add(new TextBlock
        {
            Text = $"스테이지 {_stage.Number}  ·  {_stage.Name}",
            FontSize = 24, FontWeight = FontWeights.Bold,
            Foreground = Brushes.Gold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(20, 20, 20, 4)
        });
        panel.Children.Add(new TextBlock
        {
            Text = "출현 예정 적",
            FontSize = 13, Foreground = Brushes.LightGray,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 12)
        });

        var wrap = new WrapPanel
        {
            Margin = new Thickness(16, 0, 16, 8),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        var seen = new HashSet<EnemyKind>();
        foreach (var entry in _stage.Waves.SelectMany(w => w.Entries))
        {
            if (!seen.Add(entry.Enemy)) continue;
            var def  = EnemyCatalog.Enemies[entry.Enemy];
            var item = new StackPanel
            {
                Width = 100, Margin = new Thickness(6),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            item.Children.Add(new Image
            {
                Source = EnemyFallbackImageFactory.CreateIcon(entry.Enemy),
                Width = 58, Height = 58, Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            item.Children.Add(new TextBlock
            {
                Text = def.Name,
                Foreground = Brushes.White,
                FontSize = 11,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 4, 0, 0)
            });
            wrap.Children.Add(item);
        }
        panel.Children.Add(wrap);

        var start = MakeButton("시작 ▶", 130, 36, "#1B6B2A");
        start.HorizontalAlignment = HorizontalAlignment.Center;
        start.Margin = new Thickness(0, 12, 0, 22);
        start.Click += (s, e) => ClearSelection();
        panel.Children.Add(start);

        Overlay.Visibility     = Visibility.Visible;
        OverlayContent.Content = border;
    }

    // ─── RESULT SCREEN ───────────────────────────────────────────────────
    private void ShowResult()
    {
        _timer.Stop();
        int stars = _engine.ComputeStars();
        if (_engine.Result == GameResult.Won)
            SaveManager.RecordStageStars(_stage.Number, stars);

        var panel = new StackPanel
        {
            Background = new SolidColorBrush(Color.FromRgb(18, 22, 30)),
            Width = 500
        };
        var border = new Border
        {
            Child = panel,
            BorderBrush = new SolidColorBrush(_engine.Result == GameResult.Won
                ? Color.FromRgb(200, 160, 0) : Color.FromRgb(160, 30, 30)),
            BorderThickness = new Thickness(2), CornerRadius = new CornerRadius(10)
        };

        bool won = _engine.Result == GameResult.Won;
        panel.Children.Add(new TextBlock
        {
            Text = won ? "✦  승리!  ✦" : "✗  패배  ✗",
            FontSize = 38, FontWeight = FontWeights.Bold,
            Foreground = won ? Brushes.Gold : Brushes.IndianRed,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(20, 24, 20, 4)
        });

        if (won)
        {
            // Stars
            var starRow = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 6, 0, 6) };
            for (int i = 0; i < 3; i++)
            {
                starRow.Children.Add(new TextBlock
                {
                    Text = i < stars ? "★" : "☆",
                    FontSize = 44,
                    Foreground = i < stars ? Brushes.Gold : new SolidColorBrush(Color.FromRgb(80, 80, 80)),
                    Margin = new Thickness(4, 0, 4, 0)
                });
            }
            panel.Children.Add(starRow);

            panel.Children.Add(new TextBlock
            {
                Text = $"남은 라이프  {_engine.Lives} / {_engine.LivesMax}",
                Foreground = Brushes.White, FontSize = 15,
                HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 4, 0, 2)
            });
            panel.Children.Add(new TextBlock
            {
                Text = $"보유 골드  {_engine.Gold}G",
                Foreground = Brushes.LightGoldenrodYellow, FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 0, 0, 12)
            });
        }
        else
        {
            panel.Children.Add(new TextBlock
            {
                Text = "다시 도전해 방어선을 지켜내세요!",
                Foreground = Brushes.LightGray, FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 8, 0, 16)
            });
        }

        var sp = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 8, 0, 24)
        };
        var retry = MakeButton("재도전", 110, 40, "#1B6B2A");
        retry.HorizontalContentAlignment = HorizontalAlignment.Center;
        retry.Click += (_, _) => MainWindow.Instance!.NavigateTo(new GamePage(_stage));
        var stageBtn = MakeButton("스테이지", 110, 40, "#1565C0");
        stageBtn.HorizontalContentAlignment = HorizontalAlignment.Center;
        stageBtn.Click += (_, _) => MainWindow.Instance!.NavigateTo(new StageSelectPage());
        var techBtn = MakeButton("테크트리", 110, 40, "#6A1B9A");
        techBtn.HorizontalContentAlignment = HorizontalAlignment.Center;
        techBtn.Click += (_, _) => MainWindow.Instance!.NavigateTo(new TechTreePage());
        sp.Children.Add(retry);
        sp.Children.Add(stageBtn);
        sp.Children.Add(techBtn);
        panel.Children.Add(sp);

        Overlay.Visibility     = Visibility.Visible;
        OverlayContent.Content = border;
    }

    // ─── Utility ─────────────────────────────────────────────────────────
    private static SolidColorBrush HexBrush(string hex)
        => (SolidColorBrush)new BrushConverter().ConvertFromString(hex)!;
}
