using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
namespace _2D_Engine_Sokov.GameObjects
{
    public abstract class Unit : Sprite
    {
        public float Health { get; set; } = 100f;
        public float AttackDamage { get; set; } = 10f;
        public float AttackRange { get; set; } = 100f;
        public float DetectionRange { get; set; } = 200f;
        public float MoveSpeed { get; set; } = 100f;
        public List<Vector2> Path { get; set; } = new List<Vector2>();
        public Unit Target { get; set; }
        protected float AttackCooldown { get; set; } = 1f;
        protected float CooldownTimer { get; set; }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            CooldownTimer -= (float)deltaTime;

            if (Target != null && Vector2.Distance(Position, Target.Position) <= AttackRange)
            {
                Attack(Target);
            }
            else if (Path.Count > 0)
            {
                MoveAlongPath((float)deltaTime);
            }

            DetectUnits();
        }

        protected void DetectUnits()
        {
            var units = LogicSystem.FindGameObjectsByTag(this is PlayerUnit ? "Enemy" : "Player");
            Unit closest = null;
            float closestDistance = float.MaxValue;

            foreach (var unit in units)
            {
                if (!unit.IsActive) continue;
                float distance = Vector2.Distance(Position, unit.Position);
                if (distance <= DetectionRange && distance < closestDistance)
                {
                    closest = unit as Unit;
                    closestDistance = distance;
                }
            }

            Target = closest;
            if (Target != null && this is EnemyUnit)
            {
                Path = Pathfinding.FindPath(Game.instance._currentLevel.TileMap, Position, Target.Position);
            }
        }

        protected void Attack(Unit target)
        {
            if (CooldownTimer <= 0)
            {
                target.Health -= AttackDamage;
                CooldownTimer = AttackCooldown;
                if (target.Health <= 0)
                {
                    target.IsActive = false;
                    LogicSystem.RemoveGameObject(target);
                }
            }
        }

        protected void MoveAlongPath(float deltaTime)
        {
            if (Path.Count == 0) return;

            var targetPos = Path[0];
            var direction = Vector2.Normalize(targetPos - Position);
            var distance = Vector2.Distance(Position, targetPos);

            if (distance < MoveSpeed * deltaTime)
            {
                Position = targetPos;
                Path.RemoveAt(0);
            }
            else
            {
                Position += direction * MoveSpeed * deltaTime;
            }
        }
    }
}
