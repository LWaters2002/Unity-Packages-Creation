using System;
using System.Collections.Generic;

namespace EventBus
{
    public interface IEvent { }

    internal interface IEventBinding<T>
    {
        public Action<T> OnEvent { get; set; }
        public Action OnEventNoArgs { get; set; }
    }

    public class EventBinding<T> : IEventBinding<T> where T : IEvent
    {
        public Action<T> OnEvent { get; set; }
        public Action OnEventNoArgs { get; set; }

        public EventBinding(Action<T> onEvent) => this.OnEvent = onEvent;
        public EventBinding(Action onEventNoArgs) => this.OnEventNoArgs = onEventNoArgs;
    }

    public static class EventBus<T> where T : IEvent
    {
        private static readonly HashSet<IEventBinding<T>> Bindings = new();
        
        public static void RegisterEvent(EventBinding<T> binding) => Bindings.Add(binding);
        public static void UnregisterEvent(EventBinding<T> binding) => Bindings.Remove(binding);

        public static void Execute(T @event)
        {
            foreach (IEventBinding<T> binding in Bindings)
            {
                binding.OnEvent(@event);
                binding.OnEventNoArgs();
            }
        }
    }
}