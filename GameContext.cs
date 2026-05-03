using System;
using System.Collections.Generic;
using System.Linq;
using _2D_Engine_Sokov.GameObjects;
using _2D_Engine_Sokov.UIElements;

namespace _2D_Engine_Sokov
{
    public static class GameContext
    {
        public static GameLevel CurrentLevel { get; private set; }
        public static TileMap TileMap => CurrentLevel?.TileMap;

        // 🔒 Централизованные коллекции с защитой от многопоточности
        private static readonly List<GameObject> _gameObjects = new();
        private static readonly List<UIElement> _uiElements = new();
        private static readonly object _gameObjLock = new();
        private static readonly object _uiLock = new();

        public static void SetLevel(GameLevel level)
        {
            CurrentLevel = level;
            Console.WriteLine($"[GameContext] Уровень привязан: {level?.Name}");
        }

        // 🛡️ GameObjects
        public static void AddGameObject(GameObject obj)
        {
            if (obj == null) return;
            lock (_gameObjLock) if (!_gameObjects.Contains(obj)) _gameObjects.Add(obj);
        }

        public static void RemoveGameObject(GameObject obj)
        {
            if (obj == null) return;
            lock (_gameObjLock) _gameObjects.Remove(obj);
        }

        public static void ClearGameObjects()
        {
            lock (_gameObjLock) _gameObjects.Clear();
        }

        public static List<GameObject> GetGameObjects()
        {
            lock (_gameObjLock) return _gameObjects.ToList(); // Безопасный снимок
        }

        public static GameObject[] FindGameObjectsByTag(string tag) =>
            GetGameObjects().Where(g => g.IsActive && g.Tag == tag).ToArray();

        public static GameObject FindGameObjectByTag(string tag) =>
            GetGameObjects().FirstOrDefault(g => g.Tag == tag);

        public static GameObject[] FindGameObjectsByType(Type type) =>
            GetGameObjects().Where(g => g.IsActive && (g.GetType() == type || g.GetType().IsSubclassOf(type))).ToArray();

        public static GameObject[] FindGameObjectsByName(string name) =>
            GetGameObjects().Where(g => g.IsActive && g.Name == name).ToArray();

        public static GameObject FindGameObjectByName(string name) =>
            GetGameObjects().FirstOrDefault(g => g.Name == name);

        // 🖥️ UIElements
        public static void AddUIElement(UIElement ui)
        {
            if (ui == null) return;
            lock (_uiLock) if (!_uiElements.Contains(ui)) _uiElements.Add(ui);
        }

        public static void RemoveUIElement(UIElement ui)
        {
            if (ui == null) return;
            lock (_uiLock) _uiElements.Remove(ui);
        }

        public static void ClearUIElements()
        {
            lock (_uiLock) _uiElements.Clear();
        }

        public static List<UIElement> GetUIElements()
        {
            lock (_uiLock) return _uiElements.ToList(); // Безопасный снимок
        }

        public static UIElement[] FindUIElementsByType(Type type) =>
            GetUIElements().Where(u => u.IsActive && (u.GetType() == type || u.GetType().IsSubclassOf(type))).ToArray();

        public static UIElement[] FindUIElementsByName(string name) =>
            GetUIElements().Where(u => u.IsActive && u.Name == name).ToArray();
    }
}