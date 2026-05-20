using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using KingdomRushClone.Data;
using KingdomRushClone.Models;

namespace KingdomRushClone.Views;

public partial class TowerGuidePage : Page
{
    // ─── Narrative text only — all stats come from TowerCatalog (SSOT) ──
    private record Narrative(string Summary, string Tip, string[] Strengths, string[] Weaknesses);

    private static readonly Dictionary<TowerKind, Narrative> Narratives = new()
    {
        [TowerKind.Archer] = new(
            "원거리 단일 타겟 공격 타워. 빠른 공격속도로 적을 지속적으로 견제합니다.",
            "적은 비용으로 빠르게 설치할 수 있어 초반 운영에 필수입니다. 비행 적(와이번)에 효과적이며, 병영과 함께 배치하면 차단된 적을 집중 사격할 수 있습니다.",
            new[] { "비행 적 (와이번)", "저HP 다수 적", "치명타 12% (BranchA 25%)" },
            new[] { "중장갑 (오크, 암흑기사)" }),

        [TowerKind.Mage] = new(
            "마법 피해를 주는 원거리 타워. 장갑을 무시하고 적의 이동속도를 감소시킵니다.",
            "물리 저항이 높은 중장갑 적에게 특히 강합니다. 슬로우 효과로 다른 타워가 더 오래 적을 공격할 수 있도록 도와줍니다. 비용이 높지만 범용성이 좋습니다.",
            new[] { "중장갑 (오크, 암흑기사)", "마법 저항 없는 모든 적" },
            new[] { "마법 저항 적 (트롤 주술사)" }),

        [TowerKind.Bombard] = new(
            "광역 스플래시 피해를 주는 공성 타워. 느리지만 몰려오는 적 무리를 한번에 제압합니다.",
            "공격 속도가 느린 대신 넓은 범위에 피해를 줍니다. 경로가 좁은 구간이나 적이 밀집하는 코너에 배치하면 효율이 극대화됩니다. 비행 적에게는 피해를 줄 수 없으니 주의.",
            new[] { "밀집된 다수 적", "중장갑 + 군중 동시 처리" },
            new[] { "비행 적 (폭발물 미적용)", "빠른 단독 적" }),

        [TowerKind.Barracks] = new(
            "근접 병사를 소환하여 경로 위에서 적을 직접 차단하는 전략 타워. DPS보다 '역할'이 핵심.",
            "병사들이 적의 이동을 막는 동안 아처·마법사 타워가 집중 사격합니다. 병영 단독으론 적을 처치하기 어렵지만, 다른 타워와 조합하면 시너지가 폭발적으로 증가합니다.",
            new[] { "모든 지상 적 (차단 역할)", "원거리 타워와 시너지" },
            new[] { "비행 적 (근접 불가)", "단독 DPS 낮음" }),

        [TowerKind.Slow] = new(
            "광역 슬로우 효과를 부여하는 서포트 타워입니다.",
            "데미지는 낮지만 강력한 둔화 효과로 적들의 진격을 늦춥니다. 폭격 타워나 아처 타워 근처에 배치하여 화력을 집중할 시간을 벌어주세요.",
            new[] { "빠른 적 (고블린 스카우트)", "밀집된 군중" },
            new[] { "슬로우 면역 적", "보스 (낮은 대미지)" }),
    };

    // ─── Guides built from TowerCatalog at startup ──────────────────────
    private static readonly List<TowerGuideData> Guides = BuildGuides();

    private static List<TowerGuideData> BuildGuides()
    {
        var order = new[] { TowerKind.Archer, TowerKind.Mage, TowerKind.Bombard, TowerKind.Barracks, TowerKind.Slow };
        var list = new List<TowerGuideData>();
        foreach (var kind in order)
        {
            var def = TowerCatalog.Towers[kind];
            var nar = Narratives[kind];

            var levels = new List<LevelInfo>();
            for (int i = 0; i < def.Levels.Count; i++)
            {
                var lv = def.Levels[i];
                int dmgCol = kind == TowerKind.Barracks ? (int)lv.SoldierDamage : (int)lv.Damage;
                string note = BuildLevelNote(kind, i, lv);
                levels.Add(new LevelInfo($"Lv {i + 1}", lv.Cost, dmgCol, (int)lv.Range, (float)lv.AttackInterval, note));
            }

            list.Add(new TowerGuideData(
                kind, def.Name,
                def.GuideColorHex, def.TowerColorHex, def.ProjectileColorHex,
                def.Icon,
                nar.Summary, nar.Tip, nar.Strengths, nar.Weaknesses,
                levels.ToArray(),
                BuildBranch(def.BranchA, kind),
                BuildBranch(def.BranchB, kind)));
        }
        return list;
    }

