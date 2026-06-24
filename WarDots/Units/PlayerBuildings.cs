using _2D_Engine_Sokov.GameObjects;
using Microsoft.Xna.Framework;
using System;

namespace _2D_Engine_Sokov.WarDots.Units
{
    public class WarDotsPlayerInfantryFactory : WarDotsPlayerFactory
    {
        public WarDotsPlayerInfantryFactory()
        {
            Tag = "Player";
            Health = 280f; ProducingTime = 5f; MaxQueueSize = 6; UnitProductionCost = 10;
			SpawnTarget = new Vector2(Position.X + Size.X, Position.Y + Size.Y);
			RenderSystem.EnqueueTextureLoad(this, "Content/Textures/plbuild_infantry.png");
            Size = new Vector2(64, 64);
        }
    }

    public class WarDotsPlayerTankFactory : WarDotsPlayerFactory
    {
        public WarDotsPlayerTankFactory()
        {
            Tag = "Player";
            Health = 450f; ProducingTime = 9f; MaxQueueSize = 3; UnitProductionCost = 20;
			SpawnTarget = new Vector2(Position.X + Size.X, Position.Y + Size.Y);
			RenderSystem.EnqueueTextureLoad(this, "Content/Textures/plbuild_tank.png");
            Size = new Vector2(64, 64);
        }
    }

    public class WarDotsPlayerArtilleryFactory : WarDotsPlayerFactory
    {
        public WarDotsPlayerArtilleryFactory()
        {
            Tag = "Player";
            Health = 320f; ProducingTime = 12f; MaxQueueSize = 2; UnitProductionCost = 30;
			SpawnTarget = new Vector2(Position.X + Size.X, Position.Y + Size.Y);
			RenderSystem.EnqueueTextureLoad(this, "Content/Textures/plbuild_artillery.png");
            Size = new Vector2(64, 64);
        }
    }
}