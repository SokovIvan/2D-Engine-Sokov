using _2D_Engine_Sokov.GameObjects;
using _2D_Engine_Sokov.UIElements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace _2D_Engine_Sokov
{
    internal class Game
    {
        private GameLevel _currentLevel;

        XMLParser parser;
        private Thread _renderThread;
        public bool _isRunning;
        private readonly LinkedList<GameObject> _gameObjects = new();
        private readonly LinkedList<UIElement> _UIElements = new();
        private readonly int _targetFps = 60;
        private readonly int _frameTimeMs;

        public static Game instance;
        public static KeyboardState keyboardState;

        public Game()
        {
            instance = this;
            _frameTimeMs = 1000 / _targetFps;
            parser = new XMLParser();
        }

        public void Run()
        {
            RenderSystem.Initialize(800, 600);
            PhysicsSystem.Initialize();
            LogicSystem.Initialize();
            UISystem.Initialize();
            RenderSystem.EnableFrustumCulling(true);
            // Загрузка начального уровня
            LoadLevel("Content/Levels/Scene_0.xml");

            _isRunning = true;
            var lastUpdate = System.Environment.TickCount;
            while (_isRunning)
            {
                var currentTime = System.Environment.TickCount;
                var deltaTime = currentTime - lastUpdate;

                if (deltaTime >= _frameTimeMs)
                {
                    Update();
                    lastUpdate = currentTime;
                }
                Thread.Sleep(1);
            }
            UISystem.Shutdown();
            LogicSystem.Shutdown();
            PhysicsSystem.Shutdown();
            RenderSystem.Shutdown();

        }
        public static void DisposeObject(GameObject gameObject)
        {
            if(instance._gameObjects.Contains(gameObject))
            instance._gameObjects.(gameObject);
        }
        public static void DisposeUIElement(UIElement uIElement)
        {
            if(instance._UIElements.Contains(uIElement))
            instance._UIElements.Remove(uIElement);
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
            var newLevel = parser.LoadLevel(path);

            // Очистка текущих объектов
            _gameObjects.Clear();
            _UIElements.Clear();

            // Добавление новых объектов
            foreach (var obj in newLevel.gameObjects)
            {
                SubmitObject(obj);
            }

            foreach (var ui in newLevel.uIElements)
            {
                SubmitUIElement(ui);
            }

            // Установка параметров уровня
            PhysicsSystem.GRAVITY = newLevel.gravityForce;
            RenderSystem.backgroundColor = newLevel.backColor;
            RenderSystem.SubmitBackgrounds(newLevel.backgrounds.ToArray());
            _currentLevel = newLevel;
        }
    }
}
