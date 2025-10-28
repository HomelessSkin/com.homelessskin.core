using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Networking;

namespace Core.Util
{
    public static class Web
    {
        public static async Task<Texture2D> DownloadSpriteTexture(string spriteName, string url, bool cacheLocally)
        {
            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
            {
                var operation = webRequest.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Failed to download sprite: {webRequest.error}");

                    return null;
                }

                return DownloadHandlerTexture.GetContent(webRequest);
            }
        }
    }
}