using _2D_Engine_Sokov.UIElements;
using _2D_Engine_Sokov.WarDots;

namespace _2D_Engine_Sokov.WarDots.UI
{
    internal class WarDotsIntermediaController : UIElement
    {
        private double timer = 0f;
        public string NextLevel = "Content/Levels/WarDots/LevelMenu.xml";
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

            if (educ_anim_completed)
            {
                timer += deltaTime;
                if (timer > 5)
                {
                    if (GameContext.GetUIElements().OfType<Animation>().ToArray().Length == 0 && educ_anim_completed)
                    {
                        checkTimes += 1;
                        if (!load && checkTimes > 10)
                        {
                            load = true;
                            WarDotsGame.Instance.LoadLevel(NextLevel);
                        }
                    }
                    else checkTimes = 0;
                }

            }
        }
    }
}
