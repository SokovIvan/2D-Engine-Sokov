using Assimp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Engine_Sokov.MapGeneration
{
    internal class MapReader
    {
        public static MapState LoadFromImage(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                throw new FileNotFoundException("Файл карты не найден: " + path);
            }

            using (Bitmap bitmap = new Bitmap(Image.FromFile(path)))
            {
                int width = bitmap.Width;
                int height = bitmap.Height;

                MapGroundStates[,] map = new MapGroundStates[width, height];
                int[,] heightMap = new int[width, height];

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Color color = bitmap.GetPixel(x, y);

                        sbyte h = (sbyte)color.R;
                        heightMap[x, y] = (int)h;

                        short typeVal = (short)color.G;

                        if (Enum.IsDefined(typeof(MapGroundStates), typeVal))
                        {
                            map[x, y] = (MapGroundStates)typeVal;
                        }
                        else
                        {
                            map[x, y] = MapGroundStates.emptiness;
                        }
                    }
                }

                return new MapState(map, heightMap, path);
            }
        }
    }
}
