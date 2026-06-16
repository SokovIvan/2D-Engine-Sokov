using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace _2D_Engine_Sokov
{
    public class UIElement
    {
        private Texture2D _texture;
        private Vector2 _size;
        private Vector2? _pendingSize = null; 
        private Vector2 _scale = Vector2.One;
        private string _texturePath;
        public string TexturePath
        {
            get => _texturePath;
        }
        public string Name { get; set; } = "Unnamed";
        public string Tag { get; set; } = "Untagged";
        public bool IsActive { get; set; } = true;
        public Vector2 Position { get; set; } = Vector2.Zero;
        public float Rotation { get; set; } = 0f;
        public Vector2 Origin { get; set; } = Vector2.Zero;
        public float LayerDepth { get; set; } = 0.9f;
        public Color Color { get; set; } = Color.White;
        public Rectangle? SourceRectangle { get; set; } = null;
        public SpriteEffects Effects { get; set; } = SpriteEffects.None;
        public Action OnClick { get; set; }
        public string text { get; set; } = string.Empty;
        public Color textColor { get; set; } = Color.Red;
        public float fontSize { get; set; } = 1f;
        public string Text
        {
            get => text;
            set => text = value;
        }

        public Color TextColor
        {
            get => textColor;
            set => textColor = value;
        }

        public float FontSize
        {
            get => fontSize;
            set => fontSize = MathHelper.Clamp(value, 0.1f, 10f);
        }
        private Vector2? _cachedTextPosition;
        private string _cachedText;
        private float _cachedFontSize;
        public SpriteFont Font { get; set; } 
        public Vector2 TextOffset { get; set; } = Vector2.Zero; 
        public bool AutoCenterText { get; set; } = true; 
        public Texture2D Texture
        {
            get => _texture;
            set
            {
                // Освобождаем старую текстуру, если она была
                if (!string.IsNullOrEmpty(_texturePath))
                {
                    TextureManager.ReleaseTexture(_texturePath);                  
                }
                _texture = value;
                if (_texture != null)
                {
                    // Применяем отложенный размер, если он есть
                    if (_pendingSize.HasValue)
                    {
                        Size = _pendingSize.Value;
                        _pendingSize = null;
                    }
                    else
                    {
                        // Иначе используем размер текстуры
                        _size = new Vector2(
                            SourceRectangle?.Width ?? _texture.Width,
                            SourceRectangle?.Height ?? _texture.Height
                        );
                        UpdateScaleFromSize();
                    }
                }
            }
        }

        public Vector2 Scale
        {
            get => _scale;
            set
            {
                if (value.X > 0 && value.Y > 0)
                {
                    _scale = value;
                    UpdateSizeFromScale();
                }
            }
        }

        public Vector2 Size
        {
            get => _size;
            set
            {
                if (_texture == null)
                {
                    // Если текстура не загружена, сохраняем желаемый размер
                    _pendingSize = value;
                    return;
                }

                if (value.X > 0 && value.Y > 0)
                {
                    _size = value;
                    UpdateScaleFromSize();
                }
            }
        }

        public UIElement()
        {
        }

        ~UIElement()
        {
            GameContext.RemoveUIElement(this);
           // UISystem.UnregisterUIElement(this);
            if (!string.IsNullOrEmpty(_texturePath))
            {
                TextureManager.ReleaseTexture(_texturePath);
            }
        }
        private bool started = false;
        public virtual void Start()
        {
            started = true;
        }
        public virtual void Update(double deltaTime)
        {
            if (!started) { 
                Start();
                return;
            }
            // Рисуем текст, только если он есть
            if (!string.IsNullOrEmpty(Text))
            {
                var bounds = this.Bounds;
                // Проверяем, изменился ли текст, размер шрифта или позиция (чтобы не считать заново каждый кадр)
                if (_cachedText != Text || _cachedFontSize != FontSize || _cachedTextPosition == null)
                {

                    var font = this.Font ?? RenderSystem.GetDefaultFont();

                    if (font == null) return;

                    // --- НОВАЯ ЛОГИКА ПЕРЕНОСА СТРОК ---
                    float maxWidth = bounds.Width;

                    // Разбиваем текст на строки
                    List<string> lines = WrapText(font, Text, FontSize, maxWidth);

                    // Вычисляем общую высоту всего блока текста
                    float totalTextHeight = lines.Count * font.MeasureString("A").Y * FontSize;

                    // Центрируем блок текста вертикально внутри элемента (если нужно)
                    float startY = bounds.Y + bounds.Height/2 - totalTextHeight - TextOffset.Y; //+ (bounds.Height - totalTextHeight) / 2;

                    // Если текст больше высоты элемента, можно либо обрезать, либо начать с начала (здесь начинаем с начала)
                    //if (startY < bounds.Y) startY = bounds.Y;

                    _cachedLines = lines; // Сохраняем строки в кэш
                    _cachedStartY = startY; // Сохраняем стартовую позицию Y
                    _cachedText = Text;
                    _cachedFontSize = FontSize;
                }

                // Отрисовка каждой строки по очереди
                if (_cachedLines != null)
                {
                    var font = this.Font ?? RenderSystem.GetDefaultFont();
                    float lineHeight = font.MeasureString("A").Y * FontSize;

                    for (int i = 0; i < _cachedLines.Count; i++)
                    {
                        string line = _cachedLines[i];
                        Vector2 linePosition = new Vector2(bounds.X + TextOffset.X, _cachedStartY + (i * lineHeight) + TextOffset.Y);

                        // Используем SubmitPersistentCommand для отрисовки (как было раньше)
                        // Важно: замыкаем переменные правильно
                        string lineToDraw = line;
                        Vector2 posToDraw = linePosition;

                        RenderSystem.SubmitPersistentCommand(() =>
                        {
                            RenderSystem.DrawText(
                                font,
                                lineToDraw,
                                posToDraw,
                                TextColor,
                                FontSize,
                                false
                            );
                        }, framesToLive: 3, useCamera: false);
                    }
                }
            }
        }

        public void LoadTexture(string path)
        {
            if (_texturePath == path) return; 

            if (!string.IsNullOrEmpty(_texturePath))
            {
                TextureManager.ReleaseTexture(_texturePath);
            }

            _texturePath = path;
            TextureManager.LoadTexture(this, path, texture =>
            {
                Texture = texture;
            });
        }

        public bool IsMouseOver()
        {
            return UISystem.IsMouseOver(this);
        }

        public Vector2 GetLocalMousePosition()
        {
            var mousePos = UISystem.GetMousePosition();
            return mousePos - Position;
        }

        public Rectangle Bounds
        {
            get
            {
                var width = SourceRectangle?.Width ?? Texture?.Width ?? 0;
                var height = SourceRectangle?.Height ?? Texture?.Height ?? 0;
                var scaledWidth = width * _scale.X;
                var scaledHeight = height * _scale.Y;
                var topLeft = Position - Origin * _scale;

                return new Rectangle(
                    (int)topLeft.X,
                    (int)topLeft.Y,
                    (int)scaledWidth,
                    (int)scaledHeight
                );
            }
        }

        private void UpdateScaleFromSize()
        {
            if (_texture != null)
            {
                var refWidth = SourceRectangle?.Width ?? _texture.Width;
                var refHeight = SourceRectangle?.Height ?? _texture.Height;

                if (refWidth > 0 && refHeight > 0)
                {
                    _scale = new Vector2(
                        _size.X / refWidth,
                        _size.Y / refHeight
                    );
                }
            }
        }
        public bool IsVisible()
        {
            return IsActive;
        }
        private void UpdateSizeFromScale()
        {
            if (_texture != null)
            {
                var refWidth = SourceRectangle?.Width ?? _texture.Width;
                var refHeight = SourceRectangle?.Height ?? _texture.Height;

                _size = new Vector2(
                    refWidth * _scale.X,
                    refHeight * _scale.Y
                );
            }
        }
        // --- ВСПОМОГАТЕЛЬНЫЙ МЕТОД ДЛЯ ПЕРЕНОСА ---
        private List<string> _cachedLines;
        private float _cachedStartY;

        private List<string> WrapText(SpriteFont font, string text, float scale, float maxWidth)
        {
            List<string> lines = new List<string>();

            // Разбиваем текст на слова
            string[] words = text.Split(' ');
            if (words.Length == 0) return lines;

            StringBuilder currentLine = new StringBuilder(words[0]);

            for (int i = 1; i < words.Length; i++)
            {
                string word = words[i];

                // Измеряем текущую строку + новое слово
                string testLine = currentLine.ToString() + " " + word;
                float testWidth = font.MeasureString(testLine).X * scale;

                if (testWidth <= maxWidth)
                {
                    // Если влезает, добавляем слово к текущей строке
                    currentLine.Append(" ").Append(word);
                }
                else
                {
                    // Если не влезает, сохраняем текущую строку и начинаем новую с этого слова
                    lines.Add(currentLine.ToString());
                    currentLine.Clear();
                    currentLine.Append(word);
                }
            }

            // Добавляем последнюю строку
            if (currentLine.Length > 0)
            {
                lines.Add(currentLine.ToString());
            }

            return lines;
        }
    }
}