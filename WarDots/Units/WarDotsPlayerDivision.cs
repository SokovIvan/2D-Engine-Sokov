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
            AttackDamage = 22f;
            AttackRange = 85f;
            DetectionRange = 260f;
            MoveSpeed = 85f;
            Radius = 11f;
            AltitudeFullDetail = 45f;
            AltitudeHidden = 130f;
            string texturePath = "Content/Textures/player.png";
            RenderSystem.EnqueueTextureLoad(this, texturePath);
        }

        public override void Start()
        {
            base.Start();
            // Texture уже обнуляется в базовом классе
        }
        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);   // ← КРИТИЧНО!

        }
    }
}
