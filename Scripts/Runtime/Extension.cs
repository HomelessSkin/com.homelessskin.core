using UnityEngine;

namespace Core
{
    public static class Extension
    {
        public static void SavePrefInt(this IPrefKey key, int value)
        {
            PlayerPrefs.SetInt(key._Key, value);
            PlayerPrefs.Save();
        }
        public static int LoadPrefInt(this IPrefKey key) => PlayerPrefs.GetInt(key._Key);

        public static void SavePrefString(this IPrefKey key, string value)
        {
            PlayerPrefs.SetString(key._Key, value);
            PlayerPrefs.Save();
        }
        public static string LoadPrefString(this IPrefKey key) => PlayerPrefs.GetString(key._Key);

        public static int GetPrefabID(this GameObject obj)
        {
            var ids = obj.GetComponents<Personality>();
            var l = 0L;
            for (int i = 0; i < ids.Length; i++)
                l += ids[i].GetID();

            return (int)(l & 0xFFFFFFFFL);
        }
        public static uint ToUint(this string value) => uint.Parse(Mathf.Abs(value.GetHashCode()).ToString());
    }
}