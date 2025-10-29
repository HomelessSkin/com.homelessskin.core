using System.Collections.Generic;
using System.Threading.Tasks;

using TMPro;

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TextCore;

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

        public static void CreateSpriteAsset(string name, Texture2D texture)
        {
            var asset = TMP_SpriteAsset.CreateInstance<TMP_SpriteAsset>();
            asset.name = name;
            asset.spriteInfoList = new List<TMP_Sprite>();

            var s = new TMP_Sprite();
            s.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), Vector2.zero);
            s.width = texture.width;
            s.height = texture.height;
            s.name = name;

            asset.spriteInfoList.Add(s);
        }

        public static TMP_SpriteAsset CreateSpriteAssetFromTexture(
            Texture2D texture,
            string spriteName = "NewSpriteAsset",
            int spriteWidth = 64,
            int spriteHeight = 64,
            int padding = 0,
            float pixelsPerUnit = 100f)
        {
            if (texture == null)
            {
                Debug.LogError("Texture cannot be null!");
                return null;
            }

            var spriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
            spriteAsset.name = spriteName;
            spriteAsset.hashCode = TMP_TextUtilities.GetSimpleHashCode(spriteName);

            spriteAsset.spriteSheet = texture;
            spriteAsset.material = CreateSpriteMaterial(texture, spriteName);

            GenerateSpritesForAsset(spriteAsset, texture, spriteWidth, spriteHeight, padding, pixelsPerUnit);

            spriteAsset.UpdateLookupTables();

            return spriteAsset;
        }

        static Material CreateSpriteMaterial(Texture2D texture, string name)
        {
            var shader = Shader.Find("TextMeshPro/Sprite");
            if (!shader)
                return null;

            var material = new Material(shader);
            material.name = name + " Material";
            material.mainTexture = texture;
            material.SetTexture(ShaderUtilities.ID_MainTex, texture);

            return material;
        }

        static void GenerateSpritesForAsset(
            TMP_SpriteAsset spriteAsset,
            Texture2D texture,
            int spriteWidth,
            int spriteHeight,
            int padding,
            float pixelsPerUnit)
        {
            var sprites = new List<TMP_Sprite>();
            var spriteCharacters = new List<TMP_SpriteCharacter>();
            var spriteGlyphs = new List<TMP_SpriteGlyph>();

            var columns = texture.width / (spriteWidth + padding);
            var rows = texture.height / (spriteHeight + padding);
            var spriteIndex = 0;
            var unicode = 0xE000u;

            for (int y = rows - 1; y >= 0; y--)
                for (int x = 0; x < columns; x++)
                {
                    var xPos = x * (spriteWidth + padding);
                    var yPos = y * (spriteHeight + padding);

                    var spriteGlyph = new TMP_SpriteGlyph();
                    spriteGlyph.index = (uint)spriteIndex;
                    spriteGlyph.metrics = new GlyphMetrics(spriteWidth, spriteHeight, 0, spriteHeight, spriteWidth);
                    spriteGlyph.glyphRect = new GlyphRect((int)xPos, (int)yPos, spriteWidth, spriteHeight);
                    spriteGlyph.scale = 1.0f;
                    spriteGlyph.sprite = null;

                    spriteGlyphs.Add(spriteGlyph);

                    var spriteCharacter = new TMP_SpriteCharacter();
                    spriteCharacter.unicode = unicode++;
                    spriteCharacter.glyph = spriteGlyph;
                    spriteCharacter.glyphIndex = spriteGlyph.index;
                    spriteCharacter.scale = 1.0f;
                    spriteCharacter.name = $"Sprite_{spriteIndex}";

                    spriteCharacters.Add(spriteCharacter);

                    spriteIndex++;

                    if (unicode > 0xE0FF)
                    {
                        Debug.LogWarning("Reached maximum sprite count for Private Use Area");

                        break;
                    }
                }

            spriteAsset.spriteCharacterTable.AddRange(spriteCharacters);
            spriteAsset.spriteGlyphTable.AddRange(spriteGlyphs);

            spriteAsset.spriteInfoList = new List<TMP_Sprite>();
            for (int i = 0; i < spriteCharacters.Count; i++)
            {
                var sprite = new TMP_Sprite();
                sprite.id = i;
                sprite.name = spriteCharacters[i].name;
                sprite.unicode = (int)spriteCharacters[i].unicode;
                sprite.x = spriteGlyphs[i].glyphRect.x;
                sprite.y = spriteGlyphs[i].glyphRect.y;
                sprite.width = spriteGlyphs[i].glyphRect.width;
                sprite.height = spriteGlyphs[i].glyphRect.height;
                sprite.xOffset = 0;
                sprite.yOffset = spriteHeight;
                sprite.xAdvance = spriteWidth;
                sprite.scale = 1.0f;
                sprites.Add(sprite);
            }

            spriteAsset.spriteInfoList = sprites;
        }

        public static TMP_SpriteAsset CreateAndSaveSpriteAsset(
            Texture2D texture,
            string savePath,
            string assetName = "NewSpriteAsset",
            int spriteWidth = 64,
            int spriteHeight = 64,
            int padding = 0)
        {
            return CreateSpriteAssetFromTexture(texture, assetName, spriteWidth, spriteHeight, padding);
        }
    }
}