    private static string BuildLevelNote(TowerKind kind, int levelIdx, TowerLevel lv)
    {
        bool last = levelIdx == 2;
        string suffix = last ? "  →  분기 선택 가능" : "";
        return kind switch
        {
            TowerKind.Mage when lv.SlowAmount > 0
                => $"슬로우 {lv.SlowAmount * 100:F0}% · {lv.SlowDuration:F1}s{suffix}",
            TowerKind.Bombard when lv.SplashRadius > 0
                => $"스플래시 반경 {lv.SplashRadius:F0}{suffix}",
            TowerKind.Barracks
                => $"병사 {lv.SoldierCount}체 · HP {lv.SoldierHp:F0} · 리스폰 {lv.SoldierRespawn:F0}s{suffix}",
            TowerKind.Slow when lv.SlowAmount > 0
                => $"슬로우 {lv.SlowAmount * 100:F0}% · 반경 {lv.SplashRadius:F0}",
            _ => string.IsNullOrEmpty(lv.UpgradeNote) ? (last ? "분기 선택 가능" : "기본") : lv.UpgradeNote + suffix,
        };
    }

    private static BranchInfo BuildBranch(TowerDef? branch, TowerKind parentKind)
    {
        if (branch == null || branch.Levels.Count == 0)
            return new BranchInfo("(없음)", "#444444", "?", "", 0, 0, 0, 0f, "");

        var lv = branch.Levels[0];
        int dmgCol = parentKind == TowerKind.Barracks ? (int)lv.SoldierDamage : (int)lv.Damage;
        string feature = string.IsNullOrEmpty(lv.UpgradeNote) ? branch.Description : lv.UpgradeNote;
        return new BranchInfo(
            branch.Name, branch.GuideColorHex, branch.Icon, branch.Description,
            lv.Cost, dmgCol, (int)lv.Range, (float)lv.AttackInterval, feature);
    }

    private TowerKind _selected = TowerKind.Archer;

    public TowerGuidePage()
    {
        InitializeComponent();
        BuildList();
        ShowDetail(_selected);
    }

    private void BuildList()
    {
        TowerListPanel.Children.Clear();
        TowerListPanel.Children.Add(new TextBlock
        {
            Text = "타워 목록",
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.LightGoldenrodYellow,
            Margin = new Thickness(0, 0, 0, 12)
        });

        foreach (var g in Guides)
        {
            bool sel = g.Kind == _selected;
            var btn = new Button
            {
                Height = 56,
                Margin = new Thickness(0, 4, 0, 4),
                Background = sel
                    ? (Brush)new BrushConverter().ConvertFromString(g.GuideColorHex)!
                    : new SolidColorBrush(Color.FromRgb(42, 50, 60)),
                BorderThickness = new Thickness(sel ? 2 : 0),
                BorderBrush = Brushes.White,
                Tag = g.Kind,
                Content = BuildListItem(g, sel)
            };
            btn.Click += (s, e) =>
            {
                _selected = (TowerKind)((Button)s).Tag;
                BuildList();
                ShowDetail(_selected);
            };
            TowerListPanel.Children.Add(btn);
        }
    }

