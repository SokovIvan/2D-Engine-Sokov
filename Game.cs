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
        Thread render_thread;
        Thread logic_thread;
        Thread ui_thread;
        Thread physics_thread;
        XMLParser parser;
        private Thread _renderThread;
        public bool _isRunning;
        private readonly LinkedList<GameObject> _gameObjects = new();
        private readonly int _targetFps = 60;
        private readonly int _frameTimeMs;
        Sprite testSprite;
        public Game()
        {
            _frameTimeMs = 1000 / _targetFps;
        }

        public void Run()
        {
            // Инициализация рендер-системы
            RenderSystem.Initialize(800, 600);
            PhysicsSystem.Initialize();
            // Создание и настройка спрайта
            testSprite = new Sprite
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

            _gameObjects.AddLast(testSprite);

            var ground = new Sprite
            {
                Position = new Microsoft.Xna.Framework.Vector2(-100, 400),
                Scale = new Microsoft.Xna.Framework.Vector2(2f, 0.5f),
                Size = new Microsoft.Xna.Framework.Vector2(2000, 100),
                LayerDepth = 0,
                Color = Color.Red,
                CollisionEnabled = true,
                IsStatic = true,
                Tag = "testGround"
            };
            ground.LoadTexture("C:\\Users\\IvanS\\Pictures\\canvasImage (2).png");

            _gameObjects.AddLast(ground);

            _isRunning = true;
            var lastUpdate = System.Environment.TickCount;

            // Основной игровой цикл
            while (_isRunning)
            {
                var currentTime = System.Environment.TickCount;
                var deltaTime = currentTime - lastUpdate;

                if (deltaTime >= _frameTimeMs)
                {
                    // Обновление игровой логики
                    Update();
                    lastUpdate = currentTime;
                }
                Thread.Sleep(1);
            }

            // Завершение работы
            RenderSystem.Shutdown();
            PhysicsSystem.Shutdown();
        }

        private void Update()
        {
            // Обработка ввода
            HandleInput();

            // Отправляем объекты в физическую систему
            PhysicsSystem.SubmitGameObjects(_gameObjects.ToArray());

            // Ограничиваем максимальную скорость
            /*foreach (var obj in _gameObjects)
            {
                if (obj.IsStatic) continue;

                Vector2 velocity = obj.Velocity;
                if (velocity.Length() > MAX_SPEED)
                {
                    obj.Velocity = Vector2.Normalize(velocity) * MAX_SPEED;
                }
            }*/

            Render();
        }
        private void HandleInput()
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                // Применяем импульс вместо установки скорости
                testSprite.Velocity += new Microsoft.Xna.Framework.Vector2(0, -300);
            }
        }
        private void Render()
        {
            // Отправляем только активные спрайты
            // foreach (var sprite in _gameObjects.OfType<Sprite>().Where(s => s.IsActive))
            //{
            //    RenderSystem.SubmitSprite(sprite);
            //}
            RenderSystem.SubmitSprites(_gameObjects.OfType<Sprite>().Where(s => s.IsActive).ToArray());

        }

        public void Stop()
        {
            _isRunning = false;
        }
    }
}
