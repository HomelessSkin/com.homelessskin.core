using System.Threading.Tasks;

using Unity.Collections;

using UnityEngine;
using UnityEngine.Networking;

namespace Core
{
    public static class Web
    {
        public static async Task<Frame> DownloadSpriteData(string url, Allocator allocator = Allocator.Temp)
        {
            var texture = await DownloadSpriteTexture(url);
            if (!texture)
                return null;

            var frame = new Frame(texture.width, texture.height, allocator);

            var pixels = texture.GetPixels32();
            for (int p = 0; p < pixels.Length; p++)
                frame.RawImage[p] = pixels[p];

            return frame;
        }
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