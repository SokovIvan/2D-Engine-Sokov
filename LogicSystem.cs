using System;
using System.Linq;
using System.Threading;
using _2D_Engine_Sokov.GameObjects;
using _2D_Engine_Sokov.UIElements;

namespace _2D_Engine_Sokov
{
    internal class LogicSystem
    {
        private static bool _isRunning;
        private static Thread _logicThread;
        private static volatile bool _isPaused = false;

        public static event Action OnLogicUpdate;

        public static void Pause() => _isPaused = true;
        public static void Resume() => _isPaused = false;

        public static void Initialize()
        {
            if (_isRunning) return;
            _isRunning = true;
            _logicThread = new Thread(LogicThreadLoop) { IsBackground = true, Name = "LogicThread", Priority = ThreadPriority.AboveNormal };
            _logicThread.Start();

        }

        // 🔄 Обновление теперь берёт безопасный снимок из контекста
        private static void UpdateLogic(double deltaTime)
        {
            InputSystem.Update();
            OnLogicUpdate?.Invoke();

            var objects = GameContext.GetGameObjects(); // 🔒 Thread-safe snapshot
            foreach (var gameObject in objects)
            {
                if (gameObject.IsActive)
                    gameObject.Update(deltaTime);
            }
        }

        private static void LogicThreadLoop()
        {
            double lastUpdateTime = 0;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            double updateInterval = 1000.0 / 60;

            while (_isRunning)
            {
                if (_isPaused) { Thread.Sleep(10); continue; }

                double currentTime = timer.Elapsed.TotalMilliseconds;
                double deltaTime = currentTime - lastUpdateTime;

                if (deltaTime >= updateInterval)
                {
                    lastUpdateTime = currentTime;
                    UpdateLogic(deltaTime / 1000.0);
                }
                else
                {
                    int sleepTime = (int)(updateInterval - deltaTime);
                    if (sleepTime > 0) Thread.Sleep(sleepTime);
                }
            }
        }

        // 📝 Поиск теперь делегируется контексту
        public static GameObject FindGameObjectByTag(string tag) => GameContext.FindGameObjectByTag(tag);
        public static GameObject[] FindGameObjectsByTag(string tag) => GameContext.FindGameObjectsByTag(tag);
        public static GameObject[] FindGameObjectsByType(Type type) => GameContext.FindGameObjectsByType(type);
        public static GameObject[] FindGameObjectsByName(string name) => GameContext.FindGameObjectsByName(name);
        public static GameObject FindGameObjectByName(string name) => GameContext.FindGameObjectByName(name);

        public static void Shutdown() { _isRunning = false; _logicThread?.Join(); }

        // 🔧 UIActions оставляем без изменений, они используют GameContext.Find...

    }
}