using System.Collections.Generic;
using UnityEngine;

namespace StatSystem
{
    [CreateAssetMenu(fileName = "Stat Table", menuName = "StatSystem/StatTable")]
    public class StatTable : ScriptableObject
    {
        public Dictionary<StatDefinition, Stat> StatLookup = new Dictionary<StatDefinition, Stat>();

        public bool Get(StatDefinition statKey, out Stat stat)
        {
            return StatLookup.TryGetValue(statKey, out stat);
        }
    }
}