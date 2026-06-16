using _2D_Engine_Sokov.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Engine_Sokov.WarDots.Units
{
    public class WarDotsEnemyFactory : WarDotsBuilding
    {
        public int FactoryCost { get; set; } = 100;
        public int UnitProductionCost { get; set; } = 25;
        public WarDotsEnemyFactory()
        {
            Tag = "Enemy";
            Health = 300f;
            ProducingTime = 7f;
            MaxQueueSize = 4;
        }
        public override void Start()
        {
            base.Start();
        }

        protected override bool HasResources(Type unitType)
            => WarDotsEnemyAI.GlobalResources >= UnitProductionCost;
        protected override void SpendResources(Type unitType)
            => WarDotsEnemyAI.GlobalResources -= UnitProductionCost;
        protected override void RefundResources(Type unitType)
            => WarDotsEnemyAI.GlobalResources += UnitProductionCost;

        protected override void OnBeforeUnitSpawned(Unit unit)
        {
            if (unit is WarDotsDivision div) {
                div.Tag = "Enemy";
                div.Size = new Microsoft.Xna.Framework.Vector2(32, 32);
            }

        }
    }
}
