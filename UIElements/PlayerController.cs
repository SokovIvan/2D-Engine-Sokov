using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Input;
namespace _2D_Engine_Sokov.UIElements
{
    public class PlayerController:UIElement
    {        

        public override void Update(double deltaTime)
        {
            if (Game.keyboardState.IsKeyDown(Keys.W))
            {
                var camera = RenderSystem.GetCamera();
                camera.Move(new Microsoft.Xna.Framework.Vector2(0, 1000 * (float)deltaTime)); // Движение вправо          
            }
            if (Game.keyboardState.IsKeyDown(Keys.S))
            {
                var camera = RenderSystem.GetCamera();
                camera.Move(new Microsoft.Xna.Framework.Vector2(0, -1000 * (float)deltaTime)); // Движение вправо      

            }
            if (Game.keyboardState.IsKeyDown(Keys.D))
            {
                var camera = RenderSystem.GetCamera();
                camera.Move(new Microsoft.Xna.Framework.Vector2(1000 * (float)deltaTime, 0)); // Движение вправо                                                
            }
            if (Game.keyboardState.IsKeyDown(Keys.A))
            {
                var camera = RenderSystem.GetCamera();
                camera.Move(new Microsoft.Xna.Framework.Vector2(-1000 * (float)deltaTime, 0)); // Движение вправо                                                
            }
            //base.Update(deltaTime);
        }
    }
}
