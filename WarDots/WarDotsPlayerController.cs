using Microsoft.Xna.Framework;
using _2D_Engine_Sokov.WarDots.Units;
using Microsoft.Xna.Framework.Input;

namespace _2D_Engine_Sokov.WarDots
{
    public static class WarDotsPlayerController
    {
        public static int PlayerResources { get; set; } = 200;

        private static readonly List<WarDotsPlayerDivision> _selectedUnits = new();
        private static Vector2 _selStart, _selEnd;
        private static bool _isSelecting;

        private static Type _buildType;
        private static bool _isPlacing;
        private static int _previousScrollValue = 0;
        public static void Initialize()
        {
            LogicSystem.OnLogicUpdate += Update;
            Console.WriteLine("[CONTROLLER] WarDotsPlayerController подключён к LogicSystem.");
        }

        private static void Update()
        {
            if (GameContext.CurrentLevel?.Name != "LevelMenu") {
                HandleCamera();
                HandleSelection();
                HandleCameraZoom();
                HandleCommands();
                HandlePlacementPreview();
                DrawSelection();
            }

        }
        private static void HandleCamera()
        {
            var cam = RenderSystem.GetCamera();
            if (cam == null) return;

            var kb = InputSystem.GetKeyboardState();
            //Console.WriteLine($"GetKeyboardState: {kb.GetPressedKeys().ToString()}");
            float speed = 12f;

            if (kb.IsKeyDown(Keys.W)) cam.Move(Vector2.UnitY * -speed);
            if (kb.IsKeyDown(Keys.S)) cam.Move(Vector2.UnitY * speed);
            if (kb.IsKeyDown(Keys.A)) cam.Move(Vector2.UnitX * -speed);
            if (kb.IsKeyDown(Keys.D)) cam.Move(Vector2.UnitX * speed);
        }

        private static void HandleSelection()
        {
            var cam = RenderSystem.GetCamera();
            if (cam == null) return;

            var mouse = InputSystem.GetMouseState();
            var world = Vector2.Transform(new Vector2(mouse.X, mouse.Y), Matrix.Invert(cam.TransformMatrix));

            if (mouse.LeftButton == ButtonState.Pressed)
            {
                if (!_isSelecting) { _selStart = world; _isSelecting = true; }
                _selEnd = world;
            }
            else if (mouse.LeftButton == ButtonState.Released && _isSelecting)
            {
                _isSelecting = false;
                _selectedUnits.Clear();

                float minX = Math.Min(_selStart.X, _selEnd.X);
                float maxX = Math.Max(_selStart.X, _selEnd.X);
                float minY = Math.Min(_selStart.Y, _selEnd.Y);
                float maxY = Math.Max(_selStart.Y, _selEnd.Y);

                float width = maxX - minX;
                float height = maxY - minY;
                bool isClick = width < 5f && height < 5f;

                var units = GameContext.GetGameObjects().OfType<WarDotsPlayerDivision>()
                                         .Where(u => u.IsActive && u.Tag=="Player");
                //Console.WriteLine(units.Count());
                foreach (var u in units)
                {
                    bool selected = false;
                    if (isClick)
                    {
                        // При клике выбираем юнитов в радиусе ~20 единиц от курсора
                        if (Vector2.DistanceSquared(u.Position, world) < 640f)
                            selected = true;
                    }
                    else
                    {
                        // При рамке проверяем точные float-границы без округления
                        if (u.Position.X >= minX && u.Position.X <= maxX &&
                            u.Position.Y >= minY && u.Position.Y <= maxY)
                            selected = true;
                    }

                    if (selected) _selectedUnits.Add(u);
                }

                _selStart = _selEnd = Vector2.Zero;
            }
        }
        private static void HandleCameraZoom()
        {
            var cam = RenderSystem.GetCamera();
            if (cam == null) return;

            var mouse = InputSystem.GetMouseState();
            int scroll = mouse.ScrollWheelValue - _previousScrollValue;

            if (scroll != 0)
            {
                float zoomFactor = 1.1f; // скорость зума
                if (scroll > 0)
                    cam.Zoom *= zoomFactor;
                else
                    cam.Zoom /= zoomFactor;

                // Ограничения зума
                cam.Zoom = MathHelper.Clamp(cam.Zoom, 0.2f, 10.0f);
            }

            _previousScrollValue = mouse.ScrollWheelValue;
        }
        private static void HandlePlacementPreview()
        {
            var cam = RenderSystem.GetCamera();
            if (cam == null || !_isPlacing) return;

            var mouse = Mouse.GetState();
            var world = Vector2.Transform(new Vector2(mouse.X, mouse.Y), Matrix.Invert(cam.TransformMatrix));

            // Проверяем пригодность места в реальном времени
            bool isValid = CheckPlacementValidity(world, out Color previewColor);

            RenderSystem.SubmitPersistentCommand(() => {
                // Зелёный круг, если место подходит, красный - если нет
                RenderSystem.FillCircle(world, 18f, previewColor, 20);
                // Обводка тоже меняет цвет для чёткости
                RenderSystem.DrawCircle(world, 18f, isValid ? Color.White : new Color(255, 80, 80), 20);
            }, 2, useCamera: true);
        }

