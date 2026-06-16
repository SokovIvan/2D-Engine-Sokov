using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Engine_Sokov.WarDots.Units
{
    public class WarDotsEnemyBase : WarDotsBuilding
    {
        public bool IsDefeated { get; private set; }

        public WarDotsEnemyBase()
        {
            Tag = "Enemy";
            Health = 600f;
            ProduceUnit = null;
            string texturePath = "Content/Textures/enbuild.png";
            RenderSystem.EnqueueTextureLoad(this, texturePath);
        }

        public override void Start()
        {
            base.Start();
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            if (Health <= 0 && !IsDefeated)
            {
                IsDefeated = true;
                TriggerDefeatSequence();
            }
        }

        private void TriggerDefeatSequence()
        {
            Console.WriteLine("[AI] БАЗА ПРОТИВНИКА РАЗРУШЕНА. ПОБЕДА.");
            LogicSystem.Pause();
            PhysicsSystem.Pause();
            // Здесь можно добавить загрузку экрана победы через Game.instance.LoadLevel("Victory.xml");
        }

        protected override bool HasResources(Type unitType) => true;
        protected override void SpendResources(Type unitType) { }
        protected override void RefundResources(Type unitType) { }
        protected override void ProduceUnits() { }
    }
}
