using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Networking;

namespace Core.Util
{
    namespace Core.Util
    {
        public static class Web
        {
            public static async Task<Texture2D> DownloadSpriteTexture(string url)
            {
                using (var webRequest = UnityWebRequestTexture.GetTexture(url))
                {
                    await webRequest.SendWebRequest();

                    if (webRequest.result == UnityWebRequest.Result.Success)
                        return DownloadHandlerTexture.GetContent(webRequest);
                    else
                        return null;
                }
            }
        }
    }
}