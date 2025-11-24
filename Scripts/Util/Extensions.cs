using UnityEngine;

namespace Core.Util
{
    public static class Extensions
    {
        public static void SavePrefString(this IPrefKey key, string value)
        {
            PlayerPrefs.SetString(key._Key, value);
            PlayerPrefs.Save();
        }
        public static string LoadPrefString(this IPrefKey key) => PlayerPrefs.GetString(key._Key);
    }
}