using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

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
            }
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

        public void GenerateMapTexture(GraphicsDevice graphicsDevice, Dictionary<string, Texture2D> tileTextures)
        {
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
                        texture.GetData(tileData);
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
            //RenderSystem.SubmitBackgrounds(new[] { MapSprite });
        }

        public Vector2 GridToWorldPosition(int x, int y)
        {
            return new Vector2(x * TileWidth, y * TileHeight);
        }

        public Point WorldToGridPosition(Vector2 worldPos)
        {
            return new Point((int)(worldPos.X / TileWidth), (int)(worldPos.Y / TileHeight));
        }
    }

    public class Tile
    {
        public bool IsWalkable { get; set; }
        public string TextureName { get; set; }

        public Tile(bool isWalkable, string textureName)
        {
            IsWalkable = isWalkable;
            TextureName = textureName;
        }
    }
}
