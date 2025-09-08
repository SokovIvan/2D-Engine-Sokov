using Microsoft.Xna.Framework;

namespace _2D_Engine_Sokov
{
    public class GameObject
    {
        public string Name { get; set; } = "Unnamed";
        public string Tag { get; set; } = "Untagged";
        public bool IsActive { get; set; } = true;
        public bool GravityEnabled { get; set; } = false;
        public bool CollisionEnabled { get; set; } = false;
        public bool IsStatic { get; set; } = false;
        public virtual Vector2 Scale { get; set; } = Vector2.One;
        public virtual Vector2 Size { get; set; } = Vector2.One;
        public float Rotation { get; set; } = 0f;
        public float Mass { get; set; } = 1f;
        public Vector2 Origin { get; set; } = Vector2.Zero;
        public float LayerDepth { get; set; } = 0f;
        public GameObject Parent { get; set; }
        private List<GameObject> children = new();
        public bool IsSleeping { get; set; }
        // В GameObject
        public Vector2 _lastCollisionNormal;
        public float _normalStickTime = 0f;
        private Vector2 _position;
        private readonly object _positionLock = new object();

        public Vector2 Position
        {
            get { lock (_positionLock) return _position; }
            set { lock (_positionLock) _position = value; }
        }

        // Аналогично для Velocity
        private Vector2 _velocity;
        private readonly object _velocityLock = new object();
        public Vector2 Velocity
        {
            get { lock (_velocityLock) return _velocity; }
            set { lock (_velocityLock) _velocity = value; }
        }
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
        ~GameObject()
        {
            try { 
            Game.DisposeObject(this);
            }
            catch { Console.WriteLine("Ошибка"); }
        }
        public bool IsOnGround { get; set; }

        private bool started = false;
        public virtual void Start() { 
        
        }
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

        public virtual void Update(double deltaTime)
        {
            if (!started)
            {
                Start();
                started = true;
                return;
            }
        }
        public void Rotate(float degrees)
        {
            Rotation += MathHelper.ToRadians(degrees);
        }

        public void SetRotation(float degrees)
        {
            Rotation = MathHelper.ToRadians(degrees);
        }

        public void LookAt(Vector2 targetPosition)
        {
            Vector2 direction = targetPosition - Position;
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
                Rotation = (float)Math.Atan2(direction.Y, direction.X);
            }
        }

        public void LookAt(GameObject target)
        {
            if (target != null)
            {
                LookAt(target.Position);
            }
        }
    }
}
