using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Engine_Sokov.GameObjects
{
    public class PlayerUnit : Unit
    {
        public override void Start()
        {
            base.Start();
            Tag = "Player";
            CollisionEnabled = true;
            Mass = 100f;
            GravityEnabled = false;
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);

           /* var keyboard = Keyboard.GetState();
            var mouse = Mouse.GetState();
            if (mouse.LeftButton == ButtonState.Pressed)
            {
                var mousePos = new Vector2(mouse.X, mouse.Y);
                var camera = RenderSystem.GetCamera();
                var worldPos = Vector2.Transform(mousePos, Matrix.Invert(camera.TransformMatrix));
                Path = Pathfinding.FindPath(Game.instance._currentLevel.TileMap, Position, worldPos);
            }*/
        }
    }
}
