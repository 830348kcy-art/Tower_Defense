using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using KingdomRushClone.Models;

namespace KingdomRushClone.Views;

/// <summary>
/// Generates programmatic <see cref="ImageSource"/> sprites/icons for every enemy kind.
/// All images are cached on first use and frozen for cross-thread/render-thread reuse.
/// </summary>
public static class EnemyFallbackImageFactory
{
    private static readonly object CacheLock = new();
    private static readonly Dictionary<string, ImageSource> Cache = new();

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

    /// <summary>Icon with light card background — used for stage intro / dex panels.</summary>
    public static ImageSource CreateIcon(EnemyKind kind)   => Create(kind, includeBackground: true);

    /// <summary>Transparent sprite — used on the game canvas as the enemy body.</summary>
    public static ImageSource CreateSprite(EnemyKind kind) => Create(kind, includeBackground: false);

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
            case EnemyKind.EliteGhost:       DrawGhost(context); return;
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

    private static void DrawGhost(DrawingContext context)
    {
        var body = Geometry.Parse("M 22 62 C 24 29 30 18 40 18 C 51 18 57 29 58 62 L 50 56 L 44 62 L 38 56 L 31 62 L 26 56 Z");
        context.DrawGeometry(Brush(Color.FromRgb(167, 139, 250)), Pen(InkColor, 2), body);
        context.DrawEllipse(Brush(Color.FromArgb(120, 255, 255, 255)), null, new Point(34, 32), 5, 5);
        context.DrawEllipse(Brush(Color.FromRgb(76, 29, 149)), null, new Point(48, 42), 3, 3);
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
}
