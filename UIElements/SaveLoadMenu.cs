using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using _2D_Engine_Sokov.UIElements;

namespace _2D_Engine_Sokov
{
    public class SaveLoadMenu : UIElement
    {
        private List<UIElement> _dynamicButtons = new List<UIElement>();
        private const string SavesDirectory = "Saves";
        private bool _isInitialized = false;

        public SaveLoadMenu()
        {
            Name = "SaveLoadMenu";
            LayerDepth = 0.95f;
            IsActive = false; // По умолчанию скрыто
        }

        // Метод для открытия меню (вызывается из UISystem через действие)
        public void OpenMenu()
        {
            if (IsActive) return;

            ClearDynamicButtons();
            IsActive = true;

            // Создаем фон-подложку
            var overlay = new UIElement()
            {
                Position = Vector2.Zero,
                Size = new Vector2(1280, 720),
                Color = new Color(0, 0, 0, 200),
                IsActive = true,
                LayerDepth = 0.8f,
                Name = "LoadOverlay"
            };
            RenderSystem.EnqueueTextureLoad(overlay, "Content/Textures/im1.png");
            // Важно: добавляем оверлей в GameContext, чтобы он обновлялся и рисовался
            GameContext.AddUIElement(overlay);
            _dynamicButtons.Add(overlay);

            // Заголовок
            var title = new UIElement()
            {
                Position = new Vector2(640 - 100, 100),
                Size = new Vector2(200, 50),
                Text = "ЗАГРУЗИТЬ ИГРУ",
                Color = Color.White,
                IsActive = true,
                LayerDepth = 0.9f,
                TextOffset = new Vector2(0, 0)
            };
            GameContext.AddUIElement(title);
            _dynamicButtons.Add(title);

            // Кнопка "Назад"
            AddButton("◀ Назад", Color.Gray, new Vector2(640 - 100, 150), () => CloseMenu());

            // Список сохранений
            string saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SavesDirectory);
            float startY = 210f;
            float spacing = 50f;

            if (Directory.Exists(saveDir))
            {
                var files = Directory.GetFiles(saveDir, "*.xml")
                                     .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                                     .ToList();

                if (files.Count == 0)
                {
                    var noSaves = new UIElement()
                    {
                        Position = new Vector2(640 - 100, startY),
                        Size = new Vector2(200, 50),
                        Text = "Нет сохранений",
                        Color = Color.DarkGray,
                        IsActive = true,
                        LayerDepth = 0.9f,
                        TextOffset = new Vector2(0, 0)
                    };
                    GameContext.AddUIElement(noSaves);
                    _dynamicButtons.Add(noSaves);
                }
                else
                {
                    foreach (var file in files)
                    {
                        string fileName = Path.GetFileNameWithoutExtension(file);
                        if (fileName.Length > 25) fileName = fileName.Substring(0, 22) + "...";

                        string fullPath = file;
                        AddButton(fileName, Color.Purple, new Vector2(640 - 100, startY), () => LoadGame(fullPath));
                        startY += spacing;
                    }
                }
            }
        }

        public void CloseMenu()
        {
            IsActive = false;
            ClearDynamicButtons();
        }

        private void LoadGame(string path)
        {
            CloseMenu();
            WarDots.WarDotsGame.Instance?.LoadLevel(path);
        }

        private void AddButton(string text, Color color, Vector2 pos, Action onClick)
        {
            var btn = new Button()
            {
                Position = pos,
                Size = new Vector2(200, 45),
                Color = color,
                IsActive = true,
                Text = text,
                LayerDepth = 0.95f,
                TextOffset = new Vector2(0, 0),
                Name = "LoadBtn_" + text
            };
            RenderSystem.EnqueueTextureLoad(btn, "Content/Textures/im1.png");
            btn.OnClick = onClick;
            GameContext.AddUIElement(btn);
            _dynamicButtons.Add(btn);
        }

        private void ClearDynamicButtons()
        {
            foreach (var btn in _dynamicButtons)
            {
                GameContext.RemoveUIElement(btn);
            }
            _dynamicButtons.Clear();
        }

        // Переопределяем Update, если нужно обрабатывать закрытие по Escape, 
        // но пока оставим управление через кнопки
        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
        }
    }
}