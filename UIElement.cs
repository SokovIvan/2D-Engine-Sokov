using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Engine_Sokov
{
    public class UIElement
    {
        public string Name { get; set; } = "Unnamed";
        public string Tag { get; set; } = "Untagged";
        public bool IsActive { get; set; } = true;
        public Vector2 Position { get; set; } = Vector2.Zero;
        public Vector2 Scale { get; set; } = Vector2.One;
        public float Rotation { get; set; } = 0f;
        public Vector2 Origin { get; set; } = Vector2.Zero;
        public float LayerDepth { get; set; } = 0.9f; 
        public Texture2D Texture { get; set; }
        public Color Color { get; set; } = Color.White;
        public Rectangle? SourceRectangle { get; set; } = null;
        public SpriteEffects Effects { get; set; } = SpriteEffects.None;

        // Делегат для обработки кликов
        public Action OnClick { get; set; }

        public UIElement()
        {
            UISystem.RegisterUIElement(this);
        }

        ~UIElement()
        {
            UISystem.UnregisterUIElement(this);
        }
        public virtual void Update(double deltaTime) { 
        
        }
        public void LoadTexture(string path)
        {
            RenderSystem.EnqueueTextureLoad(this, path);
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
                var scaledWidth = width * Scale.X;
                var scaledHeight = height * Scale.Y;
                var topLeft = Position - Origin * Scale;

                return new Rectangle(
                    (int)topLeft.X,
                    (int)topLeft.Y,
                    (int)scaledWidth,
                    (int)scaledHeight
                );
            }
        }
    }
}
