using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using KingdomRushClone.Models;

namespace KingdomRushClone.Views;

/// <summary>
/// Creates enemy visuals. Sprite files are preferred; missing sprites use the sheet, then code-drawn fallback.
/// </summary>
public static class EnemyFallbackImageFactory
{
    private static readonly object CacheLock = new();
    private static readonly Dictionary<string, ImageSource> Cache = new();
    private static readonly Dictionary<EnemyKind, ImageSource?> SpriteCache = new();
    private static readonly Dictionary<string, BitmapSource> SheetCache = new();
    private static readonly string[] SpriteExtensions = { ".png", ".jpg", ".jpeg", ".bmp", ".gif" };
    private const string SpriteSheetFileName = "EnemySpriteSheet.png";

    private static readonly Dictionary<EnemyKind, Int32Rect> SpriteSheetRegions = new()
    {
        [EnemyKind.Normal] = new(33, 104, 181, 223),
        [EnemyKind.Fast] = new(217, 104, 181, 223),
        [EnemyKind.SplitBody] = new(401, 104, 181, 223),
        [EnemyKind.SplitSmall] = new(583, 104, 181, 223),
        [EnemyKind.Elite] = new(766, 104, 181, 223),
        [EnemyKind.EliteCharge] = new(949, 104, 181, 223),
        [EnemyKind.EliteWyvern] = new(1130, 104, 181, 223),
        [EnemyKind.EliteRegenerator] = new(1435, 104, 181, 223),
        [EnemyKind.MidBossNormal] = new(277, 354, 263, 264),
        [EnemyKind.MidBossCharge] = new(552, 354, 263, 264),
        [EnemyKind.MidBossSpeed] = new(827, 354, 263, 264),
        [EnemyKind.MidBossSplit] = new(1103, 354, 263, 264),
        [EnemyKind.BossNormal] = new(277, 618, 263, 284),
        [EnemyKind.BossCharge] = new(552, 618, 263, 284),
        [EnemyKind.BossSpeed] = new(827, 618, 263, 284),
        [EnemyKind.BossSplit] = new(1103, 618, 263, 284)
    };

    private static readonly Color NormalColor   = Color.FromRgb(34, 197, 94);
    private static readonly Color FastColor     = Color.FromRgb(14, 165, 233);
    private static readonly Color SplitColor    = Color.FromRgb(132, 204, 22);
    private static readonly Color HeavyColor    = Color.FromRgb(85, 139, 47);
    private static readonly Color FlyingColor   = Color.FromRgb(144, 164, 174);
    private static readonly Color MagicColor    = Color.FromRgb(106, 27, 154);
    private static readonly Color KnightColor   = Color.FromRgb(38, 50, 56);
    private static readonly Color MiniBossColor = Color.FromRgb(249, 115, 22);
    private static readonly Color BossColor     = Color.FromRgb(220, 38, 38);
    private static readonly Color InkColor      = Color.FromRgb(15, 23, 42);
    private static readonly Color ShineColor    = Color.FromArgb(130, 255, 255, 255);

    public static FrameworkElement CreateSpriteVisual(EnemyKind kind, double size) => CreateVisual(kind, size);

    public static FrameworkElement CreateIconVisual(EnemyKind kind, double size) => CreateVisual(kind, size);

    /// <summary>Compatibility helper for older image-source call sites.</summary>
    public static ImageSource CreateIcon(EnemyKind kind) => TryLoadSprite(kind) ?? Create(kind, includeBackground: false);

    /// <summary>Compatibility helper for older image-source call sites.</summary>
    public static ImageSource CreateSprite(EnemyKind kind) => TryLoadSprite(kind) ?? Create(kind, includeBackground: false);

    private static FrameworkElement CreateVisual(EnemyKind kind, double size)
    {
        var sprite = TryLoadSprite(kind);
        if (sprite != null)
        {
            return new Image
            {
                Width = size,
                Height = size,
                Source = sprite,
                Stretch = Stretch.Uniform,
                IsHitTestVisible = false
            };
        }

        return new EnemyCodeVisual(kind, size);
    }

    private static ImageSource? TryLoadSprite(EnemyKind kind)
    {
        lock (CacheLock)
        {
            if (SpriteCache.TryGetValue(kind, out var cached))
                return cached;
        }

        ImageSource? source = null;
        foreach (var path in CandidateSpritePaths(kind))
        {
            if (!File.Exists(path)) continue;

            try
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(path, UriKind.Absolute);
                image.EndInit();
                image.Freeze();
                source = image;
                break;
            }
            catch
            {
                source = null;
            }
        }

        source ??= TryLoadSpriteSheetSprite(kind);

