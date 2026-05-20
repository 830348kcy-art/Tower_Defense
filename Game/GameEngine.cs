using System;
using System.Collections.Generic;
using KingdomRushClone.Data;
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

    /// <summary>
    /// Accumulated this tick. GamePage drains this each frame to spawn floating damage numbers.
    /// </summary>
    public record struct DamageEvent(Vec2 Pos, double Amount, DamageType Type, bool IsCrit);
    public readonly List<DamageEvent> DamageEvents = new();

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
        ApplyEnemySpeedBonuses();
        foreach (var e in Enemies)
        {
            e.Tick(dt);
            TickEnemySupport(e, dt);
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
                SpawnDeathSpawns(e);
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

        // Clear damage events after one render cycle (drained by GamePage)
        DamageEvents.Clear();
    }

    /// <summary>
    /// Centralized factory for runtime enemies — applies stage HP/speed scaling and tech reductions.
    /// Does NOT add the enemy to the Enemies list; the caller is responsible for that.
    /// </summary>
    public EnemyInstance CreateEnemy(EnemyDef def, Vec2 pos, List<Vec2> path, int waypointIndex)
    {
        double hpScale    = Stage.EnemyHpScale    * (1 - SaveManager.TechEffect(TechId.EnemyHpReduction));
        double speedScale = Stage.EnemySpeedScale * (1 - SaveManager.TechEffect(TechId.EnemySpeedReduction));
        double maxHp      = def.MaxHp * hpScale;

        return new EnemyInstance
        {
            Def           = def,
            Pos           = pos,
            Path          = path,
            WaypointIndex = waypointIndex,
            MaxHp         = maxHp,
            Hp            = maxHp,
            Speed         = def.Speed * speedScale,
            Alive         = true,
            PathIndex     = Stage.Paths.IndexOf(path),
            ShieldCharges = def.ShieldCharges,
            RegenerateTimer = def.RegenerateInterval,
            GhostTimer    = def.GhostCycle
        };
    }

    /// <summary>
    /// If the dying enemy has DeathSpawns, create children at its position on the same path.
    /// Children inherit the parent's waypoint index so they continue from where the parent fell.
    /// </summary>
    private void SpawnDeathSpawns(EnemyInstance parent)
    {
        if (parent.Def.DeathSpawns.Count == 0) return;

        double center = (parent.Def.DeathSpawns.Count - 1) / 2.0;
        for (int i = 0; i < parent.Def.DeathSpawns.Count; i++)
        {
            var childDef = EnemyCatalog.Enemies[parent.Def.DeathSpawns[i]];
            var offset   = new Vec2((i - center) * Math.Max(12, childDef.Radius), 0);
            var child    = CreateEnemy(childDef, parent.Pos + offset, parent.Path, parent.WaypointIndex);
            child.PathIndex = parent.PathIndex;
            Enemies.Add(child);
        }
    }

    private void ApplyEnemySpeedBonuses()
    {
        foreach (var enemy in Enemies)
            enemy.ExternalSpeedBonus = 0;

        foreach (var source in Enemies)
        {
            if (!source.Alive) continue;

            if (source.Def.GlobalSpeedBonus > 0)
            {
                foreach (var target in Enemies)
                    if (target != source && target.Alive)
                        target.ExternalSpeedBonus += source.Def.GlobalSpeedBonus;
            }

            if (source.Def.AuraSpeedBonus > 0 && source.Def.AuraRadius > 0)
            {
                foreach (var target in Enemies)
                {
                    if (target == source || !target.Alive) continue;
                    if (source.Pos.DistanceTo(target.Pos) <= source.Def.AuraRadius)
                        target.ExternalSpeedBonus += source.Def.AuraSpeedBonus;
                }
            }
        }
    }

    private void TickEnemySupport(EnemyInstance e, double dt)
    {
        if (!e.Alive) return;

        if (e.Def.RegenerateInterval > 0)
        {
            e.RegenerateTimer -= dt;
            if (e.RegenerateTimer <= 0)
            {
                e.RegenerateTimer = e.Def.RegenerateInterval;
                Heal(e, e.MaxHp * e.Def.RegenerateSelfPercent);
                foreach (var ally in Enemies)
                {
                    if (ally == e || !ally.Alive) continue;
                    if (e.Pos.DistanceTo(ally.Pos) <= e.Def.RegenerateRadius)
                        Heal(ally, ally.MaxHp * e.Def.RegenerateAllyPercent);
                }
            }
            return;
        }

        if (e.Def.IsHealer)
        {
            e.HealTimer -= dt;
            if (e.HealTimer <= 0)
            {
                e.HealTimer = e.Def.HealInterval;
                foreach (var ally in Enemies)
                    if (ally != e && ally.Alive && e.Pos.DistanceTo(ally.Pos) < e.Def.HealRadius)
                        Heal(ally, e.Def.HealAmount);
            }
        }
    }

    private static void Heal(EnemyInstance enemy, double amount)
    {
        if (amount <= 0) return;
        enemy.Hp = Math.Min(enemy.MaxHp, enemy.Hp + amount);
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
