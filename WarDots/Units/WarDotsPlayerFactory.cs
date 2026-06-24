using _2D_Engine_Sokov.GameObjects;
using Microsoft.Xna.Framework;

namespace _2D_Engine_Sokov.WarDots.Units
{
    public class WarDotsPlayerFactory : WarDotsBuilding
    {
        public int UnitProductionCost { get; set; } = 25;
        public Vector2 SpawnTarget { get; set; } = Vector2.One;
		public WarDotsPlayerFactory() {
            Tag = "Player";
            Health = 320f;
            ProducingTime = 6.5f;
            MaxQueueSize = 4;
			SpawnTarget = new Vector2(Position.X+Size.X, Position.Y+Size.Y);
		}
        public override void Start()
        {
            base.Start();

        }
        protected override void OnBeforeUnitSpawned(Unit unit)
        {
            if (unit is WarDotsDivision div)
            {
                div.Tag = "Player";
                div.Size = new Microsoft.Xna.Framework.Vector2(32, 32);
				div.PathTask = Pathfinding.FindPathAsync(GameContext.TileMap, div.Position, SpawnTarget);
			}

        }

        protected override bool HasResources(Type unitType)
            => WarDotsPlayerController.PlayerResources >= UnitProductionCost;
        protected override void SpendResources(Type unitType)
            => WarDotsPlayerController.PlayerResources -= UnitProductionCost;
        protected override void RefundResources(Type unitType)
            => WarDotsPlayerController.PlayerResources += UnitProductionCost;
    }
}
