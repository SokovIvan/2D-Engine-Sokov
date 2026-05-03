using Microsoft.Xna.Framework;
using _2D_Engine_Sokov.GameObjects;
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

            //DrawHeightDependent();
        }

        private void DrawHeightDependent()
        {
            // Если "высота" выше порога видимости, не рисуем
            if (Altitude >= AltitudeHidden) return;

            Color mainColor = Tag == "Player" ? Color.CornflowerBlue : Color.Crimson;
            if (!IsActive) mainColor = Color.Gray;

            // RenderSystem использует ConcurrentQueue, поэтому вызов из потока логики безопасен
            if (Altitude <= AltitudeFullDetail)
            {
                RenderSystem.SubmitPersistentCommand(() => RenderSystem.FillCircle(Position, Radius, mainColor, 32), framesToLive: 5);
                RenderSystem.SubmitPersistentCommand(() => RenderSystem.DrawCircle(Position, Radius, Color.Black, 32, 2f), framesToLive: 5);
                // Полная детализация: заполненный круг + чёрная обводка

            }
            else
            {
                // Упрощённая отрисовка: только обводка, радиус плавно уменьшается
                float t = (Altitude - AltitudeFullDetail) / (AltitudeHidden - AltitudeFullDetail);
                float simplifiedRadius = MathHelper.Lerp(Radius, Radius * 0.5f, t);
                RenderSystem.DrawCircle(Position, simplifiedRadius, mainColor, 16, 1.5f);
            }
        }
    }
}