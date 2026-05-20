using System;
using System.Collections.Generic;
using KingdomRushClone.Managers;
using KingdomRushClone.Models;

namespace KingdomRushClone.Game;

public class TowerInstance
{
    public TowerDef    Def    = null!;
    public int         Level;
    public TowerBranch Branch = TowerBranch.None;
    public Vec2        Pos;
    public double      Cooldown;
    public List<Soldier> Soldiers = new();
    public Vec2        RallyPoint;
    public bool        RallyCustom;

    /// <summary>
    /// Targeting priority for this tower.
    /// Default: First (furthest along the path).
    /// </summary>
    public TargetMode TargetMode = TargetMode.First;

    // ─── Level resolution ────────────────────────────────────────────────
    public TowerLevel CurrentLevel
    {
        get
        {
            if (Branch == TowerBranch.A && Def.BranchA != null) return Def.BranchA.Levels[0];
            if (Branch == TowerBranch.B && Def.BranchB != null) return Def.BranchB.Levels[0];
            return Def.Levels[Level];
        }
    }

    // ─── Effective stats (after tech bonuses) ────────────────────────────
    public double EffectiveRange         => CurrentLevel.Range          * (1 + SaveManager.TechEffect(TechId.TowerRange));
    public double EffectiveDamage        => CurrentLevel.Damage         * (1 + SaveManager.TechEffect(TechId.TowerAttack));
    public double EffectiveAttackInterval => CurrentLevel.AttackInterval / (1 + SaveManager.TechEffect(TechId.TowerSpeed));

    // ─── Upgrade state ───────────────────────────────────────────────────
    public bool IsBarracks      => Def.Kind == TowerKind.Barracks;
    public bool IsBranched      => Branch != TowerBranch.None;
    public bool CanUpgrade      => !IsBranched && Level < Def.Levels.Count - 1;
    public bool CanChooseBranch => !IsBranched && Level == Def.Levels.Count - 1;

    public int UpgradeCost
    {
        get
        {
            if (CanChooseBranch) return 0;
            if (CanUpgrade)      return ApplyCostTech(Def.Levels[Level + 1].Cost);
            return 0;
        }
    }
    public int BranchACost => Def.BranchA != null ? ApplyCostTech(Def.BranchA.Levels[0].Cost) : 0;
    public int BranchBCost => Def.BranchB != null ? ApplyCostTech(Def.BranchB.Levels[0].Cost) : 0;

    private static int ApplyCostTech(int cost) =>
        (int)(cost * (1 - SaveManager.TechEffect(TechId.TowerCostReduction)));

    public int SellValue()
    {
        int total = Def.Levels[0].Cost;
        for (int i = 1; i <= Level && i < Def.Levels.Count; i++) total += Def.Levels[i].Cost;
        if (Branch == TowerBranch.A && Def.BranchA != null) total += Def.BranchA.Levels[0].Cost;
        if (Branch == TowerBranch.B && Def.BranchB != null) total += Def.BranchB.Levels[0].Cost;
        return (int)(total * 0.7);
    }

    // ─── Display ─────────────────────────────────────────────────────────
    public string DisplayName
    {
        get
        {
            if (Branch == TowerBranch.A && Def.BranchA != null) return Def.BranchA.Name;
            if (Branch == TowerBranch.B && Def.BranchB != null) return Def.BranchB.Name;
            return Def.Name + $" Lv{Level + 1}";
        }
    }

    /// <summary>Tile color (taking branch override into account). Used for the tower body sprite.</summary>
    public string CurrentColorHex
    {
        get
        {
            if (Branch == TowerBranch.A && Def.BranchA != null) return Def.BranchA.TowerColorHex;
            if (Branch == TowerBranch.B && Def.BranchB != null) return Def.BranchB.TowerColorHex;
            return Def.TowerColorHex;
        }
    }

    /// <summary>Projectile color (taking branch override into account). Used for the shot trail.</summary>
    public string CurrentProjectileColorHex
    {
        get
        {
            if (Branch == TowerBranch.A && Def.BranchA != null) return Def.BranchA.ProjectileColorHex;
            if (Branch == TowerBranch.B && Def.BranchB != null) return Def.BranchB.ProjectileColorHex;
            return Def.ProjectileColorHex;
        }
    }

    public string CurrentIcon
    {
        get
        {
            if (Branch == TowerBranch.A && Def.BranchA != null) return Def.BranchA.Icon;
            if (Branch == TowerBranch.B && Def.BranchB != null) return Def.BranchB.Icon;
            return Def.Icon;
        }
    }

    // ─── Tick ────────────────────────────────────────────────────────────
    public void Tick(double dt, GameEngine game)
    {
        Cooldown -= dt;

        if (IsBarracks)
        {
            EnsureSoldiers(game);
            foreach (var s in Soldiers) s.Tick(dt, game);
            return;
        }

        if (Cooldown > 0) return;

        var lvl  = CurrentLevel;
        var best = PickTarget(game);
        if (best == null) return;

        Cooldown = EffectiveAttackInterval;
        Fire(best, game, lvl);
    }

