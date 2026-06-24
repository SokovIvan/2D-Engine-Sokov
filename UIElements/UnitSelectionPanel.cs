using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using _2D_Engine_Sokov.WarDots.Units;
using _2D_Engine_Sokov.UIElements;
using _2D_Engine_Sokov.WarDots;

namespace _2D_Engine_Sokov.UIElements
{
    public class UnitSelectionPanel : UIElement
    {
        private List<WarDotsPlayerDivision> _currentSelection = new();
        private const int IconSize = 35;
        private const int Padding = 5;

        public UnitSelectionPanel()
        {
            // Полупрозрачный фон для панели
            this.Color = new Color(20, 20, 40, 200);
            this.IsActive = true;
            RenderSystem.EnqueueTextureLoad(this, "Content/Textures/im1.png");
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            var selected = WarDotsPlayerController.SelectedUnits;

            if (selected != null && selected.Count > 0)
            {
                _currentSelection = selected.ToList();
                this.IsActive = true;

                // Динамический размер
                int count = _currentSelection.Count;
                this.Size = new Vector2(count * (IconSize + Padding) + Padding, IconSize + Padding * 2);

                // Фиксированная позиция в НИЖНЕМ ЛЕВОМ углу экрана
                var viewport = RenderSystem._graphicsDevice.Viewport;
                this.Position = new Vector2(10, viewport.Height - this.Size.Y - 10);

                // Отправляем команду отрисовки в UI-слой (без камеры!)
                RenderSystem.SubmitPersistentCommand(() =>
                {
                    DrawPanelContent();
                }, 2, useCamera: false); // ← Ключевой параметр!
            }
            else
            {
                this.IsActive = false;
                _currentSelection.Clear();
            }
        }

        private void DrawPanelContent()
        {
            if (Texture == null || _currentSelection.Count == 0) return;

            float currentX = Position.X + Padding;
            float currentY = Position.Y + Padding;

            foreach (var unit in _currentSelection)
            {
                Rectangle iconRect = new Rectangle((int)currentX, (int)currentY, IconSize, IconSize);

                RenderSystem.FillRectangleUI(iconRect, new Color(((unit.MaxHealth - unit.Health)/ unit.MaxHealth)*255, 150, 0, 200)); //  подложка
                RenderSystem.DrawRectangleUI(iconRect, Color.White, 2f); // Белая рамка
                RenderSystem.DrawText(RenderSystem.GetDefaultFont(), ((unit.Health/unit.MaxHealth)*100).ToString() + "%", new Vector2(currentX, currentY + 8), Color.Red, 0.8f, useCamera: false);

                currentX += IconSize + Padding;
            }
        }
    }
}