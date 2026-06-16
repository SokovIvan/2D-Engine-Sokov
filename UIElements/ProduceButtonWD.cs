using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using _2D_Engine_Sokov.GameObjects;
using _2D_Engine_Sokov.WarDots.Units;
using _2D_Engine_Sokov.UIElements;
using _2D_Engine_Sokov.WarDots;

namespace _2D_Engine_Sokov.UIElements
{
    public class ProduceButtonWD : Button
    {
        public Type UnitType { get; set; }
        public WarDotsBuilding TargetFactory { get; set; }
        public int Cost { get; set; } = 25;
        private bool _isContextActive = false;

        public ProduceButtonWD()
        {
            UnitType = typeof(WarDotsPlayerDivision);
            this.IsActive = false;
            this.Position = new Vector2(RenderSystem._graphicsDevice.Viewport.Width / 2 - Size.X / 2,
                            RenderSystem._graphicsDevice.Viewport.Height - Size.Y - 20);
            HideContext();
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            HandleContextClicks();

            if (_isContextActive && this.IsActive)
            {
                var mouse = InputSystem.GetMouseState();
                var rect = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);

                OnClick = () =>
                {
                    if (TargetFactory == null || !TargetFactory.IsActive) { HideContext(); return; }

                    if (WarDotsPlayerController.PlayerResources >= Cost)
                    {
                        bool success = TargetFactory.EnqueueProduction(UnitType);
                        if (!success) Console.WriteLine("[UI] Очередь полна.");
                        else Console.WriteLine($"[UI] Заказан: {UnitType.Name}");
                    }
                    else Console.WriteLine("[UI] Мало ресурсов.");
                };
            }
        }

        private void HandleContextClicks()
        {
            var mouse = InputSystem.GetMouseState();
            var cam = RenderSystem.GetCamera();
            if (cam == null) return;

            var worldPos = Vector2.Transform(new Vector2(mouse.X, mouse.Y), Matrix.Invert(cam.TransformMatrix));

            if (mouse.LeftButton == ButtonState.Pressed && !_isContextActive)
            {
                // Ищем ВСЕ заводы игрока (базовый + специализированные)
                var factories = LogicSystem.FindGameObjectsByTag("Player")
                                           .OfType<WarDotsPlayerFactory>(); // Базовый тип ловит наследников

                foreach (var factory in factories)
                {
                    float halfW = factory.Size.X / 2f;
                    float halfH = factory.Size.Y / 2f;
                    if (worldPos.X >= factory.Position.X - halfW && worldPos.X <= factory.Position.X + halfW &&
                        worldPos.Y >= factory.Position.Y - halfH && worldPos.Y <= factory.Position.Y + halfH)
                    {
                        ShowContext(factory);
                        break;
                    }
                }
            }

            if (mouse.RightButton == ButtonState.Pressed && _isContextActive) HideContext();
        }

        public void ShowContext(WarDotsPlayerFactory factory)
        {
            TargetFactory = factory;
            _isContextActive = true;
            this.IsActive = true;
            this.Position = new Vector2(RenderSystem._graphicsDevice.Viewport.Width / 2 - Size.X / 2,
                                        RenderSystem._graphicsDevice.Viewport.Height - Size.Y - 20);

            // Автоматический подбор типа юнита под завод
            switch (factory)
            {
                case WarDotsPlayerInfantryFactory:
                    UnitType = typeof(WarDotsPlayerInfantry);
                    Cost = factory.UnitProductionCost;
                    Text = "Пехота";
                    break;
                case WarDotsPlayerTankFactory:
                    UnitType = typeof(WarDotsPlayerTank);
                    Cost = factory.UnitProductionCost;
                    Text = "Танки";
                    break;
                case WarDotsPlayerArtilleryFactory:
                    UnitType = typeof(WarDotsPlayerArtillery);
                    Cost = factory.UnitProductionCost;
                    Text = "Артиллерия";
                    break;
                default:
                    UnitType = typeof(WarDotsPlayerDivision);
                    Cost = factory.UnitProductionCost;
                    Text = "Дивизия";
                    break;
            }
            Console.WriteLine("[UI] Меню производства открыто.");
        }

        public void HideContext()
        {
            _isContextActive = false;
            this.IsActive = false;
            Text = " ";
            TargetFactory = null;
            Console.WriteLine("[UI] Меню производства закрыто.");
        }
    }
}