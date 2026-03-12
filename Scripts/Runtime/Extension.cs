using System.Collections.Generic;

using UnityEngine;

namespace Core
{
    public static class Extension
    {
        public static Color HexToRGB(this string hex, byte Alpha = 255)
        {
            if (string.IsNullOrEmpty(hex))
                return Color.white;

            hex = hex.Replace("#", "");

            if (int.TryParse(hex, out var decimalHexColor))
            {
                var R = (byte)((decimalHexColor >> 16) & byte.MaxValue);
                var G = (byte)((decimalHexColor >> 8) & byte.MaxValue);
                var B = (byte)(decimalHexColor & byte.MaxValue);

                return new Color(R, G, B, Alpha);
            }

            return Color.white;
        }

        public static bool IsEmpty<T>(this List<T> list) => list.Count == 0;

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