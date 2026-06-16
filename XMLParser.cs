using Microsoft.Xna.Framework;
using System.Xml.Linq;
using _2D_Engine_Sokov.GameObjects;
using _2D_Engine_Sokov.UIElements;
using Microsoft.Xna.Framework.Graphics;
using _2D_Engine_Sokov.WarDots.Units;
using _2D_Engine_Sokov.WarDots.UI;
using _2D_Engine_Sokov.MapGeneration;
using System.Globalization;
using static _2D_Engine_Sokov.WarDots.WarDotsEnemyAI;

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
                TileMap = ParseTileMap(root.Element("TileMap")),
                MusicPath = root.Attribute("Music")?.Value,

            };
            string strategyStr = root.Attribute("Strategy")?.Value;
            if (!string.IsNullOrEmpty(strategyStr) && Enum.TryParse(strategyStr, true, out AiStrategy loadedStrategy))
            {
                SetStrategy(loadedStrategy);
            }
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

        private TileMap ParseTileMap(XElement mapElement)
        {
            if (mapElement == null) return null;

            int width = int.Parse(mapElement.Attribute("Width")?.Value ?? "64");
            int height = int.Parse(mapElement.Attribute("Height")?.Value ?? "64");
            int tileW = int.Parse(mapElement.Attribute("TileWidth")?.Value ?? "32");
            int tileH = int.Parse(mapElement.Attribute("TileHeight")?.Value ?? "32");
            bool autoGenerate = bool.Parse(mapElement.Attribute("AutoGenerate")?.Value ?? "false");
            int hash = int.Parse(mapElement.Attribute("Hash")?.Value ?? "0");
            int minH = int.Parse(mapElement.Attribute("MinHeight")?.Value ?? "-5");
            int maxH = int.Parse(mapElement.Attribute("MaxHeight")?.Value ?? "5");

            if (autoGenerate)
            {
                // Безопасное ожидание инициализации GPU
                while (RenderSystem._graphicsDevice == null) Thread.Sleep(10);

                Console.WriteLine($"[Parser] Генерация BattleMap: {width}x{height} (Hash: {hash})");
                var mapState = MapGenerator.GenerateMapState(width, height, minH, maxH, hash);
                var battlemap = BattleMap.FromMapState(mapState, tileW, tileH, RenderSystem._graphicsDevice);
                battlemap.Hash = hash;
                battlemap.MinHeight = minH;
                battlemap.MaxHeight = maxH;
                return battlemap;
            }

            // Fallback: ручная расстановка тайлов (совместимость со старыми уровнями)
            var manualMap = new BattleMap(width, height, tileW, tileH, new MapState());
            var tileTextures = new Dictionary<string, Texture2D>();

            foreach (var tileEl in mapElement.Elements("Tile"))
            {
                int x = int.Parse(tileEl.Attribute("X")?.Value ?? "0");
                int y = int.Parse(tileEl.Attribute("Y")?.Value ?? "0");
                bool walkable = bool.Parse(tileEl.Attribute("IsWalkable")?.Value ?? "true");
                string tex = tileEl.Attribute("Texture")?.Value;

                if (!string.IsNullOrEmpty(tex) && !tileTextures.ContainsKey(tex))
                {
                    try
                    {
                        using var stream = File.OpenRead(tex);
                        tileTextures[tex] = Texture2D.FromStream(RenderSystem._graphicsDevice, stream);
                    }
                    catch { /* Игнорируем битые ссылки */ }
                }

                manualMap.SetTile(x, y, new Tile(walkable, tex));
            }

            manualMap.GenerateMapTexture(RenderSystem._graphicsDevice, tileTextures);
            return manualMap;
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
            float rotation = 0f;
            string rotStr = element.Attribute("Rotation")?.Value;
            if (!string.IsNullOrEmpty(rotStr))
            {
                float.TryParse(rotStr, NumberStyles.Float, CultureInfo.InvariantCulture, out rotation);
            }

            GameObject gameObject = null;

            switch (typeName)
            {
                // 🟢 Все дивизии (игрок и враг)
                case "WarDotsPlayerDivision":
                case "WarDotsPlayerInfantry":
                case "WarDotsPlayerTank":
                case "WarDotsPlayerArtillery":
                case "WarDotsEnemyDivision":
                case "WarDotsEnemyInfantry":
                case "WarDotsEnemyTank":
                case "WarDotsEnemyArtillery":
                // 🏭 Все здания и заводы (игрок и враг)
                case "WarDotsPlayerFactory":
                case "WarDotsPlayerInfantryFactory":
                case "WarDotsPlayerTankFactory":
                case "WarDotsPlayerArtilleryFactory":
                case "WarDotsEnemyFactory":
                case "WarDotsEnemyInfantryFactory":
                case "WarDotsEnemyTankFactory":
                case "WarDotsEnemyArtilleryFactory":
                case "WarDotsPlayerBase":
                case "WarDotsEnemyBase":
                case "WarDotsPlayerResourceGenerator":
                case "WarDotsEnemyResourceGenerator":
                    var targetType = Type.GetType($"_2D_Engine_Sokov.WarDots.Units.{typeName}");
                    if (targetType != null)
                        gameObject = (GameObject)Activator.CreateInstance(targetType);
                    break;

                case "PlayerUnit":
                    gameObject = new PlayerUnit();
                    break;
                case "EnemyUnit":
                    gameObject = new EnemyUnit();
                    break;
                case "EnemyTurret":
                    gameObject = new EnemyTurret();
                    break;
                case "PlayerResGen":
                    gameObject = new PlayerResGen();
                    break;
                case "EnemyResGen":
                    gameObject = new EnemyResGen();
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
                case "ControlPoint":
                    gameObject = new ControlPoint();
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
            // Специфичная логика для WarDotsBuilding: очередь производства из XML
            if (gameObject is WarDotsBuilding wb)
            {
                string produceType = element.Attribute("ProduceType")?.Value;
                if (!string.IsNullOrEmpty(produceType))
                {
                    var unitType = Type.GetType($"_2D_Engine_Sokov.GameObjects.{produceType}");
                    if (unitType != null && unitType.IsSubclassOf(typeof(WarDotsDivision)))
                    {
                        // Безопасно ставим первый юнит в очередь (проверит ресурсы при старте)
                        wb.EnqueueProduction(unitType);
                    }
                }
            }
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
                case "WarDotsIntermediaController":
                    var ic = new WarDotsIntermediaController()
                    {
                        Position = position,
                        Size = size,
                        Color = color,
                        IsActive = isActive,
                        text = text,
                        TextOffset = textOffset
                    };
                    string lvlPath = element.Attribute("NextLevel")?.Value;
                    if (!string.IsNullOrEmpty(lvlPath)) ic.NextLevel = lvlPath;
                    uiElement = ic; 
                    RenderSystem.EnqueueTextureLoad(uiElement, texturePath);

                    break;
                case "WarDotsMatchEndController":
                    var im = new WarDotsMatchEndController()
                    {
                        Position = position,
                        Size = size,
                        Color = color,
                        IsActive = isActive,
                        text = text,
                        TextOffset = textOffset
                    };
                    string nextlvlPath = element.Attribute("NextLevelPath")?.Value;
                    if (!string.IsNullOrEmpty(nextlvlPath)) im.NextLevelPath = nextlvlPath;
                    string curlvlPath = element.Attribute("CurrentLevelPath")?.Value;
                    if (!string.IsNullOrEmpty(curlvlPath)) im.CurrentLevelPath = curlvlPath;
                    uiElement = im;
                    RenderSystem.EnqueueTextureLoad(uiElement, texturePath);
                    break;
                case "SaveLoadMenu":
                    uiElement = new SaveLoadMenu()
                    {
                        Position = position,
                        Size = size,
                        Color = color,
                        IsActive = isActive, // Обычно false при загрузке уровня
                        text = text,
                        TextOffset = textOffset
                    };
                    // Если нужна текстура фона для самого контейнера, можно загрузить
                    RenderSystem.EnqueueTextureLoad(uiElement, texturePath);
                    break;
                case "PauseMenu":
                    var pm = new PauseMenu()
                    {
                        Position = position,
                        Size = size,
                        Color = color,
                        IsActive = isActive,
                        text = text,
                        TextOffset = textOffset
                    };
                    // Если в XML будет атрибут LevelPath="...", можно распарсить его:
                    string lPath = element.Attribute("LevelPath")?.Value;
                    if (!string.IsNullOrEmpty(lPath)) pm.LevelPath = lPath;
                    uiElement = pm;
                    break;
                case "ResShow":
                    uiElement = new ResShow()
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
                case "ResShowWD":
                    uiElement = new ResShowWD()
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
                        text = text,                        
                    };
                    RenderSystem.EnqueueTextureLoad(uiElement, texturePath);
                    foreach (var compElement in element.Elements("GameObject"))
                    {
                        var child = ParseGameObject(compElement);
                        ((BuildButton)uiElement).building = (Building)child;
                    }
                    break;
                case "BuildButtonWD":
                    var wdButton = new BuildButtonWD()
                    {
                        Position = position,
                        Size = size,
                        Color = color,
                        IsActive = isActive,
                        text = text,
                        // Парсим стоимость из атрибута Cost, если он есть, иначе 100
                        Cost = int.Parse(element.Attribute("Cost")?.Value ?? "100")
                    };

                    // Определяем тип здания из атрибута BuildingType
                    string buildTypeName = element.Attribute("BuildingType")?.Value;
                    if (!string.IsNullOrEmpty(buildTypeName))
                    {
                        // Предполагаем, что типы зданий лежат в _2D_Engine_Sokov.WarDots.Units
                        var type = Type.GetType($"_2D_Engine_Sokov.WarDots.Units.{buildTypeName}");
                        if (type != null)
                        {
                            wdButton.BuildingType = type;
                        }
                    }

                    RenderSystem.EnqueueTextureLoad(wdButton, texturePath);
                    uiElement = wdButton;
                    break;
                case "UnitSelectionPanel":
                    uiElement = new UnitSelectionPanel()
                    {
                        Position = position,
                        Size = size,
                        Color = color,
                        IsActive = isActive,
                        text = text,
                        TextOffset = textOffset
                    };
                    // Текстура здесь может быть фоном, сами иконки рисуются внутри
                    RenderSystem.EnqueueTextureLoad(uiElement, texturePath);
                    break;
                case "ProduceButtonWD":
                    var prodButton = new ProduceButtonWD()
                    {
                        Position = position,
                        Size = size,
                        Color = color,
                        IsActive = isActive, // Будет false при загрузке
                        text = text,
                        TextOffset = textOffset,
                        Cost = int.Parse(element.Attribute("Cost")?.Value ?? "25")
                    };

                    string unitTypeName = element.Attribute("UnitType")?.Value;
                    if (!string.IsNullOrEmpty(unitTypeName))
                    {
                        var type = Type.GetType($"_2D_Engine_Sokov.WarDots.Units.{unitTypeName}");
                        if (type != null)
                        {
                            prodButton.UnitType = type;
                        }
                    }

                    RenderSystem.EnqueueTextureLoad(prodButton, texturePath);
                    uiElement = prodButton;
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
                case "IntermediaController":
                    uiElement = new IntermediaController()
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
            Vector2 size = ParseVector2(element.Attribute("Size")?.Value) ?? Vector2.One;
            Color color = ParseColor(element.Attribute("Color")?.Value) ?? Color.White;
            bool isActive = bool.Parse(element.Attribute("IsActive")?.Value ?? "true");

            if (string.IsNullOrEmpty(texturePath)) return null;

            Sprite sprite = new Sprite()
            {
                Position = position,
                Scale = scale,
                Color = color,
                IsActive = isActive,
                Size = size,
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
