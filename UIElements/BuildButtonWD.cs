using System;
using Microsoft.Xna.Framework;
using _2D_Engine_Sokov.WarDots;
using _2D_Engine_Sokov.WarDots.Units;

namespace _2D_Engine_Sokov.UIElements
{
    public class BuildButtonWD : Button
    {
        // Тип здания, которое мы хотим построить
        public Type BuildingType { get; set; }

        // Стоимость постройки (для проверки ресурсов)
        public int Cost { get; set; }

        public BuildButtonWD()
        {
            // Значения по умолчанию, чтобы ничего не сломалось
            BuildingType = typeof(WarDotsPlayerFactory);
            Cost = 100;
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            // Обновляем действие при клике
            OnClick = () =>
            {
                // Проверяем, хватает ли ресурсов у игрока
                if (WarDotsPlayerController.PlayerResources >= Cost)
                {
                    Console.WriteLine($"[UI] Запрошено строительство: {BuildingType.Name}");

                    // Передаем запрос в контроллер игрока
                    WarDotsPlayerController.RequestPlacement(BuildingType);
                }
                else
                {
                    Console.WriteLine("[UI] Недостаточно ресурсов!");
                    // Здесь можно добавить визуальный эффект ошибки, если хочешь, Иван-кун...
                }
            };
        }
    }
}