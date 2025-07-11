using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
namespace _2D_Engine_Sokov.UIElements
{
    public class Button:UIElement
    {
        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            if (IsMouseOver())
            {
                this.Color = Color.Red;
            }
            else {
                this.Color = Color.White;
            }
        }
    }
}
