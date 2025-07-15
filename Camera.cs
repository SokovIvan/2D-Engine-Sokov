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
        private bool _frustumCullingEnabled = true;

        public bool FrustumCullingEnabled
        {
            get => _frustumCullingEnabled;
            set => _frustumCullingEnabled = value;
        }

        public Rectangle GetVisibleArea()
        {
            if (!_frustumCullingEnabled)
                return new Rectangle(0, 0, Viewport.Width, Viewport.Height);

            var inverseViewMatrix = Matrix.Invert(TransformMatrix);
            var tl = Vector2.Transform(Vector2.Zero, inverseViewMatrix);
            var tr = Vector2.Transform(new Vector2(Viewport.Width, 0), inverseViewMatrix);
            var bl = Vector2.Transform(new Vector2(0, Viewport.Height), inverseViewMatrix);
            var br = Vector2.Transform(new Vector2(Viewport.Width, Viewport.Height), inverseViewMatrix);

            var min = new Vector2(
                Math.Min(Math.Min(tl.X, tr.X), Math.Min(bl.X, br.X)),
                Math.Min(Math.Min(tl.Y, tr.Y), Math.Min(bl.Y, br.Y)));
            var max = new Vector2(
                Math.Max(Math.Max(tl.X, tr.X), Math.Max(bl.X, br.X)),
                Math.Max(Math.Max(tl.Y, tr.Y), Math.Max(bl.Y, br.Y)));

            return new Rectangle(
                (int)min.X,
                (int)min.Y,
                (int)(max.X - min.X),
                (int)(max.Y - min.Y));
        }
    }
}
