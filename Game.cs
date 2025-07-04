using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public Game()
        {
            _frameTimeMs = 1000 / _targetFps;
        }

        public void Run()
        {
            // Инициализация рендер-системы
            RenderSystem.Initialize(800, 600);

            // Создание и настройка спрайта
            var sprite = new Sprite
            {
                Position = new Vector2(0, 0),
                Scale = new Vector2(0.5f, 0.5f),
                LayerDepth = 1,
                Color = Color.White
            };
            sprite.LoadTexture("C:\\Users\\IvanS\\Pictures\\canvasImage (2).png");

            _gameObjects.AddLast(sprite);

            // Создание и настройка спрайта
            sprite = new Sprite
            {
                Position = new Vector2(0, 0),
                Scale = new Vector2(1f, 1f),
                LayerDepth = 0,
                Color = Color.Red
            };
            sprite.LoadTexture("C:\\Users\\IvanS\\Pictures\\canvasImage (2).png");

            _gameObjects.AddLast(sprite);

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

                    // Отправка спрайтов на рендеринг
                    Render();

                    lastUpdate = currentTime;
                }

                Thread.Sleep(1);
            }

            // Завершение работы
            RenderSystem.Shutdown();
        }

        private void Update()
        {
            // Здесь будет обновление игровой логики
            // Можно добавить движение, анимации и т.д.
        }

        private void Render()
        {
            // Отправляем только активные спрайты
            foreach (var sprite in _gameObjects.OfType<Sprite>().Where(s => s.IsActive))
            {
                RenderSystem.SubmitSprite(sprite);
            }
        }

        public void Stop()
        {
            _isRunning = false;
        }
    }
}
