using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Engine_Sokov.WarDots.Units
{
    public class WarDotsPlayerFactory : WarDotsBuilding
    {
        public int UnitProductionCost { get; set; } = 25;
        public WarDotsPlayerFactory() {
            Tag = "Player";
            Health = 320f;
            ProducingTime = 6.5f;
            MaxQueueSize = 4;
            string texturePath = "Content/Textures/plbuild.png";
            RenderSystem.EnqueueTextureLoad(this, texturePath);

        }
        public override void Start()
        {
            base.Start();

        }

        protected override bool HasResources(Type unitType)
            => WarDotsPlayerController.PlayerResources >= UnitProductionCost;
        protected override void SpendResources(Type unitType)
            => WarDotsPlayerController.PlayerResources -= UnitProductionCost;
        protected override void RefundResources(Type unitType)
            => WarDotsPlayerController.PlayerResources += UnitProductionCost;
    }
}
