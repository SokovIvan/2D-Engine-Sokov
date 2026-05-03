using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace _2D_Engine_Sokov.GameObjects
{
    public abstract class Unit : Sprite
    {
        public float Health { get; set; } = 100f;
        public float AttackDamage { get; set; } = 10f;
        public float AttackRange { get; set; } = 100f;
        public float DetectionRange { get; set; } = 200f;
        public float MoveSpeed { get; set; } = 100f;
        public List<Vector2> Path { get; set; } = new List<Vector2>();
        public GameObject Target { get; set; }
        protected float AttackCooldown { get; set; } = 1f;
        protected float CooldownTimer { get; set; }
        protected float StopCooldown { get; set; } = 1f;
        protected Tile OccupiedTile { get; set; }
        protected Point OccupiedTilePosition { get; set; }

        private bool _isMapReady = false;

        private float _detectTimer = 0f;

        private Point _lastGridPos = Point.Zero;

        public override void Start()
        {
            _detectTimer = (float)Random.Shared.NextDouble();
            base.Start();
            _isMapReady = false;
        }

        // Замени Update() на этот вариант:
        public override void Update(double deltaTime)
        {
            if (!_isMapReady)
            {
                var tileMap = GameContext.TileMap;
                if (tileMap == null) return;
                tileMap.OccupyTile(Position);
                var pt = tileMap.WorldToGridPosition(Position);
                OccupiedTile = tileMap.GetTile(pt.X, pt.Y);
                OccupiedTilePosition = pt;
                _lastGridPos = pt;
                SetOriginToCenter();
                _isMapReady = true;
                return;
            }

            base.Update(deltaTime);

            CooldownTimer -= (float)deltaTime;
            StopCooldown += (float)deltaTime;
            _detectTimer += (float)deltaTime;

            if (_detectTimer >= 0.5f)
            {
                _detectTimer = 0f;
                DetectUnits();
            }

            // === ИЗМЕНЁННАЯ ЛОГИКА ПОВОРОТА ===
            if (Path.Count > 0)
            {
                // Если есть путь — смотрим по направлению движения
                LookAt(Path[0]);
            }
            else if (Target != null)
            {
                // Если пути нет, но есть цель — смотрим на цель
                // (обычно это происходит, когда юнит дошёл и атакует)
                LookAt(Target);
            }

            // Атака
            if (Target != null && Vector2.DistanceSquared(Position, Target.Position) <= AttackRange * AttackRange)
            {
                if (Target.GetType().IsSubclassOf(typeof(Unit)))
                    Attack((Unit)Target);
                else if (Target.GetType().IsSubclassOf(typeof(Building)))
                    Attack((Building)Target);
            }
            else if (Path.Count > 0)
            {
                MoveAlongPath((float)deltaTime);
            }
        }

        protected void DetectUnits()
        {
            var units = LogicSystem.FindGameObjectsByTag(Tag=="Player" ? "Enemy" : "Player");
            Sprite closest = null;
            float closestDistance = float.MaxValue;

            foreach (var unit in units)
            {
                if (!unit.IsActive) continue;
                float distance = Vector2.Distance(Position, unit.Position);
                if (distance <= DetectionRange && distance < closestDistance)
                {
                    closest = unit as Sprite;
                    closestDistance = distance;
                }
            }

            Target = closest;
            if (Target != null && this is EnemyUnit)
            {
                if (MoveSpeed > 0)
                    if (StopCooldown > 100 / MoveSpeed)
                    {
                        var ignore = GameContext.TileMap.WorldToGridPosition(Position);
                        Path = Pathfinding.FindPath(GameContext.TileMap, Position, Target.Position, 1, 1);//, ignore);
                        StopCooldown = 0;
                    }
            }
        }

        protected void Attack(Unit target)
        {
            if (CooldownTimer <= 0)
            {
                RenderSystem.SubmitPersistentCommand(() => RenderSystem.DrawLine(Position, target.Position, Color.Red, 2f), framesToLive: 3);
                target.Health -= AttackDamage;
                CooldownTimer = AttackCooldown;
                if (target.Health <= 0)
                {
                    var tm = GameContext.TileMap;
                    tm?.DeoccupyTile(target.Position);
                    tm?.DeoccupyTile(tm.GridToWorldPosition(target.OccupiedTilePosition.X, target.OccupiedTilePosition.Y));
                    target.IsActive = false;
                    //LogicSystem.RemoveGameObject(target);
                }
            }
            else
            {
                RenderSystem.SubmitPersistentCommand(() => RenderSystem.DrawLine(Position, target.Position, Color.Gray, 1f), framesToLive: 2);
            }
        }

        protected void Attack(Building target)
        {
            if (CooldownTimer <= 0)
            {
                RenderSystem.SubmitPersistentCommand(() => RenderSystem.DrawLine(Position, target.Position, Color.Red, 2f), framesToLive: 3);
                target.Health -= AttackDamage;
                CooldownTimer = AttackCooldown;
                if (target.Health <= 0)
                {
                    var tm = GameContext.TileMap;
                    tm?.DeoccupyTile(target.Position);
                    tm?.DeoccupyTile(tm.GridToWorldPosition(target.OccupiedTilePosition.X, target.OccupiedTilePosition.Y));
                    target.IsActive = false;
                  //  LogicSystem.RemoveGameObject(target);
                }
            }
            else
            {
                RenderSystem.SubmitPersistentCommand(() => RenderSystem.DrawLine(Position, target.Position, Color.Gray, 1f), framesToLive: 2);
            }
        }

        protected void MoveAlongPath(float deltaTime)
        {
            if (Path.Count == 0) return;
            var targetPos = Path[0];
            var tm = GameContext.TileMap;
            if (tm == null) return;

            var direction = Vector2.Normalize(targetPos - Position);
            var distance = Vector2.Distance(Position, targetPos);

            // Плавное движение к цели
            Position += direction * MoveSpeed * (float)deltaTime;

            // Если дошли до точки пути, фиксируем позицию и убираем её из списка
            if (distance < MoveSpeed * (float)deltaTime + 1f)
            {
                Position = targetPos;
                Path.RemoveAt(0);

                // 🛡️ Обновляем занятость тайла только при смене клетки
                var newGridPos = tm.WorldToGridPosition(Position);
                if (newGridPos != OccupiedTilePosition)
                {
                    tm.DeoccupyTile(tm.GridToWorldPosition(OccupiedTilePosition.X, OccupiedTilePosition.Y));
                    OccupiedTilePosition = newGridPos;
                    tm.OccupyTile(Position);
                }
            }
        }
    }
}