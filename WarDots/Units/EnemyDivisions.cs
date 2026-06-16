using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Engine_Sokov.WarDots.Units
{
    public class WarDotsEnemyInfantry : WarDotsEnemyDivision
    {
        public WarDotsEnemyInfantry()
        {
            Tag = "Enemy";
            Health = 80f; AttackDamage = 0.8f; AttackRange = 200f; DetectionRange = 400f;
            MoveSpeed = 25f; Radius = 10f; AltitudeFullDetail = 35f; AltitudeHidden = 110f;
            RenderSystem.EnqueueTextureLoad(this, "Content/Textures/enemy_infantry.png");
            Size = new Vector2(32, 32);
            stabiliseNormalParameteres();
        }
    }

    public class WarDotsEnemyTank : WarDotsEnemyDivision
    {
        public WarDotsEnemyTank()
        {
            Tag = "Enemy";
            Health = 220f; AttackDamage = 2.5f; AttackRange = 350f; DetectionRange = 450f;
            MoveSpeed = 50f; Radius = 14f; AltitudeFullDetail = 45f; AltitudeHidden = 125f;
            RenderSystem.EnqueueTextureLoad(this, "Content/Textures/enemy_tank.png");
            Size = new Vector2(32, 32);
            stabiliseNormalParameteres();
        }
    }

    public class WarDotsEnemyArtillery : WarDotsEnemyDivision
    {
        public WarDotsEnemyArtillery()
        {
            Tag = "Enemy";
            Health = 100f; AttackDamage = 5f; AttackRange = 750f; DetectionRange = 1000f;
            MoveSpeed = 10f; Radius = 12f; AltitudeFullDetail = 40f; AltitudeHidden = 120f;
            RenderSystem.EnqueueTextureLoad(this, "Content/Textures/enemy_artillery.png");
            Size = new Vector2(32, 32);
            stabiliseNormalParameteres();
        }
    }
}
