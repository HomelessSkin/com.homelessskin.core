using UnityEngine;

namespace Core
{
    public class Parent : MonoBehaviour
    {
        public virtual Transform[] GetChildren() => GetComponentsInChildren<Transform>();
    }
}