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

        // Временное сообщение, показываемое после клика
        private double _infoTimer = 0.0;
        private bool _showingInfo = false;
        private string _originalText = string.Empty;
        private const double InfoDurationSeconds = 5.0;
		private double _clickTimer = 0.0;
		private const double ClickDurationSeconds = 0.5;

		public BuildButtonWD()
        {
            // Значения по умолчанию, чтобы ничего не сломалось
            BuildingType = typeof(WarDotsPlayerFactory);
            Cost = 100;
            // Устанавливаем действие при клике один раз в конструкторе
            OnClick = () =>
            {
                // Проверяем, хватает ли ресурсов у игрока
                if (WarDotsPlayerController.PlayerResources >= Cost)
                {
                    Console.WriteLine($"[UI] Запрошено строительство: {BuildingType.Name}");

                    // Показать информационное сообщение на короткое время
                    if(!_showingInfo)
                        _originalText = this.Text;
                    this.Text = $"Запрошено строительство";
                    _showingInfo = true;
                    _infoTimer = InfoDurationSeconds;
                    WarDotsPlayerController.ClickOnButton = true;
					_clickTimer = ClickDurationSeconds;
					WarDotsPlayerController.RequestPlacement(BuildingType);
                }
                else
                {
                    Console.WriteLine("[UI] Недостаточно ресурсов!");
					if (!_showingInfo)
						_originalText = this.Text;
                    this.Text = "Недостаточно ресурсов!";
                    _showingInfo = true;
                    _infoTimer = InfoDurationSeconds;
                }
            };
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            if (_clickTimer > 0) {
				_clickTimer -= deltaTime;
				if (_clickTimer <= 0) WarDotsPlayerController.ClickOnButton = false;
			}


			// Обрабатываем таймер для информационного сообщения
			if (_showingInfo)
            {

				_infoTimer -= deltaTime;
                if (_infoTimer <= 0)
                {
                    // Восстанавливаем исходный текст
                    this.Text = _originalText;
                    _showingInfo = false;
                    _infoTimer = 0.0;
                }
            }
        }
    }
}