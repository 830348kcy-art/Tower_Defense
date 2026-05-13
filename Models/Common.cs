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
        var l = Length;
        return l < 1e-9 ? new Vec2(0, 0) : new Vec2(X / l, Y / l);
    }
    public double DistanceTo(Vec2 o) => (this - o).Length;
}

public enum DamageType { Physical, Magic, Explosive, True }

public enum ArmorType { None, Light, Heavy, Magical, Flying }

public enum TowerKind { Archer, Mage, Bombard, Barracks }

public enum TowerBranch { None, A, B }

public enum EnemyKind
{
    GoblinSoldier,
    GoblinScout,
    OrcWarrior,
    Wyvern,
    TrollShaman,
    DarkKnight,
    MidBoss,
    Boss
}

public enum GameSpeed { Paused = 0, Normal = 1, Fast = 2 }

public enum TileKind { Ground, Path, Buildable, Obstacle, Spawn, Base }
