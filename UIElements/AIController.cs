using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _2D_Engine_Sokov.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace _2D_Engine_Sokov.UIElements
{

    internal class AIController : UIElement
    {
        private double timer = 0f;
        public static string AITeam = "Enemy";
        public override void Start()
        {
            base.Start();
            Console.WriteLine("Start AI");
        }
        public override void Update(double deltaTime)
        { 
            base.Update(deltaTime);
            timer += deltaTime;
            if (timer > 0.5f) {
                timer = 0;
                EnemyMap.calculateDangerMap();
                foreach (Unit unit in LogicSystem.FindGameObjectsByType(typeof(Unit)))
                {
                    if (unit.Tag == AITeam)
                    {
                        var tP = Game.instance._currentLevel.TileMap.GetTilePoint(EnemyMap.mostDangerTile);
                        //Console.WriteLine(EnemyMap.mostDangerTile.id);
                       // Console.WriteLine(tP.ToString());
                        var destP = Game.instance._currentLevel.TileMap.GridToWorldPosition(tP.X, tP.Y);
                        //Console.WriteLine(destP.ToString());
                        unit.Path = Pathfinding.FindPath(Game.instance._currentLevel.TileMap, unit.Position, destP);

                    }
                }
                 
            }
        }
        private class EnemyMap
        {
            public static Dictionary<Tile, int> dangerousTiles = new();
            public static int dangerScale = 1;
            public static Tile mostDangerTile;
            public static string AITeam = "Enemy";
            public static void calculateDangerMap() {
                dangerousTiles.Clear();
                var map = Game.instance._currentLevel.TileMap;
                mostDangerTile = map.GetTile(map.Width/2,map.Height/2);
                foreach (Tile tile in map.GetTiles()) {
                    dangerousTiles.Add(tile, 0);
                }
                //Console.WriteLine(LogicSystem.FindGameObjectsByType(typeof(Unit)).Length);
                if (LogicSystem.FindGameObjectsByType(typeof(Unit)).Length > 0) {

                    foreach (Unit unit in LogicSystem.FindGameObjectsByType(typeof(Unit)))
                    {
                        if (unit.Tag != AITeam)
                        {
                            var tileP = map.WorldToGridPosition(unit.Position);
                            var tile = map.GetTile(tileP.X, tileP.Y);
                            dangerousTiles[tile] = dangerousTiles.GetValueOrDefault(tile, 0) + 1;
                        }
                    }
                    mostDangerTile = dangerousTiles.MaxBy(entry => entry.Value).Key;
                }

            }
        }
            
    }
}
