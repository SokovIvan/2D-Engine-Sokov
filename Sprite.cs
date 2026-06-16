using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace _2D_Engine_Sokov
{
    public class Sprite : GameObject
    {
        private string _texturePath;
        private Texture2D _texture;
        private Vector2 _size;
        private Vector2? _pendingSize = null; // Желаемый размер, если текстура ещё не загружена
        
        public void SetOriginToCenter()
        {
            if (Texture != null)
            {
                Origin = new Vector2(Texture.Width / 2f, Texture.Height / 2f);
            }
        }
        public Texture2D Texture
        {
            get => _texture;
            set
            {
                _texture = value;
                if (_texture != null)
                {
                    if (_pendingSize.HasValue)
                    {
                        Size = _pendingSize.Value;
                        _pendingSize = null;
                        UpdateScaleFromSize();
                    }
                    else
                    {
                        _size = new Vector2(_texture.Width, _texture.Height);
                        UpdateScaleFromSize();
                    }
                    // 🔪 АВТОМАТИЧЕСКИ ЦЕНТРИРУЕМ ПРИ ЗАГРУЗКЕ
                    SetOriginToCenter();
                }
            }
        }
        public string TexturePath { 
            get => _texturePath;
        }
        public Color Color { get; set; } = Color.White;
        public Rectangle? SourceRectangle { get; set; } = null;
        public SpriteEffects Effects { get; set; } = SpriteEffects.None;

        public override Vector2 Size
        {
            get => _size;
            set
            {
                if (_texture == null)
                {
                    _pendingSize = value;
                    return;
                }

                if (value.X > 0 && value.Y > 0)
                {
                    _size = value;
                    UpdateScaleFromSize();
                    // ОБНОВЛЯЕМ ЦЕНТР ПРИ ИЗМЕНЕНИИ РАЗМЕРА
                    SetOriginToCenter();
                }
            }
        }

        public override Vector2 Scale
        {
            get => base.Scale;
            set
            {
                if (_texture != null && value.X > 0 && value.Y > 0)
                {
                    base.Scale = value;
                    UpdateSizeFromScale();
                }
            }
        }

        private void UpdateScaleFromSize()
        {
            if (_texture != null && _texture.Width > 0 && _texture.Height > 0)
            {
                base.Scale = new Vector2(
                    _size.X / _texture.Width,
                    _size.Y / _texture.Height
                );
            }
        }

        private void UpdateSizeFromScale()
        {
            if (_texture != null)
            {
                _size = new Vector2(
                    _texture.Width * base.Scale.X,
                    _texture.Height * base.Scale.Y
                );
            }
        }


        public void LoadTexture(string path)
        {
            if (_texturePath == path) return; // Уже загружаем эту текстуру

            // Освобождаем предыдущую текстуру
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

        ~Sprite()
        {
            if (!string.IsNullOrEmpty(_texturePath))
            {
                TextureManager.ReleaseTexture(_texturePath);
            }
        }
        public bool IsVisible(Camera camera)
        {
            if (camera == null || !camera.FrustumCullingEnabled)
                return true;

            if (Texture == null)
                return false;

            var visibleArea = camera.GetVisibleArea();
            var bounds = new Rectangle(
                (int)(Position.X - Origin.X * Scale.X),
                (int)(Position.Y - Origin.Y * Scale.Y),
                (int)(Texture.Width * Scale.X),
                (int)(Texture.Height * Scale.Y));

            return visibleArea.Intersects(bounds);
        }
    }
}
