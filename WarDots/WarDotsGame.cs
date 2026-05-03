using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using _2D_Engine_Sokov.GameObjects;
using _2D_Engine_Sokov.WarDots;
using _2D_Engine_Sokov.WarDots.Units;

namespace _2D_Engine_Sokov.WarDots
{
    /// <summary>
    /// Полная замена Game.cs, адаптированная под режим WarDots (RTS).
    /// Сохраняет оригинальную многопоточную архитектуру, но добавляет стратегию-специфику.
    /// </summary>
    public sealed class WarDotsGame
    {
        public GameLevel _currentLevel;
        private readonly XMLParser _parser;
        private bool _isRunning;
        private readonly int _targetFps = 60;
        private readonly int _frameTimeMs;

        public static WarDotsGame Instance { get; private set; }
        public static KeyboardState keyboardState;
        public bool IsLoading { get; private set; } = false;
        public bool IsMatchActive { get; private set; } = true;

        private int _frontlineUpdateFrames = 0;
        public WarDotsGame()
        {
            Instance = this;
            _frameTimeMs = 1000 / _targetFps;
            _parser = new XMLParser();
        }
        public void Run()
        {
            // Инициализация систем (те же потоки, что и в оригинале)
            RenderSystem.Initialize(1280, 720); // Шире для RTS-вида
            PhysicsSystem.Initialize();
            LogicSystem.Initialize();
            UISystem.Initialize();
            SoundSystem.Initialize();

            RenderSystem.EnableFrustumCulling(true);

            // 🔑 Инициализация WarDots-систем вместо старых
            WarDotsPlayerController.Initialize();
            WarDotsEnemyAI.Initialize();

            // Ждём полной инициализации графического устройства И камеры
            while (RenderSystem._graphicsDevice == null || RenderSystem.GetCamera() == null)
                Thread.Sleep(1);

            // Загрузка карты (можно заменить на меню)
            LoadLevel("Content/Levels/WarDots/Level_Test.xml");

            _isRunning = true;
            var lastUpdate = Environment.TickCount;

            // 🔁 Главный цикл (идентичен оригиналу, но с RTS-флагами)
            while (_isRunning)
            {
                var currentTime = Environment.TickCount;
                var deltaTime = currentTime - lastUpdate;

                if (deltaTime >= _frameTimeMs)
                {
                    if (!IsLoading && IsMatchActive) Update();
                    lastUpdate = currentTime;
                }
                Thread.Sleep(1);
            }

            // Корректное завершение потоков
            UISystem.Shutdown();
            LogicSystem.Shutdown();
            PhysicsSystem.Shutdown();
            RenderSystem.Shutdown();
            SoundSystem.Shutdown();
        }

