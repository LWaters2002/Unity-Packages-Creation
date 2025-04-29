using System;
using UnityEngine;

namespace StatSystem
{
    [CreateAssetMenu(fileName = "Stat Definition", menuName = "StatSystem/StatDefinition")]
    public class StatDefinition : ScriptableObject
    {
        public Sprite icon;
        public string displayName;
        [TextArea] public string description;
        public Guid identifier = Guid.NewGuid();
    }
}