using System.Collections.Generic;

using Core.Util;

using Unity.Collections;

using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace Core.Rendering
{
    public class ObjectRenderer : MonoBehaviour
    {
        [SerializeField] PropertiesList[] DefaultProperties;
        [SerializeField] RenderableObject[] Objects;

        int BatchCapacity = 1024;
        Dictionary<int, (Mesh, Material)> RuntimeData = new Dictionary<int, (Mesh, Material)>();
        Dictionary<int, MaterialPropertyBlock> Properties = new Dictionary<int, MaterialPropertyBlock>();
        Dictionary<int, List<Batch>> Batches = new Dictionary<int, List<Batch>>();
        Dictionary<int, List<Batch>> StaticBatches = new Dictionary<int, List<Batch>>();

        void Start()
        {
            if (DefaultProperties != null)
                for (int d = 0; d < DefaultProperties.Length; d++)
                {
                    var list = DefaultProperties[d];
                    Properties[list.ID] = CreatePropertyBlock(list.Value, out var a);
                }
        }

        public void Render(NativeArray<Renderable> query)
        {
            Profiler.BeginSample("Render");
            UpdMat(query);
            Draw();
            Profiler.EndSample();
        }
        internal int AddMaterial(Material material)
        {
            var index = RuntimeData.Keys.Count + 1000;
            RuntimeData[index] = (null, material);

            return index;
        }
        internal int AddProperty(List<RenderProperty> properties)
        {
            var block = CreatePropertyBlock(properties, out var id);
            if (id != -1)
                Properties[id] = block;

            return id;
        }

        void UpdMat(NativeArray<Renderable> query)
        {
            for (int q = 0; q < query.Length; q++)
                GetBatch(query[q]);

            var remKeys = new List<int>();
            foreach (var (key, list) in Batches)
            {
                var remBatches = new List<int>();
                for (int l = 0; l < list.Count; l++)
                {
                    var batch = list[l];
                    if (batch.Count == 0)
                    {
                        remBatches.Add(l);

                        continue;
                    }

                    var diff = batch.Count - batch.Value.Count;
                    if (diff < 0)
                        batch.Value.RemoveRange(batch.Count, -diff);

                    batch.Count = 0;
                    list[l] = batch;
                }

                for (int r = remBatches.Count - 1; r >= 0; r--)
                    list.RemoveAt(remBatches[r]);

                if (list.Count == 0)
                    remKeys.Add(key);
            }

            for (int r = 0; r < remKeys.Count; r++)
                Batches.Remove(remKeys[r]);
        }
        void GetBatch(Renderable r)
        {
            if (!r.IsVisible)
                return;

            switch (r.RenderType)
            {
                case Renderable.Type.Static:
                {
                    if (!StaticBatches.TryGetValue(r.Key, out var list))
                    {
                        StaticBatches[r.Key] = new List<Batch>
                        {
                            new Batch
                            {
                                Count = 1,
                                PropertyIndex = r.PropertyIndex,
                                Value = new List<Matrix4x4> { r.Matrix }
                            }
                        };

                        return;
                    }

                    for (int l = 0; l < list.Count; l++)
                    {
                        var batch = list[l];
                        if (batch.PropertyIndex == r.PropertyIndex &&
                             batch.Count < BatchCapacity)
                        {
                            if (batch.Value.Count > batch.Count)
                                batch.Value[batch.Count] = r.Matrix;
                            else
                                batch.Value.Add(r.Matrix);

                            batch.Count++;
                            list[l] = batch;

                            return;
                        }
                    }

                    list.Add(new Batch
                    {
                        Count = 1,
                        PropertyIndex = r.PropertyIndex,
                        Value = new List<Matrix4x4> { r.Matrix }
                    });
                }
                break;
                case Renderable.Type.Dynamic:
                {
                    if (!Batches.TryGetValue(r.Key, out var list))
                    {
                        Batches[r.Key] = new List<Batch>
                        {
                            new Batch
                            {
                                Count = 1,
                                PropertyIndex = r.PropertyIndex,
                                Value = new List<Matrix4x4> { r.Matrix }
                            }
                        };

                        return;
                    }

                    for (int l = 0; l < list.Count; l++)
                    {
                        var batch = list[l];
                        if (batch.PropertyIndex == r.PropertyIndex &&
                             batch.Count < BatchCapacity)
                        {
                            if (batch.Value.Count > batch.Count)
                                batch.Value[batch.Count] = r.Matrix;
                            else
                                batch.Value.Add(r.Matrix);

                            batch.Count++;
                            list[l] = batch;

                            return;
                        }
                    }

                    list.Add(new Batch
                    {
                        Count = 1,
                        PropertyIndex = r.PropertyIndex,
                        Value = new List<Matrix4x4> { r.Matrix }
                    });
                }
                break;
            }
        }
        void Draw()
        {
            DrawBatches(StaticBatches);
            DrawBatches(Batches);
        }
        void DrawBatches(Dictionary<int, List<Batch>> batches)
        {
            foreach (var (key, list) in batches)
            {
                var index = GetObjectIndex(key);
                Mesh mesh = null;
                Material material = null;
                var castShadows = ShadowCastingMode.Off;
                var receiveShadows = false;
                if (index >= 0)
                {
                    var obj = Objects[index];

                    mesh = obj.Mesh;
                    material = obj.Material;
                    castShadows = obj.ShadowCasting;
                    receiveShadows = obj.ReceiveShadows;
                }
                else
                    (mesh, material) = RuntimeData[key];

                if (!mesh || !material)
                    continue;

                for (int l = 0; l < list.Count; l++)
                {
                    var batch = list[l];
                    Properties.TryGetValue(batch.PropertyIndex, out var properties);

                    Graphics.DrawMeshInstanced(
                     mesh,
                     0,
                     material,
                     batch.Value,
                     properties,
                     castShadows,
                     receiveShadows);
                }
            }
        }

        int GetObjectIndex(int id)
        {
            for (int o = 0; o < Objects.Length; o++)
                if (Objects[o].ID == id)
                    return o;

            return -1;
        }
        MaterialPropertyBlock CreatePropertyBlock(List<RenderProperty> propList, out int id)
        {
            id = -1;
            var block = new MaterialPropertyBlock();
            for (int p = 0; p < propList.Count; p++)
            {
                var prop = propList[p];
                id += prop.GetHashCode();

                switch (prop.ValueType)
                {
                    case RenderProperty.Type.Int:
                    block.SetInt(prop.Reference, prop.IntValue);
                    break;
                    case RenderProperty.Type.Float:
                    block.SetFloat(prop.Reference, prop.FloatValue);
                    break;
                    case RenderProperty.Type.Vector3:
                    block.SetVector(prop.Reference, prop.Vector3Value);
                    break;
                    case RenderProperty.Type.Vector2:
                    block.SetVector(prop.Reference, prop.Vector2Value);
                    break;
                    case RenderProperty.Type.Boolean:
                    block.SetInt(prop.Reference, prop.BooleanValue ? 1 : 0);
                    break;
                }
            }

            return block;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            var list = new List<RenderableObject>();
            if (Objects != null)
                list.AddRange(Objects);

            var objs = Resources.LoadAll<RenderableObject>("Presentation/Serializables/Objects/");
            for (int o = 0; o < objs.Length; o++)
                if (!list.Contains(objs[o]))
                    list.Add(objs[o]);

            Objects = list.ToArray();
        }
        void Reset()
        {
            Tool.CreateTag("Renderer");
            gameObject.tag = "Renderer";
        }
#endif

        [System.Serializable]
        public struct PropertiesList
        {
            public int ID;
            public List<RenderProperty> Value;
        }
    }
}