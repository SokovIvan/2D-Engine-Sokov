using Microsoft.Xna.Framework;
using _2D_Engine_Sokov.WarDots.Units;

namespace _2D_Engine_Sokov.WarDots
{
    public static class WarDotsEnemyAI
    {
        public static int GlobalResources { get; set; } = 150;

        // для распределения нагрузки
        private static int _aiState = 0; // 0=Units, 1=Buildings, 2=Strategy, 3=Idle
        private static int _aiSubIndex = 0; // Индекс для поочерёдной обработки списков
        private static readonly int AiUpdateInterval = 2; // Выполнять 1 шаг каждые 2 кадра логики

        private static int _decisionFrames = 0;
        private static readonly int DecisionIntervalFrames = 90;

        // КЭШИ для уменьшения поиска объектов
        private static List<WarDotsEnemyDivision> _cachedEnemyDivisions = new();
        private static List<GameObject> _cachedPlayerObjects = new();
        private static int _cacheValidFrames = 0;
        private static readonly int CacheLifetime = 30; // Обновлять кэш раз в 0.5 сек
        public enum AiStrategy { InfantryRush, Balanced, Defensive, ArtilleryHeavy, TankOffensive, Random }
        private static AiStrategy _currentStrategy = AiStrategy.Random;
        private static readonly Random _strategyRandom = new();

        public static void SetStrategy(AiStrategy strategy)
        {
            if (strategy == AiStrategy.Random)
            {
                AiStrategy[] options = { AiStrategy.InfantryRush, AiStrategy.Balanced, AiStrategy.TankOffensive, AiStrategy.Defensive, AiStrategy.ArtilleryHeavy };
                _currentStrategy = options[_strategyRandom.Next(options.Length)];
            }
            else _currentStrategy = strategy;
            Console.WriteLine($"[AI] Выбрана стратегия: {_currentStrategy}");
        }

        public static void Initialize()
        {
            SetStrategy(_currentStrategy); // Если в XML не указано, сработает Random
            LogicSystem.OnLogicUpdate += Update;
            Console.WriteLine("[AI] WarDotsEnemyAI инициализирован (оптимизирован).");
        }


        private static void Update()
        {
            _decisionFrames++;
            _cacheValidFrames++;

            // 🔧 Обновляем кэш объектов периодически, а не каждый кадр
            if (_cacheValidFrames >= CacheLifetime)
            {
                _cacheValidFrames = 0;
                _cachedEnemyDivisions = LogicSystem.FindGameObjectsByType(typeof(WarDotsEnemyDivision))
                                                      .OfType<WarDotsEnemyDivision>()
                                                      .Where(d => d.IsActive)
                                                      .ToList();
                _cachedPlayerObjects = LogicSystem.FindGameObjectsByTag("Player")
                                                     .Where(p => p.IsActive)
                                                     .ToList();
            }

            // 🔧 Раз в 90 кадров запускаем "тяжёлую" фазу, но выполняем её ПОШАГОВО
            if (_decisionFrames >= DecisionIntervalFrames)
            {
                _decisionFrames = 0;
                ExecuteAiStep(); // ← Выполняем только ОДИН шаг, а не всё сразу!
            }
        }

        // 🔧 НОВЫЙ МЕТОД: выполняет один маленький шаг ИИ за вызов
        private static void ExecuteAiStep()
        {
            var tileMap = GameContext.TileMap;
            if (tileMap == null) return;

            switch (_aiState)
            {
                case 0: // 🔹 Шаг 1: Поведение юнитов
                    UpdateUnitsBehaviorBatch(5);
                    _aiState = 1;
                    break;

                case 1: // 🔹 Шаг 2: Логика зданий 
                    UpdateBuildingsLogicBatch(5);
                    _aiState = 2;
                    break;

                case 2: // 🔹 Шаг 3: Стратегические решения 
                    MakeOneStrategicDecision(tileMap);
                    _aiState = 3; // Переход в "ожидание"
                    break;

                case 3: // 🔹 Шаг 4: Пауза перед следующим циклом
                    _aiState = 0;
                    //_aiSubIndex = 0;
                    break;
            }
        }

