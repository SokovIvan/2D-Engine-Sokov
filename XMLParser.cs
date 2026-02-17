using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;

using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using _2D_Engine_Sokov.GameObjects;
using _2D_Engine_Sokov.UIElements;
using System.ComponentModel;
using Microsoft.Xna.Framework.Graphics;

namespace _2D_Engine_Sokov
{
    public class XMLParser
    {
        public GameLevel current_level;

        public GameLevel LoadLevel(string path)
        {
            XDocument doc = XDocument.Load(path);
            XElement root = doc.Element("Level");

            GameLevel level = new GameLevel
            {
                Name = root.Attribute("Name")?.Value ?? "Unnamed",
                backColor = ParseColor(root.Attribute("BackgroundColor")?.Value) ?? Color.CornflowerBlue,
                gravityForce = float.Parse(root.Attribute("Gravity")?.Value ?? "500"),
                gameObjects = new List<GameObject>(),
                uIElements = new List<UIElement>(),
                backgrounds = new List<Sprite>(),
                TileMap = ParseTileMap(root.Element("TileMap"))
            };

            // Парсинг игровых объектов
            foreach (var objElement in root.Elements("GameObject"))
            {
                GameObject obj = ParseGameObject(objElement);
                if (obj != null) level.gameObjects.Add(obj);
            }

            // Парсинг UI элементов
            foreach (var uiElement in root.Elements("UIElement"))
            {
                UIElement ui = ParseUIElement(uiElement);
                if (ui != null) level.uIElements.Add(ui);
            }

            // Парсинг фоновых спрайтов
            foreach (var bgElement in root.Elements("Background"))
            {
                foreach (var spriteEl in bgElement.Elements("Sprite"))
                {
                    Sprite bg = ParseSprite(spriteEl);
                    if (bg != null) level.backgrounds.Add(bg);
                }
            }

            current_level = level;
            return level;
        }

        private TileMap ParseTileMap(XElement tileMapElement)
        {
            if (tileMapElement == null) return null;

            int width = int.Parse(tileMapElement.Attribute("Width")?.Value ?? "10");
            int height = int.Parse(tileMapElement.Attribute("Height")?.Value ?? "10");
            int tileWidth = int.Parse(tileMapElement.Attribute("TileWidth")?.Value ?? "32");
            int tileHeight = int.Parse(tileMapElement.Attribute("TileHeight")?.Value ?? "32");

            var tileMap = new TileMap(width, height, tileWidth, tileHeight);
            var tileTextures = new Dictionary<string, Texture2D>();

            foreach (var tileElement in tileMapElement.Elements("Tile"))
            {
                int x = int.Parse(tileElement.Attribute("X")?.Value ?? "0");
                int y = int.Parse(tileElement.Attribute("Y")?.Value ?? "0");
                bool isWalkable = bool.Parse(tileElement.Attribute("IsWalkable")?.Value ?? "true");
                string textureName = tileElement.Attribute("Texture")?.Value;

                if (!string.IsNullOrEmpty(textureName) && !tileTextures.ContainsKey(textureName))
                {
                    using var stream = System.IO.File.OpenRead(textureName);
                    tileTextures[textureName] = Texture2D.FromStream(RenderSystem._graphicsDevice, stream);
                }

                tileMap.SetTile(x, y, new Tile(isWalkable, textureName));
            }

            tileMap.GenerateMapTexture(RenderSystem._graphicsDevice, tileTextures);
            return tileMap;
        }

        private Color? ParseColor(string colorStr)
        {
            if (string.IsNullOrEmpty(colorStr)) return null;

            try
            {
                var parts = colorStr.Split(',');
                return new Color(
                    byte.Parse(parts[0]),
                    byte.Parse(parts[1]),
                    byte.Parse(parts[2]));
            }
            catch
            {
                return null;
            }
        }

