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
    internal class PlayerBuilding : Building
    {
        public override void Start()
        {
            base.Start();
            Tag = "Player";
            CollisionEnabled = true;
            Mass = 100f;
            GravityEnabled = false;
            //ProduceUnit = new PlayerUnit();

           // ProduceUnit.Tag = Tag;
           // ProduceUnit.Name = "UnitPl_"+Name;
          //  ProduceUnit.Position = Position;
          //  ProduceUnit.Rotation = Rotation;
          //  ProduceUnit.IsActive = IsActive;
          //  ProduceUnit.Size = new Vector2(32, 32);

            ProduceOffset = new Vector2(Size.X, 0);
        }
    }
}
