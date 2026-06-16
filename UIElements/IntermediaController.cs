namespace _2D_Engine_Sokov.UIElements
{
    internal class IntermediaController : UIElement
    {
        private double timer = 0f;
        public static string NameLevel = "Enemy";
        private int checkTimes = 10;
        private bool load = false;
        public bool educ_anim_completed = false;

        public override void Start()
        {
            base.Start();
        }
        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            if (GameContext.GetUIElements().OfType<Animation>().ToArray().Length == 0) educ_anim_completed = true;
            else educ_anim_completed = false;
  

            if (educ_anim_completed) {
                timer += deltaTime;
                if (timer > 5) {
                    NameLevel = Game.instance._currentLevel.Name;
                    if (GameContext.GetUIElements().OfType<Animation>().ToArray().Length == 0 && educ_anim_completed)
                    {
                        checkTimes += 1;
                        if (!load && checkTimes > 10)
                        {
                            load = true;
                            switch (NameLevel) {
                                case "LevelIntermedia_1":
                                    Game.instance.LoadLevel("Content/Levels/Level2.xml");
                                    break;
                                case "LevelIntermedia_2":
                                    Game.instance.LoadLevel("Content/Levels/Level3.xml");
                                    break;
                                case "LevelIntermedia_3":
                                    Game.instance.LoadLevel("Content/Levels/LevelMenu.xml");
                                    break;
                            }
                        }
                    }
                    else checkTimes = 0;

                }

            }
        }
    }
}
