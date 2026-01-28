using System;
using System.Collections.Generic;

using TMPro;

using UnityEngine;

namespace Core
{
    [Serializable]
    public class StreamingSprites
    {
        public TMP_SpriteAsset Asset;

        public int SpriteDensity;
        public Texture2D Texture;
        public Texture2D[] DefaultSprites;

        bool[] TextureMap;
        List<int> ReservedKeys = new List<int>();
        Dictionary<int, (int, List<GameObject>)> HashSprite = new Dictionary<int, (int, List<GameObject>)>();

        public int WidthSprites => Texture.width / SpriteDensity;
        public int HeightSprites => Texture.height / SpriteDensity;

        public void Prepare()
        {
            TextureMap = new bool[SpriteDensity * SpriteDensity];

            for (int d = 0; d < DefaultSprites.Length; d++)
                Draw(DefaultSprites[d], d, d);
        }
        public void ReserveKey(int key) => ReservedKeys.Add(key);
        public void Draw(Texture2D smile, int hash, int id = -1)
        {
            if (IsKeyReserved(hash))
                UnreserveKey(hash);

            if (id < 0)
            {
                var got = false;
                for (int t = DefaultSprites.Length; t < TextureMap.Length; t++)
                    if (!TextureMap[t])
                    {
                        got = TextureMap[t] = true;
                        id = t;

                        break;
                    }

                if (!got)
                    id = UnityEngine.Random.Range(DefaultSprites.Length, SpriteDensity * SpriteDensity);
            }

            HashSprite[hash] = (id, new List<GameObject>());

            smile = DataUtil.ResizeBilinear(smile, WidthSprites, HeightSprites);
            var x = WidthSprites * (id % SpriteDensity);
            var y = Texture.height - HeightSprites * (id / SpriteDensity + 1);

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

            Texture.SetPixels32(x, y, WidthSprites, HeightSprites, pixels);
            Texture.Apply();
            Asset.UpdateLookupTables();
        }
        public void RemoveRange(List<int> toRemove, GameObject holder)
        {
            for (int s = 0; s < toRemove.Count; s++)
            {
                var key = toRemove[s];
                if (IsNonDefKey(key))
                {
                    var smile = HashSprite[key];
                    if (smile.Item2.Contains(holder))
                    {
                        smile.Item2.Remove(holder);

                        if (smile.Item2.Count == 0)
                        {
                            TextureMap[smile.Item1] = false;

                            HashSprite.Remove(key);
                        }
                        else
                            HashSprite[key] = smile;
                    }
                }
            }
        }
        public int GetSpriteID(int key, GameObject requester = null)
        {
            var smile = HashSprite[key];

            if (IsNonDefKey(key) &&
                 requester &&
                !smile.Item2.Contains(requester))
            {
                smile.Item2.Add(requester);

                HashSprite[key] = smile;
            }

            return smile.Item1;
        }
        public bool HasSprite(int hash) => HashSprite.ContainsKey(hash);
        public bool IsKeyReserved(int key) => ReservedKeys.Contains(key);

        bool IsNonDefKey(int key) => key < 0 || key >= DefaultSprites.Length;
        void UnreserveKey(int key) => ReservedKeys.Remove(key);
    }
}