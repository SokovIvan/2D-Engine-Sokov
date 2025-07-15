using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
namespace _2D_Engine_Sokov.GameObjects
{
    internal class Enemy : Sprite
    {        
        public override void Update(double deltaTime)
        {
            var player = LogicSystem.FindGameObjectByTag("Player");
            if (player != null) { 
                Velocity = (player.Position-Position);
                Velocity.Normalize();
                Velocity = new Vector2(Velocity.X*1000 * (float)deltaTime, Velocity.Y * 1000 * (float)deltaTime);
            }
        }
    }
}
