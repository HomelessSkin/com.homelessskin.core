using System.Threading.Tasks;

using TMPro;

using UnityEngine;
using UnityEngine.Networking;

namespace Core.Util
{
    public static class Web
    {
        public static async Task<Texture2D> DownloadSpriteTexture(string url)
        {
            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
            {
                var operation = webRequest.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                if (webRequest.result == UnityWebRequest.Result.Success)
                    return DownloadHandlerTexture.GetContent(webRequest);
                else
                {
                    Debug.Log($"Failed to download sprite: {webRequest.error}");

                    return null;
                }
            }
        }

        public static void CreateSpriteAsset(int id, Texture2D texture)
        {
            var asset = TMP_SpriteAsset.CreateInstance<TMP_SpriteAsset>();

            var s = new TMP_Sprite();
            s.id = id;
            s.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.zero);
            s.width = texture.width;
            s.height = texture.height;
            s.name = id.ToString();

            asset.spriteInfoList.Add(s);
        }
    }
}