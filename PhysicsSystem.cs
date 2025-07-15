using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace _2D_Engine_Sokov
{
    internal class PhysicsSystem
    {
        private static bool _isRunning;
        private static readonly List<GameObject> _physicsBufferA = new List<GameObject>();
        private static readonly List<GameObject> _physicsBufferB = new List<GameObject>();
        private static List<GameObject> _currentPhysicsList = _physicsBufferA;
        private static List<GameObject> _nextPhysicsList = _physicsBufferB;
        private static readonly object _bufferLock = new object();
        //private const float GRAVITY = 980f; 
        public static float MAX_SPEED = 1000f;
        public static float GRAVITY = 500f; 
        private const float FIXED_TIMESTEP = 1f / 60f; 
        private static Thread _physicsThread;

        public static void Initialize()
        {
            _isRunning = true;
            _physicsThread = new Thread(PhysicsLoop);
            _physicsThread.IsBackground = true;
            _physicsThread.Start();
        }
        private static void PhysicsLoop()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            float accumulatedTime = 0f;

            while (_isRunning)
            {
                float deltaTime = (float)stopwatch.Elapsed.TotalSeconds;
                stopwatch.Restart();
                accumulatedTime += deltaTime;
                const float MAX_DELTA = 0.25f;
                if (accumulatedTime > MAX_DELTA)
                    accumulatedTime = MAX_DELTA;

                while (accumulatedTime >= FIXED_TIMESTEP)
                {
                    UpdatePhysics(FIXED_TIMESTEP);
                    accumulatedTime -= FIXED_TIMESTEP;
                }

                Thread.Sleep(1);
            }
        }

        private static void UpdatePhysics(float deltaTime)
        {
            List<GameObject> processingList;
            lock (_bufferLock)
            {
                processingList = _currentPhysicsList;
                _currentPhysicsList = _nextPhysicsList;
                _nextPhysicsList = new List<GameObject>();
            }
            for (int i = 0; i < processingList.Count; i++)
            {
                processingList[i].IsOnGround = false;
            }
            for (int i = 0; i < processingList.Count; i++)
            {
                GameObject obj = processingList[i];
                if (obj.IsStatic || !obj.IsActive) continue;
                if (obj.GravityEnabled && !obj.IsOnGround)
                {
                    obj.Velocity += new Vector2(0, GRAVITY * deltaTime);
                }
                if (obj.Velocity.LengthSquared() > MAX_SPEED * MAX_SPEED)
                {
                    obj.Velocity = Vector2.Normalize(obj.Velocity) * MAX_SPEED;
                }
            }

            for (int i = 0; i < processingList.Count; i++)
            {
                GameObject obj = processingList[i];
                if (!obj.IsActive || obj.IsStatic) continue;
                Vector2 newPosition = obj.Position + obj.Velocity * deltaTime;
                obj.Position = newPosition;
            }

            const int COLLISION_ITERATIONS = 3;
            for (int iter = 0; iter < COLLISION_ITERATIONS; iter++)
            {
                for (int i = 0; i < processingList.Count; i++)
                {
                    GameObject objA = processingList[i];
                    if (!objA.CollisionEnabled || !objA.IsActive || objA.IsStatic) continue;

                    for (int j = i + 1; j < processingList.Count; j++)
                    {
                        GameObject objB = processingList[j];
                        if (!objB.CollisionEnabled || !objB.IsActive) continue;

                        if (CheckAABBCollision(objA, objB, out Vector2 normal, out float depth))
                        {
                            ResolveCollision(objA, objB, normal, depth);
                            //Debug.WriteLine($"Collision: {objA.Tag}-{objB.Tag} | N: {normal} | D: {depth}");
                            UpdateGroundState(objA, objB, normal);
                        }
                    }
                }
            }
        }
        private static void UpdateGroundState(GameObject a, GameObject b, Vector2 normal)
        {
            const float GROUND_NORMAL_THRESHOLD = 0.95f; 
            const float GROUND_VELOCITY_THRESHOLD = 10f; 

            if (normal.Y < -GROUND_NORMAL_THRESHOLD &&
                Math.Abs(a.Velocity.Y) < GROUND_VELOCITY_THRESHOLD)
            {
                a.IsOnGround = true;
                a.Velocity =new Vector2(a.Velocity.X, 0);
            }
            if (normal.Y > GROUND_NORMAL_THRESHOLD &&
                Math.Abs(b.Velocity.Y) < GROUND_VELOCITY_THRESHOLD)
            {
                b.IsOnGround = true;
                b.Velocity = new Vector2(b.Velocity.X, 0);
            }
        }
        private static bool CheckAABBCollision(GameObject a, GameObject b, out Vector2 normal, out float depth)
        {
            normal = Vector2.Zero;
            depth = 0f;
            Vector2 aSize = a.Size * a.Scale;
            Vector2 bSize = b.Size * b.Scale;

            Vector2 aMin = a.Position - a.Origin * a.Scale;
            Vector2 aMax = aMin + aSize;
            Vector2 bMin = b.Position - b.Origin * b.Scale;
            Vector2 bMax = bMin + bSize;
            float left = bMax.X - aMin.X;
            float right = aMax.X - bMin.X;
            float top = bMax.Y - aMin.Y;
            float bottom = aMax.Y - bMin.Y;

            if (left <= 0 || right <= 0 || top <= 0 || bottom <= 0)
                return false;

            float minOverlap = Math.Min(Math.Min(left, right), Math.Min(top, bottom));
            depth = minOverlap;
            if (minOverlap == left) normal = new Vector2(-1, 0);
            else if (minOverlap == right) normal = new Vector2(1, 0);
            else if (minOverlap == top) normal = new Vector2(0, -1);
            else normal = new Vector2(0, 1);
            if (a._normalStickTime > 0 && Vector2.Dot(normal, a._lastCollisionNormal) > 0.7f)
            {
                normal = a._lastCollisionNormal;
                a._normalStickTime -= FIXED_TIMESTEP;
            }
            else
            {
                a._lastCollisionNormal = normal;
                a._normalStickTime = 0.1f; 
            }
            return true;
        }

        private static void ResolveCollision(GameObject a, GameObject b, Vector2 normal, float depth)
        {
            const float POSITION_CORRECTION = 0.8f;
            const float VELOCITY_THRESHOLD = 0.5f;
            const float STATIC_FRICTION = 0.96f;
            const float DYNAMIC_FRICTION = 0.92f;

            if (depth < 0.01f) return;
            float aInvMass = a.IsStatic ? 0 : 1f / a.Mass;
            float bInvMass = b.IsStatic ? 0 : 1f / b.Mass;
            float totalInvMass = aInvMass + bInvMass;
            if (totalInvMass == 0) return;

            Vector2 correction = normal * depth * POSITION_CORRECTION / totalInvMass;
            a.Position -= correction * aInvMass;
            b.Position += correction * bInvMass;

            Vector2 relativeVelocity = b.Velocity - a.Velocity;
            float velocityAlongNormal = Vector2.Dot(relativeVelocity, normal);

            if (velocityAlongNormal > VELOCITY_THRESHOLD) return;

            float restitution = (a.IsStatic || b.IsStatic) ? 0.1f : 0.3f;
            float impulse = -(1 + restitution) * velocityAlongNormal / totalInvMass;
            Vector2 impulseVec = normal * impulse;

            if (!a.IsStatic) a.Velocity -= impulseVec * aInvMass;
            if (!b.IsStatic) b.Velocity += impulseVec * bInvMass;

            if (normal.Y < -0.95f && Math.Abs(a.Velocity.Y) < 2f)
            {
                a.Velocity = new Vector2(a.Velocity.X * STATIC_FRICTION, 0);
                a.IsOnGround = true;
            }
            else if (Math.Abs(normal.Y) > 0.5f)
            {
                float friction = (a.IsOnGround) ? STATIC_FRICTION : DYNAMIC_FRICTION;
                a.Velocity = new Vector2(a.Velocity.X * friction, 0);
            }
        }
        public static void Shutdown()
        {
            _isRunning = false;
            _physicsThread?.Join();
        }
        public static void SubmitGameObject(GameObject gameObject)
        {
            if (gameObject == null) return;

            lock (_bufferLock)
            {
                if (!_nextPhysicsList.Contains(gameObject))
                {
                    _nextPhysicsList.Add(gameObject);
                }
            }
        }
        public static void SubmitGameObjects(GameObject[] gameObjects)
        {
            foreach (GameObject gameObject in gameObjects)
            {
                if (gameObject == null) return;

                lock (_bufferLock)
                {
                    if (!_nextPhysicsList.Contains(gameObject))
                    {
                        _nextPhysicsList.Add(gameObject);
                    }
                }
            }
        }
    }
}
