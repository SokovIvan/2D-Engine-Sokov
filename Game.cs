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
        }

        public void Run()
        {
            RenderSystem.Initialize(800, 600);
            PhysicsSystem.Initialize();
            LogicSystem.Initialize();
            UISystem.Initialize(); 
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
    }
}
