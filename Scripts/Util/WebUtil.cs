using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Networking;

namespace Core.Util
{
    public static class Web
    {
        public static async Task<Texture2D> DownloadSpriteTexture(string url)
        {
            using (var webRequest = UnityWebRequestTexture.GetTexture(url))
            {
                var operation = webRequest.SendWebRequest();
                while (!operation.isDone)
                    await Task.Yield();

                if (webRequest.result == UnityWebRequest.Result.Success)
                    return DownloadHandlerTexture.GetContent(webRequest);
                else
                    return null;
            }
        }
    }
}