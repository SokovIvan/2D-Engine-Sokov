using Microsoft.Xna.Framework.Input;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace _2D_Engine_Sokov
{
    public static class InputSystem
    {
        // Обычные поля. Они обновляются и читаются только внутри LogicThread
        private static MouseState _mouseState;
        private static KeyboardState _kbState;

        // Вызывается РОВНО ОДИН раз перед каждым тиком логики
        public static void Update()
        {
            _mouseState = Mouse.GetState();
            _kbState = Keyboard.GetState();
        }

        public static MouseState GetMouseState() => _mouseState;
        public static KeyboardState GetKeyboardState() => _kbState;
    }
}
