using UnityEngine;

namespace Mechanics
{
    [CreateAssetMenu(fileName = "Mechanics", menuName = "Mechanics/ResourceData")]
    public class ResourceData: ScriptableObject
    {
        public bool basicResource;
        public string displayName;
        public Sprite uiSprite;
        public GameObject gamePrefab;
        public float productionTime;
    }
}