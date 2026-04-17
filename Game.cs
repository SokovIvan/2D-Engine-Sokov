using Microsoft.Xna.Framework.Input;

namespace _2D_Engine_Sokov
{
    internal class Game
    {
        public GameLevel _currentLevel;

        XMLParser parser;
        private Thread _renderThread;
        public bool _isRunning;
        private LinkedList<GameObject> _gameObjects = new();
        private LinkedList<UIElement> _UIElements = new();
        private readonly int _targetFps = 60;
        private readonly int _frameTimeMs;

        public static Game instance;
        public static KeyboardState keyboardState;
        public bool loading = false;
        public Game()
        {
            instance = this;
            _frameTimeMs = 1000 / _targetFps;
            parser = new XMLParser();
        }
        public List<GameObject> get_gameObjects() { 
            return _gameObjects.ToList();
        }
        public void Run()
        {

            RenderSystem.Initialize(800, 600);
            PhysicsSystem.Initialize();
            LogicSystem.Initialize();
            UISystem.Initialize();
            RenderSystem.EnableFrustumCulling(true);
            EnemyAI.Initialize();
            SoundSystem.Initialize();

            while (RenderSystem._graphicsDevice==null) { }
            LoadLevel("Content/Levels/LevelMenu.xml");

            _isRunning = true;
            var lastUpdate = System.Environment.TickCount;
            while (_isRunning)
            {
                var currentTime = System.Environment.TickCount;
                var deltaTime = currentTime - lastUpdate;

                if (deltaTime >= _frameTimeMs)
                {
                    if(!loading)
                    Update();
                    lastUpdate = currentTime;
                }
                Thread.Sleep(1);
            }
            UISystem.Shutdown();
            LogicSystem.Shutdown();
            PhysicsSystem.Shutdown();
            RenderSystem.Shutdown();
            SoundSystem.Shutdown();

        }
        public static void DisposeObject(GameObject gameObject)
        {
            if(instance._gameObjects.Contains(gameObject))
            instance._gameObjects.Remove(gameObject);
        }
        public static void DisposeUIElement(UIElement uIElement)
        {
            if (instance._UIElements.Contains(uIElement)) {
                UISystem.UnregisterUIElement(uIElement);
                instance._UIElements.Remove(uIElement);
                RenderSystem.RemoveUIElement(uIElement);
            }

        }
        public static void SubmitObject(GameObject gameObject) {
            instance._gameObjects.AddLast(gameObject);
        }
        public static void SubmitUIElement(UIElement uIElement)
        {
            instance._UIElements.AddLast(uIElement);
        }
        private void Update()
        {  
            HandleInput();
            // Синхронизация перед обновлением
            if (loading) return;
            LogicUpdate();
            PhysicsUpdate();
            UIUpdate();  
            Render();
        }

        private void HandleInput()
        {
            keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                Stop();
            }
        }
        private void Render()
        {
            if(instance._currentLevel.TileMap!=null) RenderSystem.SubmitSprite(instance._currentLevel.TileMap.MapSprite);
            RenderSystem.SubmitSprites(_gameObjects.OfType<Sprite>().Where(s => s.IsActive).ToArray());
        }
        private void PhysicsUpdate()
        {
            PhysicsSystem.SubmitGameObjects(_gameObjects.Where(s => s.IsActive).ToArray());
        }
        private void LogicUpdate()
        {
            LogicSystem.SubmitGameObjects(_gameObjects.Where(s => s.IsActive).ToArray());
        }
        private void UIUpdate()
        {
            RenderSystem.SubmitUIElements(_UIElements.Where(s => s.IsActive).ToArray()); 
        }
        public void Stop()
        {
            _isRunning = false;
        }
        public void LoadLevel(string path)
        {
            loading = true;
            PhysicsSystem.Pause();
            LogicSystem.Pause();
            RenderSystem.Pause();
            UISystem.Pause();
            Thread.Sleep(50);
            // Полная очистка всех систем
            LogicSystem.ClearAllBuffers();
            PhysicsSystem.ClearAllBuffers();
            RenderSystem.ClearAllBuffers();
            UISystem.ClearAllUIElements();
            Thread.Sleep(50);
            // Очистка внутренних списков
            lock (_gameObjects) // Важно: защищаем список игры
            {
                _gameObjects.Clear();
            }
            lock (_UIElements)
            {
                _UIElements.Clear();
            }

            if (_currentLevel != null)
            {
                _currentLevel.uIElements.Clear();
                _currentLevel.gameObjects.Clear();
                _currentLevel.backgrounds.Clear();
                _currentLevel.TileMap = null;
            }

            // Загрузка нового уровня
            var newLevel = parser.LoadLevel(path);
            _currentLevel = newLevel;

            // Добавление новых объектов с проверкой на null
            foreach (var obj in newLevel.gameObjects.Where(obj => obj != null))
            {
                SubmitObject(obj);
            }

            foreach (var ui in newLevel.uIElements.Where(ui => ui != null))
            {
                SubmitUIElement(ui);
            }

            // Установка параметров уровня
            PhysicsSystem.GRAVITY = newLevel.gravityForce;
            RenderSystem.backgroundColor = newLevel.backColor;
            RenderSystem.SubmitBackgrounds(newLevel.backgrounds.Where(b => b != null).ToArray());

            // Принудительная сборка мусора
            GC.Collect();
            GC.WaitForPendingFinalizers();

            PhysicsSystem.Resume();
            LogicSystem.Resume();
            RenderSystem.Resume();
            UISystem.Resume();
            if (!string.IsNullOrEmpty(_currentLevel.MusicPath))
            {
                // Можно сделать путь абсолютным, если нужно
                string musicPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _currentLevel.MusicPath);
                // или просто передать как есть, если путь относительный от .exe
                SoundSystem.PlayBackgroundMusic(musicPath);
            }
            else SoundSystem.StopMusic();
            loading = false;
        }

    }
}
