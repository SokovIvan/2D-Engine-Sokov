using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public override void Start()
        {
            base.Start();
            Game.instance._currentLevel.TileMap.OccupyTile(Position);
            var pt = Game.instance._currentLevel.TileMap.WorldToGridPosition(Position);
            OccupiedTile = Game.instance._currentLevel.TileMap.GetTile(pt.X, pt.Y);
            OccupiedTilePosition = pt;
            SetOriginToCenter();
        }
        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            CooldownTimer -= (float)deltaTime;


            if (Target != null)
            {
                LookAt(Target);
            }
            else if (Path.Count > 0)
            {
                //Vector2 dir = Path[0] - Position;
                LookAt(Path[0]);
                /*if (dir != Vector2.Zero)
                {
                    dir.Normalize();
                    dir = new Vector2((float)Math.Round(dir.X), (float)Math.Round(dir.Y));
                    //Console.WriteLine(dir.ToString());
                    if(dir ==Vector2.UnitX)
                    SetRotation(0f);
                    else if (dir == Vector2.UnitY)
                        SetRotation(90f);
                    else if (dir == -Vector2.UnitX)
                        SetRotation(180f);
                    else if (dir == -Vector2.UnitY)
                        SetRotation(270f);
                }*/
            }

            if (Target != null && Vector2.Distance(Position, Target.Position) <= AttackRange)
            {
                if(Target.GetType().IsSubclassOf(typeof(Unit)))
                Attack((Unit)Target);
                else if (Target.GetType().IsSubclassOf(typeof(Building)))
                    Attack((Building)Target);
            }
            else if (Path.Count > 0)
            {                
                MoveAlongPath((float)deltaTime);
            }

            DetectUnits();
        }

        protected void DetectUnits()
        {
            var units = LogicSystem.FindGameObjectsByTag(this is PlayerUnit ? "Enemy" : "Player");
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
                        Path = Pathfinding.FindPath(Game.instance._currentLevel.TileMap, Position, Target.Position);
                        StopCooldown = 0;
                    }
                
            }
        }

        protected void Attack(Unit target)
        {
            if (CooldownTimer <= 0)
            {                
                RenderSystem.SubmitPersistentCommand(() => {
                    RenderSystem.DrawLine(Position, target.Position , Color.Red, 2f);
                }, framesToLive: 3);
                target.Health -= AttackDamage;
                CooldownTimer = AttackCooldown;
                if (target.Health <= 0)
                {
                    Game.instance._currentLevel.TileMap.DeoccupyTile(target.Position);
                    Game.instance._currentLevel.TileMap.DeoccupyTile(Game.instance._currentLevel.TileMap.GridToWorldPosition(target.OccupiedTilePosition.X, target.OccupiedTilePosition.Y));

                    target.IsActive = false;
                    LogicSystem.RemoveGameObject(target);
                }
            }
            else RenderSystem.SubmitPersistentCommand(() => {
                RenderSystem.DrawLine(Position , target.Position  , Color.Gray, 1f);
            }, framesToLive: 2);
        }
        protected void Attack(Building target)
        {
            if (CooldownTimer <= 0)
            {
                RenderSystem.SubmitPersistentCommand(() => {
                    RenderSystem.DrawLine(Position, target.Position, Color.Red, 2f);
                }, framesToLive: 3);
                target.Health -= AttackDamage;
                CooldownTimer = AttackCooldown;
                if (target.Health <= 0)
                {
                    Game.instance._currentLevel.TileMap.DeoccupyTile(target.Position);
                    Game.instance._currentLevel.TileMap.DeoccupyTile(Game.instance._currentLevel.TileMap.GridToWorldPosition(target.OccupiedTilePosition.X, target.OccupiedTilePosition.Y));

                    target.IsActive = false;
                    LogicSystem.RemoveGameObject(target);
                }
            }
            else RenderSystem.SubmitPersistentCommand(() => {
                RenderSystem.DrawLine(Position, target.Position, Color.Gray, 1f);
            }, framesToLive: 2);
        }
        protected void MoveAlongPath(float deltaTime)
        {
            if (Path.Count == 0) return;

            var targetPos = Path[0];
            Point targetTile = Game.instance._currentLevel.TileMap.WorldToGridPosition(targetPos);
            
                if (!Game.instance._currentLevel.TileMap.IsOccupied(targetTile.X, targetTile.Y))
                {
                    Game.instance._currentLevel.TileMap.DeoccupyTile(Game.instance._currentLevel.TileMap.GridToWorldPosition(OccupiedTilePosition.X, OccupiedTilePosition.Y));
                    OccupiedTile = Game.instance._currentLevel.TileMap.GetTile(targetTile.X, targetTile.Y);
                    OccupiedTilePosition = targetTile;
                    Game.instance._currentLevel.TileMap.DeoccupyTile(Position);
                    Game.instance._currentLevel.TileMap.OccupyTile(targetPos);

                }
                
                var direction = Vector2.Normalize(targetPos - Position);
                var distance = Vector2.Distance(Position, targetPos);

                if (distance < 2 * MoveSpeed * deltaTime)
                {
                    Position = targetPos;
                    Game.instance._currentLevel.TileMap.OccupyTile(Position);
                    Path.RemoveAt(0);
                }
                else
                {
                    Position += direction * MoveSpeed * (float)(deltaTime+Random.Shared.NextDouble()* deltaTime);
                }

            

            

        }
    }
}
