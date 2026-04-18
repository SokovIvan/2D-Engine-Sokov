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
                int r = avgRadius + rand.Next(0, avgRadius);
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
                        if (distSq < r * r)
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
        private static Color ApplyHeightShading(Color baseColor, float heightNormalized)
        {
            // Нижний рельеф чуть темнее, верхний - светлее (диапазон 0.85 - 1.15)
            float factor = 0.85f + 0.35f * heightNormalized;
            int r = (int)Math.Max(0, Math.Min(255, baseColor.R * factor));
            int g = (int)Math.Max(0, Math.Min(255, baseColor.G * factor));
            int b = (int)Math.Max(0, Math.Min(255, baseColor.B * factor));
            return Color.FromArgb(baseColor.A, r, g, b);
        }
        private static MapGroundStates[,] ApplyMapLayers(Random random, int width, int height, MapGroundStates[,] Map, int[,] HeightMap, int min_height = -5, int max_height = 5)
        {

            float xenoTh = min_height;
            float toxicTh = min_height * 0.95f;
            float waterTh = min_height * 0.7f;
            float grassTh = min_height * 0.2f;
            float forestTh = max_height * 0.2f;
            float groundTh = max_height * 0.6f;
            float stoneTh = max_height * 0.75f;
            float metalTh = max_height * 0.9f;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    float currentH = (float)HeightMap[i, j];

                    if (currentH <= xenoTh) Map[i, j] = MapGroundStates.xeno;
                    else if (currentH <= toxicTh) Map[i, j] = MapGroundStates.toxic;
                    else if (currentH <= waterTh) Map[i, j] = MapGroundStates.water;
                    else if (currentH <= grassTh) Map[i, j] = MapGroundStates.grass;
                    else if (currentH <= forestTh) Map[i, j] = MapGroundStates.forest;
                    else if (currentH <= groundTh) Map[i, j] = random.Next(100) < 5 ? MapGroundStates.resource : MapGroundStates.ground;
                    else if (currentH <= stoneTh) Map[i, j] = MapGroundStates.stone;
                    else if (currentH <= metalTh) Map[i, j] = random.Next(10) > 8 ? MapGroundStates.metal : MapGroundStates.stone;
                    else Map[i, j] = currentH >= max_height ? MapGroundStates.emptiness : MapGroundStates.stone;
                }
            }
            return Map;
        }
        private static int[,] SmoothHeightMap(int[,] heightMap, int width, int height, int passes = 2)
        {
            // Клонирование, чтобы не менять исходный массив случайно
            int[,] current = (int[,])heightMap.Clone();
            int[,] buffer = new int[width, height];

            for (int pass = 0; pass < passes; pass++)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int sum = 0;
                        int count = 0;

                        // Ядро 3x3
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            for (int dy = -1; dy <= 1; dy++)
                            {
                                int nx = x + dx;
                                int ny = y + dy;

                                // Безопасная проверка границ
                                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                                {
                                    sum += current[nx, ny];
                                    count++;
                                }
                            }
                        }
                        buffer[x, y] = sum / count;
                    }
                }

                // Быстрая замена ссылок без выделения памяти
                var temp = current;
                current = buffer;
                buffer = temp;
            }
            return current;
        }
        public static MapState GenerateMapState(int width = 96, int height = 96, int min_height = -5, int max_height = 5, int hash = 0)
        {
            MapGroundStates[,] Map = new MapGroundStates[width, height];
            int[,] HeightMap = new int[width, height];
            Random random = new Random(hash);
            int c = 5;
            int rad = width / c;
            HeightMap = ApplyBlobLayer(random, width, height, HeightMap, count: c, avgIntensity: (max_height - min_height), avgRadius: rad , min_height: min_height, max_height: max_height);
            c = c*c;
            rad = width / c;
            HeightMap = ApplyBlobLayer(random, width, height, HeightMap, count: c, avgIntensity: (max_height - min_height)/3, avgRadius: rad, min_height: min_height, max_height: max_height);
            c = c*c;
            rad = width / c;
            HeightMap = ApplyBlobLayer(random, width, height, HeightMap, count: c, avgIntensity: (max_height - min_height)/6, avgRadius: rad, min_height: min_height, max_height: max_height);

            HeightMap = SmoothHeightMap(HeightMap, width, height, passes: 3);

            Map = ApplyMapLayers(random, width, height, Map, HeightMap, min_height: min_height, max_height: max_height);

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

            // Находим реальные min/max высоты для корректной нормализации
            int actualMin = int.MaxValue;
            int actualMax = int.MinValue;
            foreach (var h in heightmap)
            {
                if (h < actualMin) actualMin = h;
                if (h > actualMax) actualMax = h;
            }
            float heightRange = actualMax - actualMin;
            if (heightRange == 0) heightRange = 1;

            using (Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        MapGroundStates state = map[x, y];
                        uint colorVal = GetColorForState(state);

                        Color baseColor = Color.FromArgb(
                            (int)((colorVal >> 24) & 0xFF),
                            (int)((colorVal >> 16) & 0xFF),
                            (int)((colorVal >> 8) & 0xFF),
                            (int)(colorVal & 0xFF)
                        );

                        // 1. Плавный шейдинг по высоте
                        float hNorm = (heightmap[x, y] - actualMin) / heightRange;
                        Color shadedColor = ApplyHeightShading(baseColor, hNorm);

                        // 2. Изолинии (контуры)
                        // Контур рисуется каждые 2 единицы высоты (можно менять contourStep)
                        int contourStep = 5;
                        bool isContour = ((heightmap[x, y] - actualMin) % contourStep == 0);

                        if (isContour)
                        {
                            // Затемняем пиксель, чтобы линия была видна, но не перекрывала базовый цвет
                            int darken = 25;
                            int r = Math.Max(0, shadedColor.R - darken);
                            int g = Math.Max(0, shadedColor.G - darken);
                            int b = Math.Max(0, shadedColor.B - darken);
                            shadedColor = Color.FromArgb(shadedColor.A, r, g, b);
                        }

                        bitmap.SetPixel(x, y, shadedColor);
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
            Grass = 0xFF228B22,    // GrassGreen
            Water = 0xFF1E90FF,    // DeepSkyBlue
            Lava = 0xFFFF4500,     // OrangeRed
            Toxic = 0xFF32CD32,    // LimeGreen
            Resource = 0xFFFFD700, // Gold
            Emptiness = 0xFF000000, // Black
            Forest = 0xFF11FF11,    // ForestGreen
            Xeno = 0xFF1FF0FF,    // Xeno
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
                case MapGroundStates.forest: return (uint)MapVisualColors.Forest;
                case MapGroundStates.xeno: return (uint)MapVisualColors.Xeno;
                case MapGroundStates.emptiness:
                default: return (uint)MapVisualColors.Emptiness;
            }
        }

    }
}