        lock (CacheLock) { SpriteCache[kind] = source; }
        return source;
    }

    private static ImageSource? TryLoadSpriteSheetSprite(EnemyKind kind)
    {
        if (!SpriteSheetRegions.TryGetValue(kind, out var region))
            return null;

        foreach (var path in CandidateSpriteSheetPaths())
        {
            if (!File.Exists(path)) continue;

            try
            {
                var sheet = LoadSpriteSheet(path);
                var crop = new CroppedBitmap(sheet, region);
                crop.Freeze();
                return crop;
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    private static BitmapSource LoadSpriteSheet(string path)
    {
        lock (CacheLock)
        {
            if (SheetCache.TryGetValue(path, out var cached))
                return cached;
        }

        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(path, UriKind.Absolute);
        image.EndInit();
        image.Freeze();

        lock (CacheLock) { SheetCache[path] = image; }
        return image;
    }

    private static IEnumerable<string> CandidateSpritePaths(EnemyKind kind)
    {
        foreach (var root in CandidateAssetRoots())
        foreach (var extension in SpriteExtensions)
            yield return Path.Combine(root, $"{kind}{extension}");
    }

    private static IEnumerable<string> CandidateSpriteSheetPaths()
    {
        foreach (var root in CandidateAssetRoots())
            yield return Path.Combine(root, SpriteSheetFileName);
    }

    private static IEnumerable<string> CandidateAssetRoots()
    {
        yield return Path.Combine(AppContext.BaseDirectory, "Assets", "Enemies");
        yield return Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Enemies");
    }

    private static ImageSource Create(EnemyKind kind, bool includeBackground)
    {
        var cacheKey = includeBackground ? $"icon:{kind}" : $"sprite:{kind}";
        lock (CacheLock)
        {
            if (Cache.TryGetValue(cacheKey, out var cached))
                return cached;
        }

        var drawing = new DrawingGroup();
        using (var context = drawing.Open())
        {
            if (includeBackground) DrawBackground(context);
            DrawEnemy(context, kind);
        }

        drawing.Freeze();
        var image = new DrawingImage(drawing);
        image.Freeze();

        lock (CacheLock) { Cache[cacheKey] = image; }
        return image;
    }

    private static void DrawBackground(DrawingContext context)
    {
        context.DrawRoundedRectangle(
            Brush(Color.FromRgb(239, 246, 255)),
            Pen(Color.FromRgb(191, 219, 254), 2),
            new Rect(2, 2, 76, 76),
            10, 10);
    }

    private static void DrawEnemy(DrawingContext context, EnemyKind kind)
    {
        switch (kind)
        {
            case EnemyKind.Normal:           DrawNormal(context); return;
            case EnemyKind.Fast:             DrawFast(context); return;
            case EnemyKind.SplitBody:        DrawSplit(context, SplitColor, 17, 14, true); return;
            case EnemyKind.SplitSmall:       DrawSplit(context, SplitColor, 12, 10, false); return;
            case EnemyKind.Elite:            DrawHeavy(context); DrawAuraMark(context); DrawShieldMark(context); return;
            case EnemyKind.EliteCharge:      DrawHeavy(context); DrawChargeMark(context); return;
            case EnemyKind.EliteRegenerator: DrawMagic(context); DrawRegenMark(context); return;
            case EnemyKind.EliteWyvern:      DrawWyvern(context); return;
            case EnemyKind.MidBossNormal:    DrawMiniBoss(context, Color.FromRgb(216, 67, 21)); DrawCrownMark(context, Color.FromRgb(216, 67, 21)); return;
            case EnemyKind.MidBossCharge:    DrawMiniBoss(context, MiniBossColor); DrawChargeMark(context); return;
            case EnemyKind.MidBossSplit:     DrawSplit(context, MiniBossColor, 20, 17, true); DrawMiniBossBadge(context); return;
            case EnemyKind.MidBossSpeed:     DrawMiniBoss(context, Color.FromRgb(2, 132, 199)); DrawSpeedMark(context); return;
            case EnemyKind.BossNormal:       DrawBoss(context, Color.FromRgb(74, 20, 140)); DrawCrownMark(context, Color.FromRgb(74, 20, 140)); return;
            case EnemyKind.BossCharge:       DrawBoss(context, Color.FromRgb(180, 83, 9)); DrawChargeMark(context); return;
            case EnemyKind.BossSplit:        DrawSplit(context, BossColor, 25, 21, true); DrawBossBadge(context); return;
            case EnemyKind.BossSpeed:        DrawBoss(context, Color.FromRgb(3, 105, 161)); DrawSpeedMark(context); return;
            default:                         DrawNormal(context); return;
        }
    }

    private static void DrawNormal(DrawingContext context)
    {
        context.DrawEllipse(Brush(NormalColor), Pen(InkColor, 2), new Point(40, 43), 21, 18);
        context.DrawEllipse(Brush(ShineColor), null, new Point(33, 36), 5, 5);
        context.DrawEllipse(Brush(Color.FromRgb(22, 101, 52)), null, new Point(48, 45), 3, 3);
    }

    private static void DrawFast(DrawingContext context)
    {
        var body = Geometry.Parse("M 23 58 L 60 40 L 23 22 Z");
        context.DrawGeometry(Brush(FastColor), Pen(InkColor, 2), body);
        context.DrawLine(Pen(Color.FromRgb(2, 132, 199), 3), new Point(14, 30), new Point(31, 30));
        context.DrawLine(Pen(Color.FromRgb(2, 132, 199), 3), new Point(11, 43), new Point(28, 43));
        context.DrawEllipse(Brush(ShineColor), null, new Point(35, 38), 4, 4);
    }

    private static void DrawSplit(DrawingContext context, Color color, double leftRadius, double rightRadius, bool crack)
    {
        context.DrawEllipse(Brush(color), Pen(InkColor, 2), new Point(34, 43), leftRadius, leftRadius + 2);
        context.DrawEllipse(Brush(color), Pen(InkColor, 2), new Point(51, 40), rightRadius, rightRadius + 2);
        context.DrawEllipse(Brush(ShineColor), null, new Point(29, 35), 4, 4);

        if (crack)
        {
            context.DrawLine(Pen(Color.FromRgb(120, 53, 15), 2), new Point(40, 24), new Point(43, 38));
            context.DrawLine(Pen(Color.FromRgb(120, 53, 15), 2), new Point(43, 38), new Point(39, 56));
        }
    }

    private static void DrawHeavy(DrawingContext context)
    {
        context.DrawRoundedRectangle(Brush(HeavyColor), Pen(InkColor, 2), new Rect(22, 19, 37, 48), 13, 13);
        context.DrawRectangle(Brush(Color.FromArgb(120, 255, 255, 255)), null, new Rect(29, 25, 24, 7));
        context.DrawEllipse(Brush(ShineColor), null, new Point(35, 37), 4, 4);
    }

    private static void DrawFlying(DrawingContext context)
    {
        var wingLeft  = Geometry.Parse("M 39 39 L 12 24 L 24 48 Z");
        var wingRight = Geometry.Parse("M 42 39 L 68 24 L 56 48 Z");
        context.DrawGeometry(Brush(Color.FromRgb(203, 213, 225)), Pen(InkColor, 1.5), wingLeft);
        context.DrawGeometry(Brush(Color.FromRgb(203, 213, 225)), Pen(InkColor, 1.5), wingRight);
        context.DrawEllipse(Brush(FlyingColor), Pen(InkColor, 2), new Point(40, 42), 14, 20);
        context.DrawEllipse(Brush(ShineColor), null, new Point(35, 32), 4, 4);
    }

    private static void DrawMagic(DrawingContext context)
    {
        context.DrawEllipse(Brush(MagicColor), Pen(InkColor, 2), new Point(40, 43), 20, 21);
        context.DrawEllipse(Brush(ShineColor), null, new Point(33, 35), 5, 5);
    }

    private static void DrawKnight(DrawingContext context)
    {
        context.DrawRoundedRectangle(Brush(KnightColor), Pen(Color.FromRgb(226, 232, 240), 2), new Rect(23, 17, 34, 51), 10, 10);
        context.DrawLine(Pen(Color.FromRgb(148, 163, 184), 4), new Point(29, 32), new Point(51, 32));
        context.DrawLine(Pen(Color.FromRgb(148, 163, 184), 4), new Point(31, 47), new Point(49, 47));
    }

    private static void DrawWyvern(DrawingContext context)
    {
        var leftWing = Geometry.Parse("M 40 39 L 11 23 L 23 49 Z");
        var rightWing = Geometry.Parse("M 42 39 L 69 23 L 57 49 Z");
        var body = Geometry.Parse("M 25 47 C 30 27 46 22 58 36 C 49 42 43 53 29 60 Z");
        context.DrawGeometry(Brush(Color.FromRgb(109, 40, 217)), Pen(InkColor, 1.5), leftWing);
        context.DrawGeometry(Brush(Color.FromRgb(79, 70, 229)), Pen(InkColor, 1.5), rightWing);
        context.DrawGeometry(Brush(Color.FromRgb(124, 58, 237)), Pen(InkColor, 2), body);
        context.DrawEllipse(Brush(ShineColor), null, new Point(46, 34), 4, 4);
        context.DrawLine(Pen(Color.FromRgb(196, 181, 253), 3), new Point(25, 47), new Point(13, 58));
    }

    private static void DrawMiniBoss(DrawingContext context, Color color)
    {
        context.DrawEllipse(Brush(color), Pen(InkColor, 2), new Point(40, 43), 23, 21);
        context.DrawRectangle(Brush(Color.FromArgb(115, 255, 255, 255)), null, new Rect(27, 24, 26, 7));
        context.DrawEllipse(Brush(ShineColor), null, new Point(32, 35), 4, 4);
    }

    private static void DrawBoss(DrawingContext context, Color color)
    {
        context.DrawEllipse(Brush(color), Pen(InkColor, 2), new Point(40, 43), 28, 25);
        context.DrawRectangle(Brush(Color.FromArgb(110, 255, 255, 255)), null, new Rect(23, 20, 34, 8));
        context.DrawEllipse(Brush(Color.FromArgb(120, 255, 255, 255)), null, new Point(31, 34), 5, 5);
    }

    private static void DrawRegenMark(DrawingContext context)
    {
        context.DrawRectangle(Brush(Color.FromRgb(187, 247, 208)), null, new Rect(36, 28, 8, 27));
        context.DrawRectangle(Brush(Color.FromRgb(187, 247, 208)), null, new Rect(27, 37, 26, 8));
    }

    private static void DrawAuraMark(DrawingContext context)
    {
        context.DrawEllipse(null, Pen(Color.FromRgb(125, 211, 252), 2), new Point(40, 43), 29, 26);
    }

    private static void DrawShieldMark(DrawingContext context)
    {
        var shield = Geometry.Parse("M 40 19 L 54 25 L 51 43 C 48 51 43 56 40 58 C 37 56 32 51 29 43 L 26 25 Z");
        context.DrawGeometry(Brush(Color.FromArgb(85, 219, 234, 254)), Pen(Color.FromRgb(191, 219, 254), 1.5), shield);
    }

    private static void DrawChargeMark(DrawingContext context)
    {
        var bolt = Geometry.Parse("M 45 16 L 29 43 L 41 43 L 34 64 L 56 35 L 44 35 Z");
        context.DrawGeometry(Brush(Color.FromRgb(250, 204, 21)), Pen(Color.FromRgb(120, 53, 15), 1.4), bolt);
    }

    private static void DrawSpeedMark(DrawingContext context)
    {
        var pen = Pen(Color.FromRgb(186, 230, 253), 3);
        context.DrawLine(pen, new Point(14, 30), new Point(32, 30));
        context.DrawLine(pen, new Point(10, 43), new Point(31, 43));
        context.DrawLine(pen, new Point(16, 56), new Point(34, 56));
    }

    private static void DrawCrownMark(DrawingContext context, Color color)
    {
        var crown = Geometry.Parse("M 26 25 L 33 15 L 40 25 L 48 15 L 55 25 L 55 31 L 26 31 Z");
        context.DrawGeometry(Brush(Color.FromRgb(250, 204, 21)), Pen(Color.FromRgb(120, 53, 15), 1.5), crown);
        context.DrawEllipse(Brush(color), null, new Point(40, 29), 3, 3);
    }

    private static void DrawMiniBossBadge(DrawingContext context)
    {
        context.DrawRectangle(Brush(Color.FromRgb(254, 215, 170)), null, new Rect(31, 19, 18, 6));
    }

    private static void DrawBossBadge(DrawingContext context)
    {
        context.DrawRectangle(Brush(Color.FromRgb(254, 202, 202)), null, new Rect(27, 17, 26, 7));
    }

    private static Brush Brush(Color color) => new SolidColorBrush(color);

    private static Pen Pen(Color color, double thickness) => new(Brush(color), thickness);

    private sealed class EnemyCodeVisual : FrameworkElement
    {
        private const double DesignSize = 80;
        private readonly EnemyKind _kind;

        public EnemyCodeVisual(EnemyKind kind, double size)
        {
            _kind = kind;
            Width = size;
            Height = size;
            IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext context)
        {
            base.OnRender(context);

            double width = ActualWidth > 0 ? ActualWidth : Width;
            double height = ActualHeight > 0 ? ActualHeight : Height;
            double scale = Math.Min(width, height) / DesignSize;
            double offsetX = (width - DesignSize * scale) / 2;
            double offsetY = (height - DesignSize * scale) / 2;

            context.PushTransform(new TranslateTransform(offsetX, offsetY));
            context.PushTransform(new ScaleTransform(scale, scale));
            DrawEnemy(context, _kind);
            context.Pop();
            context.Pop();
        }
    }
}