        private static bool CheckPlacementValidity(Vector2 worldPos, out Color color)
        {
            // По умолчанию - красный полупрозрачный (непригодно)
            color = new Color(255, 50, 50, 110);

            var tileMap = GameContext.TileMap;
            if (tileMap == null) return false;

            var grid = tileMap.WorldToGridPosition(worldPos);
            int gx = Math.Clamp(grid.X, 0, tileMap.Width - 1);
            int gy = Math.Clamp(grid.Y, 0, tileMap.Height - 1);

            var tile = tileMap.GetTile(gx, gy);
            // 1. Проверка проходимости (земля, трава, камень...)
            if (tile == null || !tile.IsWalkable) return false;
            // 2. Клетка уже занята?
            if (tileMap.IsOccupied(gx, gy)) return false;

            // 3. Проверка близости к твоим зданиям
            const float maxBuildDistance = 500f;
            var playerBuildings = GameContext.GetGameObjects()
                .OfType<WarDotsBuilding>()
                .Where(b => b.Tag == "Player");

            if (playerBuildings.Any())
            {
                bool isNear = false;
                foreach (var b in playerBuildings)
                {
                    // DistanceSquared быстрее и безопаснее для превью
                    if (Vector2.DistanceSquared(b.Position, worldPos) <= maxBuildDistance * maxBuildDistance)
                    {
                        isNear = true;
                        break;
                    }
                }
                // Если построек нет, разрешаем (первая база). Если есть, но далеко - запрет.
                if (!isNear) return false;
            }

            // Если все условия выполнены - меняем на нежно-зелёный
            color = new Color(0, 255, 0, 110);
            return true;
        }

        private static void HandleCommands()
        {
            var mouse = InputSystem.GetMouseState();
            if (mouse.RightButton == ButtonState.Pressed)
            {
                var cam = RenderSystem.GetCamera();
                var world = Vector2.Transform(new Vector2(mouse.X, mouse.Y), Matrix.Invert(cam.TransformMatrix));
                var tileMap = GameContext.TileMap;

                if (_isPlacing && _buildType != null)
                {
                    TryPlaceBuilding(world, tileMap);
                }

                else if (_selectedUnits.Count > 0)
                {
                    Vector2 offset = Vector2.Zero;
                    foreach (var u in _selectedUnits)
                    {
                        // 🔥 Запускаем расчёт в фоне!
                        u.PathTask = Pathfinding.FindPathAsync(tileMap, u.Position, world + offset);
                        // Очищаем старый путь, чтобы юнит не метался, пока считается новый
                        u.Path.Clear();

                        offset += new Vector2(18f, 18f);
                    }
                }
            }
        }


        public static void RequestPlacement(Type buildingType)
        {
            _buildType = buildingType;
            _isPlacing = true;
        }

        private static void TryPlaceBuilding(Vector2 worldPos, TileMap tileMap)
        {
            if (tileMap == null) return;

            var grid = tileMap.WorldToGridPosition(worldPos);
            // Защита от выхода за границы карты
            int gx = Math.Clamp(grid.X, 0, tileMap.Width - 1);
            int gy = Math.Clamp(grid.Y, 0, tileMap.Height - 1);

            // 1. Проверка пригодности территории (клетка должна быть проходимой/земной)
            var tile = tileMap.GetTile(gx, gy);
            if (tile == null || !tile.IsWalkable)
            {
                Console.WriteLine("[CONTROLLER] Здесь нельзя строить: непригодная местность.");
                return;
            }

            // 2. Клетка уже занята объектом?
            if (tileMap.IsOccupied(gx, gy)) return;

            // 3. Проверка близости к твоим зданиям
            const float maxBuildDistance = 500f; // Настрой под размер своего тайла (обычно 3-4 клетки)
            bool isNearPlayerStructure = false;

            var existingPlayerBuildings = GameContext.GetGameObjects()
                .OfType<WarDotsBuilding>()
                .Where(b => b.Tag == "Player");

            if (existingPlayerBuildings.Any())
            {
                foreach (var b in existingPlayerBuildings)
                {
                    // Используем DistanceSquared для производительности
                    if (Vector2.DistanceSquared(b.Position, worldPos) <= maxBuildDistance * maxBuildDistance)
                    {
                        isNearPlayerStructure = true;
                        break;
                    }
                }

                if (!isNearPlayerStructure)
                {
                    Console.WriteLine("[CONTROLLER] Слишком далеко от своих зданий.");
                    return;
                }
            }
            // Если построек нет ни одной — разрешаем размещение первой базы

            // 4. Проверка ресурсов
            int cost = 0;
            if (_buildType == typeof(WarDotsPlayerInfantryFactory)) cost = 500;
            else if (_buildType == typeof(WarDotsPlayerTankFactory)) cost = 500;
            else if (_buildType == typeof(WarDotsPlayerArtilleryFactory)) cost = 500;
            else if (_buildType == typeof(WarDotsPlayerResourceGenerator)) cost = 500;
            else return; // Неизвестный тип здания

            if (PlayerResources < cost) return;

            // 5. Размещение
            PlayerResources -= cost;
            var building = (WarDotsBuilding)Activator.CreateInstance(_buildType);
            building.Position = tileMap.GridToWorldPosition(gx, gy);
            building.Tag = "Player";
            WarDotsGame.SubmitObject(building);

            _isPlacing = false;
            _buildType = null;
            Console.WriteLine("[CONTROLLER] Здание успешно построено.");
        }

        // Отрисовка рамки выделения
        public static void DrawSelection()
        {
            if (!_isSelecting) return;
            var rect = new Rectangle(
                (int)Math.Min(_selStart.X, _selEnd.X),
                (int)Math.Min(_selStart.Y, _selEnd.Y),
                (int)Math.Abs(_selEnd.X - _selStart.X),
                (int)Math.Abs(_selEnd.Y - _selStart.Y)
            );
            RenderSystem.SubmitPersistentCommand(() => RenderSystem.DrawRectangle(rect, Color.Red, 5f), 2, useCamera: true);
        }
        // Добавь это в конец класса WarDotsPlayerController, чтобы UI мог читать выбор
        public static IReadOnlyList<WarDotsPlayerDivision> SelectedUnits => _selectedUnits;
    }
}