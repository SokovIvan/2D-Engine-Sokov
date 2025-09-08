using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _2D_Engine_Sokov.GameObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
namespace _2D_Engine_Sokov.UIElements
{
    public class BuildButton:Button
    {
        public Building building=null;

        public override void Update(double deltaTime)
        {
            
            base.Update(deltaTime);
            if(building!=null)
            OnClick = (() => {
                Console.WriteLine("PlacingBuilding");
                PlayerController.placingBuilding = true;
                PlayerController.placeBuilding = building; 

            });
        }
    }
}
