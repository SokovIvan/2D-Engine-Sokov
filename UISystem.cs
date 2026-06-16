using System;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using _2D_Engine_Sokov.UIElements;

namespace _2D_Engine_Sokov
{
    public class UISystem
    {
        private static MouseState _previousMouseState;
        private static MouseState _currentMouseState;
        public delegate void UIElementClickHandler(UIElement element);
        public static event UIElementClickHandler OnElementClicked;

        private static bool _isRunning = false;
        private static Thread _UIThread;
        private static volatile bool _isPaused = false;

        public static void Pause() => _isPaused = true;
        public static void Resume() => _isPaused = false;

        public static void Initialize()
        {
            _previousMouseState = Mouse.GetState();
            _currentMouseState = _previousMouseState;
            _isRunning = true;
            _UIThread = new Thread(UIThreadLoop) { IsBackground = true, Name = "UIThread", Priority = ThreadPriority.AboveNormal };
            _UIThread.Start();
            UIActionsInitialise();
        }
        static void UIActionsInitialise()
        {
            UIActions.RegisterAction("StopEnemies", () => {
                var enemy = GameContext.FindGameObjectByTag("Enemy");
                if (enemy != null) enemy.IsActive = false;
            });
            UIActions.RegisterAction("GameQuit", () => Game.instance?.Stop());
            UIActions.RegisterAction("Continue", () => Game.instance?.LoadLevel("Content/Levels/Level0.xml"));
            UIActions.RegisterAction("StartGame", () => Game.instance?.LoadLevel("Content/Levels/Level0.xml"));

            UIActions.RegisterAction("StartWarDotsGame", () =>
            {
               WarDots.WarDotsGame.Instance?.LoadLevel("Content/Levels/WarDots/LevelIntermedia_1.xml");
                Console.WriteLine("WarDotsGameStart");
            });
            UIActions.RegisterAction("Battle", () =>
            {
                WarDots.WarDotsGame.Instance?.LoadLevel("Content/Levels/WarDots/Level_Test.xml");
                Console.WriteLine("WarDotsBattleStart");
            });
            UIActions.RegisterAction("ContinueWarDots", () =>
            {
                WarDots.WarDotsGame.Instance?.LoadLevel("Content/Levels/WarDots/LevelIntermedia_1.xml");
                Console.WriteLine("WarDotsGameStart");
            });
            UIActions.RegisterAction("WarDotsGameQuit", () => WarDots.WarDotsGame.Instance?.Stop());
            UIActions.RegisterAction("LoadLastSave", () =>
            {
                string lastSavePath = SaveSystem.GetLatestSavePath();
                if (!string.IsNullOrEmpty(lastSavePath))
                {
                    Console.WriteLine($"[MENU] Загрузка последнего сохранения: {lastSavePath}");
                    WarDots.WarDotsGame.Instance?.LoadLevel(lastSavePath);
                }
                else
                {
                    WarDots.WarDotsGame.Instance?.LoadLevel("Content/Levels/WarDots/LevelIntermedia_1.xml");
                }
            });
            UIActions.RegisterAction("OpenLoadMenu", () =>
            {
                // Ищем все элементы типа SaveLoadMenu
                var menus = GameContext.GetUIElements().OfType<SaveLoadMenu>().ToList();
                if (menus.Any())
                {
                    // Берем первый найденный (обычно он один)
                    menus.First().OpenMenu();
                    Console.WriteLine("[MENU] Открыто меню загрузки");
                }
                else
                {
                    Console.WriteLine("[ERROR] SaveLoadMenu не найден в сцене!");
                }
            });

            // Также полезно добавить действие для закрытия, если нужно
            UIActions.RegisterAction("CloseLoadMenu", () =>
            {
                var menus = GameContext.GetUIElements().OfType<SaveLoadMenu>().ToList();
                if (menus.Any())
                {
                    menus.First().CloseMenu();
                }
            });
        }
        private static void UIThreadLoop()
        {
            double lastUpdateTime = 0;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            double updateInterval = 1000.0 / 60;

            while (_isRunning)
            {
                if (_isPaused) { Thread.Sleep(10); continue; }
                double currentTime = timer.Elapsed.TotalMilliseconds;
                double deltaTime = currentTime - lastUpdateTime;

                if (deltaTime >= updateInterval)
                {
                    lastUpdateTime = currentTime;
                    Update(deltaTime / 1000.0);
                }
                else
                {
                    int sleepTime = (int)(updateInterval - deltaTime);
                    if (sleepTime > 0) Thread.Sleep(sleepTime);
                }
            }
        }

        public static void Update(double deltaTime)
        {
            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();

            if (_previousMouseState.LeftButton == ButtonState.Released &&
                _currentMouseState.LeftButton == ButtonState.Pressed)
                CheckClickEvents();

            try
            {
                var elements = GameContext.GetUIElements(); // 🔒 Thread-safe snapshot
                foreach (var element in elements) element.Update(deltaTime);
            }
            catch (Exception e) { Console.WriteLine("UIElement exception: " + e); }
        }

        public static void Shutdown() { _isRunning = false; _UIThread?.Join(); }

        private static void CheckClickEvents()
        {
            var camera = RenderSystem.GetCamera();
            if (camera == null) return;

            var mouseState = Mouse.GetState();
            var mousePosition = new Vector2(mouseState.X, mouseState.Y);

            // Сортировка и клик по верхнему элементу
            var elements = GameContext.GetUIElements()
                                      .Where(e => e.IsActive)
                                      .OrderByDescending(e => e.LayerDepth)
                                      .ToList();


            foreach (var element in elements)
            {
                if (IsPointInElement(mousePosition, element))
                {
                    OnElementClicked?.Invoke(element);
                    element.OnClick?.Invoke();
                    Console.WriteLine(element.Name);
                    break;
                }
            }
        }

        private static bool IsPointInElement(Vector2 point, UIElement element) =>
            element.IsActive && element.Texture != null && element.Bounds.Contains(point);

        public static Vector2 GetMousePosition()
        {
            var m = Mouse.GetState();
            return new Vector2(m.X, m.Y);
        }

        public static bool IsMouseOver(UIElement element)
        {
            if (element == null || !element.IsActive) return false;
            return IsPointInElement(GetMousePosition(), element);

        }
    }
}