using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Engine_Sokov.WarDots.Units
{
    public class WarDotsPlayerBase : WarDotsBuilding
    {
        public bool IsDestroyed { get; private set; }

        public WarDotsPlayerBase() {
            Tag = "Player";
            Health = 650f;
            ProduceUnit = null;
            string texturePath = "Content/Textures/plbuild.png";
            RenderSystem.EnqueueTextureLoad(this, texturePath);
        }

        public override void Start()
        {
            base.Start();

        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            if (Health <= 0 && !IsDestroyed)
            {
                IsDestroyed = true;
                TriggerDefeat();
            }
        }

        private void TriggerDefeat()
        {
            Console.WriteLine("[GAME] БАЗА ИГРОКА УНИЧТОЖЕНА. ПОРАЖЕНИЕ.");
            LogicSystem.Pause();
            PhysicsSystem.Pause();
            // Здесь можно вызвать WarDotsGame.ShowDefeatScreen();
        }

        protected override bool HasResources(Type unitType) => true;
        protected override void SpendResources(Type unitType) { }
        protected override void RefundResources(Type unitType) { }
        protected override void ProduceUnits() { }
    }
}
