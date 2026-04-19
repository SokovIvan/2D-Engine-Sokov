using Microsoft.Xna.Framework;
using _2D_Engine_Sokov.GameObjects;
using _2D_Engine_Sokov.WarDots.Units;
using System;
using System.Collections.Generic;
using System.Linq;

namespace _2D_Engine_Sokov.WarDots
{
    public static class WarDotsEnemyAI
    {
        public static int GlobalResources { get; set; } = 150;
        private static int _decisionFrames = 0;
        private static readonly int DecisionIntervalFrames = 90; // ~1.5 сек при 60 FPS логики

        public static void Initialize()
        {
            LogicSystem.OnLogicUpdate += Update;
            Console.WriteLine("[AI] WarDotsEnemyAI инициализирован.");
        }

        private static void Update()
        {
            _decisionFrames++;
            UpdateUnitsBehavior();
            UpdateBuildingsLogic();

            if (_decisionFrames >= DecisionIntervalFrames)
            {
                _decisionFrames = 0;
                MakeStrategicDecisions();
            }
        }

        private static void UpdateUnitsBehavior()
        {
            var divisions = LogicSystem.FindGameObjectsByType(typeof(WarDotsEnemyDivision))
                                         .OfType<WarDotsEnemyDivision>()
                                         .Where(d => d.IsActive)
                                         .ToList();

            var players = LogicSystem.FindGameObjectsByTag("Player")
                                     .Where(p => p.IsActive)
                                     .ToList();

            foreach (var div in divisions)
            {
                if (div.Target == null || !div.Target.IsActive)
                {
                    var closest = players.OrderBy(p => Vector2.DistanceSquared(div.Position, p.Position)).FirstOrDefault();
                    if (closest != null)
                    {
                        div.Target = closest;
                        div.Path = Pathfinding.FindPath(
                            Game.instance._currentLevel.TileMap,
                            div.Position,
                            closest.Position
                        );
                    }
                }
            }
        }

        private static void UpdateBuildingsLogic()
        {
            var factories = LogicSystem.FindGameObjectsByType(typeof(WarDotsEnemyFactory))
                                       .OfType<WarDotsEnemyFactory>()
                                       .Where(f => f.IsActive)
                                       .ToList();

            foreach (var factory in factories)
            {
                // Если очередь пуста и есть ресурсы, ставим производство
                if (factory.QueueLength == 0 && GlobalResources >= factory.UnitProductionCost)
                {
                    factory.EnqueueProduction(typeof(WarDotsEnemyDivision));
                }
            }
        }

        private static void MakeStrategicDecisions()
        {
            var tileMap = Game.instance._currentLevel.TileMap;
            var bases = LogicSystem.FindGameObjectsByType(typeof(WarDotsEnemyBase)).Where(b => b.IsActive).ToList();
            if (bases.Count == 0) return;

            var mainBase = bases[0];
            var factories = LogicSystem.FindGameObjectsByType(typeof(WarDotsEnemyFactory)).Count(f => f.IsActive);
            var generators = LogicSystem.FindGameObjectsByType(typeof(WarDotsEnemyResourceGenerator)).Count(g => g.IsActive);

            // Стратегия: сначала экономика, затем армия, затем экспансия
            if (generators < 2 && GlobalResources >= 80)
            {
                TryPlaceBuilding(typeof(WarDotsEnemyResourceGenerator), mainBase.Position + new Vector2(-120, 80));
                GlobalResources -= 80;
            }
            else if (factories < 3 && GlobalResources >= 100)
            {
                TryPlaceBuilding(typeof(WarDotsEnemyFactory), mainBase.Position + new Vector2(100, 100));
                GlobalResources -= 100;
            }
            else if (GlobalResources >= 200) // Резерв на экстренное производство
            {
                foreach (var f in LogicSystem.FindGameObjectsByType(typeof(WarDotsEnemyFactory)).OfType<WarDotsEnemyFactory>().Where(f => f.IsActive))
                {
                    if (f.QueueLength < f.MaxQueueSize)
                        f.EnqueueProduction(typeof(WarDotsEnemyDivision));
                }
                GlobalResources -= 200;
            }
        }

        private static void TryPlaceBuilding(Type buildingType, Vector2 targetPos)
        {
            var tileMap = Game.instance._currentLevel.TileMap;
            var gridPos = tileMap.WorldToGridPosition(targetPos);

            // Простой поиск свободного тайла в радиусе 3 клеток
            for (int dx = -3; dx <= 3; dx++)
            {
                for (int dy = -3; dy <= 3; dy++)
                {
                    var checkX = gridPos.X + dx;
                    var checkY = gridPos.Y + dy;
                    var tile = tileMap.GetTile(checkX, checkY);

                    if (tile != null && tile.IsWalkable && !tileMap.IsOccupied(checkX, checkY))
                    {
                        var worldPos = tileMap.GridToWorldPosition(checkX, checkY);
                        var building = (WarDotsBuilding)Activator.CreateInstance(buildingType);
                        building.Position = worldPos;
                        Game.SubmitObject(building);
                        Console.WriteLine($"[AI] Построено: {buildingType.Name}");
                        return;
                    }
                }
            }
        }
    }
}