    // ─── Target picking (respects TargetMode) ────────────────────────────
    private EnemyInstance? PickTarget(GameEngine game)
    {
        EnemyInstance? best    = null;
        double         bestScore = double.MinValue;

        foreach (var e in game.Enemies)
        {
            if (!e.Alive) continue;
            if (Pos.DistanceTo(e.Pos) > EffectiveRange) continue;
            if (Def.Kind == TowerKind.Bombard && e.Def.IsFlying) continue;

            // "first" progress = how far along its path the enemy has traveled
            double progress = e.WaypointIndex
                + (e.WaypointIndex < e.Path.Count - 1
                    ? 1.0 - e.Pos.DistanceTo(e.Path[e.WaypointIndex + 1]) / 100.0
                    : 1.0);

            double score = TargetMode switch
            {
                TargetMode.First    => progress,
                TargetMode.Last     => -progress,
                TargetMode.Strongest => e.Hp,
                TargetMode.Weakest  => -e.Hp,
                TargetMode.Flying   => e.Def.IsFlying ? progress + 10_000 : progress,
                _                   => progress
            };

            if (score > bestScore) { bestScore = score; best = e; }
        }
        return best;
    }

    // ─── Fire ────────────────────────────────────────────────────────────
    private void Fire(EnemyInstance target, GameEngine game, TowerLevel lvl)
    {
        // Critical hit: Archer (base tower) 12 %, BranchA 25 %
        bool crit = false;
        double dmg = EffectiveDamage;
        if (Def.Kind == TowerKind.Archer && !IsBranched)
            crit = Random.Shared.NextDouble() < 0.12;
        else if (Def.Kind == TowerKind.Archer && Branch == TowerBranch.A)
            crit = Random.Shared.NextDouble() < 0.25;
        if (crit) dmg *= 2.2;

        var p = new Projectile
        {
            Pos          = Pos,
            StartPos     = Pos,          // needed for arc rendering
            Target       = target,
            Damage       = dmg,
            DamageType   = lvl.DamageType,
            SplashRadius = lvl.SplashRadius,
            SlowAmount   = lvl.SlowAmount,
            SlowDuration = lvl.SlowDuration,
            DotDamage    = lvl.DotDamage,
            DotDuration  = lvl.DotDuration,
            ColorHex     = CurrentProjectileColorHex,
            Speed        = Def.Kind == TowerKind.Bombard ? 280 : 520,
            IsCrit       = crit
        };

        if (Def.Kind == TowerKind.Bombard)
        {
            p.Ballistic   = true;
            p.TargetPos   = target.Pos;
            p.TravelTime  = Pos.DistanceTo(target.Pos) / p.Speed;
        }

        game.Projectiles.Add(p);

        // Archer BranchB: three simultaneous targets
        if (Branch == TowerBranch.B && Def.Kind == TowerKind.Archer)
        {
            int added = 0;
            foreach (var e in game.Enemies)
            {
                if (added >= 2) break;
                if (e == target || !e.Alive) continue;
                if (Pos.DistanceTo(e.Pos) > EffectiveRange) continue;

                game.Projectiles.Add(new Projectile
                {
                    Pos       = Pos, StartPos = Pos,
                    Target    = e,
                    Damage    = EffectiveDamage * 0.75,
                    DamageType = lvl.DamageType,
                    ColorHex  = CurrentProjectileColorHex,
                    Speed     = 520
                });
                added++;
            }
        }
    }

    // ─── Barracks soldier management ─────────────────────────────────────
    private void EnsureSoldiers(GameEngine game)
    {
        var lvl = CurrentLevel;
        while (Soldiers.Count < lvl.SoldierCount)
        {
            var rally = RallyCustom ? RallyPoint : NearestPathPoint(game);
            Soldiers.Add(new Soldier
            {
                Pos              = rally,
                RallyPos         = rally,
                Hp               = lvl.SoldierHp,
                MaxHp            = lvl.SoldierHp,
                Damage           = lvl.SoldierDamage,
                AttackInterval   = lvl.AttackInterval,
                RespawnDuration  = lvl.SoldierRespawn,
                Owner            = this,
                Alive            = true
            });
        }
        foreach (var s in Soldiers)
        {
            s.MaxHp            = lvl.SoldierHp;
            s.Damage           = lvl.SoldierDamage;
            s.AttackInterval   = lvl.AttackInterval;
            s.RespawnDuration  = lvl.SoldierRespawn;
            s.RallyPos         = RallyCustom ? RallyPoint : NearestPathPoint(game);
        }
    }

    private Vec2 NearestPathPoint(GameEngine game)
    {
        Vec2   best  = Pos;
        double bestD = double.MaxValue;
        foreach (var path in game.Stage.Paths)
            for (int i = 0; i < path.Count - 1; i++)
            {
                var    pt = ClosestOnSeg(Pos, path[i], path[i + 1]);
                double d  = pt.DistanceTo(Pos);
                if (d < bestD) { bestD = d; best = pt; }
            }
        return best;
    }

    private static Vec2 ClosestOnSeg(Vec2 p, Vec2 a, Vec2 b)
    {
        var    ab = b - a;
        var    ap = p - a;
        double t  = (ap.X * ab.X + ap.Y * ab.Y) / Math.Max(1e-6, ab.X * ab.X + ab.Y * ab.Y);
        t = Math.Max(0, Math.Min(1, t));
        return new Vec2(a.X + ab.X * t, a.Y + ab.Y * t);
    }
}
