using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace _2D_Engine_Sokov
{
    public class RenderSystem
    {
        private static Microsoft.Xna.Framework.Game _game;
        private static GraphicsDevice _graphicsDevice;
        private static SpriteBatch _spriteBatch;
        private static bool _isRunning;

        // Система двойной буферизации для спрайтов
        private static readonly List<Sprite> _frameBufferA = new List<Sprite>();
        private static readonly List<Sprite> _frameBufferB = new List<Sprite>();
        private static List<Sprite> _currentRenderList = _frameBufferA;
        private static List<Sprite> _nextFrameList = _frameBufferB;
        private static readonly object _bufferLock = new object();

        // Очередь загрузки текстур
        private static readonly ConcurrentQueue<(Sprite, string)> _textureLoadQueue = new();

        public static void Initialize(int width, int height)
        {
            _isRunning = true;
            new Thread(() => {
                _game = new RenderGame(width, height);
                var graphics = new GraphicsDeviceManager(_game)
                {
                    PreferredBackBufferWidth = width,
                    PreferredBackBufferHeight = height,
                    SynchronizeWithVerticalRetrace = true
                };
                graphics.ApplyChanges();
                _graphicsDevice = graphics.GraphicsDevice;
                _game.Run();
            })
            {
                IsBackground = true,
                Name = "RenderThread"
            }.Start();
        }

        public static void Shutdown()
        {
            _isRunning = false;
            _game?.Exit();
        }

        private class RenderGame : Microsoft.Xna.Framework.Game
        {
            private readonly int _width;
            private readonly int _height;

            public RenderGame(int width, int height)
            {

                _width = width;
                _height = height;
                
            }

            protected override void Initialize()
            {                
                _spriteBatch = new SpriteBatch(_graphicsDevice);
                IsMouseVisible = true;
                base.Initialize();
            }

            protected override void Update(GameTime gameTime)
            {
                // Обработка запросов на загрузку текстур
                ProcessTextureLoadRequests();
                base.Update(gameTime);
            }

            protected override void Draw(GameTime gameTime)
            {
                if (!_isRunning)
                {
                    Exit();
                    return;
                }

                // Получение списка спрайтов для рендеринга
                List<Sprite> renderList;
                lock (_bufferLock)
                {
                    renderList = _currentRenderList;

                    // Меняем буферы местами
                    var temp = _currentRenderList;
                    _currentRenderList = _nextFrameList;
                    _nextFrameList = temp;
                }

                // Рендеринг
                _graphicsDevice.Clear(Color.CornflowerBlue);

                if (renderList.Count > 0)
                {
                    _spriteBatch.Begin(
                        sortMode: SpriteSortMode.FrontToBack,
                        blendState: BlendState.AlphaBlend
                    );

                    foreach (var sprite in renderList.OrderBy(s => s.LayerDepth))
                    {
                        if (sprite.Texture == null) continue;

                        _spriteBatch.Draw(
                            texture: sprite.Texture,
                            position: sprite.Position,
                            sourceRectangle: sprite.SourceRectangle,
                            color: sprite.Color,
                            rotation: sprite.Rotation,
                            origin: sprite.Origin,
                            scale: sprite.Scale,
                            effects: sprite.Effects,
                            layerDepth: sprite.LayerDepth
                        );
                    }

                    _spriteBatch.End();
                }

                base.Draw(gameTime);
            }

            private void ProcessTextureLoadRequests()
            {
                while (_textureLoadQueue.TryDequeue(out var request))
                {
                    try
                    {
                        using var stream = File.OpenRead(request.Item2);
                        request.Item1.Texture = Texture2D.FromStream(_graphicsDevice, stream);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading texture: {ex.Message}");
                    }
                }
            }
        }

        public static void SubmitSprite(Sprite sprite)
        {
            if (sprite == null) return;

            lock (_bufferLock)
            {
                // Добавляем только если спрайт еще не был добавлен
                if (!_nextFrameList.Contains(sprite))
                {
                    _nextFrameList.Add(sprite);
                }
            }
        }

        public static void EnqueueTextureLoad(Sprite sprite, string path)
        {
            if (sprite == null || string.IsNullOrEmpty(path)) return;
            _textureLoadQueue.Enqueue((sprite, path));
        }

        public static void ClearFrameBuffer()
        {
            lock (_bufferLock)
            {
                _nextFrameList.Clear();
            }
        }
    }
}
