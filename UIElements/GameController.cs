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
        public override void Start()
        {
            base.Start();
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
                        load=true;
                    if (NameLevel == "Level1") Game.instance.LoadLevel("Content/Levels/Level2.xml");
                    else if (NameLevel == "Level2") Game.instance.LoadLevel("Content/Levels/Level3.xml");
                    else if(NameLevel == "Level3") Game.instance.LoadLevel("Content/Levels/Level4.xml");
                        
                    }
                }
                else checkTimes = 10;
                if (LogicSystem.FindGameObjectsByTag("Player").Length <= 0)
                {
                    checkTimes2 -= 1;
                    if (checkTimes2 <= 0 && !load)
                    {
                        load = true;
                        Game.instance.LoadLevel("Content/Levels/Level1.xml");
                    }
                }
                else checkTimes2 = 10;
            }
        }
    }
}