        private static void UpdateUnitsBehaviorBatch(int batchSize)
        {
            if (_cachedEnemyDivisions.Count == 0) return;

            if (_aiSubIndex >= _cachedEnemyDivisions.Count)
                _aiSubIndex = 0;

            var tileMap = GameContext.TileMap as BattleMap;
            if (tileMap == null) return;

            // 🔹 Получаем оборонительные позиции на фронте
            var defensivePositions = tileMap.GetEnemyDefensivePositions(_cachedEnemyDivisions.Count);
            // 🔹 Страховка: если список пуст — используем дефолтную позицию
            if (defensivePositions.Count == 0)
            {
                defensivePositions = new List<Vector2> { tileMap.GridToWorldPosition(tileMap.Width * 3 / 4, tileMap.Height / 2) };
            }
            int processed = 0;
            for (int i = _aiSubIndex; i < _cachedEnemyDivisions.Count && processed < batchSize; i++, _aiSubIndex++, processed++)
            {
                var div = _cachedEnemyDivisions[i];
                if (!div.IsActive) continue;

                // 🔹 Определяем "домашнюю" позицию для этого юнита (привязка к сектору фронта)
                if (defensivePositions.Count == 0) break;
                int sectorIndex = _cachedEnemyDivisions.IndexOf(div) % defensivePositions.Count;
                Vector2 homePosition = defensivePositions[sectorIndex];

                // 🔹 Проверяем: есть ли угроза поблизости?
                bool isThreatened = tileMap.IsPlayerThreateningPosition(homePosition, threatRadius: div.DetectionRange);

                if (isThreatened)
                {
                    // 🗡️ УГРОЗА! Перехватываем ближайшего игрока в радиусе
                    var threat = _cachedPlayerObjects
                        .Where(p => Vector2.DistanceSquared(div.Position, p.Position) < div.DetectionRange * div.DetectionRange)
                        .OrderBy(p => Vector2.DistanceSquared(div.Position, p.Position))
                        .FirstOrDefault();

                    if (threat != null)
                    {
                        div.Target = threat;
                        if (div.PathTask == null || div.PathTask.IsCompleted)
                        {
                            // Атакуем, но не уходим далеко от домашней позиции
                            var interceptPos = Vector2.Lerp(homePosition, threat.Position, 0.7f);
                            div.PathTask = Pathfinding.FindPathAsync(tileMap, div.Position, interceptPos);
                        }
                        continue;
                    }
                }

                // 🛡️ Нет угрозы — возвращаемся/удерживаем позицию на фронте
                if (Vector2.DistanceSquared(div.Position, homePosition) > 25f) // 5px порог
                {
                    div.Target = null;
                    if (div.PathTask == null || div.PathTask.IsCompleted)
                    {
                        div.PathTask = Pathfinding.FindPathAsync(tileMap, div.Position, homePosition);
                    }
                }
                else
                {
                    // Стоим на позиции, ищем цели в радиусе атаки
                    var targetInRange = _cachedPlayerObjects
                        .FirstOrDefault(p => Vector2.DistanceSquared(div.Position, p.Position) <= div.AttackRange * div.AttackRange);
                    div.Target = targetInRange;
                }
            }

            if (_aiSubIndex >= _cachedEnemyDivisions.Count) _aiSubIndex = 0;
        }

        // 🔧 Обрабатывает только часть фабрик за раз
        private static void UpdateBuildingsLogicBatch(int batchSize)
        {
            var factories = LogicSystem.FindGameObjectsByType(typeof(WarDotsEnemyFactory))
                                        .OfType<WarDotsEnemyFactory>()
                                        .Where(f => f.IsActive).ToList();

            int processed = 0;
            for (int i = 0; i < factories.Count && processed < batchSize; i++, processed++)
            {
                var factory = factories[i];
                if (factory.QueueLength >= factory.MaxQueueSize || GlobalResources < factory.UnitProductionCost) continue;

                bool shouldProduce = _currentStrategy switch
                {
                    AiStrategy.InfantryRush => factory is WarDotsEnemyInfantryFactory,
                    AiStrategy.TankOffensive => factory is WarDotsEnemyTankFactory,
                    AiStrategy.Balanced => true,
                    AiStrategy.Defensive => !(factory is WarDotsEnemyTankFactory),
                    AiStrategy.ArtilleryHeavy => factory is WarDotsEnemyArtilleryFactory || (factory is WarDotsEnemyInfantryFactory && GlobalResources > 300),
                    _ => true
                };

                if (!shouldProduce) continue;

                Type unitType = factory switch
                {
                    WarDotsEnemyInfantryFactory => typeof(WarDotsEnemyInfantry),
                    WarDotsEnemyTankFactory => typeof(WarDotsEnemyTank),
                    WarDotsEnemyArtilleryFactory => typeof(WarDotsEnemyArtillery),
                    _ => typeof(WarDotsEnemyDivision)
                };
                factory.EnqueueProduction(unitType);
                GlobalResources -= factory.UnitProductionCost;
            }
        }

