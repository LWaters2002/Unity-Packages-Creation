using System;
using UnityEngine;

namespace StatSystem
{
    [CreateAssetMenu(fileName = "StatDefinition", menuName = "StatSystem/StatDefinition")]
    public class StatDefinition : ScriptableObject
    {
        public Sprite icon;
        public string displayName;
        [TextArea] public string description;
        public Guid identifier = Guid.NewGuid();
    }
}