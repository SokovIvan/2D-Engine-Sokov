using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;
namespace _2D_Engine_Sokov
{
    public class RenderSystem
    {
        private static Microsoft.Xna.Framework.Game _game;
        private static GraphicsDevice _graphicsDevice;
        private static SpriteBatch _spriteBatch;
        private static bool _isRunning;

        private static readonly Dictionary<string, SpriteFont> _fontCache = new Dictionary<string, SpriteFont>();

        // Система двойной буферизации для спрайтов
        private static readonly List<Sprite> _frameBufferA = new List<Sprite>();
        private static readonly List<Sprite> _frameBufferB = new List<Sprite>();
        private static List<Sprite> _currentRenderList = _frameBufferA;
        private static List<Sprite> _nextFrameList = _frameBufferB;
        private static readonly object _bufferLock = new object();

        // Очередь команд отрисовки
        private static readonly ConcurrentQueue<Action> _drawCommands = new ConcurrentQueue<Action>();

        // Очередь загрузки текстур
        private static readonly ConcurrentQueue<(Sprite, string)> _textureLoadQueue = new();
        private static readonly ConcurrentQueue<(UIElement, string)> _textureLoadQueueUI = new();
        // Добавляем камеру
        private static Camera _camera;

        public static Color backgroundColor = Color.CornflowerBlue;

        // В класс RenderSystem добавляем статический список для UI элементов
        private static readonly List<UIElement> _uiElements = new List<UIElement>();

        // Примитивы
        private static Texture2D _pixelTexture;
        private static SpriteFont _defaultFont;

        public static List<Sprite> _backgrounds = new List<Sprite>();
        // Добавляем в начало класса
        private class PersistentDrawCommand
        {
            public Action Command { get; set; }
            public int FramesLeft { get; set; }
        }

        private static readonly List<PersistentDrawCommand> _persistentCommands = new();
        private static readonly object _persistentCommandsLock = new();

        // Новый метод для добавления "долгоживущих" команд
        public static void SubmitPersistentCommand(Action command, int framesToLive)
        {
            lock (_persistentCommandsLock)
            {
                _persistentCommands.Add(new PersistentDrawCommand
                {
                    Command = command,
                    FramesLeft = framesToLive
                });
            }
        }
        public static void SubmitBackgrounds(Sprite[] backgrounds)
        {
            _backgrounds.Clear();
            _backgrounds.AddRange(backgrounds);
        }

        public static void Initialize(int width, int height)
        {
            _isRunning = true;
            new Thread(() =>
            {
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
        public static void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 1f)
        {
            _drawCommands.Enqueue(() => {
                var angle = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);
                var length = Vector2.Distance(start, end);

                _spriteBatch.Draw(
                    _pixelTexture,
                    start,
                    null,
                    color,
                    angle,
                    Vector2.Zero,
                    new Vector2(length, thickness),
                    SpriteEffects.None,
                    0f);
            });
        }

        public static void DrawRectangle(Rectangle rect, Color color, float thickness = 1f)
        {
            DrawLine(new Vector2(rect.X, rect.Y), new Vector2(rect.X + rect.Width, rect.Y), color, thickness);
            DrawLine(new Vector2(rect.X + rect.Width, rect.Y), new Vector2(rect.X + rect.Width, rect.Y + rect.Height), color, thickness);
            DrawLine(new Vector2(rect.X + rect.Width, rect.Y + rect.Height), new Vector2(rect.X, rect.Y + rect.Height), color, thickness);
            DrawLine(new Vector2(rect.X, rect.Y + rect.Height), new Vector2(rect.X, rect.Y), color, thickness);
        }

        public static void FillRectangle(Rectangle rect, Color color)
        {
            _drawCommands.Enqueue(() => {
                _spriteBatch.Draw(
                    _pixelTexture,
                    rect,
                    color);
            });
        }

        public static void DrawCircle(Vector2 center, float radius, Color color, int segments = 16, float thickness = 1f)
        {
            Vector2[] points = new Vector2[segments];
            float angle = 0f;
            float angleIncrement = MathHelper.TwoPi / segments;

            for (int i = 0; i < segments; i++)
            {
                points[i] = center + radius * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                angle += angleIncrement;
            }

            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                DrawLine(points[i], points[next], color, thickness);
            }
        }
        public static void FillCircle(Vector2 center, float radius, Color color, int segments = 16)
        {
            _drawCommands.Enqueue(() => {
                _spriteBatch.Draw(_pixelTexture, center, null, color,
                                  0f, Vector2.One * 0.5f, radius * 2, SpriteEffects.None, 0f);

                float angle = 0f;
                float angleIncrement = MathHelper.TwoPi / segments;

                for (int i = 0; i < segments; i++)
                {
                    Vector2 edge = center + radius * new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
                    DrawLine(center, edge, color, radius * 0.5f);
                    angle += angleIncrement;
                }
            });
        }
        public static void DrawText(string text, Vector2 position, Color color, float scale = 1f, bool useCamera = false)
        {
            _drawCommands.Enqueue(() => {
                var transform = useCamera ? _camera?.TransformMatrix : null;
                _spriteBatch.DrawString(_defaultFont, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

            });
        }

        public static void DrawText(SpriteFont font, string text, Vector2 position, Color color, float scale = 1f, bool useCamera = false)
        {
            _drawCommands.Enqueue(() => {
                var transform = useCamera ? _camera?.TransformMatrix : null;
                _spriteBatch.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);

            });
        }
        public static void EnableFrustumCulling(bool enable)
        {
            if (_camera != null)
            {
                _camera.FrustumCullingEnabled = enable;
            }
        }

