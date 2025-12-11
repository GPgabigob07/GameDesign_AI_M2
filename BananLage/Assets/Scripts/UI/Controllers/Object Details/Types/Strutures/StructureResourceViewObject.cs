using System;
using Mechanics;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Controllers.Object_Details.Types.Strutures
{
    [Serializable, GeneratePropertyBag]
    public struct StructureResourceViewObject
    {
        public ResourceData data;
        public int amount;
        public float chance;

        [CreateProperty(ReadOnly = true)] public Sprite Icon => data.uiSprite;
        [CreateProperty(ReadOnly = true)] public string Described => chance > 0f  ? $"x{amount} ({chance * 100}%)" : $"x{amount}";
    }
}