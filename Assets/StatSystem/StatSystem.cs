using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace StatSystem
{
    [Serializable]
    public class Stat
    {
        public float Base { get; }
        public float Value { get; private set; }

        private List<StatModifier> Modifiers = new();

        public Stat(float baseValue)
        {
            Base = baseValue;
            Value = baseValue;
        }

        public Stat(float baseValue, List<StatModifier> modifiers)
        {
            Base = baseValue;
            
            foreach (var modifier in modifiers)
                AddModifier(modifier, false);
            UpdateValue();
        }

        public void UpdateValue()
        {
            float temp = Base;
            float multiplier = 1;
            
            foreach (StatModifier modifier in Modifiers)
            {
                if (modifier.addOrMultiply)
                {
                    temp += modifier.value;
                }
                else
                {
                    multiplier += modifier.value;
                }
            }
            
            temp *= multiplier;
            Value = temp;
        }

        public void AddModifier(StatModifier modifier, bool dirty = true)
        {
            Modifiers.Add(modifier);
            
            if (dirty)
                UpdateValue();
        }

        public void RemoveAllModifierByObject(Object obj)
        {
            for (int i = Modifiers.Count - 1; i >= 0; i--)
            {
                StatModifier modifier = Modifiers[i];
                if (modifier.owner == obj)
                    Modifiers.RemoveAt(i);
            }
            
            UpdateValue();
        }

        public void RemoveModifierByGuid(Guid guid)
        {
            for (int i = Modifiers.Count - 1; i >= 0; i--)
            {
                StatModifier modifier = Modifiers[i];
                if (modifier.guid == guid)
                    Modifiers.RemoveAt(i);
            }
            
            UpdateValue();
        }
    }

    [Serializable]
    public class StatModifier
    {
        public Object owner;
        public Guid guid;
        public bool addOrMultiply;
        public float value;

        public StatModifier(bool addOrMultiply, float value, Object owner = null)
        {
            this.addOrMultiply = addOrMultiply;
            this.value = value;
            this.owner = owner;      
            guid = Guid.NewGuid();
        }
    }
}