        public static bool IsFrustumCullingEnabled()
        {
            return _camera?.FrustumCullingEnabled ?? false;
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
                TextureManager.Initialize(_graphicsDevice);

                _pixelTexture = new Texture2D(_graphicsDevice, 1, 1);
                _pixelTexture.SetData(new[] { Color.White });

                _defaultFont = RuntimeFontGenerator.CreateFont(_graphicsDevice);             
                base.Initialize();

            }

            protected override void Update(GameTime gameTime)
            {
                TextureManager.Update();
                base.Update(gameTime);
            }

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

                _graphicsDevice.Clear(backgroundColor);

                if (_backgrounds.Count > 0)
                {
                    _spriteBatch.Begin(
                        sortMode: SpriteSortMode.FrontToBack,
                        blendState: BlendState.AlphaBlend,
                        samplerState: SamplerState.PointClamp
                    );

                    foreach (var element in _backgrounds.OrderBy(e => e.LayerDepth))
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
                    _spriteBatch.End();
                }

                _spriteBatch.Begin(
                    sortMode: SpriteSortMode.FrontToBack,
                    blendState: BlendState.AlphaBlend,
                    transformMatrix: _camera?.TransformMatrix
                );

                foreach (var sprite in renderList.OrderBy(s => s.LayerDepth))
                {
                    if (sprite.Texture == null || !sprite.IsActive) continue;

                    // Проверка видимости только если включено отсечение
                    if (_camera != null && _camera.FrustumCullingEnabled && !sprite.IsVisible(_camera))
                    {
                        //Console.WriteLine(sprite.Tag);
                        continue;
                    }
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

                while (_drawCommands.TryDequeue(out var command))
                {
                    command.Invoke();
                }
                lock (_persistentCommandsLock)
                {
                    for (int i = _persistentCommands.Count - 1; i >= 0; i--)
                    {
                        var cmd = _persistentCommands[i];
                        cmd.Command.Invoke();
                        cmd.FramesLeft--;

                        if (cmd.FramesLeft <= 0)
                            _persistentCommands.RemoveAt(i);
                    }
                }
                _spriteBatch.End();

                if (uiList.Count > 0)
                {
                    _spriteBatch.Begin(
                        sortMode: SpriteSortMode.FrontToBack,
                        blendState: BlendState.AlphaBlend,
                        samplerState: SamplerState.PointClamp
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
                    if (!_nextFrameList.Contains(sprite))
                    {
                        _nextFrameList.Add(sprite);
                    }
                }
            }
        }
        public static void EnqueueTextureLoad(Sprite sprite, string path)
        {
            sprite.LoadTexture(path);
        }
        public static void EnqueueTextureLoad(UIElement uIElement, string path)
        {
            uIElement.LoadTexture(path);
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
