#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Text;

using Core;

using UnityEditor;

using UnityEngine;

namespace Game.Util
{
    public class AutoEnumGenerator : MonoBehaviour
    {
        [SerializeField] string OUTPUT_PATH = "Assets/Resources/Util/Scripts/Generated/Enum.cs";
        [SerializeField] EnumData[] Data;

        public static int Enhance(string path, string type, string name, int value)
        {
            var builder = new StringBuilder();
            var data = File.ReadAllLines(path).ToList();

            var typeLine = -1;
            var nameLine = -1;
            for (int l = 0; l < data.Count; l++)
                if (typeLine < 0 && data[l].Contains(type))
                    typeLine = l;
                else if (nameLine < 0 && data[l].Contains(name))
                    nameLine = l;

            if (nameLine >= 0)
            {
                var line = data[nameLine];
                var eq = -1;
                for (int i = 0; i < line.Length; i++)
                    if (line[i] == '=')
                    {
                        eq = i;

                        break;
                    }

                var rep = line.Substring(eq + 2, line.Length - eq - 2).Replace(",", "");

                value = int.Parse(rep);
            }
            else if (typeLine >= 0)
                data.Insert(typeLine + 2, $"        {name} = {value},");

            for (int l = 0; l < data.Count; l++)
                builder.AppendLine(data[l]);

            File.WriteAllText(path, builder.ToString());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return value;
        }

        public void Generate()
        {
            var builder = new StringBuilder();
            builder.AppendLine("namespace Game.Util\n{");
            for (int d = 0; d < Data.Length; d++)
            {
                var data = Data[d];
                var objects = Resources.LoadAll<KeyScriptable>(data.Folder);
                AddEnum(data, objects, ref builder);
                builder.AppendLine("\n");
            }
            builder.AppendLine("\n}");

            var directory = Path.GetDirectoryName(OUTPUT_PATH);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var currentContent = File.Exists(OUTPUT_PATH) ? File.ReadAllText(OUTPUT_PATH) : "";
            if (currentContent != builder.ToString())
            {
                File.WriteAllText(OUTPUT_PATH, builder.ToString());
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"Auto-generated enum at {OUTPUT_PATH}");
            }
        }

        static void AddEnum(EnumData data, KeyScriptable[] objects, ref StringBuilder builder)
        {
            builder.AppendLine($"    public enum {data.Name} : int\n" + "    {");
            for (int p = 0; p < objects.Length; p++)
            {
                var obj = objects[p];
                var name = obj.name.Replace($"{data.Removable}", "");
                var value = 0;

                switch (data.Baking)
                {
                    case EnumData.Type.ByID:
                    value = obj.GetID();
                    break;
                    case EnumData.Type.ByIndex:
                    value = p;
                    break;
                }

                builder.AppendLine($"        {name} = {value},");
            }
            builder.AppendLine("\n    }");
        }

        [System.Serializable]
        public struct EnumData
        {
            public Type Baking;

            public string Folder;
            public string Name;
            public string Removable;

            public enum Type : byte
            {
                ByID = 0,
                ByIndex = 1,

            }
        }
    }
}
#endif