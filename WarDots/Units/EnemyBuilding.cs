using _2D_Engine_Sokov.GameObjects;
using Microsoft.Xna.Framework;
using System;

namespace _2D_Engine_Sokov.WarDots.Units
{
    public class WarDotsEnemyInfantryFactory : WarDotsEnemyFactory
    {
        public WarDotsEnemyInfantryFactory()
        {
            Tag = "Enemy";
            Health = 250f; ProducingTime = 5.5f; MaxQueueSize = 6; UnitProductionCost = 10;

            RenderSystem.EnqueueTextureLoad(this, "Content/Textures/enbuild_infantry.png");
            Size = new Vector2(64, 64);
        }
    }

    public class WarDotsEnemyTankFactory : WarDotsEnemyFactory
    {
        public WarDotsEnemyTankFactory()
        {
            Tag = "Enemy";
            Health = 400f; ProducingTime = 8.5f; MaxQueueSize = 3; UnitProductionCost = 20;

            RenderSystem.EnqueueTextureLoad(this, "Content/Textures/enbuild_tank.png");
            Size = new Vector2(64, 64);
        }
    }

    public class WarDotsEnemyArtilleryFactory : WarDotsEnemyFactory
    {
        public WarDotsEnemyArtilleryFactory()
        {
            Tag = "Enemy";
            Health = 300f; ProducingTime = 11f; MaxQueueSize = 2; UnitProductionCost = 30;

            RenderSystem.EnqueueTextureLoad(this, "Content/Textures/enbuild_artillery.png");
            Size = new Vector2(64, 64);
        }
    }
}