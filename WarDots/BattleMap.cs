using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using _2D_Engine_Sokov.MapGeneration;

namespace _2D_Engine_Sokov
{
    /// <summary>
    /// Состояние контроля клетки: 0-Нейтральная, 1-Игрок, 2-Враг, 3-Спорная
    /// </summary>
    public enum CellControlState : byte { Neutral = 0, Player = 1, Enemy = 2, Contested = 3 }

    public class BattleMap : TileMap
    {
        public MapState MapState { get; private set; }
        private readonly List<Vector2> _playerUnits = new();
        private readonly List<Vector2> _enemyUnits = new();

        // 4) Отдельная структура для хранения состояния каждой клетки
        private CellControlState[,] _territoryMap;

        // 3) Поддержка разорванных линий (окружения, анклавы)
        private List<List<Vector2>> _frontlineSegments = new();
        private List<Vector2> _cachedDefensivePositions = new();
        private bool _defensivePositionsValid = false; // Флаг валидности кэша
        public BattleMap(int width, int height, int tileWidth, int tileHeight, MapState mapState)
            : base(width, height, tileWidth, tileHeight)
        {
            MapState = mapState;
            _territoryMap = new CellControlState[width, height];
            InitializeTilesFromMapState();
        }
        public MapGroundStates getGroundStateFromWorldPosition(Vector2 worldPos)
        {
            try
            {
                Point gridPos = WorldToGridPosition(worldPos);
                return MapState.getGroundState(gridPos.X, gridPos.Y);
            }
            catch (Exception e)
            {
                return MapGroundStates.emptiness;
            }
        }
        private void InitializeTilesFromMapState()
        {
            var defaultWalkable = new HashSet<MapGroundStates>
            {
                MapGroundStates.ground, MapGroundStates.grass, MapGroundStates.forest,
                MapGroundStates.stone, MapGroundStates.metal, MapGroundStates.resource, 
                MapGroundStates.toxic,
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
        public CellControlState GetControlStateAtPosition(Vector2 worldPosition)
        {
            Point gridPos = WorldToGridPosition(worldPosition);

            // Проверка границ карты
            if (gridPos.X < 0 || gridPos.X >= Width || gridPos.Y < 0 || gridPos.Y >= Height)
            {
                return CellControlState.Neutral; // Или можно выбросить исключение, но нейтраль безопаснее
            }

            return _territoryMap[gridPos.X, gridPos.Y];
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
                    string visualPath = state.path_to_image.Replace("_data", "_visual");
                    using var stream = System.IO.File.OpenRead(visualPath);
                    battleMap.MapSprite = new Sprite
                    {
                        Texture = Texture2D.FromStream(graphicsDevice, stream),
                        Position = Vector2.Zero,
                        Origin = Vector2.Zero,
                        Scale = Vector2.One,
                        LayerDepth = 0f,
                        IsActive = true
                    };
                    if (battleMap.MapSprite != null)
                    {
                        battleMap.MapSprite.Scale = new Vector2(
                            (float)tileWidth / battleMap.MapSprite.Texture.Width * battleMap.Width,
                            (float)tileHeight / battleMap.MapSprite.Texture.Height * battleMap.Height
                        );
                    }
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
            UpdateFrontline(); // Переименовано для ясности
        }

        public void RemoveUnit(Vector2 worldPosition, bool isPlayer)
        {
            if (isPlayer) _playerUnits.Remove(worldPosition);
            else _enemyUnits.Remove(worldPosition);
            UpdateFrontline();
        }

        public void ClearUnits(bool clearPlayer, bool clearEnemy)
        {
            if (clearPlayer) _playerUnits.Clear();
            if (clearEnemy) _enemyUnits.Clear();
            UpdateFrontline();
        }

        /// <summary>
        /// Глубокая переработка: вычисление карты контроля, извлечение контуров, адаптивное сглаживание
        /// </summary>
        public void UpdateFrontline()
        {
            _frontlineSegments.Clear();

            // 🔹 Если юнитов нет — очищаем кэш и уходим
            if (_playerUnits.Count == 0 || _enemyUnits.Count == 0)
            {
                _cachedDefensivePositions.Clear();
                _defensivePositionsValid = false;
                return;
            }

            // Шаг 1-3: расчёт фронта (без изменений)
            ComputeTerritoryMap();
            var rawSegments = ExtractRawContourSegments();

            foreach (var raw in rawSegments)
            {
                var filled = FillGaps(raw);
                var smoothed = AdaptiveSmooth(filled);
                if (smoothed.Count >= 2)
                    _frontlineSegments.Add(smoothed);
            }

            // 🔹 ШАГ 4: Рассчитываем и кэшируем оборонительные позиции
            _cachedDefensivePositions.Clear();

            int maxPoints = Math.Max(10, _enemyUnits.Count); // Динамическое количество точек
            foreach (var segment in _frontlineSegments)
            {
                for (int i = 0; i < segment.Count; i += Math.Max(1, segment.Count / maxPoints))
                {
                    var point = segment[i];
                    var grid = WorldToGridPosition(point);

                    if (grid.X >= 0 && grid.X < Width && grid.Y >= 0 && grid.Y < Height)
                    {
                        var offsetDir = GetDirectionIntoTerritory(point, CellControlState.Player);
                        var defensiveOffset = offsetDir * TileWidth * 0.5f; // 0.5 тайла вглубь
                        _cachedDefensivePositions.Add(point + defensiveOffset);
                    }
                }
            }

            // 🔹 Финальная страховка: если пусто — дефолтные позиции
            if (_cachedDefensivePositions.Count == 0)
            {
                _cachedDefensivePositions = new List<Vector2>
                {
                    GridToWorldPosition(Width * 3 / 4, Height / 4),
                    GridToWorldPosition(Width * 3 / 4, Height / 2),
                    GridToWorldPosition(Width * 3 / 4, Height * 3 / 4)
                };
            }

            _defensivePositionsValid = true; // 🔹 Кэш готов!
        }

        private void ComputeTerritoryMap()
        {
            //float contestedThreshold = TileWidth * 1.2f; // Зона "спорной" территории
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    float pDist = GetNearestDistance(x, y, _playerUnits);
                    float eDist = GetNearestDistance(x, y, _enemyUnits);

                    // Убираем присвоение Contested для геометрии фронта
                    _territoryMap[x, y] = pDist < eDist ? CellControlState.Player : CellControlState.Enemy;
                }
            }
        }