        private GameObject ParseGameObject(XElement element)
        {
            if (element == null) return null;

            string typeName = element.Attribute("Type")?.Value ?? "GameObject";
            string texturePath = element.Attribute("Texture")?.Value;
            string tag = element.Attribute("Tag")?.Value;
            string name = element.Attribute("Name")?.Value;
            Vector2 size = ParseVector2(element.Attribute("Size")?.Value) ?? Vector2.One;
            Vector2 position = ParseVector2(element.Attribute("Position")?.Value) ?? Vector2.Zero;
            Vector2 scale = ParseVector2(element.Attribute("Scale")?.Value) ?? Vector2.One;
            bool isActive = bool.Parse(element.Attribute("IsActive")?.Value ?? "true");
            float rotation = float.Parse(element.Attribute("Rotation")?.Value ?? "0");

            GameObject gameObject;

            switch (typeName)
            {
                case "PlayerUnit":
                    gameObject = new PlayerUnit();
                    break;
                case "EnemyUnit":
                    gameObject = new EnemyUnit();
                    break;
                case "PlayerBuilding":
                    gameObject = new PlayerBuilding();
                    foreach (var compElement in element.Elements("GameObject"))
                    {
                        var child = ParseGameObject(compElement);
                        ((PlayerBuilding)gameObject).ProduceUnit = (Unit)child;
                    }
                    break;
                case "EnemyBuilding":

                    gameObject = new EnemyBuilding();
                    foreach (var compElement in element.Elements("GameObject"))
                    {
                        var child = ParseGameObject(compElement);
                        ((EnemyBuilding)gameObject).ProduceUnit = (Unit)child;
                    }
                    break;
                default:
                    gameObject = new GameObject();
                    break;
            }

            gameObject.Tag = tag;
            gameObject.Name = name;
            gameObject.Position = position;
            gameObject.Rotation = rotation;
            gameObject.IsActive = isActive;
            gameObject.Size = size;

            if (gameObject is Sprite sprite && !string.IsNullOrEmpty(texturePath))
            {
                RenderSystem.EnqueueTextureLoad(sprite, texturePath);
            }

            //foreach (var compElement in element.Elements("GameObject"))
            //{
            //    var child = ParseGameObject(compElement);
            //    gameObject.AddChild(child);
            //}

            foreach (var compElement in element.Elements("Component"))
            {
                ParseComponent(gameObject, compElement);
            }

            return gameObject;
        }

