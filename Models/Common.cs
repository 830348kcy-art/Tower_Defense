using System;

namespace KingdomRushClone.Models;

public readonly struct Vec2(double x, double y)
{
    public readonly double X = x;
    public readonly double Y = y;

    public static Vec2 operator +(Vec2 a, Vec2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vec2 operator -(Vec2 a, Vec2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vec2 operator *(Vec2 a, double s) => new(a.X * s, a.Y * s);

    public double Length => Math.Sqrt(X * X + Y * Y);

    public Vec2 Normalized()
    {
        double l = Length;
        return l < 1e-9 ? new Vec2(0, 0) : new Vec2(X / l, Y / l);
    }

    public double DistanceTo(Vec2 o) => (this - o).Length;

    /// <summary>Linearly interpolates between two Vec2 positions.</summary>
    public static Vec2 Lerp(Vec2 a, Vec2 b, double t) => a + (b - a) * t;

    public override string ToString() => $"({X:F1}, {Y:F1})";
}

public enum DamageType  { Physical, Magic, Explosive, True }
public enum ArmorType   { None, Light, Heavy, Magical, Flying, slowImmune }
public enum TowerKind   { Archer, Mage, Bombard, Barracks, Slow }
public enum TowerBranch { None, A, B }

/// <summary>
/// Controls which enemy a tower prefers to target.
/// First=가장 앞 적, Last=가장 뒤 적, Strongest=최고 체력, Weakest=최저 체력, Flying=비행 우선.
/// </summary>
public enum TargetMode  { First, Last, Strongest, Weakest, Flying }

public enum EnemyKind
{
    GoblinSoldier,
    GoblinScout,
    OrcWarrior,
    Wyvern,
    TrollShaman,
    DarkKnight,
    MidBoss,
    Boss,
    SplitBody,
    SplitSmall,
    SplitMidBoss,
    SplitBoss
}

public enum GameSpeed { Paused = 0, Normal = 1, Fast = 2 }
public enum TileKind  { Ground, Path, Buildable, Obstacle, Spawn, Base }