        private List<List<Vector2>> ExtractRawContourSegments()
        {
            var segments = new List<List<Vector2>>();
            var points = new List<Vector2>();

            // Сканируем горизонтальные и вертикальные границы клеток
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var current = _territoryMap[x, y];
                    if (current == CellControlState.Neutral) continue;

                    // Право
                    if (x + 1 < Width && _territoryMap[x + 1, y] != current && IsFrontier(current, _territoryMap[x + 1, y]))
                        points.Add(new Vector2((x + 1) * TileWidth, (y + 0.5f) * TileHeight));

                    // Низ
                    if (y + 1 < Height && _territoryMap[x, y + 1] != current && IsFrontier(current, _territoryMap[x, y + 1]))
                        points.Add(new Vector2((x + 0.5f) * TileWidth, (y + 1) * TileHeight));
                }
            }

            // Собираем точки в непрерывные цепочки (простой жадный алгоритм по ближайшему соседу)
            return ClusterIntoChains(points);
        }

        private bool IsFrontier(CellControlState a, CellControlState b)
        {
            // Граница должна быть только там, где контроль меняется напрямую
            return (a == CellControlState.Player && b == CellControlState.Enemy) ||
                   (a == CellControlState.Enemy && b == CellControlState.Player);
        }

        private List<List<Vector2>> ClusterIntoChains(List<Vector2> points)
        {
            var chains = new List<List<Vector2>>();
            var used = new bool[points.Count];
            float maxGap = TileWidth * 1.5f; // Максимальное расстояние для связки звеньев

            for (int i = 0; i < points.Count; i++)
            {
                if (used[i]) continue;
                var chain = new List<Vector2> { points[i] };
                used[i] = true;
                int current = i;

                bool extended = true;
                while (extended)
                {
                    extended = false;
                    float minDist = float.MaxValue;
                    int nextIdx = -1;

                    for (int j = 0; j < points.Count; j++)
                    {
                        if (used[j]) continue;
                        float d = Vector2.DistanceSquared(points[current], points[j]);
                        if (d < maxGap * maxGap && d < minDist)
                        {
                            minDist = d;
                            nextIdx = j;
                        }
                    }

                    if (nextIdx != -1)
                    {
                        chain.Add(points[nextIdx]);
                        used[nextIdx] = true;
                        current = nextIdx;
                        extended = true;
                    }
                }

                if (chain.Count >= 2) chains.Add(chain);
            }

            return chains;
        }

        /// <summary>
        /// 2) Убирает жёсткие разрывы: достраивает промежуточные точки между далёкими звеньями
        /// </summary>
        private List<Vector2> FillGaps(List<Vector2> raw)
        {
            var result = new List<Vector2> { raw[0] };
            float step = TileWidth * 0.5f;

            for (int i = 1; i < raw.Count; i++)
            {
                var prev = raw[i - 1];
                var curr = raw[i];
                float dist = Vector2.Distance(prev, curr);

                if (dist > TileWidth * 1.2f)
                {
                    int steps = (int)Math.Ceiling(dist / step);
                    for (int s = 1; s < steps; s++)
                    {
                        float t = s / (float)steps;
                        result.Add(Vector2.Lerp(prev, curr, t));
                    }
                }
                result.Add(curr);
            }
            return result;
        }

        /// <summary>
        /// 1) Динамическое сглаживание: добавляет больше точек там, где угол изгиба резкий
        /// </summary>
        private List<Vector2> AdaptiveSmooth(List<Vector2> input)
        {
            var output = new List<Vector2>(input);
            int maxIterations = 3;
            float angleThreshold = MathF.PI / 4f; // ~45 градусов

            for (int iter = 0; iter < maxIterations; iter++)
            {
                var temp = new List<Vector2> { output[0] };
                for (int i = 1; i < output.Count - 1; i++)
                {
                    var p0 = output[i - 1];
                    var p1 = output[i];
                    var p2 = output[i + 1];

                    Vector2 v1 = p0 - p1;
                    Vector2 v2 = p2 - p1;
                    float angle = MathF.Acos(Math.Clamp(Vector2.Dot(v1, v2) / (v1.Length() * v2.Length() + 1e-5f), -1f, 1f));

                    if (angle > angleThreshold)
                    {
                        // Вставляем точку сглаживания (Chaikin's corner cutting / простая интерполяция)
                        var mid1 = Vector2.Lerp(p1, p0, 0.25f);
                        var mid2 = Vector2.Lerp(p1, p2, 0.25f);
                        temp.Add(mid1);
                        temp.Add(p1); // Сохраняем оригинал для стабильности
                        temp.Add(mid2);
                    }
                    else
                    {
                        temp.Add(p1);
                    }
                }
                temp.Add(output[^1]);
                output = temp;
            }
            return output;
        }

        /// <summary>
        /// Отрисовка всех сегментов без пропусков. Разрывы больше не игнорируются, а заполнены.
        /// </summary>
        public void DrawBoundary(Color color, float thickness = 2.5f)
        {
            foreach (var segment in _frontlineSegments)
            {
                for (int i = 0; i < segment.Count - 1; i++)
                {
                    RenderSystem.DrawLine(segment[i], segment[i + 1], color, thickness);
                }
            }
        }

        public void SubmitPersistentBoundary(Color color, float thickness = 2.5f, int framesToLive = 60)
        {
            var snapshot = _frontlineSegments.Select(s => s.ToList()).ToList();
            RenderSystem.SubmitPersistentCommand(() =>
            {
                foreach (var segment in snapshot)
                {
                    for (int i = 0; i < segment.Count - 1; i++)
                    {
                        RenderSystem.DrawLine(segment[i], segment[i + 1], color, thickness);
                    }
                }
            }, framesToLive, useCamera: true);
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
        /// Возвращает ключевые точки для обороны на границе вражеской территории
        /// </summary>
        public List<Vector2> GetEnemyDefensivePositions(int maxPoints = 10)
        {
            // 🔹 Возвращаем кэш — он уже рассчитан в UpdateFrontline()
            // Если кэш не валиден — возвращаем дефолтные позиции (на всякий случай)
            if (!_defensivePositionsValid || _cachedDefensivePositions.Count == 0)
            {
                return new List<Vector2>
                {
                    GridToWorldPosition(Width * 3 / 4, Height / 2)
                };
            }
            return _cachedDefensivePositions;
        }

        /// <summary>
        /// Проверяет, угрожает ли игрок вражеской территории в данной точке
        /// </summary>
        public bool IsPlayerThreateningPosition(Vector2 worldPos, float threatRadius = 150f)
        {
            foreach (var player in _playerUnits)
            {
                if (Vector2.DistanceSquared(worldPos, player) < threatRadius * threatRadius)
                    return true;
            }
            return false;
        }
        private Vector2 GetDirectionIntoTerritory(Vector2 frontierPoint, CellControlState targetTerritory)
        {
            // Проверяем 4 направления вокруг точки фронта
            var directions = new[] { Vector2.UnitX, -Vector2.UnitX, Vector2.UnitY, -Vector2.UnitY };
            float checkDist = TileWidth * 0.6f; // Чуть меньше тайла, чтобы не "перепрыгнуть" границу

            foreach (var dir in directions)
            {
                var checkPos = frontierPoint + dir * checkDist;
                var grid = WorldToGridPosition(checkPos);

                if (grid.X < 0 || grid.X >= Width || grid.Y < 0 || grid.Y >= Height) continue;

                if (_territoryMap[grid.X, grid.Y] == targetTerritory)
                    return dir; // Нашли направление в нужную территорию
            }

            // Фоллбэк: используем нормаль к ближайшему юниту врага
            return Vector2.Normalize(frontierPoint - GridToWorldPosition(Width * 3 / 4, Height / 2));
        }
    }
}