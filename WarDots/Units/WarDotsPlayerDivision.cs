using Microsoft.Xna.Framework;

namespace _2D_Engine_Sokov.WarDots.Units
{
    public class WarDotsPlayerDivision : WarDotsDivision
    {
        public bool selected = false;
		public List<Vector2> points = new();
		public WarDotsPlayerDivision()
        {
            Tag = "Player";
            Health = 160f;
            AttackDamage = 1f;
            AttackRange = 85f;
            DetectionRange = 260f;
            MoveSpeed = 80f;
            Radius = 11f;
            AltitudeFullDetail = 45f;
            AltitudeHidden = 130f;
            selected = false;

		}

        public override void Start()
        {
            base.Start();
        }
        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);   
            if(selected)
				DrawOnSelection();
            if (Path.Count == 0 && points.Count>0)
            {
				PathTask = Pathfinding.FindPathAsync(GameContext.TileMap, Position, points.First());
				Path.Clear();
				points.RemoveAt(0);
			}
        }
        private void DrawOnSelection() { 
        
            RenderSystem.SubmitPersistentCommand(() =>
			{
				if (Path.Count > 1)
				{
				    RenderSystem.DrawLine(Position, Path.Last(), Color.Yellow, 2f);

				}

				if (points.Count > 1)
				{
					if (Path.Count > 1)
						RenderSystem.DrawLine(Path.Last(), points[0], Color.Yellow, 2f);
					for (int i = 0; i < points.Count - 1; i++)
					{
						RenderSystem.DrawLine(points[i], points[i + 1], Color.Yellow, 2f);
					}
				}
			}, 3, useCamera: true);
		}
    }
}
