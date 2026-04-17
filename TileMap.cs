using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using _2D_Engine_Sokov.GameObjects;

namespace _2D_Engine_Sokov
{
    public class TileMap
    {
        public int TileWidth { get; private set; }
        public int TileHeight { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        private Tile[,] Tiles;
        private Texture2D MapTexture;
        public Sprite MapSprite;
        public Tile[,] GetTiles() { 
            return Tiles; 
        }
        public TileMap(int width, int height, int tileWidth, int tileHeight)
        {
            Width = width;
            Height = height;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            Tiles = new Tile[width, height];
        }

        public void SetTile(int x, int y, Tile tile)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                Tiles[x, y] = tile;
                Tiles[x, y].id = (x+1)*Width*100 + (y+1);
            }
        }
        public Point GetTilePoint(Tile tile)
        {
            for (int x=0; x<Width; x++)
                for (int y = 0; y < Height; y++)
                    if(Tiles[x, y] == tile)
                        return new Point(x, y);
                    return Point.Zero;
        }
        public Tile GetTile(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                return Tiles[x, y];
            }
            return null;
        }
        public bool IsWalkable(int x, int y)
        {
            var tile = GetTile(x, y);
            return tile != null && tile.IsWalkable;
        }
        public bool IsOccupied(int x, int y)
        {
            var tile = GetTile(x, y);
            return tile != null && tile.IsOccupied;
        }

        public void GenerateMapTexture(GraphicsDevice graphicsDevice, Dictionary<string, Texture2D> tileTextures)
        {
            using var done = new ManualResetEvent(false);

            // Отправляем генерацию в очередь рендер-потока
            RenderSystem.ExecuteOnRenderThread(() =>
            {
                try
                {
                    GenerateMapTextureInternal(graphicsDevice, tileTextures);
                }
                finally
                {
                    done.Set(); // Сигнализируем, что задача завершена
                }
            });

            // Ждём завершения, не блокируя рендер-поток
            done.WaitOne();
        }

        // Вся твоя оригинальная логика переехала сюда
        private void GenerateMapTextureInternal(GraphicsDevice graphicsDevice, Dictionary<string, Texture2D> tileTextures)
        {
            if (MapTexture != null && !MapTexture.IsDisposed)
            {
                MapTexture.Dispose();
            }

            MapTexture = new Texture2D(graphicsDevice, Width * TileWidth, Height * TileHeight);
            Color[] data = new Color[Width * TileWidth * Height * TileHeight];

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    var tile = Tiles[x, y];
                    if (tile != null && tileTextures.TryGetValue(tile.TextureName, out var texture))
                    {
                        Color[] tileData = new Color[TileWidth * TileHeight];
                        texture.GetData(tileData); // Теперь безопасно! Мы в RenderThread
                        for (int ty = 0; ty < TileHeight; ty++)
                        {
                            for (int tx = 0; tx < TileWidth; tx++)
                            {
                                int index = (y * TileHeight + ty) * (Width * TileWidth) + (x * TileWidth + tx);
                                data[index] = tileData[ty * TileWidth + tx];
                            }
                        }
                    }
                }
            }

            MapTexture.SetData(data);
            MapSprite = new Sprite
            {
                Texture = MapTexture,
                Position = Vector2.Zero,
                Origin = Vector2.Zero,
                Scale = Vector2.One,
                LayerDepth = 0f,
                IsActive = true
            };
        }
        public Vector2 GridToWorldPosition(int x, int y)
        {
            return new Vector2(x * TileWidth, y * TileHeight);
        }

        public bool OccupyTile(Vector2 worldPos)
        {
            Point grid = WorldToGridPosition(worldPos);
            Tile tile = GetTile(grid.X, grid.Y);
            if (tile.IsWalkable && !tile.IsOccupied)
            {
                tile.IsOccupied = true;
                return true;
            }
            return false;
        }
        public bool DeoccupyTile(Vector2 worldPos)
        {
            Point grid = WorldToGridPosition(worldPos);
            Tile tile = GetTile(grid.X, grid.Y);
            tile.IsOccupied = false;
            return true;
        }
        public Point WorldToGridPosition(Vector2 worldPos)
        {
            return new Point((int)(worldPos.X / TileWidth), (int)(worldPos.Y / TileHeight));
        }
    }

    public class Tile
    {
        public int id { get; set; }
        public bool IsWalkable { get; set; }
        public bool IsOccupied { get; set; }
        public string TextureName { get; set; }
        public Tile(bool isWalkable, string textureName)
        {
            IsWalkable = isWalkable;
            IsOccupied = false;
            TextureName = textureName;
        }
        public override bool Equals(object obj) { 
            if (!(obj is Tile)) return false;
            if (obj is Tile) { 
                if(id== ((Tile)obj).id)
                    return true;
            }
            return false;
        }
    }
}
