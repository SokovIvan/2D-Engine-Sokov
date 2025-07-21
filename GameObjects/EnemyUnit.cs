using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Engine_Sokov.GameObjects
{
    public class EnemyUnit : Unit
    {
        public override void Start()
        {
            base.Start();
            Tag = "Enemy";
            CollisionEnabled = true;
            Mass = 100f;
            GravityEnabled = false;
        }
    }
}
