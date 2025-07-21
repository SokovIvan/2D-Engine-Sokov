using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace _2D_Engine_Sokov
{
    public class UIElement
    {
        private Texture2D _texture;
        private Vector2 _size;
        private Vector2? _pendingSize = null; 
        private Vector2 _scale = Vector2.One;
        private string _texturePath;
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
                    _texturePath = null;
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
            UISystem.RegisterUIElement(this);
        }

        ~UIElement()
        {
            Game.DisposeUIElement(this);
            UISystem.UnregisterUIElement(this);
            if (!string.IsNullOrEmpty(_texturePath))
            {
                TextureManager.ReleaseTexture(_texturePath);
            }
        }

        public virtual void Update(double deltaTime)
        {
            if (!string.IsNullOrEmpty(Text))
            {            
                if (_cachedText != Text || _cachedFontSize != FontSize || _cachedTextPosition == null)
                {
                    var bounds = this.Bounds;
                    var font = this.Font ?? RenderSystem.GetDefaultFont();
                    if (font == null) return;

                    Vector2 textSize = font.MeasureString(Text) * FontSize;

                    _cachedTextPosition = AutoCenterText
                        ? new Vector2(
                            bounds.X + textSize.X/ 4 ,
                            bounds.Y + (bounds.Height - textSize.Y) / 2)
                        : new Vector2(bounds.X, bounds.Y);

                    _cachedTextPosition += TextOffset;
                    _cachedText = Text;
                    _cachedFontSize = FontSize;
                }

                RenderSystem.SubmitPersistentCommand(() => {
                    RenderSystem.DrawText(
                        Font ?? RenderSystem.GetDefaultFont(),
                        Text,
                        _cachedTextPosition.Value,
                        TextColor,
                        FontSize,
                        false
                    );
                }, framesToLive: 2, useCamera: false);
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
    }
}