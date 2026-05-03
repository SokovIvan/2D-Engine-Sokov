using Microsoft.Xna.Framework;
using _2D_Engine_Sokov.GameObjects;
using System;

namespace _2D_Engine_Sokov.WarDots.Units
{
    /// Абстрактная база для построек WarDots. Наследует Building, добавляет очередь производства и пассивный доход.
    /// </summary>
    /// <summary>
    /// Базовый класс для построек, производящих дивизии строго по очереди и только по команде.
    /// </summary>
    public abstract class WarDotsBuilding : Building
    {
        private readonly Queue<Type> _productionQueue = new();
        public int MaxQueueSize { get; set; } = 4;
        public int QueueLength => _productionQueue.Count;
        public bool IsProducing => _productionQueue.Count > 0;

        // Абстрактные хуки для работы с ресурсами (реализуются в конкретных постройках)
        protected abstract bool HasResources(Type unitType);
        protected abstract void SpendResources(Type unitType);
        protected abstract void RefundResources(Type unitType);

        /// <summary>
        /// Поставить юнита в очередь производства
        /// </summary>
        public bool EnqueueProduction(Type unitType)
        {
            if (!unitType.IsSubclassOf(typeof(WarDotsDivision))) return false;
            if (_productionQueue.Count >= MaxQueueSize) return false;
            if (!HasResources(unitType)) return false;

            SpendResources(unitType);
            _productionQueue.Enqueue(unitType);

            // Если очередь была пуста, сбрасываем таймер, чтобы начать производство немедленно
            if (_productionQueue.Count == 1)
                ProduceTimer = 0f;

            return true;
        }

        /// <summary>
        /// Отменить всю очередь с возвратом ресурсов
        /// </summary>
        public void CancelQueue()
        {
            foreach (var type in _productionQueue)
                RefundResources(type);

            _productionQueue.Clear();
            ProduceTimer = 0f;
        }

        /// <summary>
        /// Переопределяем производство: теперь работаем с очередью, а не с полем ProduceUnit
        /// </summary>
        protected override void ProduceUnits()
        {
            if (_productionQueue.Count == 0)
            {
                ProduceTimer = 0f;
                return;
            }

            var currentType = _productionQueue.Peek();

            // Ждём, пока таймер достигнет времени производства
            if (ProduceTimer >= ProducingTime)
            {
                var unit = (Unit)Activator.CreateInstance(currentType);
                if (unit != null)
                {
                    unit.Size = Size;
                    unit.CollisionEnabled = CollisionEnabled;
                    unit.Position = Position + ProduceOffset;
                    unit.Tag = Tag; // Наследуем фракцию (Player/Enemy)

                    OnBeforeUnitSpawned(unit);
                    WarDotsGame.SubmitObject(unit);
                }

                _productionQueue.Dequeue();
                ProduceTimer = 0f; // Сброс для следующего юнита в очереди
            }
        }

        /// <summary>
        /// Хук для дополнительной настройки юнита перед спавном (например, HP, опыт, команда ИИ)
        /// </summary>
        protected virtual void OnBeforeUnitSpawned(Unit unit) { }
    }
}