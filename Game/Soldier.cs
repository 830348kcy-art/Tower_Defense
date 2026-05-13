using KingdomRushClone.Models;

namespace KingdomRushClone.Game;

public class Soldier
{
    public Vec2 Pos;
    public Vec2 RallyPos;
    public double Hp;
    public double MaxHp;
    public double Damage;
    public double AttackInterval;
    public double AttackCooldown;
    public double Speed = 60;
    public double EngageRadius = 18;
    public bool Alive = true;
    public double RespawnTimer;
    public double RespawnDuration;
    public EnemyInstance? Target;
    public TowerInstance Owner = null!;

    public void Tick(double dt, GameEngine game)
    {
        if (!Alive)
        {
            RespawnTimer -= dt;
            if (RespawnTimer <= 0)
            {
                Hp = MaxHp;
                Pos = RallyPos;
                Alive = true;
            }
            return;
        }

        if (Target != null && (!Target.Alive || Target.EngagedBy != this))
            Target = null;

        if (Target == null)
        {
            foreach (var e in game.Enemies)
            {
                if (!e.Alive || e.Def.IsFlying || e.EngagedBy != null) continue;
                if (Pos.DistanceTo(e.Pos) < EngageRadius + 25)
                {
                    Target = e;
                    e.EngagedBy = this;
                    e.EngageTimer = 0.5;
                    break;
                }
            }
        }

        if (Target != null)
        {
            AttackCooldown -= dt;
            if (AttackCooldown <= 0)
            {
                Target.ApplyDamage(Damage, DamageType.Physical);
                AttackCooldown = AttackInterval;
                if (!Target.Alive) { Target.EngagedBy = null; Target = null; game.OnEnemyKilled(); }
            }
        }
        else
        {
            var dir = (RallyPos - Pos);
            if (dir.Length > 4)
            {
                var n = dir.Normalized();
                Pos = Pos + n * Speed * dt;
            }
        }
    }

    public void Kill()
    {
        Alive = false;
        RespawnTimer = RespawnDuration;
    }
}
