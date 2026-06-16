using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
namespace _2D_Engine_Sokov.UIElements
{
    public class ResShowWD:UIElement
    {
        public override void Update(double deltaTime)
        {            
            base.Update(deltaTime);
            Text = "РЕС:"+WarDots.WarDotsPlayerController.PlayerResources.ToString();


        }
    }
}
