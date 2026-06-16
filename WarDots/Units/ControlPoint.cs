using Microsoft.Xna.Framework;
using _2D_Engine_Sokov.GameObjects;
using System;
using System.Linq;

namespace _2D_Engine_Sokov.WarDots.Units
{
    public class ControlPoint : WarDotsBuilding
    {
        // Кто сейчас контролирует точку: 0 - нейтрально, 1 - игрок, 2 - враг
        public int Controller { get; private set; } = 0;

        // Скорость захвата
        public float CaptureThreshold { get; set; } = 100f;

        // Текущий прогресс захвата (-100 до 100)
        private float _captureProgress = 0f;

        // Сколько ресурсов дает в секунду
        public float ResourceRate { get; set; } = 5f;

        private float _resourceTimer = 0f;

        public ControlPoint()
        {
            Tag = "Neutral";
            Health = 100f;
            ProduceUnit = null;
            // Загрузи текстуру
            RenderSystem.EnqueueTextureLoad(this, "Content/Textures/control_point.png");
            Size = new Vector2(64, 64);
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            var tileMap = GameContext.TileMap;
            if (tileMap is BattleMap battleMap)
            {
                CellControlState currentControl = battleMap.GetControlStateAtPosition(this.Position);

                float delta = 0f;

                if (currentControl == CellControlState.Player)
                {
                    delta = 1f; // Игрок давит
                }
                else if (currentControl == CellControlState.Enemy)
                {
                    delta = -1f; // Враг давит
                }
                else
                {
                    delta = 0f;
                }

                // Обновляем прогресс
                UpdateCaptureProgress(delta * (float)deltaTime * 10f); // Множитель скорости
            }

            // Генерация ресурсов, если мы контролируем точку
            if (Controller != 0)
            {
                _resourceTimer += (float)deltaTime;
                if (_resourceTimer >= 1f)
                {
                    if (Controller == 1)
                    {
                        WarDotsPlayerController.PlayerResources += (int)ResourceRate;
                    }
                    else if (Controller == 2)
                    {
                        WarDotsEnemyAI.GlobalResources += (int)ResourceRate;
                    }
                    _resourceTimer -= 1f;
                }
            }
        }

        private void UpdateCaptureProgress(float delta)
        {
            _captureProgress += delta;

            // Ограничиваем прогресс
            if (_captureProgress > CaptureThreshold) _captureProgress = CaptureThreshold;
            if (_captureProgress < -CaptureThreshold) _captureProgress = -CaptureThreshold;

            // Смена владельца
            if (_captureProgress >= CaptureThreshold && Controller != 1)
            {
                SetController(1); // Игрок захватил
            }
            else if (_captureProgress <= -CaptureThreshold && Controller != 2)
            {
                SetController(2); // Враг захватил
            }
            // Можно добавить логику потери контроля, если прогресс упал до 0
            else if (Math.Abs(_captureProgress) < 10f && Controller != 0)
            {
                // Опционально: сброс в нейтралитет, если давление исчезло
                // SetController(0);
            }
        }

        public void SetController(int newController)
        {
            if (Controller != newController)
            {
                Controller = newController;
                if (Controller == 1)
                {
                    //Tag = "Player";
                    _captureProgress = CaptureThreshold; // Фиксируем захват
                }
                else if (Controller == 2)
                {
                    //Tag = "Enemy";
                    _captureProgress = -CaptureThreshold;
                }
                else
                {
                    Tag = "Neutral";
                    _captureProgress = 0;
                }

                // Визуальный эффект смены цвета
                this.Color = Controller == 1 ? Color.Red : (Controller == 2 ? Color.Green : Color.Gray);
            }
        }

        protected override bool HasResources(Type unitType) => true;
        protected override void SpendResources(Type unitType) { }
        protected override void RefundResources(Type unitType) { }
        protected override void ProduceUnits() { }
    }
}