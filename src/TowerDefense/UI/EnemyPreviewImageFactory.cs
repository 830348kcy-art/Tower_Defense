using System.Windows;
using System.Windows.Media;

namespace TowerDefense.UI;

public static class EnemyPreviewImageFactory
{
    private static readonly object CacheLock = new();
    private static readonly Dictionary<string, ImageSource> Cache = [];

    private static readonly Color NormalColor = Color.FromRgb(34, 197, 94);
    private static readonly Color FastColor = Color.FromRgb(14, 165, 233);
    private static readonly Color SplitColor = Color.FromRgb(132, 204, 22);
    private static readonly Color EliteColor = Color.FromRgb(147, 51, 234);
    private static readonly Color MiniBossColor = Color.FromRgb(234, 88, 12);
    private static readonly Color BossColor = Color.FromRgb(220, 38, 38);
    private static readonly Color InkColor = Color.FromRgb(15, 23, 42);
    private static readonly Color ShineColor = Color.FromArgb(130, 255, 255, 255);

    public static ImageSource Create(string enemyId)
    {
        return Create(enemyId, includeBackground: true);
    }

    public static ImageSource CreateSprite(string enemyId)
    {
        return Create(enemyId, includeBackground: false);
    }

    private static ImageSource Create(string enemyId, bool includeBackground)
    {
        lock (CacheLock)
        {
            if (Cache.TryGetValue(GetCacheKey(enemyId, includeBackground), out var cached))
            {
                return cached;
            }
        }

        var drawing = new DrawingGroup();

        using (var context = drawing.Open())
        {
            if (includeBackground)
            {
                DrawBackground(context);
            }

            DrawEnemy(context, enemyId);
        }

        drawing.Freeze();
        var image = new DrawingImage(drawing);
        image.Freeze();

        lock (CacheLock)
        {
            Cache[GetCacheKey(enemyId, includeBackground)] = image;
        }

        return image;
    }

    private static string GetCacheKey(string enemyId, bool includeBackground)
    {
        return includeBackground ? $"icon:{enemyId}" : $"sprite:{enemyId}";
    }

    private static void DrawBackground(DrawingContext context)
    {
        context.DrawRoundedRectangle(
            Brush(Color.FromRgb(239, 246, 255)),
            Pen(Color.FromRgb(191, 219, 254), 2),
            new Rect(2, 2, 76, 76),
            10,
            10);
    }

