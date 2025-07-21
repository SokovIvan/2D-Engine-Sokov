using _2D_Engine_Sokov.GameObjects;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Engine_Sokov
{
    public static class EnemyAI
    {
        public static void Initialize()
        {
            LogicSystem.OnLogicUpdate += Update;
        }

        private static void Update()
        {
            var enemies = LogicSystem.FindGameObjectsByTag("Enemy").OfType<EnemyUnit>().ToList();
            var players = LogicSystem.FindGameObjectsByTag("Player").OfType<PlayerUnit>().ToList();

            foreach (var enemy in enemies)
            {
                if (!enemy.IsActive) continue;

                if (enemy.Target == null)
                {
                    PlayerUnit closestPlayer = null;
                    float closestDistance = float.MaxValue;

                    foreach (var player in players)
                    {
                        if (!player.IsActive) continue;
                        float distance = Vector2.Distance(enemy.Position, player.Position);
                        if (distance < closestDistance)
                        {
                            closestPlayer = player;
                            closestDistance = distance;
                        }
                    }

                    if (closestPlayer != null && closestDistance <= enemy.DetectionRange)
                    {
                        enemy.Path = Pathfinding.FindPath(Game.instance._currentLevel.TileMap, enemy.Position, closestPlayer.Position);
                    }
                }
            }
        }
    }
}
