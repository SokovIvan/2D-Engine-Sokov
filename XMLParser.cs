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
                backgrounds = new List<Sprite>()
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
                foreach (var spriteEl in bgElement.Elements("Sprite")) { 
                    Sprite bg = ParseSprite(spriteEl);
                    if (bg != null) level.backgrounds.Add(bg);
                }

            }

            current_level = level;
            return level;
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
            float rotation = float.Parse(element.Attribute("Rotation")?.Value);
            GameObject gameObject;

            // Создаем конкретный тип объекта на основе Type
            switch (typeName)
            {
                case "Player":
                    gameObject = new Player();
                    break;
                case "Enemy":
                    gameObject = new Enemy();
                    break;
                // Добавьте другие типы по необходимости
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
                RenderSystem.EnqueueTextureLoad(sprite,texturePath);
            }

            foreach (var compElement in element.Elements("GameObject"))
            {
                var child= ParseGameObject(compElement);
                gameObject.AddChild(child);
            }

            foreach (var compElement in element.Elements("Component"))
            {
                ParseComponent(gameObject,compElement);
            }

            return gameObject;
        }

        private UIElement ParseUIElement(XElement element)
        {
            if (element == null) return null;

            string typeName = element.Attribute("Type")?.Value ?? "UIElement";
            string text = element.Attribute("Text")?.Value;
            Vector2 position = ParseVector2(element.Attribute("Position")?.Value) ?? Vector2.Zero;
            Vector2 size = ParseVector2(element.Attribute("Size")?.Value) ?? new Vector2(100, 50);
            Color color = ParseColor(element.Attribute("Color")?.Value) ?? Color.White;
            bool isActive = bool.Parse(element.Attribute("IsActive")?.Value ?? "true");
            string texturePath = element.Attribute("Texture")?.Value;

            UIElement uiElement;

            switch (typeName)
            {
                case "Button":
                    uiElement = new Button()
                    {
                        Position = position,
                        Size = size,
                        Color = color,
                        IsActive = isActive
                    };
                    RenderSystem.EnqueueTextureLoad(uiElement, texturePath);
                    break;
                /*case "Label":
                    uiElement = new Label(text)
                    {
                        Position = position,
                        Color = color,
                        IsActive = isActive
                    };
                    break;*/
                // Добавьте другие типы UI элементов по необходимости
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
            RenderSystem.EnqueueTextureLoad(sprite,texturePath);

            return sprite;
        }

        // Вспомогательные методы для парсинга

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

        private void ParseComponent(GameObject gameObject,XElement element)
        {
            if (element == null) return;

            string typeName = element.Attribute("Type")?.Value;
            switch (typeName)
            {
                case "Collider":
                    float mass = float.Parse(element.Attribute("Mass")?.Value);
                    bool gravity = Boolean.Parse(element.Attribute("Gravity")?.Value);
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
