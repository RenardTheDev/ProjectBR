using UnityEngine;
using System;

namespace Exceptions
{
    static class GameObjectExtensions
    {
        public static void HandleComponent<T>(this GameObject gameObject, Action<T> handler)
        {
            var component = gameObject.GetComponent<T>();
            if (component != null) handler?.Invoke(component);
        }
    }
}
