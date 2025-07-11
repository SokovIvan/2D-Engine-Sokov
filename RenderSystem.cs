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
        private static readonly ConcurrentQueue<(UIElement, string)> _textureLoadQueueUI = new();
        // Добавляем камеру
        private static Camera _camera;

        // В класс RenderSystem добавляем статический список для UI элементов
        private static readonly List<UIElement> _uiElements = new List<UIElement>();

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


                _camera = new Camera(new Viewport(0, 0, width, height));
                _game.Run();
            })
            {
                IsBackground = true,
                Name = "RenderThread"
            }.Start();
        }
        // Добавляем метод для доступа к камере
        public static Camera GetCamera() => _camera;
        public static void Shutdown()
        {
            _isRunning = false;
            _game?.Exit();
        }
        // Добавляем метод для регистрации UI элементов
        public static void SubmitUIElement(UIElement element)
        {
            if (element == null) return;

            lock (_bufferLock)
            {
                if (!_uiElements.Contains(element))
                {
                    _uiElements.Add(element);
                }
            }
        }
        public static void SubmitUIElements(UIElement[] elements)
        {
            foreach (UIElement element in elements)
            {
                if (element == null) return;

                lock (_bufferLock)
                {
                    if (!_uiElements.Contains(element))
                    {
                        _uiElements.Add(element);
                    }
                }
            }
        }
        // Добавляем метод для удаления UI элементов
        public static void RemoveUIElement(UIElement element)
        {
            if (element == null) return;

            lock (_bufferLock)
            {
                _uiElements.Remove(element);
            }
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

            // Модифицируем метод Draw в RenderGame
            protected override void Draw(GameTime gameTime)
            {
                if (!_isRunning)
                {
                    Exit();
                    return;
                }

                List<Sprite> renderList;
                List<UIElement> uiList;
                lock (_bufferLock)
                {
                    renderList = _currentRenderList;
                    uiList = new List<UIElement>(_uiElements);

                    var temp = _currentRenderList;
                    _currentRenderList = _nextFrameList;
                    _nextFrameList = temp;
                }

                _graphicsDevice.Clear(Color.CornflowerBlue);

                // Отрисовка обычных спрайтов
                if (renderList.Count > 0)
                {
                    _spriteBatch.Begin(
                        sortMode: SpriteSortMode.FrontToBack,
                        blendState: BlendState.AlphaBlend,
                        transformMatrix: _camera?.TransformMatrix
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

                // Отрисовка UI элементов (без преобразований камеры)
                if (uiList.Count > 0)
                {
                    _spriteBatch.Begin(
                        sortMode: SpriteSortMode.FrontToBack,
                        blendState: BlendState.AlphaBlend,
                        samplerState: SamplerState.PointClamp // Для четкости пикселей
                    );

                    foreach (var element in uiList.OrderBy(e => e.LayerDepth))
                    {
                        if (element.Texture == null || !element.IsActive) continue;

                        var sourceRect = element.SourceRectangle ?? new Rectangle(0, 0, element.Texture.Width, element.Texture.Height);

                        _spriteBatch.Draw(
                            texture: element.Texture,
                            position: element.Position,
                            sourceRectangle: sourceRect,
                            color: element.Color,
                            rotation: element.Rotation,
                            origin: element.Origin,
                            scale: element.Scale,
                            effects: element.Effects,
                            layerDepth: element.LayerDepth
                        );
                    }
                    //DrawDebugBounds(_spriteBatch);
                    _spriteBatch.End();
                }
      
                base.Draw(gameTime);
            }
            public static void DrawDebugBounds(SpriteBatch spriteBatch)
            {
                foreach (var element in _uiElements.Where(e => e.IsActive))
                {
                    var bounds = element.Bounds;
                    var texture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                    texture.SetData(new[] { Color.Red });

                    // Рисуем границы
                    spriteBatch.Draw(texture, new Rectangle(bounds.X, bounds.Y, bounds.Width, 1), Color.Red);
                    spriteBatch.Draw(texture, new Rectangle(bounds.X, bounds.Y, 1, bounds.Height), Color.Red);
                    spriteBatch.Draw(texture, new Rectangle(bounds.X, bounds.Y + bounds.Height, bounds.Width, 1), Color.Red);
                    spriteBatch.Draw(texture, new Rectangle(bounds.X + bounds.Width, bounds.Y, 1, bounds.Height), Color.Red);
                }
            }
            private void ProcessTextureLoadRequests()
            {
                while (_textureLoadQueueUI.TryDequeue(out var request))
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
        public static void SubmitSprites(Sprite[] sprites)
        {
            foreach (Sprite sprite in sprites)
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
        }
        public static void EnqueueTextureLoad(Sprite sprite, string path)
        {
            if (sprite == null || string.IsNullOrEmpty(path)) return;
            _textureLoadQueue.Enqueue((sprite, path));
        }
        public static void EnqueueTextureLoad(UIElement uIElement, string path)
        {
            if (uIElement == null || string.IsNullOrEmpty(path)) return;
            _textureLoadQueueUI.Enqueue((uIElement, path));
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
