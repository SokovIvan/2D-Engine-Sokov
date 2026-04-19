using Microsoft.Xna.Framework;
using _2D_Engine_Sokov.MapGeneration;
using System;

namespace _2D_Engine_Sokov.WarDots
{
    public static class WarDotsGame
    {
        public static BattleMap CurrentBattleMap { get; private set; }

        public static void StartNewGame(int mapSeed = 42)
        {
            var mapState = MapGenerator.GenerateMapState(128, 128, hash: mapSeed);

            CurrentBattleMap = BattleMap.FromMapState(
                mapState,
                tileWidth: 32,
                tileHeight: 32,
                RenderSystem._graphicsDevice
            );

            Game.instance._currentLevel.TileMap = CurrentBattleMap;

            // Создаём начальные юниты и здания
            SpawnStartingForces();
        }

        private static void SpawnStartingForces()
        {

        }

        public static void UpdateBoundary()
        {
            CurrentBattleMap?.UpdateBoundary();
            CurrentBattleMap?.SubmitPersistentBoundary(Color.Red, 3f, 2);
        }
    }
}