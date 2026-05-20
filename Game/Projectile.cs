using System;
using KingdomRushClone.Models;

namespace KingdomRushClone.Game;

public class Projectile
{
    // ─── Position / movement ────────────────────────────────────────────
    public Vec2 Pos;
    public Vec2 Velocity;

    // ─── Target ─────────────────────────────────────────────────────────
    public EnemyInstance? Target;

    // ─── Ballistic (arc) data ────────────────────────────────────────────
    /// <summary>Set at creation time so the arc can be rendered correctly.</summary>
    public Vec2 StartPos;
    public Vec2 TargetPos;
    public bool Ballistic;
    public double TravelTime;
    public double Elapsed;

    // ─── Damage payload ─────────────────────────────────────────────────
    public double Damage;
    public DamageType DamageType;
    public double SplashRadius;
    public double SlowAmount;
    public double SlowDuration;
    public double DotDamage;
    public double DotDuration;
    /// <summary>True when a critical hit was rolled; causes larger damage number popup.</summary>
    public bool IsCrit;

    // ─── Visual ─────────────────────────────────────────────────────────
    public string ColorHex = "#FFF59D";
    public double Speed = 400;

    // ─── Lifecycle ──────────────────────────────────────────────────────
    public bool Alive = true;

    // ─── Render position (parabolic arc for ballistic) ──────────────────
    /// <summary>
    /// Returns the visual position of this projectile this frame.
    /// Ballistic projectiles arc upward; homing projectiles return their true Pos.
    /// </summary>
    public Vec2 GetRenderPos()
    {
        if (!Ballistic) return Pos;
        double t = TravelTime > 1e-6 ? Math.Clamp(Elapsed / TravelTime, 0, 1) : 1.0;
        var linear = Vec2.Lerp(StartPos, TargetPos, t);
        double arcH = StartPos.DistanceTo(TargetPos) * 0.28;
        double arc = Math.Sin(Math.PI * t) * arcH;
        return new Vec2(linear.X, linear.Y - arc);
    }

    // ─── Tick ────────────────────────────────────────────────────────────
    public void Tick(double dt, GameEngine game)
    {
        if (!Alive) return;
        Elapsed += dt;

        if (Ballistic)
        {
            if (Elapsed >= TravelTime)
            {
                Detonate(game, TargetPos);
                Alive = false;
            }
            return;
        }

        // Homing
        if (Target != null && Target.Alive)
        {
            var dir = (Target.Pos - Pos).Normalized();
            Velocity = dir * Speed; // 타겟 추적 중 속도 벡터 갱신
            Pos = Pos + Velocity * dt;
            if (Pos.DistanceTo(Target.Pos) < 8)
            {
                Detonate(game, Target.Pos);
                Alive = false;
            }
        }
        else
        {
            // Target died in flight – keep flying forward until timeout
            Pos = Pos + Velocity * dt;
            if (Elapsed > 1.5) Alive = false;
        }
    }

    // ─── Detonate ────────────────────────────────────────────────────────
    private void Detonate(GameEngine game, Vec2 point)
    {
        if (SplashRadius > 0)
        {
            foreach (var e in game.Enemies.ToArray())
            {
                if (!e.Alive) continue;
                if (point.DistanceTo(e.Pos) > SplashRadius) continue;
                if (DamageType == DamageType.Explosive && e.Def.IsFlying) continue;

                e.ApplyDamage(Damage, DamageType);
                if (SlowAmount > 0) e.ApplySlow(SlowAmount, SlowDuration);
                if (DotDamage > 0) e.ApplyDot(DotDamage, DotDuration);

                game.DamageEvents.Add(new(e.Pos, Damage, DamageType, false));
                if (!e.Alive) game.OnEnemyKilled(e);
            }
        }
        else if (Target != null && Target.Alive)
        {
            Target.ApplyDamage(Damage, DamageType);
            if (SlowAmount > 0) Target.ApplySlow(SlowAmount, SlowDuration);
            if (DotDamage > 0) Target.ApplyDot(DotDamage, DotDuration);

            game.DamageEvents.Add(new(Target.Pos, Damage, DamageType, IsCrit));
            if (!Target.Alive) game.OnEnemyKilled(Target);
        }

        game.SpawnHitEffect(point, SplashRadius > 0 ? SplashRadius : 8, ColorHex);
    }
}
