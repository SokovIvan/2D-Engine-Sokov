using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _2D_Engine_Sokov
{
    public static class UIActions
    {
        private static readonly Dictionary<string, Action> _actions = new Dictionary<string, Action>();

        public static void RegisterAction(string name, Action action)
        {
            if (_actions.ContainsKey(name))
            {
                _actions[name] = action;
            }
            else
            {
                _actions.Add(name, action);
            }
        }

        public static Action GetAction(string name)
        {
            return _actions.TryGetValue(name, out var action) ? action : null;
        }
    }
}