        private UIElement ParseUIElement(XElement element)
        {
            if (element == null) return null;

            string typeName = element.Attribute("Type")?.Value ?? "UIElement";
            string text = element.Attribute("Text")?.Value ?? " ";
            Vector2 position = ParseVector2(element.Attribute("Position")?.Value) ?? Vector2.Zero;
            Vector2 textOffset = ParseVector2(element.Attribute("TextOffset")?.Value) ?? Vector2.Zero;
            Vector2 size = ParseVector2(element.Attribute("Size")?.Value) ?? new Vector2(100, 50);
            Color color = ParseColor(element.Attribute("Color")?.Value) ?? Color.White;
            bool isActive = bool.Parse(element.Attribute("IsActive")?.Value ?? "true");
            string texturePath = element.Attribute("Texture")?.Value;
            float rate = float.Parse(element.Attribute("Rate")?.Value ?? "10");
            UIElement uiElement;

            switch (typeName)
            {
                case "Button":
                    uiElement = new Button()
                    {
                        Position = position,
                        Size = size,
                        Color = color,
                        IsActive = isActive,
                        text = text,
                        TextOffset = textOffset
                    };
                    RenderSystem.EnqueueTextureLoad(uiElement, texturePath);
                    break;
                case "BuildButton":
                    uiElement = new BuildButton()
                    {
                        Position = position,
                        Size = size,
                        Color = color,
                        IsActive = isActive,
                        text = text
                    };
                    RenderSystem.EnqueueTextureLoad(uiElement, texturePath);
                    foreach (var compElement in element.Elements("GameObject"))
                    {
                        var child = ParseGameObject(compElement);
                        ((BuildButton)uiElement).building = (Building)child;
                    }
                    break;
                case "PlayerController":
                    uiElement = new PlayerController()
                    {
                        Position = position,
                        Size = size,
                        Color = color,
                        IsActive = isActive,
                        text = ""
                    };
                    RenderSystem.EnqueueTextureLoad(uiElement, texturePath);
                    break;
                case "GameController":
                    uiElement = new GameController()
                    {
                        Position = position,
                        Size = size,
                        Color = color,
                        IsActive = isActive,
                        text = ""
                    };
                    RenderSystem.EnqueueTextureLoad(uiElement, texturePath);
                    break;
                case "AIController":
                    uiElement = new AIController()
                    {
                        Position = position,
                        Size = size,
                        Color = color,
                        IsActive = isActive,
                        text = ""
                    };
                    RenderSystem.EnqueueTextureLoad(uiElement, texturePath);
                    break;
                case "EducationController":
                    uiElement = new EducationController()
                    {
                        Position = position,
                        Size = size,
                        Color = color,
                        IsActive = isActive,
                        text = ""
                    };
                    RenderSystem.EnqueueTextureLoad(uiElement, texturePath);
                    break;
                case "Animation":
                    Animation anim = new Animation()
                    {
                        Position = position,
                        Size = size,
                        Color = color,
                        IsActive = isActive,
                        TextOffset = textOffset,
                        time_rate = rate,
                    };

                    var frames = element.Elements("Component").ToList();
                    if (frames.Count > 0)
                    {
                        anim.AnimFrameTextures = new Texture2D[frames.Count];
                        anim.textsFrames = new string[frames.Count];
                        anim.colorsFrames = new Color[frames.Count];
                        anim.positionFrames = new Vector2[frames.Count];
                        anim.scaleFrames = new Vector2[frames.Count];

                        for (int i = 0; i < frames.Count; i++)
                        {
                            XElement frameEl = frames[i];
           
                            string frameTexturePath = frameEl.Attribute("Texture")?.Value;
                            if (!string.IsNullOrEmpty(frameTexturePath))
                            {
                                try
                                {
                                    using var stream = System.IO.File.OpenRead(frameTexturePath); 
                                    anim.AnimFrameTextures[i] = Texture2D.FromStream(RenderSystem._graphicsDevice, stream);
                                }
                                catch {  }
                            }
                            // Остальные свойства кадра
                            anim.textsFrames[i] = frameEl.Attribute("Text")?.Value ?? "";

                            string colStr = frameEl.Attribute("Color")?.Value;
                            anim.colorsFrames[i] = ParseColor(colStr) ?? Color.White;

                            string posStr = frameEl.Attribute("Position")?.Value;
                            anim.positionFrames[i] = ParseVector2(posStr) ?? Vector2.Zero;

                            string sizeStr = frameEl.Attribute("Size")?.Value;
                            Vector2 frameSize = ParseVector2(sizeStr) ?? new Vector2(1, 1);

                            anim.scaleFrames[i] = frameSize;
                        }
                    }
                    uiElement = anim;
                    break;
                default:
                    uiElement = new UIElement()
                    {
                        Position = position,
                        IsActive = isActive
                    };
                    break;
            }
            string onClickAction = element.Attribute("OnClick")?.Value;
            if (!string.IsNullOrEmpty(onClickAction))
            {
                uiElement.OnClick = UIActions.GetAction(onClickAction);

            }
            return uiElement;
        }

        private Sprite ParseSprite(XElement element)
        {
            if (element == null) return null;

            string texturePath = element.Attribute("Texture")?.Value;
            Vector2 position = ParseVector2(element.Attribute("Position")?.Value) ?? Vector2.Zero;
            Vector2 scale = ParseVector2(element.Attribute("Scale")?.Value) ?? Vector2.One;
            Color color = ParseColor(element.Attribute("Color")?.Value) ?? Color.White;
            bool isActive = bool.Parse(element.Attribute("IsActive")?.Value ?? "true");

            if (string.IsNullOrEmpty(texturePath)) return null;

            Sprite sprite = new Sprite()
            {
                Position = position,
                Scale = scale,
                Color = color,
                IsActive = isActive,
                LayerDepth = 1f
            };
            RenderSystem.EnqueueTextureLoad(sprite, texturePath);

            return sprite;
        }

        private Vector2? ParseVector2(string vectorStr)
        {
            if (string.IsNullOrEmpty(vectorStr)) return null;

            try
            {
                var parts = vectorStr.Split(',');
                return new Vector2(
                    float.Parse(parts[0]),
                    float.Parse(parts[1]));
            }
            catch
            {
                return null;
            }
        }

        private void ParseComponent(GameObject gameObject, XElement element)
        {
            if (element == null) return;

            string typeName = element.Attribute("Type")?.Value;
            switch (typeName)
            {
                case "Collider":
                    float mass = float.Parse(element.Attribute("Mass")?.Value ?? "1");
                    bool gravity = bool.Parse(element.Attribute("Gravity")?.Value ?? "false");
                    gameObject.CollisionEnabled = true;
                    gameObject.Mass = mass;
                    gameObject.GravityEnabled = gravity;
                    break;
                default:
                    break;
            }
        }
    }
}