        private static void MakeOneStrategicDecision(TileMap tileMap)
        {
            var bases = LogicSystem.FindGameObjectsByType(typeof(WarDotsEnemyBase)).Where(b => b.IsActive).ToList();
            if (bases.Count == 0) return;

            var mainBase = bases[0];
            var enemyBuildings = LogicSystem.FindGameObjectsByTag("Enemy").Where(b => b is WarDotsBuilding && b.IsActive).ToList();

            // Экономика под стратегию
            bool shouldBuildGen = _currentStrategy switch
            {
                AiStrategy.Defensive => GlobalResources >= 400 && enemyBuildings.Count(b => b is WarDotsEnemyResourceGenerator) < 3,
                AiStrategy.TankOffensive => GlobalResources >= 600 && enemyBuildings.Count(b => b is WarDotsEnemyResourceGenerator) < 2, // Танкам нужна база
                _ => false
            };

            if (shouldBuildGen && TryPlaceBuildingSmart(typeof(WarDotsEnemyResourceGenerator), mainBase.Position, tileMap, out var genPos))
            {
                var b = (WarDotsBuilding)Activator.CreateInstance(typeof(WarDotsEnemyResourceGenerator));
                b.Position = genPos; b.Tag = "Enemy"; WarDotsGame.SubmitObject(b);
                GlobalResources -= 500; return;
            }

            // Строительство заводов
            if (GlobalResources >= 500)
            {
                Type nextFactoryType = _currentStrategy switch
                {
                    AiStrategy.InfantryRush => enemyBuildings.Count(b => b is WarDotsEnemyInfantryFactory) < 3 ? typeof(WarDotsEnemyInfantryFactory) : null,
                    AiStrategy.TankOffensive => enemyBuildings.Count(b => b is WarDotsEnemyTankFactory) < 3 ? typeof(WarDotsEnemyTankFactory) : null,
                    AiStrategy.Balanced => enemyBuildings.Count(b => b is WarDotsEnemyInfantryFactory) < 2 ? typeof(WarDotsEnemyInfantryFactory) :
                                                enemyBuildings.Count(b => b is WarDotsEnemyTankFactory) < 2 ? typeof(WarDotsEnemyTankFactory) : null,
                    AiStrategy.Defensive => enemyBuildings.Count(b => b is WarDotsEnemyInfantryFactory) < 2 ? typeof(WarDotsEnemyInfantryFactory) :
                                                enemyBuildings.Count(b => b is WarDotsEnemyArtilleryFactory) < 1 ? typeof(WarDotsEnemyArtilleryFactory) : null,
                    AiStrategy.ArtilleryHeavy => enemyBuildings.Count(b => b is WarDotsEnemyArtilleryFactory) < 3 ? typeof(WarDotsEnemyArtilleryFactory) : null,
                    _ => null
                };

                if (nextFactoryType != null && TryPlaceBuildingSmart(nextFactoryType, mainBase.Position, tileMap, out var fPos))
                {
                    var b = (WarDotsBuilding)Activator.CreateInstance(nextFactoryType);
                    b.Position = fPos; b.Tag = "Enemy"; WarDotsGame.SubmitObject(b);
                    GlobalResources -= 500; return;
                }
            }
        }

        // 🔧 Оптимизированная версия с ранним выходом и ограниченным поиском
        // 🔧 НОВЫЙ МЕТОД: динамический поиск места вокруг ближайшей постройки
        private static bool TryPlaceBuildingSmart(Type buildingType, Vector2 nearPos, TileMap tileMap, out Vector2 finalPosition)
        {
            finalPosition = Vector2.Zero;
            if (tileMap == null) return false;

            // 1. Находим "якорь" - ближайшее существующее вражеское здание
            Vector2 anchor = nearPos;
            var enemyBuildings = LogicSystem.FindGameObjectsByTag("Enemy")
                .Where(b => b is WarDotsBuilding && b.IsActive)
                .ToList();

            if (enemyBuildings.Count > 0)
            {
                anchor = enemyBuildings
                    .OrderBy(b => Vector2.DistanceSquared(b.Position, nearPos))
                    .First().Position;
            }

            const float maxSearchRadius = 500f;   // Максимальный радиус поиска от базы
            const float ringStep = 50f;           // Шаг расширения кольца
            const float minBuildingDist = 45f;    // Минимальное расстояние между зданиями (чтобы не слипались)

            // 2. Поиск по расширяющимся кольцам (spiral search)
            for (float radius = 40f; radius <= maxSearchRadius; radius += ringStep)
            {
                int steps = Math.Max(6, (int)(radius / tileMap.TileWidth * 1.5f));
                for (int i = 0; i < steps; i++)
                {
                    float angle = (i / (float)steps) * MathF.Tau; // MathF.Tau = 2π
                    float checkX = anchor.X + MathF.Cos(angle) * radius;
                    float checkY = anchor.Y + MathF.Sin(angle) * radius;

                    var grid = tileMap.WorldToGridPosition(new Vector2(checkX, checkY));
                    int gx = Math.Clamp(grid.X, 1, tileMap.Width - 2);
                    int gy = Math.Clamp(grid.Y, 1, tileMap.Height - 2);

                    var tile = tileMap.GetTile(gx, gy);
                    if (tile == null || !tile.IsWalkable || tileMap.IsOccupied(gx, gy)) continue;

                    // Проверка дистанции до других зданий
                    bool tooClose = false;
                    foreach (var b in enemyBuildings)
                    {
                        if (Vector2.DistanceSquared(b.Position, new Vector2(checkX, checkY)) < minBuildingDist * minBuildingDist)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    if (tooClose) continue;

                    // Если место найдено
                    finalPosition = tileMap.GridToWorldPosition(gx, gy);
                    return true;
                }
            }

            return false; // Место не найдено в пределах радиуса
        }
    }
}