using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Text;
using System.Threading.Tasks;
using _2D_Engine_Sokov.GameObjects;
using _2D_Engine_Sokov.WarDots.Units;

namespace _2D_Engine_Sokov.UIElements
{
    internal class EducationController : UIElement
    {
        private double timer = 0f;
        public static string NameLevel = "Enemy";
        private int checkTimes = 10;
        private int checkTimes2 = 10;
        private bool load = false;
        public bool educ_anim_completed = false;
        public bool educ_movement_completed = false;
        private Vector2 startpos_unit;
        public bool educ_enemy_spawned = false;
        public bool educ_enemy_completed = false;
        public override void Start()
        {
            base.Start();
            Console.WriteLine("Start EducController");
            if(LogicSystem.FindGameObjectsByTag("Player").Length>0)
            startpos_unit = LogicSystem.FindGameObjectsByTag("Player")[0].Position;

        }
        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            if (LogicSystem.FindGameObjectsByTag("Player").Length > 0) {
                if (startpos_unit == null)
                {
                    startpos_unit = LogicSystem.FindGameObjectsByTag("Player")[0].Position;
                }
                else
                {
                    if (MathF.Abs(LogicSystem.FindGameObjectsByTag("Player")[0].Position.X - startpos_unit.X) > 100 && MathF.Abs(LogicSystem.FindGameObjectsByTag("Player")[0].Position.Y - startpos_unit.Y) > 100)
                        educ_movement_completed = true;
                }
            }


            if (GameContext.GetUIElements().OfType<Animation>().ToArray().Length == 0) educ_anim_completed = true;
            else educ_anim_completed = false;

            if (educ_anim_completed && educ_movement_completed && !educ_enemy_spawned) {
                educ_enemy_spawned =true;
                EnemyUnit enemy_unit = new EnemyUnit();

                if (enemy_unit != null)
                {
                    enemy_unit.Health = 50;
                    enemy_unit.Position = new Vector2(256,512);
                    enemy_unit.Size = new Vector2(32, 32);
                    RenderSystem.EnqueueTextureLoad(enemy_unit, "Content/Textures/enemy.png");

                    Game.SubmitObject(enemy_unit);
                }
            }           

            if (educ_enemy_spawned) {
                timer += deltaTime;
                if (timer > 5) {
                    Console.WriteLine(LogicSystem.FindGameObjectsByTag("Enemy").Length);
                    NameLevel = Game.instance._currentLevel.Name;
                    if (LogicSystem.FindGameObjectsByTag("Enemy").Length <= 0 && educ_enemy_spawned)
                    {
                        checkTimes += 1;
                        if (!load && checkTimes > 10)
                        {
                            load = true;
                            Game.instance.LoadLevel("Content/Levels/Level1.xml");

                        }
                    }
                    else checkTimes = 0;

                }

            }



        }
    }
}
