using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Engine_Sokov.WarDots.Units
{
        public class WarDotsPlayerInfantry : WarDotsPlayerDivision
        {
            public WarDotsPlayerInfantry()
            {
                Tag = "Player";
                Health = 80f; AttackDamage = 0.8f; AttackRange = 200f; DetectionRange = 400f;
                MoveSpeed = 25f; Radius = 10f; AltitudeFullDetail = 35f; AltitudeHidden = 110f;

                string texturePath = "Content/Textures/player_infantry.png";
                RenderSystem.EnqueueTextureLoad(this, texturePath);
                Size = new Vector2(32, 32);
                stabiliseNormalParameteres();
            }
        }

        public class WarDotsPlayerTank : WarDotsPlayerDivision
        {
            public WarDotsPlayerTank()
            {
                Tag = "Player";
                Health = 220f; AttackDamage = 2.5f; AttackRange = 350f; DetectionRange = 450f;
                MoveSpeed = 50f; Radius = 14f; AltitudeFullDetail = 45f; AltitudeHidden = 125f;
                string texturePath = "Content/Textures/player_tank.png";
                RenderSystem.EnqueueTextureLoad(this, texturePath);
                Size = new Vector2(32, 32);
                stabiliseNormalParameteres();
            }
        }

        public class WarDotsPlayerArtillery : WarDotsPlayerDivision
        {
            public WarDotsPlayerArtillery()
            {
                Tag = "Player";
                Health = 100f; AttackDamage = 5f; AttackRange = 750f; DetectionRange = 1000f;
                MoveSpeed = 10f; Radius = 12f; AltitudeFullDetail = 40f; AltitudeHidden = 120f;

                RenderSystem.EnqueueTextureLoad(this, "Content/Textures/player_artillery.png");
                Size = new Vector2(32, 32);
                stabiliseNormalParameteres();
            }
        }
}
