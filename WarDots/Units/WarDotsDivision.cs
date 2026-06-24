using Microsoft.Xna.Framework;
using _2D_Engine_Sokov.GameObjects;
using _2D_Engine_Sokov.MapGeneration;
using System;
using System.Collections.Generic;

namespace _2D_Engine_Sokov.WarDots.Units
{
    /// <summary>
    /// Базовый класс для дивизий, которые рисуются примитивами (кругами).
    /// Детализация отрисовки зависит от параметра Altitude.
    /// </summary>
    public abstract class WarDotsDivision : Unit
    {
        public float Radius { get; set; } = 12f;

        /// <summary>
        /// Условная высота над картой (или коэффициент зума камеры).
        /// 0 = близко, большое значение = далеко/скрыто.
        /// </summary>
        public float Altitude { get; set; } = 0f;

        public float AltitudeFullDetail { get; set; } = 50f;  // Полная отрисовка
        public float AltitudeHidden { get; set; } = 150f; // Скрытие

        public override void Start()
        {
            base.Start();
            // Отключаем стандартную текстуру, чтобы RenderSystem не пытался её рисовать
            //Texture = null;
        }

        public override void Update(double deltaTime)
        {

            base.Update(deltaTime);
            CheckCellStatePosition();

           // DrawHeightDependent();
        }
        public void CheckCellStatePosition() {
            if (GameContext.TileMap != null && GameContext.TileMap is BattleMap battleMap)
            {
                MapGroundStates cellState = battleMap.getGroundStateFromWorldPosition(Position);
                MoveSpeed = NormalMoveSpeed;
                AttackRange = NormalAttackRange;
                AttackCooldown = NormalAttackCooldown;
                AttackDamage = NormalAttackDamage;
                switch (cellState)
                {
                    case MapGroundStates.toxic:
                        Health -= 1;
                        break;
                    case MapGroundStates.lava:
                        Health -= 1;
                        break;
                    case MapGroundStates.water:
                        MoveSpeed = NormalMoveSpeed / 4;
                        break;
                    case MapGroundStates.forest:
                        AttackRange = NormalAttackRange / 2;
                        break;
                    case MapGroundStates.stone:
                        AttackCooldown = NormalAttackCooldown * 2;
                        break;
                    case MapGroundStates.xeno:
                        AttackDamage = NormalAttackDamage * 2;
                        break;
                    case MapGroundStates.resource:
                        AttackDamage = NormalAttackDamage * 2;
                        break;
                }
            }
        }

    }
}