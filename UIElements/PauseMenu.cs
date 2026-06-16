using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using _2D_Engine_Sokov.WarDots;

namespace _2D_Engine_Sokov.UIElements
{
    public class PauseMenu : UIElement
    {
        private bool _isOpen = false;
        private bool _isLoadSubmenuOpen = false;
        private bool _prevEscape = false;
        private List<UIElement> _children = new List<UIElement>();

        public string LevelPath { get; set; }
        private const string SavesDirectory = "Saves";

        public PauseMenu()
        {
            IsActive = true;
            Size = Vector2.Zero;
            Position = Vector2.Zero;
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            var kb = Keyboard.GetState();
            bool currentEscape = kb.IsKeyDown(Keys.Escape);

            if (currentEscape && !_prevEscape)
            {
                if (_isLoadSubmenuOpen) ShowMainMenu();
                else if (_isOpen) CloseMenu();
                else OpenMenu();
            }
            _prevEscape = currentEscape;
        }

        private void OpenMenu()
        {
            _isOpen = true;
            _isLoadSubmenuOpen = false;
            ClearChildren();

            PhysicsSystem.Pause();
            LogicSystem.Pause();

            CreateOverlay();

            float centerX = 640f - 100f;
            float startY = 180f;
            float spacing = 60f;

            var btnData = new[]
            {
                ("Продолжить", Color.Green, 0),
                ("Сохранить", Color.Cyan, 1),
                ("Загрузить", Color.Purple, 2),
                ("Начать заново", Color.Orange, 3),
                ("Выход", Color.Red, 4)
            };

            foreach (var (text, color, idx) in btnData)
            {
                AddMenuButton(text, color, new Vector2(centerX, startY + idx * spacing), () => HandleClick(idx));
            }
        }

        private void ShowMainMenu()
        {
            _isLoadSubmenuOpen = false;
            ClearChildren();
            OpenMenu();
        }

        private void ShowLoadMenu()
        {
            _isLoadSubmenuOpen = true;
            ClearChildren();
            CreateOverlay();

            float centerX = 640f - 100f;
            float startY = 150f;
            float spacing = 50f;

            AddMenuButton("◀ Назад", Color.Gray, new Vector2(centerX, startY), () => ShowMainMenu());

            string saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SavesDirectory);
            if (Directory.Exists(saveDir))
            {
                var files = Directory.GetFiles(saveDir, "*.xml");
                for (int i = 0; i < files.Length; i++)
                {
                    string fileName = Path.GetFileNameWithoutExtension(files[i]);
                    string fullPath = files[i];
                    AddMenuButton(fileName, Color.Purple, new Vector2(centerX, startY + 60 + i * spacing), () => LoadSavedLevel(fullPath));
                }
            }
            else
            {
                AddMenuButton("Сохранений нет", Color.DarkGray, new Vector2(centerX, startY + 60), () => { });
            }
        }

        private void HandleClick(int index)
        {
            switch (index)
            {
                case 0: CloseMenu(); break;
                case 1:
                    // Генерируем имя файла с датой
                    string fileName = $"Save_{DateTime.Now:yyyyMMdd_HHmmss}.xml";
                    SaveSystem.SaveLevel(fileName);
                    break;
                case 2: ShowLoadMenu(); break;
                case 3: CloseMenu(); if (!string.IsNullOrEmpty(LevelPath)) WarDotsGame.Instance?.LoadLevel(LevelPath); break;
                case 4: CloseMenu(); WarDotsGame.Instance?.LoadLevel("Content/Levels/WarDots/LevelMenu.xml"); break;
            }
        }

        private void LoadSavedLevel(string path)
        {
            CloseMenu();
            if (WarDotsGame.Instance != null)
                WarDotsGame.Instance.LoadLevel(path);
        }

        private void CreateOverlay()
        {
            var overlay = new UIElement()
            {
                Position = Vector2.Zero,
                Size = new Vector2(1280, 720),
                Color = new Color(0, 0, 0, 190),
                IsActive = true,
                LayerDepth = 0.8f,
                Name = "Temporal_Overlay"
            };
            AddChild(overlay);
        }

        private void AddMenuButton(string text, Color color, Vector2 pos, Action onClick)
        {
            var btn = new Button()
            {
                Position = pos,
                Size = new Vector2(200, 45),
                Color = color,
                IsActive = true,
                Text = text,
                LayerDepth = 0.9f,
                TextOffset = new Vector2(0, 0),
                Name = "Temporal_Button"
            };
            RenderSystem.EnqueueTextureLoad(btn, "Content/Textures/im1.png");
            btn.OnClick = () => onClick();
            AddChild(btn);
        }

        private void AddChild(UIElement child)
        {
            GameContext.AddUIElement(child);
            _children.Add(child);
        }

        private void ClearChildren()
        {
            foreach (var child in _children)
            {
                GameContext.RemoveUIElement(child);
            }
            _children.Clear();
        }

        private void CloseMenu()
        {
            _isOpen = false;
            _isLoadSubmenuOpen = false;
            ClearChildren();
            PhysicsSystem.Resume();
            LogicSystem.Resume();
        }
    }
}