using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StatSystem
{
    [CreateAssetMenu(fileName = "Stat Table", menuName = "StatSystem/StatTable")]
    public class StatTable : ScriptableObject
    {
        public List<StatKeyPair> StatLookup = new();

        public Stat Get(StatDefinition statKey)
        {
            return StatLookup.Find(x => x.statDefinition == statKey).stat;
        }
        
        public bool TryGet(StatDefinition statKey, out Stat stat)
        {
            stat = StatLookup.Find(x => x.statDefinition == statKey).stat;
            return stat != null;
        }

        public bool Contains(StatDefinition statKey)
        {
            return StatLookup.Any(x => x.statDefinition == statKey);
        }
    }

    [System.Serializable]
    public struct StatKeyPair
    {
        public StatDefinition statDefinition;
        public Stat stat;

        public StatKeyPair(StatDefinition statDefinition, Stat stat)
        {
            this.statDefinition = statDefinition;
            this.stat = stat;
        }
    }
}