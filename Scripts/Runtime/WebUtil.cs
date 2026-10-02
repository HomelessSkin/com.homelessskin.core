using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Networking;

namespace Core
{
    public static class Web
    {
        public static async Task<Texture2D> DownloadSpriteTexture(string url)
        {
            using (var request = UnityWebRequestTexture.GetTexture(url))
            {
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                    return DownloadHandlerTexture.GetContent(request);
                else
                {
                    Log.Error(request, request.error);

                    return null;
                }
            }
        }
    }
}