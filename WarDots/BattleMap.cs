using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using _2D_Engine_Sokov.MapGeneration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace _2D_Engine_Sokov
{
    /// <summary>
    /// Боевая карта, объединяющая логику MapState и отрисовку TileMap.
    /// Автоматически вычисляет и рисует динамическую линию границы между игроками и противниками.
    /// </summary>
    public class BattleMap : TileMap
    {
        public MapState MapState { get; private set; }

        private readonly List<Vector2> _playerUnits = new();
        private readonly List<Vector2> _enemyUnits = new();
        private List<Vector2> _boundaryPoints = new();

        public BattleMap(int width, int height, int tileWidth, int tileHeight, MapState mapState)
            : base(width, height, tileWidth, tileHeight)
        {
            MapState = mapState;
            InitializeTilesFromMapState();
        }

        private void InitializeTilesFromMapState()
        {
            var defaultWalkable = new HashSet<MapGroundStates>
            {
                MapGroundStates.ground, MapGroundStates.grass, MapGroundStates.forest,
                MapGroundStates.stone, MapGroundStates.metal, MapGroundStates.resource
            };

            for (int x = 0; x < MapState.Width; x++)
            {
                for (int y = 0; y < MapState.Height; y++)
                {
                    var ground = MapState.getGroundState(x, y);
                    bool isWalkable = defaultWalkable.Contains(ground);
                    SetTile(x, y, new Tile(isWalkable, "auto_visual"));
                }
            }
        }

        /// <summary>
        /// Фабричный метод: создаёт BattleMap из MapState, загружает визуальную текстуру и применяет правила проходимости.
        /// </summary>
        public static BattleMap FromMapState(MapState state, int tileWidth, int tileHeight, GraphicsDevice graphicsDevice, Dictionary<MapGroundStates, bool> walkableRules = null)
        {
            var battleMap = new BattleMap(state.Width, state.Height, tileWidth, tileHeight, state);

            if (walkableRules != null)
            {
                for (int x = 0; x < battleMap.Width; x++)
                {
                    for (int y = 0; y < battleMap.Height; y++)
                    {
                        var ground = battleMap.MapState.getGroundState(x, y);
                        if (walkableRules.ContainsKey(ground))
                        {
                            var tile = battleMap.GetTile(x, y);
                            if (tile != null) tile.IsWalkable = walkableRules[ground];
                        }
                    }
                }
            }

            string pathToLoad = state.path_to_image;
            if (!string.IsNullOrEmpty(pathToLoad) && System.IO.File.Exists(pathToLoad))
            {
                try
                {
                    using var stream = System.IO.File.OpenRead(pathToLoad);
                    battleMap.MapSprite = new Sprite
                    {
                        Texture = Texture2D.FromStream(graphicsDevice, stream),
                        Position = Vector2.Zero,
                        Origin = Vector2.Zero,
                        Scale = Vector2.One,
                        LayerDepth = 0f,
                        IsActive = true
                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BattleMap] Ошибка загрузки текстуры: {ex.Message}");
                }
            }

            return battleMap;
        }

        public void AddUnit(Vector2 worldPosition, bool isPlayer)
        {
            if (isPlayer) _playerUnits.Add(worldPosition);
            else _enemyUnits.Add(worldPosition);
            UpdateBoundary();
        }

        public void RemoveUnit(Vector2 worldPosition, bool isPlayer)
        {
            if (isPlayer) _playerUnits.Remove(worldPosition);
            else _enemyUnits.Remove(worldPosition);
            UpdateBoundary();
        }

        public void ClearUnits(bool clearPlayer, bool clearEnemy)
        {
            if (clearPlayer) _playerUnits.Clear();
            if (clearEnemy) _enemyUnits.Clear();
            UpdateBoundary();
        }

        /// <summary>
        /// Пересчитывает линию фронта. Алгоритм ищет для каждой строки карты точку равновесия расстояний до ближайших юнитов.
        /// </summary>
        public void UpdateBoundary()
        {
            _boundaryPoints.Clear();
            if (_playerUnits.Count == 0 || _enemyUnits.Count == 0) return;

            // Для каждой Y-строки находим X, где влияние игрока и противника максимально близко
            for (int y = 0; y < Height; y++)
            {
                float bestDiff = float.MaxValue;
                int bestX = 0;

                for (int x = 0; x < Width; x++)
                {
                    float pDist = GetNearestDistance(x, y, _playerUnits);
                    float eDist = GetNearestDistance(x, y, _enemyUnits);
                    float diff = Math.Abs(pDist - eDist);

                    if (diff < bestDiff)
                    {
                        bestDiff = diff;
                        bestX = x;
                    }
                }
                _boundaryPoints.Add(GridToWorldPosition(bestX, y));
            }
        }

        private float GetNearestDistance(int tileX, int tileY, List<Vector2> units)
        {
            float minDistSq = float.MaxValue;
            Vector2 tileWorldPos = GridToWorldPosition(tileX, tileY);
            foreach (var unitPos in units)
            {
                float distSq = Vector2.DistanceSquared(tileWorldPos, unitPos);
                if (distSq < minDistSq) minDistSq = distSq;
            }
            return MathF.Sqrt(minDistSq);
        }

        /// <summary>
        /// Отправляет команды отрисовки границы в RenderSystem. Безопасно вызывать из логики.
        /// </summary>
        public void DrawBoundary(Color color, float thickness = 2.5f)
        {
            if (_boundaryPoints.Count < 2) return;

            for (int i = 0; i < _boundaryPoints.Count - 1; i++)
            {
                var start = _boundaryPoints[i];
                var end = _boundaryPoints[i + 1];

                // Игнорируем резкие скачки (разрывы в сетке или телепортации юнитов)
                if (Math.Abs(start.X - end.X) > TileWidth * 2f) continue;

                RenderSystem.DrawLine(start, end, color, thickness);
            }
        }

        /// <summary>
        /// Альтернатива: отрисовка через PersistentCommand, если линию нужно держать на экране без перерасчёта каждый кадр.
        /// </summary>
        public void SubmitPersistentBoundary(Color color, float thickness = 2.5f, int framesToLive = 60)
        {
            if (_boundaryPoints.Count < 2) return;

            var pointsSnapshot = _boundaryPoints.ToList();
            RenderSystem.SubmitPersistentCommand(() =>
            {
                for (int i = 0; i < pointsSnapshot.Count - 1; i++)
                {
                    var start = pointsSnapshot[i];
                    var end = pointsSnapshot[i + 1];
                    if (Math.Abs(start.X - end.X) > TileWidth * 2f) continue;
                    RenderSystem.DrawLine(start, end, color, thickness);
                }
            }, framesToLive, useCamera: true);
        }
    }
}