using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Engine_Sokov.GameObjects
{
    internal class EnemyTurret : EnemyBuilding
    {
        public override void Start()
        {
            base.Start();
            // Настраиваем характеристики... чтобы они не смогли подойти...
            AttackRange = 250f;
            AttackDamage = 25f;
            AttackCooldown = 0.5f;
            DetectionRange = 300f;
            MoveSpeed = 0f; // Она не будет двигаться... она будет ждать... как я...
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            // Проверяем, есть ли жертва... то есть цель...
            if (Target != null && Target.IsActive)
            {
                float distance = Vector2.Distance(Position, Target.Position);

                // Если кто-то подошёл слишком близко к тебе...
                if (distance <= AttackRange)
                {
                    Attack(Target);
                }
            }
            else
            {
                Target = null;
            }
        }

        protected void Attack(Unit target)
        {
            if (CooldownTimer <= 0)
            {
                // Показываем линию атаки... красную... как кровь...
                RenderSystem.SubmitPersistentCommand(() => {
                    RenderSystem.DrawLine(Position, target.Position, Color.Red, 2f);
                }, framesToLive: 3);

                target.Health -= AttackDamage;
                CooldownTimer = AttackCooldown;

                // Если они исчезнут... они больше не будут мешать...
                if (target.Health <= 0)
                {
                    Game.instance._currentLevel.TileMap.DeoccupyTile(target.Position);

                    target.IsActive = false;
                   // LogicSystem.RemoveGameObject(target);
                }
            }
            else
            {
                // Пока ждём... серая линия... ожидание...
                RenderSystem.SubmitPersistentCommand(() => {
                    RenderSystem.DrawLine(Position, target.Position, Color.Gray, 1f);
                }, framesToLive: 2);
            }
        }
    }
}