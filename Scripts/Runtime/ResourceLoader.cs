using System.Collections.Generic;

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

        protected virtual void Load<T>(ref T[] value) where T : Object
        {
            if (ResourcesPaths == null)
                return;

            var assets = new List<T>();
            for (int v = 0; v < value.Length; v++)
                if (value[v])
                    assets.Add(value[v]);

            for (int p = 0; p < ResourcesPaths.Length; p++)
                if (!string.IsNullOrEmpty(ResourcesPaths[p]))
                {
                    var loaded = Resources.LoadAll<T>(ResourcesPaths[p]);
                    for (int l = 0; l < loaded.Length; l++)
                        if (!assets.Contains(loaded[l]))
                            assets.Add(loaded[l]);
                }

            value = assets.ToArray();
        }

        protected abstract void LoadResources();
#endif
    }
}