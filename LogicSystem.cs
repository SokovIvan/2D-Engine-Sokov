using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _2D_Engine_Sokov.GameObjects;
using _2D_Engine_Sokov.UIElements;

namespace _2D_Engine_Sokov
{

    internal class LogicSystem
    {
        private static bool _isRunning;
        private static Thread _logicThread;

        // Система двойной буферизации 
        private static readonly List<GameObject> _frameBufferA = new List<GameObject>();
        private static readonly List<GameObject> _frameBufferB = new List<GameObject>();
        private static List<GameObject> _currentLogicList = _frameBufferA;
        private static List<GameObject> _nextLogicList = _frameBufferB;
        private static readonly object _bufferLock = new object();
        // Для синхронизации обновлений
        private static readonly object _updateLock = new object();
        private static bool _needsUpdate = false;
        private static int _targetUpdateRate = 60; 
        private static double _updateInterval = 1000.0 / _targetUpdateRate;

        public static event Action OnLogicUpdate;

        public static void Initialize()
        {
            if (_isRunning) return;

            _isRunning = true;

            _logicThread = new Thread(LogicThreadLoop)
            {
                IsBackground = true,
                Name = "LogicThread",
                Priority = ThreadPriority.AboveNormal 
            };

            _logicThread.Start();
            UIActionsInitialise();
            //TestInititalise();
        }
        public static GameObject FindGameObjectByTag(string tag) {
            if (_currentLogicList.Count > 0) {
                foreach (GameObject go in _currentLogicList) { 
                    if(go.Tag==tag) return go;
                }
            }
            return null;
        }
        public static GameObject[] FindGameObjectsByTag(string tag)
        {
            if (_currentLogicList.Count > 0)
            return _currentLogicList.Where(s => s.IsActive && s.Tag == tag).ToArray();
            return Array.Empty<GameObject>();
        }
        public static GameObject[] FindGameObjectsByType(Type type)
        {
            if (_currentLogicList.Count > 0)
                return _currentLogicList.Where(s => s.IsActive && (s.GetType() == type || s.GetType().IsSubclassOf(type))).ToArray();
            return Array.Empty<GameObject>();
        }
        static void UIActionsInitialise() {
            UIActions.RegisterAction("StopEnemies", () => {
                Console.WriteLine("StopEnemies");
                if(FindGameObjectByTag("Enemy")!=null)
                FindGameObjectByTag("Enemy").IsActive = false;
            });
        }
        public static GameObject[] FindGameObjectsByName(string name)
        {
            if (_currentLogicList.Count > 0)
                return _currentLogicList.Where(s => s.IsActive && s.Name == name).ToArray();
            return Array.Empty<GameObject>();
        }
        public static GameObject FindGameObjectByName(string name)
        {
            if (_currentLogicList.Count > 0)
            {
                foreach (GameObject go in _currentLogicList)
                {
                    if (go.Name == name) return go;
                }
            }
            return null;
        }

        static void TestInititalise() {
            Sprite testSprite;
            testSprite = new Player
            {
                Position = new Microsoft.Xna.Framework.Vector2(0, 0),
                Scale = new Microsoft.Xna.Framework.Vector2(0.5f, 0.5f),
                Size = new Microsoft.Xna.Framework.Vector2(800, 400),
                LayerDepth = 1,
                Color = Color.White,
                CollisionEnabled = true,
                GravityEnabled = true,
                Mass = 100.0f,
                Tag = "testSprite"
            };
            testSprite.LoadTexture("C:\\Users\\IvanS\\Pictures\\canvasImage (2).png");
            var ground = new Sprite
            {
                Position = new Microsoft.Xna.Framework.Vector2(-100, 400),
                Scale = new Microsoft.Xna.Framework.Vector2(2.5f, 0.5f),
                Size = new Microsoft.Xna.Framework.Vector2(2000, 200),
                LayerDepth = 0,
                Color = Color.Red,
                CollisionEnabled = true,
                IsStatic = true,
                Tag = "testGround"
            };
            ground.LoadTexture("C:\\Users\\IvanS\\Pictures\\canvasImage (2).png");
            Game.SubmitObject(testSprite);
            Game.SubmitObject(ground);


            var button = new Button();
            button.Position = new Microsoft.Xna.Framework.Vector2(100, 100);
            button.Scale = new Microsoft.Xna.Framework.Vector2(0.5f, 0.5f);
            button.LoadTexture("C:\\Users\\IvanS\\Projects\\geass.png");
            button.OnClick = () => Console.WriteLine("Button clicked!");
            Game.SubmitUIElement(button);

        }
        private static void LogicThreadLoop()
        {
            double lastUpdateTime = 0;
            var timer = System.Diagnostics.Stopwatch.StartNew();

            while (_isRunning)
            {
                double currentTime = timer.Elapsed.TotalMilliseconds;
                double deltaTime = currentTime - lastUpdateTime;

                if (deltaTime >= _updateInterval)
                {
                    lastUpdateTime = currentTime;
                    UpdateLogic(deltaTime / 1000.0); 
                    lock (_updateLock)
                    {
                            SwapBuffers();
                    }
                }
                else
                {
                    int sleepTime = (int)(_updateInterval - deltaTime);
                    if (sleepTime > 0)
                        Thread.Sleep(sleepTime);
                }
            }
        }
        private static void UpdateLogic(double deltaTime)
        {
           
            OnLogicUpdate?.Invoke();
            try
            {
                foreach (var gameObject in _currentLogicList)
                {
                    if (gameObject.IsActive)
                        gameObject.Update(deltaTime);
                }
            }
            catch (Exception e) { 
            //pass
            }
        }
        private static void SwapBuffers()
        {
            lock (_bufferLock)
            {
                var temp = _currentLogicList;
                _currentLogicList = _nextLogicList;
                _nextLogicList = new List<GameObject>();


            }
        }

        public static void RequestBufferSwap()
        {
            lock (_updateLock)
            {
                _needsUpdate = true;
            }
        }

        public static void SubmitGameObject(GameObject gameObject)
        {
            if (gameObject == null) return;

            lock (_bufferLock)
            {
                if (!_nextLogicList.Contains(gameObject))
                {
                    _nextLogicList.Add(gameObject);
                }
            }
        }
        public static void SubmitGameObjects(GameObject[] gameObjects)
        {
            foreach (GameObject gameObject in gameObjects)
            {
                if (gameObject == null) return;

                lock (_bufferLock)
                {
                    if (!_nextLogicList.Contains(gameObject))
                    {
                        _nextLogicList.Add(gameObject);
                    }
                }
            }
        }
        public static void RemoveGameObject(GameObject gameObject)
        {
            if (gameObject == null) return;

            lock (_bufferLock)
            {
                _nextLogicList.Remove(gameObject);
                _currentLogicList.Remove(gameObject);
            }
        }

        public static void Shutdown()
        {
            _isRunning = false;
            _logicThread?.Join(); 
        }

        public static void SetUpdateRate(int updatesPerSecond)
        {
            _targetUpdateRate = updatesPerSecond;
            _updateInterval = 1000.0 / _targetUpdateRate;
        }
    }
}
