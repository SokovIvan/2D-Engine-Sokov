using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Engine_Sokov.WarDots.Units
{
    public class WarDotsPlayerDivision : WarDotsDivision
    {
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

        }

        public override void Start()
        {
            base.Start();
        }
        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);   
        }
    }
}