    private static void DrawEnemy(DrawingContext context, string enemyId)
    {
        switch (enemyId)
        {
            case "enemy_fast":
                DrawFast(context);
                return;
            case "enemy_split_body":
                DrawSplit(context, SplitColor, 17, 14, true);
                return;
            case "enemy_split_small":
                DrawSplit(context, SplitColor, 12, 10, false);
                return;
            case "elite_shield":
                DrawEliteBase(context);
                DrawShieldMark(context);
                return;
            case "elite_charge":
                DrawEliteBase(context);
                DrawChargeMark(context, EliteColor);
                return;
            case "elite_regen":
                DrawEliteBase(context);
                DrawRegenMark(context);
                return;
            case "elite_resist":
                DrawEliteBase(context);
                DrawResistMark(context);
                return;
            case "elite_ghost":
                DrawGhost(context);
                return;
            case "miniboss_normal":
                DrawMiniBoss(context);
                DrawCrownMark(context, MiniBossColor);
                return;
            case "miniboss_charge":
                DrawMiniBoss(context);
                DrawChargeMark(context, MiniBossColor);
                return;
            case "miniboss_split":
                DrawSplit(context, MiniBossColor, 20, 17, true);
                DrawMiniBossBadge(context);
                return;
            case "miniboss_speed":
                DrawMiniBoss(context);
                DrawSpeedMark(context, MiniBossColor);
                return;
            case "boss_normal":
                DrawBoss(context);
                DrawCrownMark(context, BossColor);
                return;
            case "boss_charge":
                DrawBoss(context);
                DrawChargeMark(context, BossColor);
                return;
            case "boss_split":
                DrawSplit(context, BossColor, 25, 21, true);
                DrawBossBadge(context);
                return;
            case "boss_speed":
                DrawBoss(context);
                DrawSpeedMark(context, BossColor);
                return;
            default:
                DrawNormal(context);
                return;
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

    private static void DrawEliteBase(DrawingContext context)
    {
        context.DrawRoundedRectangle(Brush(EliteColor), Pen(InkColor, 2), new Rect(24, 18, 32, 49), 13, 13);
        context.DrawEllipse(Brush(ShineColor), null, new Point(37, 30), 5, 5);
    }

    private static void DrawGhost(DrawingContext context)
    {
        var ghostBrush = Brush(Color.FromArgb(185, 168, 85, 247));
        var body = Geometry.Parse("M 24 64 L 24 35 C 24 23 32 17 40 17 C 49 17 57 23 57 35 L 57 64 L 50 58 L 43 64 L 36 58 L 29 64 Z");
        context.DrawGeometry(ghostBrush, Pen(InkColor, 2), body);
        context.DrawEllipse(Brush(Color.FromRgb(245, 243, 255)), null, new Point(34, 36), 4, 5);
        context.DrawEllipse(Brush(Color.FromRgb(245, 243, 255)), null, new Point(47, 36), 4, 5);
    }

    private static void DrawMiniBoss(DrawingContext context)
    {
        context.DrawEllipse(Brush(MiniBossColor), Pen(InkColor, 2), new Point(40, 43), 23, 21);
        context.DrawRectangle(Brush(Color.FromArgb(115, 255, 255, 255)), null, new Rect(27, 24, 26, 7));
        context.DrawEllipse(Brush(ShineColor), null, new Point(32, 35), 4, 4);
    }

    private static void DrawBoss(DrawingContext context)
    {
        context.DrawEllipse(Brush(BossColor), Pen(InkColor, 2), new Point(40, 43), 28, 25);
        context.DrawRectangle(Brush(Color.FromArgb(110, 255, 255, 255)), null, new Rect(23, 20, 34, 8));
        context.DrawEllipse(Brush(Color.FromArgb(120, 255, 255, 255)), null, new Point(31, 34), 5, 5);
    }

    private static void DrawShieldMark(DrawingContext context)
    {
        var shield = Geometry.Parse("M 40 25 L 52 30 L 49 48 C 47 56 40 60 40 60 C 40 60 33 56 31 48 L 28 30 Z");
        context.DrawGeometry(null, Pen(Color.FromRgb(216, 180, 254), 3), shield);
    }

    private static void DrawChargeMark(DrawingContext context, Color color)
    {
        var mark = Geometry.Parse("M 31 57 L 42 42 L 33 42 L 47 24 L 45 38 L 53 38 Z");
        context.DrawGeometry(Brush(Color.FromRgb(254, 240, 138)), Pen(Color.FromRgb(120, 53, 15), 1.5), mark);
        context.DrawLine(Pen(color, 4), new Point(15, 58), new Point(31, 51));
    }

    private static void DrawRegenMark(DrawingContext context)
    {
        context.DrawRectangle(Brush(Color.FromRgb(187, 247, 208)), null, new Rect(36, 28, 8, 27));
        context.DrawRectangle(Brush(Color.FromRgb(187, 247, 208)), null, new Rect(27, 37, 26, 8));
    }

    private static void DrawResistMark(DrawingContext context)
    {
        context.DrawLine(Pen(Color.FromRgb(226, 232, 240), 4), new Point(29, 34), new Point(51, 34));
        context.DrawLine(Pen(Color.FromRgb(226, 232, 240), 4), new Point(28, 45), new Point(52, 45));
        context.DrawLine(Pen(Color.FromRgb(226, 232, 240), 4), new Point(30, 56), new Point(50, 56));
    }

    private static void DrawCrownMark(DrawingContext context, Color color)
    {
        var crown = Geometry.Parse("M 26 25 L 33 15 L 40 25 L 48 15 L 55 25 L 55 31 L 26 31 Z");
        context.DrawGeometry(Brush(Color.FromRgb(250, 204, 21)), Pen(Color.FromRgb(120, 53, 15), 1.5), crown);
        context.DrawEllipse(Brush(color), null, new Point(40, 29), 3, 3);
    }

    private static void DrawSpeedMark(DrawingContext context, Color color)
    {
        DrawArc(context, Pen(Color.FromRgb(186, 230, 253), 3), new Point(23, 43), new Point(57, 43), new Size(17, 17), false);
        context.DrawLine(Pen(color, 4), new Point(16, 31), new Point(28, 31));
        context.DrawLine(Pen(color, 4), new Point(13, 53), new Point(28, 53));
    }

    private static void DrawMiniBossBadge(DrawingContext context)
    {
        context.DrawRectangle(Brush(Color.FromRgb(254, 215, 170)), null, new Rect(31, 19, 18, 6));
    }

    private static void DrawBossBadge(DrawingContext context)
    {
        context.DrawRectangle(Brush(Color.FromRgb(254, 202, 202)), null, new Rect(27, 17, 26, 7));
    }

    private static Brush Brush(Color color)
    {
        return new SolidColorBrush(color);
    }

    private static Pen Pen(Color color, double thickness)
    {
        return new Pen(Brush(color), thickness);
    }

    private static void DrawArc(DrawingContext context, Pen pen, Point start, Point end, Size size, bool clockwise)
    {
        var figure = new PathFigure
        {
            StartPoint = start,
            IsClosed = false,
            IsFilled = false
        };
        figure.Segments.Add(new ArcSegment(end, size, 0, false, clockwise ? SweepDirection.Clockwise : SweepDirection.Counterclockwise, true));
        var geometry = new PathGeometry([figure]);
        context.DrawGeometry(null, pen, geometry);
    }
}
