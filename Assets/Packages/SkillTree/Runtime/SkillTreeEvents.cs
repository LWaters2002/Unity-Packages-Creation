using System;
using System.Collections.Generic;

namespace SkillTree.Runtime
{
    public interface ISkillTreeEvent
    {
    }

    public static class SkillTreeEventBus<TEventType> where TEventType : ISkillTreeEvent
    {
        public static List<System.Action<TEventType>> Bindings = new();

        public static void RegisterCallback(Action<TEventType> callback)
        {
            Bindings.Add(callback);
        }

        public static void UnregisterCallback(Action<TEventType> callback)
        {
            Bindings.Remove(callback);
        }

        public static void ClearAllBindings()
        {
            Bindings.Clear();
        }

        public static void Execute(TEventType @event)
        {
            foreach (var binding in Bindings)
            {
                binding.Invoke(@event);
            }
        }
    }
}