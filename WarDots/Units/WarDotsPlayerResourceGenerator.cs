using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Engine_Sokov.WarDots.Units
{
    public class WarDotsPlayerResourceGenerator : WarDotsBuilding
    {
        public float GenerationRate { get; set; } = 12f;
        private float _genTimer;
        public WarDotsPlayerResourceGenerator() {
            Tag = "Player";
            Health = 180f;
            ProduceUnit = null;
            string texturePath = "Content/Textures/plbuild_resgen.png";
            this.LoadTexture(texturePath);
        }
        public override void Start()
        {
            base.Start();

        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            _genTimer += (float)deltaTime;
            if (_genTimer >= 1f)
            {
                WarDotsPlayerController.PlayerResources += (int)GenerationRate;
                _genTimer -= 1f;
            }
        }

        protected override bool HasResources(Type unitType) => true;
        protected override void SpendResources(Type unitType) { }
        protected override void RefundResources(Type unitType) { }
        protected override void ProduceUnits() { }
    }
}
