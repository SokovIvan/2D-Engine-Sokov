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
        //private const float GRAVITY = 980f; // Более реалистичная гравитация
        private const float MAX_SPEED = 1000f; // Максимальная скорость объектов
        private const float GRAVITY = 500f; // Пикселей/сек²
        private const float FIXED_TIMESTEP = 1f / 60f; // 60 FPS
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

                // Фиксированный шаг с защитой от "спирали смерти"
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
            // Создаем локальную копию для обработки
            List<GameObject> processingList;
            lock (_bufferLock)
            {
                // Быстрое переключение буферов
                processingList = _currentPhysicsList;
                _currentPhysicsList = _nextPhysicsList;

                // Используем новый список вместо очистки
                _nextPhysicsList = new List<GameObject>();
            }
            for (int i = 0; i < processingList.Count; i++)
            {
                processingList[i].IsOnGround = false;
            }
            // Ограничиваем максимальную скорость и применяем гравитацию
            for (int i = 0; i < processingList.Count; i++)
            {
                GameObject obj = processingList[i];
                if (obj.IsStatic || !obj.IsActive) continue;

                // Гравитация (только если не на земле)
                if (obj.GravityEnabled && !obj.IsOnGround)
                {
                    obj.Velocity += new Vector2(0, GRAVITY * deltaTime);
                }

                // Мягкое ограничение скорости
                if (obj.Velocity.LengthSquared() > MAX_SPEED * MAX_SPEED)
                {
                    obj.Velocity = Vector2.Normalize(obj.Velocity) * MAX_SPEED;
                }
            }
            // Обновление физики
            for (int i = 0; i < processingList.Count; i++)
            {
                GameObject obj = processingList[i];
                if (!obj.IsActive || obj.IsStatic) continue;
                // В PhysicsSystem
                // Обновляем позицию
                Vector2 newPosition = obj.Position + obj.Velocity * deltaTime;
                obj.Position = newPosition;
            }

            // Обнаружение коллизий с стабилизацией
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
                            // Обновляем состояние "на земле"
                            UpdateGroundState(objA, objB, normal);
                        }
                    }
                }
            }
        }
        // Новый метод для определения, стоит ли объект на земле
        private static void UpdateGroundState(GameObject a, GameObject b, Vector2 normal)
        {
            const float GROUND_NORMAL_THRESHOLD = 0.95f; // Более строгий порог
            const float GROUND_VELOCITY_THRESHOLD = 10f; // Меньший порог скорости

            // Объект A стоит на B
            if (normal.Y < -GROUND_NORMAL_THRESHOLD &&
                Math.Abs(a.Velocity.Y) < GROUND_VELOCITY_THRESHOLD)
            {
                a.IsOnGround = true;
                a.Velocity =new Vector2(a.Velocity.X, 0);
            }

            // Объект B стоит на A
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

            // Расчет с учетом Origin и Scale
            Vector2 aSize = a.Size * a.Scale;
            Vector2 bSize = b.Size * b.Scale;

            Vector2 aMin = a.Position - a.Origin * a.Scale;
            Vector2 aMax = aMin + aSize;
            Vector2 bMin = b.Position - b.Origin * b.Scale;
            Vector2 bMax = bMin + bSize;

            // Проверка перекрытия
            float left = bMax.X - aMin.X;
            float right = aMax.X - bMin.X;
            float top = bMax.Y - aMin.Y;
            float bottom = aMax.Y - bMin.Y;

            if (left <= 0 || right <= 0 || top <= 0 || bottom <= 0)
                return false;

            // Находим минимальное перекрытие
            float minOverlap = Math.Min(Math.Min(left, right), Math.Min(top, bottom));
            depth = minOverlap;

            // Определяем нормаль с гистерезисом
            if (minOverlap == left) normal = new Vector2(-1, 0);
            else if (minOverlap == right) normal = new Vector2(1, 0);
            else if (minOverlap == top) normal = new Vector2(0, -1);
            else normal = new Vector2(0, 1);
            // В CheckAABBCollision перед return true:
            if (a._normalStickTime > 0 && Vector2.Dot(normal, a._lastCollisionNormal) > 0.7f)
            {
                normal = a._lastCollisionNormal;
                a._normalStickTime -= FIXED_TIMESTEP;
            }
            else
            {
                a._lastCollisionNormal = normal;
                a._normalStickTime = 0.1f; // 100ms фиксации нормали
            }
            return true;
        }

        private static void ResolveCollision(GameObject a, GameObject b, Vector2 normal, float depth)
        {
            const float POSITION_CORRECTION = 0.8f;
            const float VELOCITY_THRESHOLD = 0.5f;
            const float STATIC_FRICTION = 0.96f;
            const float DYNAMIC_FRICTION = 0.92f;

            // Пропускаем разрешение при малых перекрытиях
            if (depth < 0.01f) return;

            float aInvMass = a.IsStatic ? 0 : 1f / a.Mass;
            float bInvMass = b.IsStatic ? 0 : 1f / b.Mass;
            float totalInvMass = aInvMass + bInvMass;
            if (totalInvMass == 0) return;

            // Коррекция позиции (сглаженная)
            Vector2 correction = normal * depth * POSITION_CORRECTION / totalInvMass;
            a.Position -= correction * aInvMass;
            b.Position += correction * bInvMass;

            // Расчет относительной скорости
            Vector2 relativeVelocity = b.Velocity - a.Velocity;
            float velocityAlongNormal = Vector2.Dot(relativeVelocity, normal);

            // Игнорируем столкновения при удалении
            if (velocityAlongNormal > VELOCITY_THRESHOLD) return;

            // Коэффициент упругости (разный для статичных объектов)
            float restitution = (a.IsStatic || b.IsStatic) ? 0.1f : 0.3f;
            float impulse = -(1 + restitution) * velocityAlongNormal / totalInvMass;
            Vector2 impulseVec = normal * impulse;

            // Применяем импульс
            if (!a.IsStatic) a.Velocity -= impulseVec * aInvMass;
            if (!b.IsStatic) b.Velocity += impulseVec * bInvMass;

            // Специальная обработка "земли"
            if (normal.Y < -0.95f && Math.Abs(a.Velocity.Y) < 2f)
            {
                a.Velocity = new Vector2(a.Velocity.X * STATIC_FRICTION, 0);
                a.IsOnGround = true;
            }
            else if (Math.Abs(normal.Y) > 0.5f)
            {
                // Применяем трение для наклонных поверхностей
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
