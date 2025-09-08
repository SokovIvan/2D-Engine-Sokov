using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Engine_Sokov.GameObjects
{
    internal class EnemyBuilding : Building
    {
        public override void Start()
        {
            base.Start();
            Tag = "Enemy";
            CollisionEnabled = true;
            Mass = 100f;
            GravityEnabled = false;
            //ProduceUnit = new EnemyUnit();
            ProduceOffset = new Vector2(-Size.X, 0);
        }
    }
}
