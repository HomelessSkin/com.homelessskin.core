using UnityEngine;

namespace Core
{
    public class Parent : MonoBehaviour
    {
        public virtual void DestroyChildren()
        {
            var children = GetChildren();
            for (int c = 0; c < children.Length; c++)
                if (children[c] != transform)
                    Destroy(children[c].gameObject);
        }
        public virtual Transform[] GetChildren() => GetComponentsInChildren<Transform>(true);
    }
}