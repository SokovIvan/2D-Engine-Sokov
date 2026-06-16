using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
namespace _2D_Engine_Sokov.WarDots.Units
{
    public class WarDotsEnemyDivision : WarDotsDivision
    {
        public WarDotsEnemyDivision()
        {
            Tag = "Enemy";
            Health = 160f;
            AttackDamage = 1f;
            AttackRange = 85f;
            DetectionRange = 250f;
            MoveSpeed = 80f;
            Radius = 11f;
            AltitudeFullDetail = 40f;
            AltitudeHidden = 120f;

            // RenderSystem.SubmitSprite(this);
        }

        public override void Start()
        {
            base.Start();
            // Базовый класс уже отключает Texture = null,
            // поэтому RenderSystem будет рисовать только примитивы.
        }
    }
}
