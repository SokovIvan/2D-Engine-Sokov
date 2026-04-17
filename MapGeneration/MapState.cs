using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Engine_Sokov.MapGeneration
{
    public class MapState
    {
        private MapGroundStates[,] Map;
        private int[,] HeightMap;
        private int width = 0;
        private int height = 0;
        public string path_to_image;
        public MapState()
        {
            path_to_image = "";
            Map = new MapGroundStates[0,0];
            HeightMap = new int[0, 0];
        }
        public MapState(MapGroundStates[,] map, int[,] heightmap, string path)
        {
            path_to_image= path;
            if (map != null) {
                Map = (MapGroundStates[,]?)map.Clone();
                height = Map.GetUpperBound(0) + 1;
                width = Map.Length / height;
            }
            if (heightmap != null)
                HeightMap = (int[,]?)heightmap.Clone();
        }
        public MapGroundStates getGroundState(int x, int y) {
            try {
                return Map[x,y];
            }
            catch(Exception e)
            {
                return MapGroundStates.emptiness;
            }
        }
        public int getHeight(int x, int y)
        {
            try
            {
                return HeightMap[x, y];
            }
            catch (Exception e)
            {
                return 0;
            }
        }
        public bool getFromImage(string path)
        {
            try
            {
                MapState loadedState = MapReader.LoadFromImage(path);

                this.Map = loadedState.Map; 
                this.HeightMap = loadedState.HeightMap;
                this.path_to_image = path;

                if (this.Map != null)
                {
                    this.height = this.Map.GetUpperBound(0) + 1;
                    this.width = this.Map.GetLength(0);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки карты: {ex.Message}");
                return false;
            }
        }
    }
    public enum MapGroundStates: short { 
        ground = 0,
        stone = 1,
        metal = 2,
        grass = 3,
        water = 4,
        lava = 5,
        toxic = 6,
        resource = 7, 
        emptiness = 8,
    }
}
