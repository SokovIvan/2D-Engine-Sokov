using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using System.Xml.Linq;

namespace _2D_Engine_Sokov
{
    public  class UISystem
    {
        private static readonly List<UIElement> _uiElements = new List<UIElement>();
        private static MouseState _previousMouseState;
        private static MouseState _currentMouseState;

        // Событие для кликов по UI элементам
        public delegate void UIElementClickHandler(UIElement element);
        public static event UIElementClickHandler OnElementClicked;

        public static bool _isRunning = false;
        private static Thread _UIThread;
        // Для синхронизации обновлений
        private static readonly object _updateLock = new object();
        private static int _targetUpdateRate = 60;
        private static double _updateInterval = 1000.0 / _targetUpdateRate;
        public static void Initialize()
        {
            _previousMouseState = Mouse.GetState();
            _currentMouseState = _previousMouseState;
            _isRunning = true;
            _UIThread = new Thread(UIThreadLoop)
            {
                IsBackground = true,
                Name = "UIThread",
                Priority = ThreadPriority.AboveNormal
            };

            _UIThread.Start();
        }
        public static void ClearAllUIElements()
        {
            _uiElements.Clear();
        }
        private static void UIThreadLoop()
        {
            double lastUpdateTime = 0;
            var timer = System.Diagnostics.Stopwatch.StartNew();

            while (_isRunning)
            {
                double currentTime = timer.Elapsed.TotalMilliseconds;
                double deltaTime = currentTime - lastUpdateTime;

                if (deltaTime >= _updateInterval)
                {
                    lastUpdateTime = currentTime;
                    Update(deltaTime / 1000.0);
                }
                else
                {
                    int sleepTime = (int)(_updateInterval - deltaTime);
                    if (sleepTime > 0)
                        Thread.Sleep(sleepTime);
                }
            }
        }
        public static void Update(double deltaTime)
        {
            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();
            if (_previousMouseState.LeftButton == ButtonState.Released &&
                _currentMouseState.LeftButton == ButtonState.Pressed)
            {
                CheckClickEvents();
            }
            try { 
            foreach (UIElement element in _uiElements) {
                element.Update(deltaTime);
            }
            }
            catch { }
        }

        public static void RegisterUIElement(UIElement element)
        {
            if (element != null && !_uiElements.Contains(element))
            {
                _uiElements.Add(element);
            }
        }

        public static void UnregisterUIElement(UIElement element)
        {
            if (element != null)
            {
                _uiElements.Remove(element);
            }
        }
        public static void Shutdown()
        {
            _isRunning = false;
            _UIThread?.Join();
        }
        private static void CheckClickEvents()
        {
            var camera = RenderSystem.GetCamera();
            if (camera == null)
            {
                return;
            }
            var mouseState = Mouse.GetState();
            var mousePosition =new Vector2(mouseState.X, mouseState.Y);
            foreach (var element in _uiElements.OrderByDescending(e => e.LayerDepth))
            {
                if (element.IsActive && IsPointInElement(mousePosition, element))
                {
                    OnElementClicked?.Invoke(element);
                    element.OnClick?.Invoke();
                    break; // Кликаем только верхний элемент
                }
            }
        }

        private static bool IsPointInElement(Vector2 point, UIElement element)
        {
            return element.IsActive && element.Texture != null && element.Bounds.Contains(point);
        }

        public static Vector2 GetMousePosition()
        {
            var mouseState = Mouse.GetState();
            return new Vector2(mouseState.X, mouseState.Y);
        }

        public static bool IsMouseOver(UIElement element)
        {
            if (element == null || !element.IsActive) return false;
            return IsPointInElement(GetMousePosition(), element);
        }
    }
}
