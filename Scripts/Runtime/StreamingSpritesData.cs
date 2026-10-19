using System.Collections.Generic;
using System.Threading.Tasks;

using TMPro;

using Unity.Collections;

using UnityEngine;

namespace Core
{
    #region STREAMING SPRITES DATA
    [CreateAssetMenu(fileName = "Streaming Sprites Data", menuName = "Core/Streaming Sprites Data")]
    public class StreamingSpritesData : ScriptableObject
    {
        public TMP_SpriteAsset Asset;

        public int SpriteDensity;
        public Texture2D Texture;
        public Material Smiles;

        [Space]
        public Texture2D[] DefaultSprites;

        public int WidthSprites => Texture.width / SpriteDensity;
        public int HeightSprites => Texture.height / SpriteDensity;
    }
    #endregion

    #region STREAMING SPRITES
    public static class StreamingSprites
    {
        public static string Asset => Data.Asset.name;
        static StreamingSpritesData Data;

        static float[] TextureMap;

        static Dictionary<int, (int, int)> HashIndex;
        static Dictionary<int, Animation> Animations;

        public static void Prepare(StreamingSpritesData data)
        {
            Data = data;

            TextureMap = new float[Data.SpriteDensity * Data.SpriteDensity];
            HashIndex = new Dictionary<int, (int, int)>();
            Animations = new Dictionary<int, Animation>();

            for (int d = 0; d < Data.DefaultSprites.Length; d++)
            {
                var frame = new Frame(Data.DefaultSprites[d]);
                DataUtil.ResizeBilinear(ref frame, Data.WidthSprites, Data.HeightSprites);

                Draw(frame, d);
            }

            SaveActions();
        }
        public static void UpdateAnimations(float delta)
        {
            foreach (var (id, animation) in Animations)
            {
                var frame = animation.Step(delta);
                if (frame != null)
                    Draw(frame, id);
            }

            SaveActions();
        }
        public static int GetAnimationIndex(int hash, string url)
        {
            if (HashIndex.TryGetValue(hash, out var i))
            {
                i.Item2++;
                HashIndex[hash] = i;

                return i.Item1;
            }

            var n = GetFreeOrEarlier();
            HashIndex[hash] = (n, 1);

            LoadAnimation(url, n);

            return n;
        }
        public static int GetSpriteIndex(int hash, string url)
        {
            if (HashIndex.TryGetValue(hash, out var i))
            {
                i.Item2++;
                HashIndex[hash] = i;

                return i.Item1;
            }

            var n = GetFreeOrEarlier();
            HashIndex[hash] = (n, 1);

            LoadSprite(url, n);

            return n;
        }

        //public static void RemoveRange(List<int> toRemove)
        //{
        //    for (int s = 0; s < toRemove.Count; s++)
        //    {
        //        var key = toRemove[s];
        //        if (!HashIndex.TryGetValue(key, out var smile))
        //            continue;

        //        smile.Item2--;
        //        if (smile.Item2 == 0)
        //        {
        //            TextureMap[smile.Item1] = 0f;

        //            HashIndex.Remove(key);
        //        }
        //        else
        //            HashIndex[key] = smile;
        //    }
        //}

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

                    Data.Smiles.SetInt("_Max_Smiles", t);

                    return t;
                }

                if (d < time)
                {
                    time = d;
                    earlier = t;
                }
            }

            if (Animations.ContainsKey(earlier))
                Animations.Remove(earlier);

            Data.Smiles.SetInt("_Max_Smiles", earlier);

            return earlier;
        }
        static void Draw(Frame frame, int id)
        {
            var x = Data.WidthSprites * (id % Data.SpriteDensity);
            var y = Data.Texture.height - Data.HeightSprites * (id / Data.SpriteDensity + 1);

            Data.Texture.SetPixels32(x, y, Data.WidthSprites, Data.HeightSprites, frame.RawImage.ToArray());
        }
        static void SaveActions()
        {
            Data.Texture.Apply();
            Data.Asset.UpdateLookupTables();
        }

        static async void LoadSprite(string url, int id)
        {
            var c = 0;
            while (c < 10000)
            {
                var frame = await Web.DownloadSpriteData(url);
                if (frame != null)
                {
                    DataUtil.ResizeBilinear(ref frame, Data.WidthSprites, Data.HeightSprites);

                    Draw(frame, id);

                    SaveActions();

                    break;
                }

                c++;

                await Task.Delay(300);
            }
        }
        static async void LoadAnimation(string url, int id)
        {
            var c = 0;
            while (c < 10000)
            {
                var list = await GIF.LoadAsListAsync(url);
                if (list != null)
                {
                    Animations[id] = new Animation(list);

                    break;
                }

                c++;

                await Task.Delay(300);
            }
        }

        #region ANIMATION
        class Animation
        {
            int Current = 0;
            float FrameT = 0f;

            List<Frame> Frames;

            public Animation(List<Frame> frames)
            {
                Frames = new List<Frame>();
                for (int f = 0; f < frames.Count; f++)
                {
                    var frame = frames[f];

                    DataUtil.ResizeBilinear(ref frame, Data.WidthSprites, Data.HeightSprites, Allocator.Persistent);

                    Frames.Add(frame);
                }
            }

            ~Animation()
            {
                DisposeFrames();
            }

            public Frame Step(float delta)
            {
                if (Frames != null && Frames.Count > 0)
                {
                    FrameT -= delta;
                    if (FrameT <= 0f)
                    {
                        Current = (Current + 1) % Frames.Count;
                        FrameT += Frames[Current].Delay * 0.001f;

                        return Frames[Current];
                    }
                }

                return null;
            }

            void DisposeFrames()
            {
                for (int f = 0; f < Frames.Count; f++)
                    if (Frames[f].RawImage.IsCreated)
                        Frames[f].RawImage.Dispose();

                Current = 0;
                FrameT = 0f;

                Frames.Clear();
            }
        }
        #endregion
    }
    #endregion
}