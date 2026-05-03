using System;
using System.Collections.Generic;
using System.Linq;
using _2D_Engine_Sokov.UIElements;
using Microsoft.Xna.Framework;

namespace _2D_Engine_Sokov.GameObjects
{
    public class Building : Sprite
    {
        public float Health { get; set; } = 100f;
        public float AttackDamage { get; set; } = 10f;
        public float AttackRange { get; set; } = 100f;
        public float DetectionRange { get; set; } = 200f;
        public float MoveSpeed { get; set; } = 100f;
        public List<Vector2> Path { get; set; } = new List<Vector2>();
        public Unit Target { get; set; }
        protected float AttackCooldown { get; set; } = 1f;
        protected float CooldownTimer { get; set; }
        protected float StopCooldown { get; set; } = 1f;
        protected Tile OccupiedTile { get; set; }
        public Point OccupiedTilePosition { get; set; }
        protected float ProduceTimer = 0f;
        public float ProducingTime = 5f;
        public Unit ProduceUnit;
        public Vector2 ProduceOffset = Vector2.UnitY;
        protected Vector2 placedPostion;

        private bool _isMapReady = false;

        public override void Start()
        {
            base.Start();
            _isMapReady = false; // Инициализация карты будет выполнена в первом Update
        }

        public override void Update(double deltaTime)
        {
            // 🛡️ Защита от race condition: ждём, пока карта точно загрузится
            if (!_isMapReady)
            {
                var tileMap = GameContext.TileMap;
                if (tileMap == null) return; // Карта ещё не готова, пропускаем кадр

                tileMap.OccupyTile(Position);
                var pt = tileMap.WorldToGridPosition(Position);
                OccupiedTile = tileMap.GetTile(pt.X, pt.Y);
                OccupiedTilePosition = pt;
                SetOriginToCenter();
                placedPostion = Position;
                _isMapReady = true;
            }

            base.Update(deltaTime);
            CooldownTimer -= (float)deltaTime;
            ProduceTimer += (float)deltaTime;
            Position = placedPostion;
            ProduceUnits();
            DetectUnits();
        }

        protected virtual void ProduceUnits()
        {
            if (ProduceUnit == null) return;
            if (ProduceTimer > ProducingTime)
            {
                if ((Tag == "Enemy" && GameController.instance.enemyRes > 0) ||
                    (Tag == "Player" && GameController.instance.playerRes > 0))
                {
                    if (Tag == "Enemy") GameController.instance.enemyRes -= 1;
                    if (Tag == "Player") GameController.instance.playerRes -= 1;
                    ProduceTimer = 0f;
                    Type t = ProduceUnit.GetType();
                    Unit unit = (Unit)Activator.CreateInstance(t);
                    unit.Size = ProduceUnit.Size;
                    unit.Texture = ProduceUnit.Texture;
                    unit.CollisionEnabled = ProduceUnit.CollisionEnabled;
                    unit.GravityEnabled = ProduceUnit.GravityEnabled;
                    unit.Mass = ProduceUnit.Mass;
                    if (unit != null)
                    {
                        unit.Position = Position + ProduceOffset;
                        Game.SubmitObject(unit);
                    }
                }
            }
        }

        protected void DetectUnits()
        {
            var units = LogicSystem.FindGameObjectsByTag(this is PlayerUnit ? "Enemy" : "Player");
            Unit closest = null;
            float closestDistance = float.MaxValue;

            foreach (var unit in units)
            {
                if (!unit.IsActive) continue;
                float distance = Vector2.Distance(Position, unit.Position);
                if (distance <= DetectionRange && distance < closestDistance)
                {
                    closest = unit as Unit;
                    closestDistance = distance;
                }
            }

            Target = closest;
            if (Target != null && this is EnemyUnit)
            {
                if (MoveSpeed > 0)
                    if (StopCooldown > 100 / MoveSpeed)
                    {
                        Path = Pathfinding.FindPath(GameContext.TileMap, Position, Target.Position);
                        StopCooldown = 0;
                    }
            }
        }
    }
}