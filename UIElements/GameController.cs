using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Engine_Sokov.UIElements
{
    internal class GameController : UIElement
    {
        private double timer = 0f;
        public static string NameLevel = "Enemy";
        private int checkTimes = 10;
        private int checkTimes2 = 10;
        private bool load = false;
        public static GameController instance;
        public float playerRes = 100f;
        public float enemyRes = 100f;
        public override void Start()
        {
            base.Start();
            instance = this;
            Console.WriteLine("Start GameController");

        }
        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            timer += deltaTime;
            //Console.WriteLine(LogicSystem.FindGameObjectsByTag("Enemy").Length);
            if (timer > 10) {
                NameLevel = Game.instance._currentLevel.Name;
                if (LogicSystem.FindGameObjectsByTag("Enemy").Length <= 0)
                {
                    checkTimes -= 1;
                    if (checkTimes <= 0&& !load) {             
                        switch(NameLevel) {
                            case "Level0":
                                load = true;
                                Game.instance.LoadLevel("Content/Levels/Level1.xml");
                                break;
                            case "Level1":
                                load = true;
                                Game.instance.LoadLevel("Content/Levels/LevelIntermedia_1.xml");
                                break;
                            case "Level2":                        
                                if (playerRes > 300)
                                {
                                    Game.instance.LoadLevel("Content/Levels/LevelIntermedia_2.xml");
                                    load = true;
                                }
                                break;
                            case "Level3":
                                load = true;
                                Game.instance.LoadLevel("Content/Levels/Level4.xml");
                                break;
                            case "Level4":
                                load = true;
                                Game.instance.LoadLevel("Content/Levels/Level5.xml");
                                break;
                            case "Level5":
                                load = true;
                                Game.instance.LoadLevel("Content/Levels/LevelIntermedia_3.xml");
                                break;
                        }
                    }
                }
                else checkTimes = 10;
                if (LogicSystem.FindGameObjectsByTag("Player").Length <= 0)
                {
                    checkTimes2 -= 1;
                    if (checkTimes2 <= 0 && !load)
                    {
                        load = true;
                        Game.instance.LoadLevel("Content/Levels/LevelMenu.xml");
                    }
                }
                else checkTimes2 = 10;
            }
        }
    }
}
