using UnityEngine;

namespace Core
{
    public abstract class ResourceBaker : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] bool AutoLoad;
        [SerializeField] protected string ResourcesPath;

        protected virtual void OnValidate()
        {
            if (AutoLoad &&
                !string.IsNullOrEmpty(ResourcesPath))
            {
                AutoLoad = false;

                LoadResources();
            }
        }

        protected abstract void LoadResources();
#endif
    }
}