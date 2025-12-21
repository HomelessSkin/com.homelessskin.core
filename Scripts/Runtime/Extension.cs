using UnityEngine;

namespace Core
{
    public static class Extension
    {
        public static int GetPrefabID(this MonoBehaviour obj)
        {
            var ids = obj.GetComponents<IPrefabID>();
            var l = 0L;
            for (int i = 0; i < ids.Length; i++)
                l += ids[i].GetID();

            return (int)(l & 0xFFFFFFFFL);
        }
        public static int GetPrefabID(this GameObject obj)
        {
            var ids = obj.GetComponents<IPrefabID>();
            var l = 0L;
            for (int i = 0; i < ids.Length; i++)
                l += ids[i].GetID();

            return (int)(l & 0xFFFFFFFFL);
        }
        public static void SavePrefString(this IPrefKey key, string value)
        {
            PlayerPrefs.SetString(key._Key, value);
            PlayerPrefs.Save();
        }
        public static string LoadPrefString(this IPrefKey key) => PlayerPrefs.GetString(key._Key);
    }
}