using System.Collections.Generic;
using KingdomRushClone.Managers;
using KingdomRushClone.Models;

namespace KingdomRushClone.Game;

public class TowerInstance
{
    public TowerDef Def = null!;
    public int Level;
    public TowerBranch Branch = TowerBranch.None;
    public Vec2 Pos;
    public double Cooldown;
    public List<Soldier> Soldiers = new();
    public Vec2 RallyPoint;
    public bool RallyCustom;

    public TowerLevel CurrentLevel
    {
        get
        {
            if (Branch == TowerBranch.A && Def.BranchA != null) return Def.BranchA.Levels[0];
            if (Branch == TowerBranch.B && Def.BranchB != null) return Def.BranchB.Levels[0];
            return Def.Levels[Level];
        }
    }

    public double EffectiveRange => CurrentLevel.Range * (1 + SaveManager.TechEffect(TechId.TowerRange));
    public double EffectiveDamage => CurrentLevel.Damage * (1 + SaveManager.TechEffect(TechId.TowerAttack));
    public double EffectiveAttackInterval => CurrentLevel.AttackInterval / (1 + SaveManager.TechEffect(TechId.TowerSpeed));

    public bool IsBarracks => Def.Kind == TowerKind.Barracks;
    public bool IsBranched => Branch != TowerBranch.None;
    public bool CanUpgrade => !IsBranched && Level < Def.Levels.Count - 1;
    public bool CanChooseBranch => !IsBranched && Level == Def.Levels.Count - 1;

    public int UpgradeCost
    {
        get
        {
            if (CanChooseBranch) return 0;
            if (CanUpgrade) return ApplyCostTech(Def.Levels[Level + 1].Cost);
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

    public string DisplayName
    {
        get
        {
            if (Branch == TowerBranch.A && Def.BranchA != null) return Def.BranchA.Name;
            if (Branch == TowerBranch.B && Def.BranchB != null) return Def.BranchB.Name;
            return Def.Name + (Level > 0 ? $" Lv{Level + 1}" : "");
        }
    }

    public string CurrentColorHex
    {
        get
        {
            if (Branch == TowerBranch.A && Def.BranchA != null) return Def.BranchA.ColorHex;
            if (Branch == TowerBranch.B && Def.BranchB != null) return Def.BranchB.ColorHex;
            return Def.ColorHex;
        }
    }

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

        var lvl = CurrentLevel;
        EnemyInstance? best = null;
        double bestProgress = -1;
        foreach (var e in game.Enemies)
        {
            if (!e.Alive) continue;
            if (Pos.DistanceTo(e.Pos) > EffectiveRange) continue;
            if (Def.Kind == TowerKind.Bombard && e.Def.IsFlying) continue;
            double progress = e.WaypointIndex + (e.WaypointIndex < e.Path.Count - 1 ? 1 - e.Pos.DistanceTo(e.Path[e.WaypointIndex + 1]) / 100 : 1);
            if (progress > bestProgress)
            {
                bestProgress = progress;
                best = e;
            }
        }
        if (best == null) return;

        Cooldown = EffectiveAttackInterval;
        Fire(best, game, lvl);
    }

    private void Fire(EnemyInstance target, GameEngine game, TowerLevel lvl)
    {
        var p = new Projectile
        {
            Pos = Pos,
            Target = target,
            Damage = EffectiveDamage,
            DamageType = lvl.DamageType,
            SplashRadius = lvl.SplashRadius,
            SlowAmount = lvl.SlowAmount,
            SlowDuration = lvl.SlowDuration,
            DotDamage = lvl.DotDamage,
            DotDuration = lvl.DotDuration,
            ColorHex = CurrentColorHex,
            Speed = Def.Kind == TowerKind.Bombard ? 280 : 500
        };

        if (Def.Kind == TowerKind.Bombard)
        {
            p.Ballistic = true;
            p.TargetPos = target.Pos;
            p.TravelTime = Pos.DistanceTo(target.Pos) / p.Speed;
        }
        game.Projectiles.Add(p);

        if (Branch == TowerBranch.B && Def.Kind == TowerKind.Archer)
        {
            int extra = 2;
            int added = 0;
            foreach (var e in game.Enemies)
            {
                if (added >= extra) break;
                if (e == target || !e.Alive) continue;
                if (Pos.DistanceTo(e.Pos) > EffectiveRange) continue;
                var p2 = new Projectile
                {
                    Pos = Pos, Target = e,
                    Damage = EffectiveDamage * 0.7, DamageType = lvl.DamageType,
                    ColorHex = CurrentColorHex, Speed = 500
                };
                game.Projectiles.Add(p2);
                added++;
            }
        }
    }

    private void EnsureSoldiers(GameEngine game)
    {
        var lvl = CurrentLevel;
        while (Soldiers.Count < lvl.SoldierCount)
        {
            var rally = RallyCustom ? RallyPoint : NearestPathPoint(game);
            Soldiers.Add(new Soldier
            {
                Pos = rally,
                RallyPos = rally,
                Hp = lvl.SoldierHp,
                MaxHp = lvl.SoldierHp,
                Damage = lvl.SoldierDamage,
                AttackInterval = lvl.AttackInterval,
                RespawnDuration = lvl.SoldierRespawn,
                Owner = this,
                Alive = true
            });
        }
        foreach (var s in Soldiers)
        {
            s.MaxHp = lvl.SoldierHp;
            s.Damage = lvl.SoldierDamage;
            s.AttackInterval = lvl.AttackInterval;
            s.RespawnDuration = lvl.SoldierRespawn;
            s.RallyPos = RallyCustom ? RallyPoint : NearestPathPoint(game);
        }
    }

    private Vec2 NearestPathPoint(GameEngine game)
    {
        Vec2 best = Pos;
        double bestD = double.MaxValue;
        foreach (var path in game.Stage.Paths)
        {
            for (int i = 0; i < path.Count - 1; i++)
            {
                var p = ClosestOnSeg(Pos, path[i], path[i + 1]);
                double d = p.DistanceTo(Pos);
                if (d < bestD) { bestD = d; best = p; }
            }
        }
        return best;
    }

    private static Vec2 ClosestOnSeg(Vec2 p, Vec2 a, Vec2 b)
    {
        var ab = b - a;
        var ap = p - a;
        double t = (ap.X * ab.X + ap.Y * ab.Y) / System.Math.Max(1e-6, ab.X * ab.X + ab.Y * ab.Y);
        t = System.Math.Max(0, System.Math.Min(1, t));
        return new Vec2(a.X + ab.X * t, a.Y + ab.Y * t);
    }
}
