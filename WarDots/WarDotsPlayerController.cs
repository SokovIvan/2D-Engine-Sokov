using Microsoft.Xna.Framework;
using _2D_Engine_Sokov.GameObjects;
using _2D_Engine_Sokov.WarDots.Units;
using System;
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
            HandleCamera();
            HandleSelection();
            HandleCameraZoom();
            HandleCommands();
            HandlePlacementPreview();
            DrawSelection();
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
                Console.WriteLine(units.Count());
                foreach (var u in units)
                {
                    bool selected = false;
                    if (isClick)
                    {
                        // При клике выбираем юнитов в радиусе ~20 единиц от курсора
                        if (Vector2.DistanceSquared(u.Position, world) < 400f)
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

            RenderSystem.SubmitPersistentCommand(() => {
                RenderSystem.FillCircle(world, 18f, new Color(0, 255, 0, 80), 20);
                RenderSystem.DrawCircle(world, 18f, Color.White, 20);
            }, 2, useCamera: true);
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
                        var path = Pathfinding.FindPath(tileMap, u.Position, world + offset);
                        if (path?.Count > 0) u.Path = path;
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
            int gx = Math.Clamp(grid.X, 1, tileMap.Width - 2);
            int gy = Math.Clamp(grid.Y, 1, tileMap.Height - 2);

            if (tileMap.IsOccupied(gx, gy)) return;

            int cost = _buildType == typeof(WarDotsPlayerFactory) ? 100 : 80;
            if (PlayerResources < cost) return;

            PlayerResources -= cost;
            var building = (WarDotsBuilding)Activator.CreateInstance(_buildType);
            building.Position = tileMap.GridToWorldPosition(gx, gy);
            building.Tag = "Player";
            WarDotsGame.SubmitObject(building);

            _isPlacing = false;
            _buildType = null;
            Console.WriteLine("[CONTROLLER] Здание построено.");
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
    }
}