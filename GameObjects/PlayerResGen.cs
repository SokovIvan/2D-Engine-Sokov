using _2D_Engine_Sokov.UIElements;
using Microsoft.Xna.Framework;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace _2D_Engine_Sokov.GameObjects
{
    internal class PlayerResGen : Building
    {
        protected double ResGenTimer { get; set; }
        protected double ResGenRate { get; set; } = 5f;
        protected float ResGen { get; set; } = 2f;
        public override void Start()
        {
            base.Start();
            ResGenTimer = 0;
            Tag = "Player";
            CollisionEnabled = true;
            Mass = 100f;
            GravityEnabled = false;
            ProduceUnit = null;
            ProduceOffset = new Vector2(Size.X, 0);
        }
        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            ResGenTimer += deltaTime;
            if (ResGenTimer > ResGenRate) 
            { 
                GameController.instance.playerRes += ResGen;
                ResGenTimer = 0;
            }

        }
    }
}
