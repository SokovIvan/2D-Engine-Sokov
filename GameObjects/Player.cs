
using Microsoft.Xna.Framework.Input;
namespace _2D_Engine_Sokov.GameObjects
{
    internal class Player: Sprite
    {
        public override void Update(double deltaTime)
        {            
            HandleInput();
            
        }
        private void HandleInput()
        {
            if (Game.keyboardState.IsKeyDown(Keys.Space))
            {
                this.Velocity += new Microsoft.Xna.Framework.Vector2(0, -300);
            }
            if (Game.keyboardState.IsKeyDown(Keys.S))
            {
                this.Velocity += new Microsoft.Xna.Framework.Vector2(0, 300);
            }
            if (Game.keyboardState.IsKeyDown(Keys.A))
            {
                this.Velocity += new Microsoft.Xna.Framework.Vector2(100, 0);
            }
            if (Game.keyboardState.IsKeyDown(Keys.D))
            {
                this.Velocity += new Microsoft.Xna.Framework.Vector2(-100, 0);
            }
            if (Game.keyboardState.IsKeyDown(Keys.Up))
            {
                var camera = RenderSystem.GetCamera();
                //camera.Zoom = 1.5f; // Увеличение
                camera.Move(new Microsoft.Xna.Framework.Vector2(0, 1)); // Движение вправо          
            }
            if (Game.keyboardState.IsKeyDown(Keys.Down))
            {
                var camera = RenderSystem.GetCamera();
                //camera.Zoom = 1f; // Увеличение
                camera.Move(new Microsoft.Xna.Framework.Vector2(0, -1)); // Движение вправо      

            }
            if (Game.keyboardState.IsKeyDown(Keys.Right))
            {
                var camera = RenderSystem.GetCamera();
                camera.Move(new Microsoft.Xna.Framework.Vector2(1, 0)); // Движение вправо                                                
            }
            if (Game.keyboardState.IsKeyDown(Keys.Left))
            {
                var camera = RenderSystem.GetCamera();
                camera.Move(new Microsoft.Xna.Framework.Vector2(-1, 0)); // Движение вправо                                                
            }
            if (Game.keyboardState.IsKeyDown(Keys.LeftShift))
            {
                var camera = RenderSystem.GetCamera();
                camera.CenterOn(Position);
            }
        }
    }
}
