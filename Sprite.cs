using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Threading;


namespace _2D_Engine_Sokov
{
    public class Sprite: GameObject
    {
        public Texture2D Texture { get; set; }
        public Color Color { get; set; } = Color.White;
        public Rectangle? SourceRectangle { get; set; } = null;
        public SpriteEffects Effects { get; set; } = SpriteEffects.None;

        public void LoadTexture(string path)
        {
            RenderSystem.EnqueueTextureLoad(this, path);
        }
    }
}
