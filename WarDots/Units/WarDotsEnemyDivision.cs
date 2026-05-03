using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Engine_Sokov.WarDots.Units
{
    public class WarDotsEnemyDivision : WarDotsDivision
    {
        public WarDotsEnemyDivision()
        {
            Tag = "Enemy";
            Health = 150f;
            AttackDamage = 20f;
            AttackRange = 80f;
            DetectionRange = 250f;
            MoveSpeed = 80f;
            Radius = 10f;
            AltitudeFullDetail = 40f;
            AltitudeHidden = 120f;
            string texturePath = "Content/Textures/enemy.png";
            RenderSystem.EnqueueTextureLoad(this, texturePath);
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
