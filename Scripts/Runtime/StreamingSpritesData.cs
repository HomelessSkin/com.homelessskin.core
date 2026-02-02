using System.Collections.Generic;

using TMPro;

using UnityEngine;

namespace Core
{
    [CreateAssetMenu(fileName = "StreamingSpritesData", menuName = "Core/Streaming Sprites Data")]
    public class StreamingSpritesData : ScriptableObject
    {
        public TMP_SpriteAsset Asset;

        public int SpriteDensity;
        public Texture2D Texture;
        public Texture2D[] DefaultSprites;

        public int WidthSprites => Texture.width / SpriteDensity;
        public int HeightSprites => Texture.height / SpriteDensity;
    }

    public static class StreamingSprites
    {
        static StreamingSpritesData Data;

        static float[] TextureMap;
        static Dictionary<int, (int, int)> HashIndex = new Dictionary<int, (int, int)>();

        public static void Prepare(StreamingSpritesData data)
        {
            Data = data;
            TextureMap = new float[Data.SpriteDensity * Data.SpriteDensity];

            for (int d = 0; d < Data.DefaultSprites.Length; d++)
                Draw(Data.DefaultSprites[d], d);
        }
        public static void RemoveRange(List<int> toRemove)
        {
            for (int s = 0; s < toRemove.Count; s++)
            {
                var key = toRemove[s];
                var smile = HashIndex[key];
                smile.Item2--;

                if (smile.Item2 == 0)
                {
                    TextureMap[smile.Item1] = 0f;

                    HashIndex.Remove(key);
                }
                else
                    HashIndex[key] = smile;
            }
        }
        public static int GetSpriteIndex(int hash, string uri)
        {
            if (HashIndex.TryGetValue(hash, out var i))
            {
                i.Item2++;
                HashIndex[hash] = i;

                return i.Item1;
            }

            var n = GetFreeOrEarlier();
            HashIndex[hash] = (n, 1);

            Draw(uri, n);

            return n;
        }

        static async void Draw(string uri, int id)
        {
            var smile = await Web.DownloadSpriteTexture(uri);
            if (!smile)
                return;

            Draw(smile, id);
        }
        static void Draw(Texture2D smile, int id)
        {
            smile = DataUtil.ResizeBilinear(smile, Data.WidthSprites, Data.HeightSprites);
            var x = Data.WidthSprites * (id % Data.SpriteDensity);
            var y = Data.Texture.height - Data.HeightSprites * (id / Data.SpriteDensity + 1);

            var pixels = smile.GetPixels32();
            for (int p = 0; p < pixels.Length; p++)
            {
                var pixel = pixels[p];
                if (pixel.r < 10 &&
                     pixel.g < 10 &&
                     pixel.b < 10)
                {
                    pixel.r = pixel.g = pixel.b = 10;
                    pixels[p] = pixel;
                }
            }

            Data.Texture.SetPixels32(x, y, Data.WidthSprites, Data.HeightSprites, pixels);
            Data.Texture.Apply();

            Data.Asset.UpdateLookupTables();
        }
        static int GetFreeOrEarlier()
        {
            var earlier = -1;
            var time = float.MaxValue;
            for (int t = Data.DefaultSprites.Length; t < TextureMap.Length; t++)
            {
                var d = TextureMap[t];
                if (d == 0f)
                {
                    TextureMap[t] = Time.realtimeSinceStartup;

                    return t;
                }

                if (d < time)
                {
                    time = d;
                    earlier = t;
                }
            }

            return earlier;
        }
    }
}