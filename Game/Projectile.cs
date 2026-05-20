using KingdomRushClone.Models;

namespace KingdomRushClone.Game;

public class Projectile
{
    public Vec2 Pos;
    public Vec2 Velocity;
    public EnemyInstance? Target;
    public Vec2 TargetPos;
    public double Damage;
    public DamageType DamageType;
    public double SplashRadius;
    public double SlowAmount;
    public double SlowDuration;
    public double DotDamage;
    public double DotDuration;
    public bool Alive = true;
    public bool Ballistic;
    public double TravelTime;
    public double Elapsed;
    public double Speed = 400;
    public string ColorHex = "#FFF59D";

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
                return;
            }
            return;
        }

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
            Pos = Pos + Velocity * dt;
            if (Elapsed > 1.5) Alive = false;
        }
    }

    private void Detonate(GameEngine game, Vec2 point)
    {
        if (SplashRadius > 0)
        {
            // 원본 리스트를 복사하여 순회함으로써 적 제거 시 발생하는 Collection Modified 오류 방지
            foreach (var e in game.Enemies.ToArray())
            {
                if (!e.Alive) continue;
                if (point.DistanceTo(e.Pos) <= SplashRadius)
                {
                    bool isFlying = e.Def.IsFlying;
                    if (DamageType == DamageType.Explosive && isFlying) continue;
                    e.ApplyDamage(Damage, DamageType);
                    if (SlowAmount > 0) e.ApplySlow(SlowAmount, SlowDuration);
                    if (DotDamage > 0) e.ApplyDot(DotDamage, DotDuration);
                    if (!e.Alive) game.OnEnemyKilled(e);
                }
            }
        }
        else if (Target != null)
        {
            if (Target.Alive)
            {
                Target.ApplyDamage(Damage, DamageType);
                if (SlowAmount > 0) Target.ApplySlow(SlowAmount, SlowDuration);
                if (DotDamage > 0) Target.ApplyDot(DotDamage, DotDuration);
                if (!Target.Alive) game.OnEnemyKilled(Target);
            }
        }
        game.SpawnHitEffect(point, SplashRadius > 0 ? SplashRadius : 8, ColorHex);
    }
}
