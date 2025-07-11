using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Engine_Sokov
{
    public class Camera
    {
        public Vector2 Position { get; set; }
        public float Zoom { get; set; } = 1.0f;
        public float Rotation { get; set; }
        public Viewport Viewport { get; private set; }

        public Matrix TransformMatrix =>
            Matrix.CreateTranslation(new Vector3(-Position, 0)) *
            Matrix.CreateRotationZ(Rotation) *
            Matrix.CreateScale(Zoom) *
            Matrix.CreateTranslation(new Vector3(Viewport.Width * 0.5f, Viewport.Height * 0.5f, 0));

        public Camera(Viewport viewport)
        {
            Viewport = viewport;
        }

        public void Move(Vector2 amount)
        {
            Position += amount;
        }

        public void CenterOn(Vector2 position)
        {
            Position = position;
        }
    }
}
