using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Engine_Sokov.WarDots.Units
{
    public class WarDotsEnemyResourceGenerator : WarDotsBuilding
    {
        public float GenerationRate { get; set; } = 1f; // Ресурсов в секунду
        private float _generationTimer;

        public WarDotsEnemyResourceGenerator(){
            Tag = "Enemy";
            Health = 150f;
            ProduceUnit = null; // Отключаем производство юнитов
            string texturePath = "Content/Textures/enbuild_resgen.png";
            RenderSystem.EnqueueTextureLoad(this, texturePath);
            Size = new Vector2(64, 64);
        }
        public override void Start()
        {
            base.Start();

        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            _generationTimer += (float)deltaTime;

            if (_generationTimer >= 1.0f)
            {
                WarDotsEnemyAI.GlobalResources += (int)GenerationRate;
                _generationTimer -= 1.0f;
            }
        }

        // Хуки ресурсов не нужны, генератор сам пополняет общий пул
        protected override bool HasResources(Type unitType) => true;
        protected override void SpendResources(Type unitType) { }
        protected override void RefundResources(Type unitType) { }
        protected override void ProduceUnits() { } // Переопределяем, чтобы не тратить таймер
    }
}
