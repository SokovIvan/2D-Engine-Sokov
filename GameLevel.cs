using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Engine_Sokov
{
    public class GameLevel
    {
        public List<UIElement> uIElements;
        public List<GameObject> gameObjects;
        public List<Sprite> backgrounds;
        public Color backColor = Color.CornflowerBlue;
        public float gravityForce= 500f;
        public string Name="Unnamed";
        public TileMap TileMap { get; set; }
        public GameLevel() { 
        }

    }
}
