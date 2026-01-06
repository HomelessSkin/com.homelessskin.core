using UnityEngine;

namespace Core
{
    [RequireComponent(typeof(PrefabBaker))]
    public abstract class Personality : MonoBehaviour
    {
        public abstract int GetID();
    }
}