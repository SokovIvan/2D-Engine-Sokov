using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
namespace _2D_Engine_Sokov.UIElements
{
    public class Button:UIElement
    {
        double timer=0;
        public override void Update(double deltaTime)
        {
            
            base.Update(deltaTime);

            // Рисование текста
            /* Рисуем текст
            timer += deltaTime;

            if (timer < 1) return;
            if (IsMouseOver())
            {
                this.Color = Color.Red;
                
                RenderSystem.SubmitPersistentCommand(() => {
                    RenderSystem.FillRectangle(new Rectangle(50, 50, 100, 80), Color.Blue);
                }, framesToLive: 3);

            }
            else {
                this.Color = Color.White;

                RenderSystem.SubmitPersistentCommand(() => {
                    RenderSystem.DrawLine(new Vector2(0, 0), new Vector2(100, 100), Color.Red, 2f);
                }, framesToLive: 3);                

                
                RenderSystem.SubmitPersistentCommand(() => {
                    RenderSystem.DrawCircle(new Vector2(100, 100), 50f, Color.Green, 32, 2f);
                }, framesToLive: 3);
            }*/
        }
    }
}
