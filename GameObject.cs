using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace _2D_Engine_Sokov
{
    public class GameObject
    {
        // Основные свойства
        public string Tag { get; set; } = "Untagged";
        public bool IsActive { get; set; } = true;
        public Vector2 Position { get; set; } = Vector2.Zero;
        public Vector2 Scale { get; set; } = Vector2.One;
        public float Rotation { get; set; } = 0f;
        public Vector2 Origin { get; set; } = Vector2.Zero;
        public float LayerDepth { get; set; } = 0f;

        // Компоненты и дочерние объекты
        public GameObject Parent { get; set; }
        private System.Collections.Generic.List<GameObject> children = new();

        // Трансформация
        public Matrix WorldTransform
        {
            get
            {
                var transform = Matrix.CreateTranslation(-Origin.X, -Origin.Y, 0) *
                                Matrix.CreateScale(Scale.X, Scale.Y, 1) *
                                Matrix.CreateRotationZ(Rotation) *
                                Matrix.CreateTranslation(Position.X, Position.Y, 0);

                return Parent != null ? transform * Parent.WorldTransform : transform;
            }
        }

        // Иерархия объектов
        public void AddChild(GameObject child)
        {
            child.Parent = this;
            children.Add(child);
        }

        public void RemoveChild(GameObject child)
        {
            child.Parent = null;
            children.Remove(child);
        }

    }
}
