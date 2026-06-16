using System.Globalization;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using _2D_Engine_Sokov.UIElements;
using _2D_Engine_Sokov.WarDots.Units;

namespace _2D_Engine_Sokov
{
    public static class SaveSystem
    {
        private const string SavesDirectory = "Saves";

        public static void SaveLevel(string path)
        {
            var level = GameContext.CurrentLevel;
            if (level == null) return;

            try
            {
                var doc = new XDocument();
                var root = new XElement("Level",
                    new XAttribute("Name", level.Name + "_Saved"),
                    new XAttribute("BackgroundColor", FormatColor(level.backColor)),
                    new XAttribute("Gravity", level.gravityForce.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("Music", level.MusicPath ?? "")
                );

                // Сохранение TileMap
                if (level.TileMap != null)
                {
                    root.Add(new XElement("TileMap",
                        new XAttribute("Width", level.TileMap.Width),
                        new XAttribute("Height", level.TileMap.Height),
                        new XAttribute("TileWidth", level.TileMap.TileWidth),
                        new XAttribute("TileHeight", level.TileMap.TileHeight),
                        new XAttribute("AutoGenerate", true),
                        new XAttribute("Hash", level.TileMap.Hash),
                        new XAttribute("MinHeight", level.TileMap.MinHeight),
                        new XAttribute("MaxHeight", level.TileMap.MaxHeight)
                    ));
                }

                // Сохранение GameObjects
                foreach (var obj in GameContext.GetGameObjects().Where(o => !(o is UIElement)))
                {
                    root.Add(SerializeGameObject(obj));
                }

                // Сохранение UIElements
                foreach (var ui in GameContext.GetUIElements().Where(u => !u.Name.Contains("Temporal")))
                {
                    root.Add(SerializeUIElement(ui));
                }

                doc.Add(root);

                string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SavesDirectory);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                // Если path относительный или это имя файла, корректируем
                string finalPath = path;
                if (!Path.IsPathRooted(path))
                {
                    finalPath = Path.Combine(dir, path);
                }

                doc.Save(finalPath);
                Console.WriteLine($"[SAVE] Успешно сохранено: {finalPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SAVE ERROR] {ex.Message}\n{ex.StackTrace}");
            }
        }
        public static string GetLatestSavePath()
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SavesDirectory);
            if (!Directory.Exists(dir))
                return null;

            var files = Directory.GetFiles(dir, "*.xml");
            if (files.Length == 0)
                return null;

            // Находим файл с самой поздней датой изменения
            string latestFile = files.OrderByDescending(f => new FileInfo(f).LastWriteTime).First();
            return latestFile;
        }
        private static XElement SerializeGameObject(GameObject obj)
        {
            var elem = new XElement("GameObject",
                new XAttribute("Type", obj.GetType().Name),
                new XAttribute("Position", FormatVector2(obj.Position)),
                new XAttribute("Size", FormatVector2(obj.Size)),
                new XAttribute("Scale", FormatVector2(obj.Scale)),
                new XAttribute("Rotation", obj.Rotation.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("Tag", obj.Tag ?? ""),
                new XAttribute("Name", obj.Name ?? ""),
                new XAttribute("IsActive", obj.IsActive)
            );

            if (obj is Sprite sprite && !string.IsNullOrEmpty(sprite.TexturePath))
            {
                elem.Add(new XAttribute("Texture", sprite.TexturePath));
            }

            // Специфичные данные для зданий
            if (obj is WarDotsBuilding wb)
            {
                // Можно сохранить очередь производства, если нужно
            }

            return elem;
        }

        private static XElement SerializeUIElement(UIElement ui)
        {
            var elem = new XElement("UIElement",
                new XAttribute("Type", ui.GetType().Name),
                new XAttribute("Name", ui.Name ?? ""),
                new XAttribute("Position", FormatVector2(ui.Position)),
                new XAttribute("Size", FormatVector2(ui.Size)),
                new XAttribute("Color", FormatColor(ui.Color)),
                new XAttribute("Text", ui.Text?.Trim() ?? ""),
                new XAttribute("TextOffset", FormatVector2(ui.TextOffset)),
                new XAttribute("LayerDepth", ui.LayerDepth.ToString(CultureInfo.InvariantCulture)),
                new XAttribute("IsActive", ui.IsActive)
            );

            if (!string.IsNullOrEmpty(ui.TexturePath))
            {
               // Console.WriteLine("[SaveSystem]"+ ui.TexturePath);
                elem.Add(new XAttribute("Texture", ui.TexturePath));
            }

            // Специфичные данные для кнопок WarDots
            if (ui is BuildButtonWD bbwd)
            {
                elem.Add(new XAttribute("Cost", bbwd.Cost));
                elem.Add(new XAttribute("BuildingType", bbwd.BuildingType?.Name ?? ""));
            }
            if (ui is PauseMenu pm)
            {
                elem.Add(new XAttribute("LevelPath", pm.LevelPath));
            }
            else if (ui is ProduceButtonWD pbwd)
            {
                elem.Add(new XAttribute("Cost", pbwd.Cost));
                elem.Add(new XAttribute("UnitType", pbwd.UnitType?.Name ?? ""));
            }

            return elem;
        }

        // --- Helpers ---

        private static string FormatVector2(Vector2 v)
        {
            return $"{(int)v.X},{(int)v.Y}";
        }

        private static string FormatColor(Color c)
        {
            return $"{c.R},{c.G},{c.B},{c.A}";
        }
    }
}