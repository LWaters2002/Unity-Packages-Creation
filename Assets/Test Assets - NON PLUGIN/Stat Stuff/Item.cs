using System.Collections.Generic;
using StatSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class Item : ScriptableObject
{
    public StatDefinition statDefinition;
    public List<StatModifier> modifiers;
}