        private void Update()
        {
            HandleInput(); 
            BattleMapUpdate();
            LogicUpdate();         // 📦 Сначала загрузим объекты в систему
     // 🔍 А потом уже ищем их
            PhysicsUpdate();
            UIUpdate();

            Render();

        }
        private void BattleMapUpdate()
        {
            if (GameContext.TileMap is not BattleMap battleMap) return;

            battleMap.ClearUnits(true, true);

            var players = LogicSystem.FindGameObjectsByTag("Player")
                                     .Where(u => u.IsActive)
                                     .ToList();
            var enemies = LogicSystem.FindGameObjectsByTag("Enemy")
                                     .Where(u => u.IsActive)
                                     .ToList();

            foreach (var p in players) battleMap.AddUnit(p.Position, true);
            foreach (var e in enemies) battleMap.AddUnit(e.Position, false);

            battleMap.UpdateBoundary();
            battleMap.SubmitPersistentBoundary(Color.Black, thickness: 15f, framesToLive: 15);

        }
        private void HandleInput()
        {
            keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape))
                Stop();
        }

        private void Render()
        {
            RenderSystem.PrepareNextFrame();
            if (_currentLevel?.TileMap != null)
                RenderSystem.SubmitSprite(_currentLevel.TileMap.MapSprite);

            RenderSystem.SubmitSprites(GameContext.GetGameObjects().OfType<Sprite>().Where(s => s.IsActive).ToArray());
        }

        private void PhysicsUpdate()
        {
            PhysicsSystem.SubmitGameObjects(GameContext.GetGameObjects().Where(s => s.IsActive).ToArray());
        }

        private void LogicUpdate()
        {
          //  LogicSystem.SubmitGameObjects(_gameObjects.Where(s => s.IsActive).ToArray());
        }

        private void UIUpdate()
        {
            RenderSystem.SubmitUIElements(GameContext.GetUIElements().Where(s => s.IsActive).ToArray());
        }

        public void Stop() => _isRunning = false;

        // 📦 Безопасные методы добавления/удаления (как в оригинале)
        public static void SubmitObject(GameObject obj) => GameContext.AddGameObject(obj);
        public static void SubmitUIElement(UIElement ui) => GameContext.AddUIElement(ui);

        public static void DisposeObject(GameObject obj) => GameContext.RemoveGameObject(obj);
        public static void DisposeUIElement(UIElement ui)
        {
            GameContext.RemoveUIElement(ui);
           // UISystem.UnregisterUIElement(ui); // Если нужно
            RenderSystem.RemoveUIElement(ui);
        }

        /// <summary>
        /// Загрузка уровня с RTS-спецификой (камера, гравитация, очистка)
        /// </summary>
        public void LoadLevel(string path)
        {
            IsLoading = true;
            PhysicsSystem.Pause();
            LogicSystem.Pause();
            RenderSystem.Pause();
            UISystem.Pause();
            Thread.Sleep(50);


            PhysicsSystem.ClearAllBuffers();
            RenderSystem.ClearAllBuffers();
            GameContext.ClearGameObjects();
            GameContext.ClearUIElements();
            Thread.Sleep(50);

            lock (GameContext.GetGameObjects()) GameContext.GetGameObjects().Clear();
            lock (GameContext.GetUIElements()) GameContext.GetUIElements().Clear();

            if (_currentLevel != null)
            {
                _currentLevel.uIElements?.Clear();
                _currentLevel.gameObjects?.Clear();
                _currentLevel.backgrounds?.Clear();
                _currentLevel.TileMap = null;
            }

            var newLevel = _parser.LoadLevel(path);
            _currentLevel = newLevel;
            GameContext.SetLevel(newLevel);

            if (newLevel?.gameObjects != null)
                foreach (var obj in newLevel.gameObjects.Where(o => o != null))
                    SubmitObject(obj);

            if (newLevel?.uIElements != null)
                foreach (var ui in newLevel.uIElements.Where(u => u != null))
                    SubmitUIElement(ui);

            PhysicsSystem.GRAVITY = newLevel?.gravityForce ?? 500f;
            RenderSystem.backgroundColor = newLevel?.backColor ?? Color.CornflowerBlue;
            if (newLevel?.backgrounds != null)
                RenderSystem.SubmitBackgrounds(newLevel.backgrounds.Where(b => b != null).ToArray());

            // 🎯 RTS-специфика: отдалённая камера для тактического обзора
            var cam = RenderSystem.GetCamera();
            if (cam != null && newLevel?.TileMap != null)
            {
                var map = newLevel.TileMap;
                float mapWorldWidth = map.Width * map.TileWidth;
                float mapWorldHeight = map.Height * map.TileHeight;

                cam.CenterOn(new Vector2(mapWorldWidth / 2f, mapWorldHeight / 2f));

                // Начальный зум — чтобы карта почти полностью помещалась
                float zoomX = RenderSystem._graphicsDevice.Viewport.Width / mapWorldWidth * 0.9f;
                float zoomY = RenderSystem._graphicsDevice.Viewport.Height / mapWorldHeight * 0.9f;
                cam.Zoom = Math.Min(zoomX, zoomY);

                Console.WriteLine($"[CAMERA] Карта {mapWorldWidth}x{mapWorldHeight} | Начальный зум: {cam.Zoom:F3}");
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            PhysicsSystem.Resume();
            LogicSystem.Resume();
            RenderSystem.Resume();
            UISystem.Resume();

            if (!string.IsNullOrEmpty(newLevel?.MusicPath))
                SoundSystem.PlayBackgroundMusic(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, newLevel.MusicPath));
            else
                SoundSystem.StopMusic();

            IsLoading = false;
            //SpawnInitialUnitsAndBuildings();
            IsMatchActive = true;
        }

        /// <summary>
        /// Корректное завершение матча (победа/поражение)
        /// </summary>
        public void EndMatch(bool playerWon)
        {
            if (!IsMatchActive) return;
            IsMatchActive = false;
            LogicSystem.Pause();
            PhysicsSystem.Pause();

            Console.WriteLine(playerWon ? "[WAR DOTS] 🌸 ПОБЕДА!" : "[WAR DOTS] 💔 ПОРАЖЕНИЕ.");
            // Здесь можно подключить загрузку экрана результатов через UISystem или LoadLevel
        }       
    }

    // 🔒 Алиас совместимости: чтобы Unit.cs и Building.cs не требовали замены Game.instance
    public static class Game
    {
        public static WarDotsGame instance => WarDotsGame.Instance;
    }
}