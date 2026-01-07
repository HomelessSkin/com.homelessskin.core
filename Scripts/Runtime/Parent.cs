using UnityEngine;

namespace Core
{
    public abstract class Parent : MonoBehaviour
    {
        public virtual Transform[] GetChildren() => GetComponentsInChildren<Transform>();
    }
}