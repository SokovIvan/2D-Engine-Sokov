using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace _2D_Engine_Sokov.UIElements
{
    public class Animation:UIElement
    {
        double timer=0;
        public double time_rate = 10;
        public Texture2D[] AnimFrameTextures;
        public string[] textsFrames;
        public Color[] colorsFrames;
        public Vector2[] positionFrames;
        public Vector2[] scaleFrames;
        private int cur_frame = 0;
        public bool completed = false;
        public override void Start()
        {
            base.Start();
            cur_frame = 0;
            if (AnimFrameTextures[cur_frame] != null) {
                Texture = AnimFrameTextures[cur_frame];
                Text = textsFrames[cur_frame];
                Color = colorsFrames[cur_frame];
                Position = positionFrames[cur_frame];
                Size = scaleFrames[cur_frame];
            }

        }
        public override void Update(double deltaTime)
        {

            base.Update(deltaTime);
            timer += deltaTime;

            if (AnimFrameTextures != null) {

                if (timer > time_rate) { 
                    timer = 0;
                    cur_frame += 1;
                    if (cur_frame < AnimFrameTextures.Length)
                    {
                        Texture = AnimFrameTextures[cur_frame];
                        Text = textsFrames[cur_frame];
                        Color = colorsFrames[cur_frame];
                        Position = positionFrames[cur_frame];
                        Size = scaleFrames[cur_frame];
                    }
                    else { 
                        completed = true;
                        GameContext.RemoveUIElement(this);
                    }
                }
            }
        }
    }
}
