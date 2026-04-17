using System.Drawing;
using System.Drawing.Imaging;
using System;
using NAudio.Codecs;
using Assimp;

namespace _2D_Engine_Sokov.MapGeneration
{
    internal class MapGenerator
    {
        public class Blob
        {
            public float X, Y;
            public float Radius;
            public float Intensity; 
        }
        private static int[,] ApplyBlobLayer(Random rand, int width, int height, int[,] HeightMap, 
            int count = 5, int avgRadius = 5, int avgIntensity = 40, int min_height = -5, int max_height = 5)
        {
            float[,] height_map = new float[width,height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    height_map[i, j] = HeightMap[i, j];
                }
            }
            for (int i = 0; i < count; i++)
            {
                int x = rand.Next(width);
                int y = rand.Next(height);
                int r = avgRadius + rand.Next(-avgRadius, avgRadius);
                int intensity = rand.Next(-avgIntensity, avgIntensity+1);

                int minX = Math.Max(0, (int)(x - r * r));
                int maxX = Math.Min(width, (int)(x + r * r));
                int minY = Math.Max(0, (int)(y - r * r));
                int maxY = Math.Min(height, (int)(y + r * r));

                for (int cx = minX; cx < maxX; cx++)
                {
                    for (int cy = minY; cy < maxY; cy++)
                    {
                        float distSq = (cx - x) * (cx - x) + (cy - y) * (cy - y);
                        if (distSq < r * r * r)
                        {
                            // Гауссово затухание
                            float val = (intensity * MathF.Exp(-distSq / (2 * r * r)));
                            height_map[cx, cy] += val;
                            
                        }
                    }
                }
            }
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (height_map[i, j] < min_height) height_map[i, j] = min_height;
                    if (height_map[i, j] > max_height) height_map[i, j] = max_height;
                }
            }
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    HeightMap[i, j] = (int)height_map[i, j];
                }
            }
            return HeightMap;
        }
        private static MapGroundStates[,] ApplyMapLayers(Random random, int width, int height, MapGroundStates[,] Map, int[,] HeightMap)
        {



            float waterThreshold = -3.0f;   
            float toxicThreshold = -4.5f;  

            float grassThreshold = 0.0f;    
            float groundThreshold = 2.0f;   

            float stoneThreshold = 4.0f;   
            float metalThreshold = 4.5f;    

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    float currentHeight = (float)HeightMap[i, j];
                    MapGroundStates terrainType;

                    if (currentHeight <= toxicThreshold)
                    {
                        terrainType = MapGroundStates.toxic;
                    }
                    else if (currentHeight <= waterThreshold)
                    {
                        terrainType = MapGroundStates.water;
                    }
                    else if (currentHeight <= grassThreshold)
                    {

                        terrainType = MapGroundStates.grass;
                    }
                    else if (currentHeight <= groundThreshold)
                    {
                        if (random.Next(0, 100) < 5)
                            terrainType = MapGroundStates.resource;
                        else
                            terrainType = MapGroundStates.ground;
                    }
                    else if (currentHeight <= stoneThreshold)
                    {

                        terrainType = MapGroundStates.stone;
                    }
                    else if (currentHeight <= metalThreshold)
                    {
 
                        if (random.Next(0, 10) > 8) 
                            terrainType = MapGroundStates.metal;
                        else
                            terrainType = MapGroundStates.stone;
                    }
                    else
                    {

                        if (currentHeight >= 5.0f)
                            terrainType = MapGroundStates.emptiness;
                        else
                            terrainType = MapGroundStates.stone; 
                    }

                    Map[i, j] = terrainType;
                }
            }
            return Map;
        }
        public static MapState GenerateMapState(int width = 96, int height = 96, int min_height = -5, int max_height = 5, int hash = 0)
        {
            MapGroundStates[,] Map = new MapGroundStates[width, height];
            int[,] HeightMap = new int[width, height];
            Random random = new Random(hash);
            int c = 10;
            int rad = width / c;
            HeightMap = ApplyBlobLayer(random, width, height, HeightMap, count: c, avgIntensity: 10, avgRadius: rad );
            c = 50;
            rad = width / c;
            HeightMap = ApplyBlobLayer(random, width, height, HeightMap, count: c, avgIntensity: 3, avgRadius: rad  );
            c = 100;
            rad = width / c;
            HeightMap = ApplyBlobLayer(random, width, height, HeightMap, count: c, avgIntensity: 1, avgRadius: rad );
            HeightMap = ApplyBlobLayer(random, width, height, HeightMap);
            Map = ApplyMapLayers(random, width, height, Map, HeightMap);

            string rawDataPath = GenerateRawDataImage(Map, HeightMap);

            string visualPath = GenerateVisualMap(Map, HeightMap);

            return new MapState(Map, HeightMap, rawDataPath);
        }
        private static void EnsureDirectoryExists(string path)
        {
            string dir = System.IO.Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
            {
                System.IO.Directory.CreateDirectory(dir);
            }
        }
        public static string GenerateRawDataImage(MapGroundStates[,] map, int[,] heightmap, string path = "Content/Maps/map_data.png")
        {
            int height = map.GetUpperBound(0) + 1;
            int width = map.GetLength(0);

            using (Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        byte r = (byte)((sbyte)heightmap[x, y]);
                        byte g = (byte)map[x, y];
                        byte b = 0;
                        byte a = 255;

                        Color color = Color.FromArgb(a, r, g, b);
                        bitmap.SetPixel(x, y, color);
                    }
                }

                EnsureDirectoryExists(path);
                bitmap.Save(path, ImageFormat.Png);
            }
            return path;
        }

        public static string GenerateVisualMap(MapGroundStates[,] map, int[,] heightmap, string path = "Content/Maps/map_visual.png")
        {
            int height = map.GetUpperBound(0) + 1;
            int width = map.GetLength(0);

            using (Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        MapGroundStates state = map[x, y];
                        uint colorVal = GetColorForState(state);

                        // Преобразуем uint ARGB в Color
                        Color color = Color.FromArgb(
                            (int)((colorVal >> 24) & 0xFF), // A
                            (int)((colorVal >> 16) & 0xFF), // R
                            (int)((colorVal >> 8) & 0xFF),  // G
                            (int)(colorVal & 0xFF)          // B
                        );

                        bitmap.SetPixel(x, y, color);
                    }
                }

                EnsureDirectoryExists(path);
                bitmap.Save(path, ImageFormat.Png);
            }
            return path;
        }
        public enum MapVisualColors : uint
        {
            Ground = 0xFF8B4513,   // SaddleBrown
            Stone = 0xFF696969,    // DimGray
            Metal = 0xFF708090,    // SlateGray
            Grass = 0xFF228B22,    // ForestGreen
            Water = 0xFF1E90FF,    // DeepSkyBlue
            Lava = 0xFFFF4500,     // OrangeRed
            Toxic = 0xFF32CD32,    // LimeGreen
            Resource = 0xFFFFD700, // Gold
            Emptiness = 0xFF000000 // Black
        }
        private static uint GetColorForState(MapGroundStates state)
        {
            switch (state)
            {
                case MapGroundStates.ground: return (uint)MapVisualColors.Ground;
                case MapGroundStates.stone: return (uint)MapVisualColors.Stone;
                case MapGroundStates.metal: return (uint)MapVisualColors.Metal;
                case MapGroundStates.grass: return (uint)MapVisualColors.Grass;
                case MapGroundStates.water: return (uint)MapVisualColors.Water;
                case MapGroundStates.lava: return (uint)MapVisualColors.Lava;
                case MapGroundStates.toxic: return (uint)MapVisualColors.Toxic;
                case MapGroundStates.resource: return (uint)MapVisualColors.Resource;
                case MapGroundStates.emptiness:
                default: return (uint)MapVisualColors.Emptiness;
            }
        }

    }
}
