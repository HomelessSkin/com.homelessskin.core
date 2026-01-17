using UnityEngine;

namespace Core
{
    public abstract class ResourceLoader : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] bool AutoLoad;
        [SerializeField] protected string[] ResourcesPaths;

        protected virtual void OnValidate()
        {
            if (AutoLoad)
            {
                AutoLoad = false;

                LoadResources();
            }
        }

        protected abstract void LoadResources();
#endif
    }
}