    private static UIElement BuildListItem(TowerGuideData g, bool selected)
    {
        var sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(6, 0, 6, 0) };
        sp.Children.Add(new TextBlock { Text = g.Icon, FontSize = 22, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) });
        var right = new StackPanel();
        right.Children.Add(new TextBlock { Text = g.Name, FontSize = 14, FontWeight = FontWeights.Bold, Foreground = Brushes.White });
        right.Children.Add(new TextBlock { Text = RoleShort(g.Kind), FontSize = 11, Foreground = selected ? Brushes.White : Brushes.LightGray });
        sp.Children.Add(right);
        return sp;
    }

    private static string RoleShort(TowerKind k) => k switch
    {
        TowerKind.Archer => "단일 원거리 · 속사",
        TowerKind.Mage => "마법 피해 · 슬로우",
        TowerKind.Bombard => "광역 스플래시",
        TowerKind.Barracks => "근접 차단 · 소환",
        TowerKind.Slow => "광역 둔화 · 서포트",
        _ => ""
    };

    private void ShowDetail(TowerKind kind)
    {
        var g = Guides.Find(x => x.Kind == kind)!;
        DetailPanel.Children.Clear();

        // 타이틀
        var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
        header.Children.Add(new TextBlock { Text = g.Icon, FontSize = 36, Margin = new Thickness(0, 0, 12, 0) });
        var htitle = new StackPanel();
        htitle.Children.Add(new TextBlock
        {
            Text = g.Name,
            FontSize = 28,
            FontWeight = FontWeights.Bold,
            Foreground = (Brush)new BrushConverter().ConvertFromString(g.GuideColorHex)!
        });
        htitle.Children.Add(new TextBlock
        {
            Text = RoleShort(kind),
            FontSize = 14,
            Foreground = Brushes.LightGray
        });
        header.Children.Add(htitle);
        DetailPanel.Children.Add(header);
        DetailPanel.Children.Add(Divider());

        // 설명
        DetailPanel.Children.Add(Label("개요"));
        DetailPanel.Children.Add(Body(g.Summary));
        DetailPanel.Children.Add(SectionSpace());
        DetailPanel.Children.Add(Body(g.Tip));
        DetailPanel.Children.Add(SectionSpace());

        // 강점 / 약점
        var swGrid = new Grid { Margin = new Thickness(0, 0, 0, 16) };
        swGrid.ColumnDefinitions.Add(new ColumnDefinition());
        swGrid.ColumnDefinitions.Add(new ColumnDefinition());
        var sp = new StackPanel { Margin = new Thickness(0, 0, 8, 0) };
        sp.Children.Add(new TextBlock { Text = "✅  강점", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = Brushes.LightGreen, Margin = new Thickness(0, 0, 0, 6) });
        foreach (var s in g.Strengths)
            sp.Children.Add(new TextBlock { Text = "  • " + s, Foreground = Brushes.LightGreen, FontSize = 13, Margin = new Thickness(0, 2, 0, 2) });
        var wp = new StackPanel { Margin = new Thickness(8, 0, 0, 0) };
        wp.Children.Add(new TextBlock { Text = "❌  약점", FontSize = 14, FontWeight = FontWeights.Bold, Foreground = Brushes.Tomato, Margin = new Thickness(0, 0, 0, 6) });
        foreach (var w in g.Weaknesses)
            wp.Children.Add(new TextBlock { Text = "  • " + w, Foreground = Brushes.Tomato, FontSize = 13, Margin = new Thickness(0, 2, 0, 2) });
        Grid.SetColumn(sp, 0); Grid.SetColumn(wp, 1);
        swGrid.Children.Add(sp); swGrid.Children.Add(wp);
        DetailPanel.Children.Add(swGrid);
        DetailPanel.Children.Add(Divider());

        // 레벨 업그레이드
        DetailPanel.Children.Add(Label("레벨 업그레이드 (Lv 1 → 3)"));
        foreach (var lv in g.Levels)
            DetailPanel.Children.Add(LevelCard(lv, g.GuideColorHex));
        DetailPanel.Children.Add(SectionSpace());
        DetailPanel.Children.Add(Divider());

        // 분기
        DetailPanel.Children.Add(Label("Lv 4 분기 선택"));
        DetailPanel.Children.Add(new TextBlock
        {
            Text = "Lv 3 도달 후 두 가지 전문화 경로 중 하나를 선택합니다. 선택 후 변경 불가.",
            Foreground = Brushes.LightGray,
            FontSize = 13,
            Margin = new Thickness(0, 0, 0, 12),
            TextWrapping = TextWrapping.Wrap
        });

        var branchGrid = new Grid { Margin = new Thickness(0, 0, 0, 0) };
        branchGrid.ColumnDefinitions.Add(new ColumnDefinition());
        branchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(16) });
        branchGrid.ColumnDefinitions.Add(new ColumnDefinition());
        var ba = BranchCard(g.BranchA, "분기 A");
        var bb = BranchCard(g.BranchB, "분기 B");
        Grid.SetColumn(ba, 0); Grid.SetColumn(bb, 2);
        branchGrid.Children.Add(ba); branchGrid.Children.Add(bb);
        DetailPanel.Children.Add(branchGrid);

        // 운영 팁
        DetailPanel.Children.Add(SectionSpace());
        DetailPanel.Children.Add(Divider());
        DetailPanel.Children.Add(Label("배치 팁"));
        foreach (var tip in PlacementTips(kind))
            DetailPanel.Children.Add(new TextBlock
            {
                Text = "💡  " + tip,
                Foreground = Brushes.LightGoldenrodYellow,
                  FontSize = 13,
                Margin = new Thickness(0, 4, 0, 4),
                TextWrapping = TextWrapping.Wrap
            });
    }

    private static UIElement LevelCard(LevelInfo lv, string colorHex)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(30, 38, 50)),
            CornerRadius = new CornerRadius(6),
            Margin = new Thickness(0, 4, 0, 4),
            Padding = new Thickness(14, 10, 14, 10)
        };

        var g = new Grid();

        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(90) });
        g.ColumnDefinitions.Add(new ColumnDefinition());

        var lvLabel = new TextBlock
        {
             Text = lv.Label,
            FontWeight = FontWeights.Bold,
            FontSize = 15,
            Foreground = (Brush)new BrushConverter().ConvertFromString(colorHex)!,
            VerticalAlignment = VerticalAlignment.Center
        };

           UIElement Stat(string icon, string val)
        {
            var s = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            s.Children.Add(new TextBlock
            {
                Text = icon,
                FontSize = 11,
                Foreground = Brushes.Gray,
                HorizontalAlignment = HorizontalAlignment.Center
            });

            s.Children.Add(new TextBlock
            {
                Text = val,
                FontSize = 13,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            return s;
        }

         var costStat = Stat("비용", $"{lv.Cost}G");
        var dmgStat = Stat("피해", $"{lv.Damage}");
        var rangeStat = Stat("사거리", $"{lv.Range}");
        var intervalStat = Stat("공격간격", $"{lv.Interval}s");

        var note = new TextBlock
        {
            Text = lv.Note,
            FontSize = 12,
            Foreground = Brushes.LightGray,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap
        };

        Grid.SetColumn(lvLabel, 0);
        Grid.SetColumn(costStat, 1);
        Grid.SetColumn(dmgStat, 2);
        Grid.SetColumn(rangeStat, 3);
        Grid.SetColumn(intervalStat, 4);
        Grid.SetColumn(note, 5);

        g.Children.Add(lvLabel);
        g.Children.Add(costStat);
        g.Children.Add(dmgStat);
        g.Children.Add(rangeStat);
        g.Children.Add(intervalStat);
        g.Children.Add(note);

        border.Child = g;
        return border;
    }

    private static UIElement BranchCard(BranchInfo b, string tag)
    {
        var border = new Border
        {
            Background = (Brush)new BrushConverter().ConvertFromString(b.ColorHex)! is SolidColorBrush cb
                ? new SolidColorBrush(Color.FromArgb(60, cb.Color.R, cb.Color.G, cb.Color.B))
                : Brushes.Transparent,
            BorderBrush = (Brush)new BrushConverter().ConvertFromString(b.ColorHex)!,
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16)
        };
        var sp = new StackPanel();
        sp.Children.Add(new TextBlock { Text = tag, FontSize = 11, Foreground = Brushes.LightGray, Margin = new Thickness(0, 0, 0, 4) });
        var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
        row.Children.Add(new TextBlock { Text = b.Icon, FontSize = 24, Margin = new Thickness(0, 0, 8, 0) });
        row.Children.Add(new TextBlock { Text = b.Name, FontSize = 18, FontWeight = FontWeights.Bold, Foreground = Brushes.White, VerticalAlignment = VerticalAlignment.Center });
        sp.Children.Add(row);
        sp.Children.Add(new TextBlock { Text = b.Description, FontSize = 13, Foreground = Brushes.LightGray, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 12) });

        var stats = new WrapPanel { Margin = new Thickness(0, 0, 0, 8) };
        void AddChip(string txt, Brush fg) => stats.Children.Add(new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(80, 255, 255, 255)),
            CornerRadius = new CornerRadius(4),
            Margin = new Thickness(0, 0, 6, 4),
            Padding = new Thickness(8, 3, 8, 3),
            Child = new TextBlock { Text = txt, Foreground = fg, FontSize = 12 }
        });
        AddChip($"비용 {b.Cost}G", Brushes.LightGoldenrodYellow);
        AddChip($"피해 {b.Damage}", Brushes.Tomato);
        AddChip($"사거리 {b.Range}", Brushes.LightSkyBlue);
        AddChip($"간격 {b.Interval}s", Brushes.LightGreen);
        sp.Children.Add(stats);
        sp.Children.Add(new TextBlock { Text = "★  " + b.Feature, FontSize = 12, Foreground = Brushes.LightGoldenrodYellow, TextWrapping = TextWrapping.Wrap });
        border.Child = sp;
        return border;
    }

    private static string[] PlacementTips(TowerKind k) => k switch
    {
        TowerKind.Archer => new[]
        {
            "직선 경로 중간에 배치하면 사거리를 최대로 활용할 수 있습니다.",
            "병영과 조합 시, 차단된 적을 집중 사격하여 처리 속도가 크게 증가합니다.",
            "와이번 등 비행 적이 많은 스테이지에서 반드시 포함하세요.",
            "분기 A(사격수): 보스·탱커 처리 / 분기 B(속사): 군중 처리"
        },
        TowerKind.Mage => new[]
        {
            "오크 전사·암흑기사가 많이 등장하는 6스테이지 이후부터 필수입니다.",
            "슬로우 효과 덕분에 아처·폭격 타워의 명중 기회가 늘어납니다.",
            "코너(방향 전환 지점)에 배치하면 적이 잠시 멈추는 효과를 얻습니다.",
            "분기 A(서리): 군중 슬로우 / 분기 B(화염): 도트 딜 + 광역"
        },
        TowerKind.Bombard => new[]
        {
            "경로가 꺾이는 코너 지점에 배치하면 광역 범위를 최대화할 수 있습니다.",
            "병영이 차단한 밀집 구간에 배치하면 극대 데미지를 노릴 수 있습니다.",
            "비행 적(와이번)이 등장하는 구간에는 아처를 반드시 함께 배치하세요.",
            "분기 A(박격포): 후방 지원 장거리 / 분기 B(지뢰): 보스 처치용"
        },
        TowerKind.Barracks => new[]
        {
            "단독으로 배치하면 처치가 어렵습니다. 반드시 원거리 타워 사거리 내에 배치하세요.",
            "경로 폭이 좁은 구간에 배치하면 적을 더 오래 차단할 수 있습니다.",
            "병사가 전멸해도 일정 시간 후 자동 리스폰됩니다. 포기하지 마세요.",
            "분기 A(성기사): 체력 중시 장기전 / 분기 B(도적): 빠른 처치 속전속결"
        },
        _ => System.Array.Empty<string>()
    };

    private static TextBlock Label(string text) => new()
    {
        Text = text,
        FontSize = 16,
        FontWeight = FontWeights.Bold,
        Foreground = Brushes.LightGoldenrodYellow,
        Margin = new Thickness(0, 0, 0, 10)
    };

    private static TextBlock Body(string text) => new()
    {
        Text = text,
        FontSize = 14,
        Foreground = Brushes.LightGray,
        TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(0, 0, 0, 8),
        LineHeight = 22
    };

    private static UIElement Divider() => new Rectangle
    {
        Height = 1,
        Fill = new SolidColorBrush(Color.FromRgb(50, 60, 75)),
        Margin = new Thickness(0, 10, 0, 14)
    };

    private static UIElement SectionSpace() => new Rectangle { Height = 8 };

    private void OnBack(object s, RoutedEventArgs e) => MainWindow.Instance!.NavigateTo(new MainMenuPage());
}

// ── 데이터 클래스 ─────────────────────────────────────

record TowerGuideData(
   TowerKind Kind, string Name, string GuideColorHex, string TowerColorHex, string ProjectileColorHex, string Icon,
    string Summary, string Tip,
    string[] Strengths, string[] Weaknesses,
    LevelInfo[] Levels, BranchInfo BranchA, BranchInfo BranchB);

record LevelInfo(string Label, int Cost, int Damage, int Range, float Interval, string Note);

record BranchInfo(string Name, string ColorHex, string Icon, string Description,
    int Cost, int Damage, int Range, float Interval, string Feature);
