using System;
using System.Collections.Generic;
using KingdomRushClone.Managers;
using KingdomRushClone.Models;

namespace KingdomRushClone.Game;

public enum GameResult { Playing, Won, Lost }

public class HitEffect
{
    public Vec2 Pos;
    public double Radius;
    public double TimeLeft;
    public double TotalTime;
    public string ColorHex = "#FFFFFF";
}

public class GameEngine
{
    public StageDef Stage { get; }
    public List<EnemyInstance> Enemies { get; } = new();
    public List<TowerInstance> Towers { get; } = new();
    public List<Projectile> Projectiles { get; } = new();
    public List<HitEffect> Effects { get; } = new();
    public WaveSpawner Spawner { get; }
    public int Gold;
    public int Lives;
    public int LivesMax;
    public GameResult Result = GameResult.Playing;
    public GameSpeed Speed = GameSpeed.Normal;

    public double MeteorCooldown;
    public double MeteorCooldownMax = 30;
    public double ReinforcementCooldown;
    public double ReinforcementCooldownMax = 25;
    public double ReinforcementDuration = 15;
    public List<Soldier> Reinforcements { get; } = new();
    public double ReinforcementTimer;

    public event Action? StateChanged;

    public GameEngine(StageDef stage)
    {
        Stage = stage;
        Gold = stage.StartingGold;
        Lives = stage.StartingLives;
        LivesMax = stage.StartingLives;
        Spawner = new WaveSpawner(stage, this);
        MeteorCooldownMax = Math.Max(5, 30 - SaveManager.TechEffect(TechId.MeteorCooldown));
        ReinforcementDuration = 15 + SaveManager.TechEffect(TechId.ReinforcementDuration);
    }

    public void Tick(double dt)
    {
        if (Result != GameResult.Playing) return;
        if (Speed == GameSpeed.Paused) return;
        double scale = Speed == GameSpeed.Fast ? 2.0 : 1.0;
        dt *= scale;

        Spawner.Tick(dt);
        if (MeteorCooldown > 0) MeteorCooldown -= dt;
        if (ReinforcementCooldown > 0) ReinforcementCooldown -= dt;

        foreach (var t in Towers) t.Tick(dt, this);
        foreach (var e in Enemies)
        {
            e.Tick(dt);
            if (e.Def.IsHealer && e.Alive)
            {
                e.HealTimer -= dt;
                if (e.HealTimer <= 0)
                {
                    e.HealTimer = e.Def.HealInterval;
                    foreach (var ally in Enemies)
                        if (ally != e && ally.Alive && e.Pos.DistanceTo(ally.Pos) < e.Def.HealRadius)
                            ally.Hp = Math.Min(ally.MaxHp, ally.Hp + e.Def.HealAmount);
                }
            }
        }

        if (ReinforcementTimer > 0)
        {
            ReinforcementTimer -= dt;
            foreach (var s in Reinforcements) s.Tick(dt, this);
            if (ReinforcementTimer <= 0) Reinforcements.Clear();
        }

        foreach (var p in Projectiles) p.Tick(dt, this);
        foreach (var fx in Effects) fx.TimeLeft -= dt;

        for (int i = Enemies.Count - 1; i >= 0; i--)
        {
            var e = Enemies[i];
            if (e.ReachedBase)
            {
                Lives -= e.Def.LivesCost;
                if (e.EngagedBy != null) e.EngagedBy = null;
                Enemies.RemoveAt(i);
            }
            else if (!e.Alive)
            {
                Gold += e.Def.GoldReward + (int)SaveManager.TechEffect(TechId.KillGoldBonus);
                if (e.EngagedBy != null) e.EngagedBy = null;
                Enemies.RemoveAt(i);
            }
        }
        Projectiles.RemoveAll(p => !p.Alive);
        Effects.RemoveAll(e => e.TimeLeft <= 0);

        if (Lives <= 0) { Lives = 0; Result = GameResult.Lost; }
        else if (Spawner.AllWavesStarted && !Spawner.HasPendingSpawns && Enemies.Count == 0)
        {
            Result = GameResult.Won;
            int waveBonus = Stage.Waves.Count * (int)SaveManager.TechEffect(TechId.WaveGoldBonus);
            Gold += waveBonus;
        }

        StateChanged?.Invoke();
    }

    public bool TryBuild(Vec2 slot, TowerKind kind)
    {
        var def = Data.TowerCatalog.Towers[kind];
        int cost = (int)(def.Levels[0].Cost * (1 - SaveManager.TechEffect(TechId.TowerCostReduction)));
        if (Gold < cost) return false;
        foreach (var t in Towers) if (t.Pos.DistanceTo(slot) < 4) return false;
        Gold -= cost;
        var inst = new TowerInstance { Def = def, Pos = slot, Level = 0 };
        Towers.Add(inst);
        return true;
    }

    public bool TryUpgrade(TowerInstance t)
    {
        if (!t.CanUpgrade) return false;
        int cost = t.UpgradeCost;
        if (Gold < cost) return false;
        Gold -= cost;
        t.Level++;
        return true;
    }

    public bool TryBranch(TowerInstance t, TowerBranch branch)
    {
        if (!t.CanChooseBranch) return false;
        int cost = branch == TowerBranch.A ? t.BranchACost : t.BranchBCost;
        if (Gold < cost) return false;
        Gold -= cost;
        t.Branch = branch;
        return true;
    }

    public int Sell(TowerInstance t)
    {
        int v = t.SellValue();
        Gold += v;
        Towers.Remove(t);
        return v;
    }

    public int CallNextWaveEarly()
    {
        int bonus = Spawner.CallNextWaveEarly();
        Gold += bonus;
        return bonus;
    }

    public bool CastMeteor(Vec2 point)
    {
        if (MeteorCooldown > 0) return false;
        MeteorCooldown = MeteorCooldownMax;
        const double radius = 80;
        const double damage = 250;
        foreach (var e in Enemies)
        {
            if (!e.Alive) continue;
            if (point.DistanceTo(e.Pos) <= radius)
            {
                e.ApplyDamage(damage, DamageType.Explosive);
                if (!e.Alive) Gold += e.Def.GoldReward + (int)SaveManager.TechEffect(TechId.KillGoldBonus);
            }
        }
        SpawnHitEffect(point, radius, "#FF6F00");
        return true;
    }

    public bool CastReinforcements(Vec2 point)
    {
        if (ReinforcementCooldown > 0) return false;
        ReinforcementCooldown = ReinforcementCooldownMax;
        ReinforcementTimer = ReinforcementDuration;
        Reinforcements.Clear();
        for (int i = 0; i < 2; i++)
        {
            var offset = new Vec2(i * 24 - 12, 0);
            var rally = new Vec2(point.X + offset.X, point.Y + offset.Y);
            Reinforcements.Add(new Soldier
            {
                Pos = rally, RallyPos = rally,
                Hp = 80, MaxHp = 80, Damage = 8, AttackInterval = 0.9,
                RespawnDuration = 999, Alive = true,
                Owner = null!
            });
        }
        return true;
    }

    public void OnEnemyKilled() { }
    public void OnEnemyKilled(EnemyInstance _) { }

    public void SpawnHitEffect(Vec2 pos, double radius, string color)
    {
        Effects.Add(new HitEffect { Pos = pos, Radius = radius, TimeLeft = 0.4, TotalTime = 0.4, ColorHex = color });
    }

    public int ComputeStars()
    {
        if (Result != GameResult.Won) return 0;
        double ratio = (double)Lives / LivesMax;
        if (ratio >= 0.999) return 3;
        if (ratio >= 0.7) return 2;
        return 1;
    }
}
