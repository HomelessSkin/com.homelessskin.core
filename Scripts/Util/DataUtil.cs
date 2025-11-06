using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using TMPro;

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;

#endif

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

using static Unity.Mathematics.math;
using static Unity.Mathematics.noise;

using Random = Unity.Mathematics.Random;

namespace Core.Util
{
    public static class Lists
    {
        public static void AddIndex(int index, ref NativeList<int> list)
        {
            if (!list.Contains(index))
                list.Add(index);
        }
        public static void ApplyListToList<T>(NativeList<T> values, ref NativeList<T> result) where T : unmanaged
        {
            for (int i = 0; i < values.Length; i++)
                result.Add(values[i]);
        }
        public static void ApplyListToList<T>(List<T> values, ref NativeList<T> result) where T : unmanaged
        {
            for (int i = 0; i < values.Count; i++)
                result.Add(values[i]);
        }
        public static void ApplyListToList<T>(NativeList<T> values, ref List<T> result) where T : unmanaged
        {
            for (int i = 0; i < values.Length; i++)
                result.Add(values[i]);
        }
        public static void ApplyListToList<T>(List<T> values, ref List<T> result) where T : unmanaged
        {
            for (int i = 0; i < values.Count; i++)
                result.Add(values[i]);
        }
        public static bool CompareListValues<T>(List<T> mustLower, List<T> mustGreater) where T : unmanaged, IComparable<T>
        {
            if (mustLower.Count != mustGreater.Count)
                return false;

            for (int i = 0; i < mustLower.Count; i++)
                if (mustLower[i].CompareTo(mustGreater[i]) > 0)
                    return false;

            return true;
        }
        public static bool CompareListValues<T>(NativeList<T> mustLower, NativeList<T> mustGreater) where T : unmanaged, IComparable<T>
        {
            if (mustLower.Length != mustGreater.Length)
            {
#if UNITY_EDITOR
                Debug.Log("lengths not equals");
#endif
                return false;
            }

            for (int i = 0; i < mustLower.Length; i++)
                if (mustLower[i].CompareTo(mustGreater[i]) > 0)
                {
#if UNITY_EDITOR
                    Debug.Log("must lower value is greater");
#endif
                    return false;
                }

            return true;
        }
        public static bool GetListIndexOf_UM<T>(T element, NativeList<T> list, out int index) where T : unmanaged
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (element.Equals(list[i]))
                {
                    index = i;
                    return true;
                }
            }

            index = 0;

            return false;
        }
        public static bool GetListIndexOf_EQ<T>(T element, NativeList<T> list, out int index) where T : unmanaged, IEquatable<T>
        {
            for (int i = 0; i < list.Length; i++)
            {
                if (element.Equals(list[i]))
                {
                    index = i;
                    return true;
                }
            }

            index = 0;

            return false;
        }
        public static bool Contains_UM<T>(NativeList<T> list, T element) where T : unmanaged
        {
            for (int i = 0; i < list.Length; i++)
                if (list[i].Equals(element))
                    return true;

            return false;
        }
        public static bool Contains_EQ<T>(NativeList<T> list, T element) where T : unmanaged, IEquatable<T>
        {
            for (int i = 0; i < list.Length; i++)
                if (list[i].Equals(element))
                    return true;

            return false;
        }
        public static void RemoveFromList_UM<T>(T element, NativeList<T> list) where T : unmanaged
        {
            if (GetListIndexOf_UM(element, list, out var index))
                list.RemoveAt(index);
        }
        public static void RemoveFromList_EQ<T>(T element, NativeList<T> list) where T : unmanaged, IEquatable<T>
        {
            if (GetListIndexOf_EQ(element, list, out var index))
                list.RemoveAt(index);
        }
        public static void Reverse<T>(ref NativeList<T> list) where T : unmanaged
        {
            var len = list.Length;
            var holder = new NativeList<T>(Allocator.Persistent);

            for (int i = len - 1; i >= 0; i--)
                holder.Add(list[i]);

            list = holder;
        }
        public static bool IsSubList_UM<T>(NativeList<T> sub, NativeList<T> list) where T : unmanaged
        {
            for (int i = 0; i < sub.Length; i++)
                if (!Contains_UM(list, sub[i]))
                    return false;

            return true;
        }
        public static bool IsSubList_EQ<T>(NativeList<T> sub, NativeList<T> list) where T : unmanaged, IEquatable<T>
        {
            for (int i = 0; i < sub.Length; i++)
                if (!Contains_EQ(list, sub[i]))
                    return false;

            return true;
        }
        public static NativeList<T> GetSubset_UM<T>(NativeList<T> actions, T action) where T : unmanaged
        {
            var list = new NativeList<T>(Allocator.Persistent);

            for (int i = 0; i < actions.Length; i++)
                if (!actions[i].Equals(action))
                    list.Add(actions[i]);

            return list;
        }
        public static NativeList<T> GetSubset_EQ<T>(NativeList<T> actions, T action) where T : unmanaged, IEquatable<T>
        {
            var list = new NativeList<T>(Allocator.Persistent);

            for (int i = 0; i < actions.Length; i++)
                if (!actions[i].Equals(action))
                    list.Add(actions[i]);

            return list;
        }
    }

    #region BEZIER
    public static class Bezier
    {
        public static NativeList<int> GetCurve(NativeList<float3> points, int width, float step)
        {
            var list = new NativeList<int>(Allocator.Persistent);

            for (int c = 0; c < points.Length; c++)
            {
                var a = c - 2;
                if (a == -1)
                    a = points.Length - 1;
                else if (a == -2)
                    a = points.Length - 2;
                var A = points[a];

                var b = c - 1;
                if (b == -1)
                    b = points.Length - 1;
                var B = points[b];
                var C = points[c];

                var d = c + 1;
                if (d == points.Length)
                    d = 0;
                var D = points[d];

                var e = c + 2;
                if (e == points.Length)
                    e = 0;
                else if (e == points.Length + 1)
                    e = 1;
                var E = points[e];

                var f = 0f;
                var prev = (int)floor(C.x) + (int)floor(C.z) * width;

                while (f <= 1f)
                {
                    var aF = A + f * (B - A);
                    var bF = B + f * (C - B);
                    var cF = C + f * (D - C);
                    var dF = D + f * (E - D);

                    var xF = (aF + bF + cF + f * (dF - aF)) / 3f;

                    var final = (int)floor(xF.x) + (int)floor(xF.z) * width;
                    Lists.AddIndex(final, ref list);

                    prev = final;
                    f += step;
                }
            }

            return list;
        }
        public static NativeList<float3> GetCurve(float3x2 segment, float3 pivot, float step)
        {
            var list = new NativeList<float3>(Allocator.Persistent);

            var a = segment.c0;
            var b = segment.c1;
            var aP = pivot - a;
            var pB = b - pivot;

            var f = 0f;
            while (f <= 1f)
            {
                var aF = a + f * aP;
                var bF = pivot + f * pB;

                list.Add(aF + f * (bF - aF));

                f += step;
            }

            return list;
        }
    }
    #endregion

    #region POISSON
    [System.Serializable]
    public struct Poisson
    {
        public uint Seed;
        public int StartCount;
        public int Iterations;
        public int MaxIterations;
        public int GridWidth;
        public int GridHeight;
        public float CellWidth;
        public float MinDist;
        public bool Radial;
        public float Radius;
        public float3 Center;

        public int GridSize { get { return GridWidth * GridHeight; } }
        public int HalfW { get { return GridWidth / 2; } }
        public int HalfH { get { return GridHeight / 2; } }
        public float LeftX { get { return Center.x - HalfW * CellWidth; } }
        public float BackZ { get { return Center.z - HalfH * CellWidth; } }
        public float3 Start { get { return float3(LeftX, Center.y, BackZ); } }

        //public float RightX { get { return Center.x + HalfW * CellWidth; } }
        //public float ForwardZ { get { return Center.z + HalfH * CellWidth; } }

        public NativeArray<float3> GetGrid_Job(Bounds[] obstacles, Allocator allocator = Allocator.Persistent)
        {
            var grid = new NativeArray<float3>(GridSize, allocator);
            var obsts = new NativeArray<Bounds>(obstacles, Allocator.TempJob);

            new GetGridJob
            {
                Settings = this,
                Obstacles = obsts,
                Grid = grid,
            }
            .Schedule()
            .Complete();

            obsts.Dispose();

            return grid;
        }

        [BurstCompile]
        struct GetGridJob : IJob
        {
            [ReadOnly] public Poisson Settings;
            [ReadOnly] public NativeArray<Bounds> Obstacles;

            public NativeArray<float3> Grid;

            public void Execute()
            {
                var random = new Random(Settings.Seed);
                var active = new NativeList<float3>(Allocator.Temp);
                var count = 0;
                while (count < Settings.StartCount)
                {
                    var gridPoint = int2(random.NextInt(0, Settings.GridWidth), random.NextInt(0, Settings.GridHeight));
                    var worldPoint = Settings.Start + Settings.CellWidth * float3(gridPoint.x, 0f, gridPoint.y);
                    if (Check(gridPoint, worldPoint))
                    {
                        Grid[gridPoint.ToIndex(Settings.GridWidth)] = worldPoint;
                        active.Add(worldPoint);
                    }

                    count++;
                }

                count = 0;
                while (active.Length > 0 && count < Settings.MaxIterations)
                {
                    var current = active[random.NextInt(0, active.Length)];
                    var stay = false;

                    for (int j = 0; j < Settings.Iterations; j++)
                        stay |= Sample(current, ref active, ref random);

                    if (!stay)
                        Lists.RemoveFromList_EQ(current, active);

                    count++;
                }
            }

            bool Sample(float3 current, ref NativeList<float3> active, ref Random random)
            {
                var pretender = current.xz + random.NextFloat(Settings.MinDist, 2f * Settings.MinDist) * random.NextFloat2Direction();
                var gridPoint = int2(floor(float2(pretender.x / Settings.CellWidth, pretender.y / Settings.CellWidth) + float2(Settings.HalfW, Settings.HalfH)));
                var worldPoint = float3(pretender.x, current.y, pretender.y);

                if (Check(gridPoint, worldPoint))
                {
                    Grid[gridPoint.ToIndex(Settings.GridWidth)] = worldPoint;
                    active.Add(worldPoint);

                    return true;
                }

                return false;
            }
            bool Check(int2 gridPoint, float3 worldPoint)
            {
                if (gridPoint.x < 0 ||
                     gridPoint.y < 0 ||
                     gridPoint.x >= Settings.GridWidth ||
                     gridPoint.y >= Settings.GridHeight)
                    return false;

                if (Settings.Radial && distance(Settings.Center, worldPoint) > Settings.Radius)
                    return false;

                for (int o = 0; o < Obstacles.Length; o++)
                    if (Obstacles[o].Contains(worldPoint))
                        return false;

                return CheckNearPoints(gridPoint, worldPoint, (int)ceil(Settings.MinDist));
            }
            bool CheckNearPoints(int2 gridPoint, float3 worldPoint, int range)
            {
                for (int x = -range; x <= range; x++)
                    for (int y = -range; y <= range; y++)
                    {
                        var xx = gridPoint.x + x;
                        var yy = gridPoint.y + y;

                        if (xx >= 0 &&
                             yy >= 0 &&
                             xx < Settings.GridWidth &&
                             yy < Settings.GridHeight)
                        {
                            var p = Grid[xx + yy * Settings.GridWidth];
                            if (length(p) > 0f &&
                                 distance(worldPoint, p) < Settings.MinDist)
                                return false;
                        }
                    }

                return true;
            }
        }
    }
    #endregion

    public static class DataUtil
    {
        public static Texture2D ResizeBilinear(Texture2D source, int newWidth, int newHeight)
        {
            var result = new Texture2D(newWidth, newHeight);
            var pixels = new Color32[newWidth * newHeight];

            for (int y = 1; y < newHeight - 1; y++)
                for (int x = 1; x < newWidth - 1; x++)
                {
                    var u = x / (float)newWidth;
                    var v = y / (float)newHeight;

                    var srcX = u * source.width;
                    var srcY = v * source.height;

                    var x1 = (int)Mathf.Floor(srcX);
                    var y1 = (int)Mathf.Floor(srcY);
                    var x2 = Mathf.Min(x1 + 1, source.width - 1);
                    var y2 = Mathf.Min(y1 + 1, source.height - 1);

                    var xLerp = srcX - x1;
                    var yLerp = srcY - y1;

                    var top = Color.Lerp(source.GetPixel(x1, y1), source.GetPixel(x2, y1), xLerp);
                    var bottom = Color.Lerp(source.GetPixel(x1, y2), source.GetPixel(x2, y2), xLerp);

                    pixels[y * newWidth + x] = Color.Lerp(top, bottom, yLerp);
                }

            result.SetPixels32(pixels);
            result.Apply();

            return result;
        }
        public static void FitSize<T, U>(ref T[] values, U[] other)
        {
            if (values.Length < other.Length)
            {
                var list = new List<T>(values);
                for (int b = list.Count; b < other.Length; b++)
                    list.Add(default(T));

                values = list.ToArray();
            }
            else if (values.Length > other.Length)
            {
                var list = new List<T>(values);
                for (int b = list.Count - 1; b >= other.Length; b--)
                    list.RemoveAt(b);

                values = list.ToArray();
            }
        }
        public static bool Remove<T>(ref this NativeList<T> list, T value) where T : unmanaged, IEquatable<T>
        {
            var index = -1;
            for (int l = 0; l < list.Length; l++)
                if (list[l].Equals(value))
                {
                    index = l;

                    break;
                }

            if (index >= 0)
            {
                list.RemoveAt(index);

                return true;
            }

            return false;
        }
        public static void ToConvexHull(this List<Vector3> points)
        {
            if (points.Count <= 1)
                return;

            var sortedPoints = points.OrderBy(p => p.x).ThenBy(p => p.z).ToList();
            var lowerHull = new List<Vector3>();

            for (int s = 0; s < sortedPoints.Count; s++)
            {
                var point = sortedPoints[s];
                while (lowerHull.Count >= 2 &&
                    Cross(lowerHull[lowerHull.Count - 2], lowerHull[lowerHull.Count - 1], point) <= 0)
                    lowerHull.RemoveAt(lowerHull.Count - 1);

                lowerHull.Add(point);
            }

            var upperHull = new List<Vector3>();
            sortedPoints.Reverse();
            for (int s = 0; s < sortedPoints.Count; s++)
            {
                var point = sortedPoints[s];
                while (upperHull.Count >= 2 &&
                    Cross(upperHull[upperHull.Count - 2], upperHull[upperHull.Count - 1], point) <= 0)
                    upperHull.RemoveAt(upperHull.Count - 1);

                upperHull.Add(point);
            }

            lowerHull.RemoveAt(lowerHull.Count - 1);
            upperHull.RemoveAt(upperHull.Count - 1);
            lowerHull.AddRange(upperHull);

            points.Clear();
            points.AddRange(lowerHull);
        }
        public static string SplitByCapitals(string input)
        {
            var a = Regex.Split(input, @"(?<!^)(?=[A-Z])");
            var s = "";
            for (int i = 0; i < a.Length; i++)
                s += a[i] + " ";

            return s;
        }
        public static int GetFace(Vector3 dir)
        {
            dir = dir.normalized;
            var face = 0;
            var dt = float.MinValue;
            for (int d = 0; d < Directions.Length; d++)
            {
                var v = Vector3.Dot(dir, Directions[d]);
                if (v > dt)
                {
                    face = d;
                    dt = v;
                }
            }

            return face;
        }
        public static int ToIndex(this int2 vector, int width) => vector.x + width * vector.y;
        public static int ToIndex(this Vector3Int vector, int width, int height) => vector.x + width * vector.y + width * height * vector.z;
        public static float GetNoise(float x, float z, float4 settings, float2 offset = default)
        {
            var result = 0f;
            var p = settings.y;
            var a = settings.z;
            var f = settings.w;

            for (int o = 0; o < settings.x; o++)
            {
                result += a * cnoise(float2(f * (x + offset.x), f * (z + offset.y)));

                a *= p;
                f *= 2f;
            }

            return result;
        }
        public static float GetNoise3D(float3 position, float4 settings, float3 offset)
        {
            var result = 0f;
            var p = settings.y;
            var a = settings.z;
            var f = settings.w;

            for (int o = 0; o < settings.x; o++)
            {
                result += a * cnoise(f * position.xy + offset.xy) * cnoise(f * position.yz + offset.yz) * cnoise(f * position.zx + +offset.zx);

                a *= p;
                f *= 2f;
            }

            return result;
        }
        public static float DistanceToLine(this float3 point, float3 A, float3 B)
        {
            var D = B - A;
            var V = point - A;

            var len = length(D);
            if (len == 0f)
                return distance(point, A);

            var crss = length(cross(V, D));
            var dist = crss / len;

            return dist;
        }
        public static float DistanceToLine(this Vector3 point, Vector3 A, Vector3 B)
        {
            var D = B - A;
            var V = point - A;

            if (D == Vector3.zero)
                return Vector3.Distance(point, A);

            var cross = Vector3.Cross(V, D);
            var distance = cross.magnitude / D.magnitude;

            return distance;
        }
        public static float DistanceToPlane(this Vector3 point, Vector3 planePos, Vector3 planeNormal)
            => abs(dot(planeNormal, point) - dot(planeNormal, planePos));
        public static float Cross(Vector3 a, Vector3 b, Vector3 c) => (b.x - a.x) * (c.z - a.z) - (b.z - a.z) * (c.x - a.x);
        public static bool Contains2D(this float3x2 rect, float3 point) =>
            point.x >= rect.c0.x && point.x <= rect.c1.x &&
            point.z >= rect.c0.z && point.z <= rect.c1.z;
        public static Vector3Int ToVector(this int index, int width, int height)
        {
            var z = index / (width * height);
            var remainder = index % (width * height);
            var y = remainder / width;
            var x = remainder % width;

            return new Vector3Int(x, y, z);
        }
        public static Vector3 TransformPart(Vector3 nodePos, Quaternion nodeRot, Vector3 pos, bool scale = false)
        {
            if (scale)
            {
                pos.y *= 0.5f;

                return new Vector3(nodePos.x, 0.5f * nodePos.y, nodePos.z) + nodeRot * pos;
            }

            return nodePos + nodeRot * pos;
        }
        public static Vector3 Rotate(int rotation, Vector3 vec, bool inverse = false)
        {
            switch (rotation)
            {
                case 1:
                return (inverse ? YRotations[3] : YRotations[0]) * vec;
                case 2:
                return (inverse ? YRotations[4] : YRotations[1]) * vec;
                case 3:
                return (inverse ? YRotations[5] : YRotations[2]) * vec;
                default:
                return vec;
            }
        }
        public static Vector3[] GetCorners(this Bounds bounds)
        {
            var corners = new Vector3[8];
            var min = bounds.min;
            var max = bounds.max;

            corners[0] = bounds.min;
            corners[1] = new Vector3(min.x, min.y, max.z);
            corners[2] = new Vector3(min.x, max.y, min.z);
            corners[3] = new Vector3(min.x, max.y, max.z);
            corners[4] = new Vector3(max.x, min.y, min.z);
            corners[5] = new Vector3(max.x, min.y, max.z);
            corners[6] = new Vector3(max.x, max.y, min.z);
            corners[7] = bounds.max;

            return corners;
        }
        public static NativeArray<Vector3> GetCorners_Native(this Bounds bounds, Allocator allocator = Allocator.Temp)
        {
            var corners = new NativeArray<Vector3>(8, allocator);
            var min = bounds.min;
            var max = bounds.max;

            corners[0] = bounds.min;
            corners[1] = new Vector3(min.x, min.y, max.z);
            corners[2] = new Vector3(min.x, max.y, min.z);
            corners[3] = new Vector3(min.x, max.y, max.z);
            corners[4] = new Vector3(max.x, min.y, min.z);
            corners[5] = new Vector3(max.x, min.y, max.z);
            corners[6] = new Vector3(max.x, max.y, min.z);
            corners[7] = bounds.max;

            return corners;
        }
        public static Color GetPixel(int index, int rowLen, Texture2D colors) => colors.GetPixel(index % rowLen * colors.width / rowLen + 1,
            index / rowLen * colors.width / rowLen + 1, 0);
        public static Color[] GetPixels(int index, int width, int height, int rowLen, Texture2D colors) => colors.GetPixels(index % rowLen * width,
            index / rowLen * height, width, height, 0);
        public static NativeArray<int> CalculateRotationMatrix(int x, int y, int z)
        {
            var rx = GetXRotationMatrix(x);
            var ry = GetYRotationMatrix(y);
            var rz = GetZRotationMatrix(z);
            var ry_rx = MultiplyMatrices(ry, rx, Allocator.Temp);

            return MultiplyMatrices(rz, ry_rx, Allocator.Persistent);
        }
        public static NativeArray<int> MultiplyMatrices(NativeArray<int> a, NativeArray<int> b, Allocator allocator)
        {
            var result = new NativeArray<int>(9, allocator);
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    result[i + 3 * j] = a[i] * b[3 * j] + a[i + 3] * b[1 + 3 * j] + a[i + 6] * b[2 + 3 * j];

            return result;
        }
        public static Sprite CaptureCameraSprite(Camera camera)
        {
            var originalTarget = camera.targetTexture;

            var width = camera.pixelWidth;
            var height = camera.pixelHeight;

            var renderTexture = RenderTexture.GetTemporary(width, height, 24);

            camera.targetTexture = renderTexture;
            camera.Render();

            RenderTexture.active = renderTexture;

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();

            camera.targetTexture = originalTarget;
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(renderTexture);

            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        }
        public static IEnumerator LoadTexture(string url, Image image)
        {
            using (var webRequest = UnityWebRequestTexture.GetTexture(url))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                    Debug.LogError("Image Load Error: " + webRequest.error);
                else
                {
                    Debug.Log("Setting Image");

                    var texture = DownloadHandlerTexture.GetContent(webRequest);
                    image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                }
            }
        }
        public static Mesh CalculateSelectedMesh(Vector3[] corners, Mesh sample, Mesh mesh = null)
        {
            if (!mesh)
                mesh = new Mesh();

            mesh.Clear();

            var vertices = new List<Vector3>();
            var triangles = new List<int>();

            for (int c = 0; c < 8; c++)
                AddCorner(corners[c], GetRotation(c));

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;

            void AddCorner(Vector3 corner, Quaternion rotation)
            {
                for (int s = 0; s < sample.triangles.Length; s++)
                {
                    triangles.Add(vertices.Count);
                    vertices.Add(corner + rotation * sample.vertices[sample.triangles[s]]);
                }
            }
            Quaternion GetRotation(int c)
            {
                switch (c)
                {
                    case 1:
                    return Quaternion.Euler(0f, 90f, 0f);
                    case 2:
                    return Quaternion.Euler(0f, 0f, -90f);
                    case 3:
                    return Quaternion.Euler(180f, 0f, 0f);
                    case 4:
                    return Quaternion.Euler(0f, -90f, 0f);
                    case 5:
                    return Quaternion.Euler(0f, 180f, 0f);
                    case 6:
                    return Quaternion.Euler(0f, 0f, 180f);
                    case 7:
                    return Quaternion.Euler(90f, 180f, 0f);
                    default:
                    return Quaternion.identity;
                }
            }
        }

        static NativeArray<int> GetXRotationMatrix(int degrees)
        {
            var arr = new NativeArray<int>(9, Allocator.Temp);
            switch (degrees % 360)
            {
                case 90:
                arr[0] = 1;
                arr[1] = 0;
                arr[2] = 0;
                arr[3] = 0;
                arr[4] = 0;
                arr[5] = -1;
                arr[6] = 0;
                arr[7] = 1;
                arr[8] = 0;
                break;
                case 180:
                arr[0] = 1;
                arr[1] = 0;
                arr[2] = 0;
                arr[3] = 0;
                arr[4] = -1;
                arr[5] = 0;
                arr[6] = 0;
                arr[7] = 0;
                arr[8] = -1;
                break;
                case 270:
                arr[0] = 1;
                arr[1] = 0;
                arr[2] = 0;
                arr[3] = 0;
                arr[4] = 0;
                arr[5] = 1;
                arr[6] = 0;
                arr[7] = -1;
                arr[8] = 0;
                break;
                default:
                arr[0] = 1;
                arr[1] = 0;
                arr[2] = 0;
                arr[3] = 0;
                arr[4] = 1;
                arr[5] = 0;
                arr[6] = 0;
                arr[7] = 0;
                arr[8] = 1;
                break;
            }

            return arr;
        }
        static NativeArray<int> GetYRotationMatrix(int degrees)
        {
            var arr = new NativeArray<int>(9, Allocator.Temp);
            switch (degrees % 360)
            {
                case 90:
                arr[0] = 0;
                arr[1] = 0;
                arr[2] = 1;
                arr[3] = 0;
                arr[4] = 1;
                arr[5] = 0;
                arr[6] = -1;
                arr[7] = 0;
                arr[8] = 0;
                break;
                case 180:
                arr[0] = -1;
                arr[1] = 0;
                arr[2] = 0;
                arr[3] = 0;
                arr[4] = 1;
                arr[5] = 0;
                arr[6] = 0;
                arr[7] = 0;
                arr[8] = -1;
                break;
                case 270:
                arr[0] = 0;
                arr[1] = 0;
                arr[2] = -1;
                arr[3] = 0;
                arr[4] = 1;
                arr[5] = 0;
                arr[6] = 1;
                arr[7] = 0;
                arr[8] = 0;
                break;
                default:
                arr[0] = 1;
                arr[1] = 0;
                arr[2] = 0;
                arr[3] = 0;
                arr[4] = 1;
                arr[5] = 0;
                arr[6] = 0;
                arr[7] = 0;
                arr[8] = 1;
                break;
            }

            return arr;
        }
        static NativeArray<int> GetZRotationMatrix(int degrees)
        {
            var arr = new NativeArray<int>(9, Allocator.Temp);
            switch (degrees % 360)
            {
                case 90:
                arr[0] = 0;
                arr[1] = -1;
                arr[2] = 0;
                arr[3] = 1;
                arr[4] = 0;
                arr[5] = 0;
                arr[6] = 0;
                arr[7] = 0;
                arr[8] = 1;
                break;
                case 180:
                arr[0] = -1;
                arr[1] = 0;
                arr[2] = 0;
                arr[3] = 0;
                arr[4] = -1;
                arr[5] = 0;
                arr[6] = 0;
                arr[7] = 0;
                arr[8] = 1;
                break;
                case 270:
                arr[0] = 0;
                arr[1] = 1;
                arr[2] = 0;
                arr[3] = -1;
                arr[4] = 0;
                arr[5] = 0;
                arr[6] = 0;
                arr[7] = 0;
                arr[8] = 1;
                break;
                default:
                arr[0] = 1;
                arr[1] = 0;
                arr[2] = 0;
                arr[3] = 0;
                arr[4] = 1;
                arr[5] = 0;
                arr[6] = 0;
                arr[7] = 0;
                arr[8] = 1;
                break;
            }

            return arr;
        }

        #region DIRECTIONS
        public static readonly Vector3Int[] Directions = new Vector3Int[6]
        {
            Vector3Int.up,
            Vector3Int.forward,
            Vector3Int.right,
            Vector3Int.back,
            Vector3Int.left,
            Vector3Int.down,
         };
        #endregion

        #region YROTATIONS
        public static readonly Quaternion[] YRotations = new Quaternion[6]
        {
            // Counter-clockwise rotations (positive angles)
            new Quaternion(0f, 0.7071068f, 0f, 0.7071068f),     // 90° CCW
            new Quaternion(0f, 1f, 0f, 0f),                                         // 180° CCW
            new Quaternion(0f, 0.7071068f, 0f, -0.7071068f),   // 270° CCW

            // Clockwise rotations (negative angles)
            new Quaternion(0f, -0.7071068f, 0f, 0.7071068f),   // 90° CW
            new Quaternion(0f, -1f, 0f, 0f),                                       // 180° CW
            new Quaternion(0f, -0.7071068f, 0f, -0.7071068f)  // 270° CW
        };
        #endregion

        #region EDGES
        static readonly int2[] Edges = new int2[12]
        {
            // X-axis edges (min to max in x)
            new int2(0, 4), new int2(1, 5), new int2(2, 6), new int2(3, 7),
            // Y-axis edges (min to max in y)
            new int2(0, 2), new int2(1, 3), new int2(4, 6), new int2(5, 7),
            // Z-axis edges (min to max in z)
            new int2(0, 1), new int2(2, 3), new int2(4, 5), new int2(6, 7)
        };
        #endregion
    }

    #region ELBO LOCAL TRANSFORM
    [System.Serializable]
    public struct ElboLocalTransform
    {
        public Vector3 VectorPosition;
        public byte2 Rotation;
        public Quaternion QuaternionRotation;

        public Matrix4x4 TRS
        {
            get
            {
                var pos = VectorPosition;
                pos.y /= 2f;
                return Matrix4x4.TRS(pos, QuaternionRotation, Vector3.one);
            }
        }
    }
    #endregion

    #region BYTE2
    [System.Serializable]
    public struct byte2
    {
        public byte x;
        public byte y;

        public byte2(byte xx, byte yy)
        {
            x = xx;
            y = yy;
        }
    }
    #endregion

    #region BATCH
    public struct Batch
    {
        public int Count;
        public int PropertyIndex;

        public List<Matrix4x4> Value;
    }
    #endregion

    #region ROTATION DEFINITION
    public struct RotationDefinition
    {
        public Vector3Int EulerAngles;
        public readonly NativeArray<int> RotationMatrix;

        public RotationDefinition(int x, int y, int z)
        {
            EulerAngles = new Vector3Int(x, y, z);
            RotationMatrix = DataUtil.CalculateRotationMatrix(x, y, z);
        }
    }
    #endregion

    #region TRIANGLE
    public struct Triangle
    {
        public float Area;
        public Vector3 A;
        public Vector3 B;
        public Vector3 C;
        public Vector3 InPoint;
        public FixedList32Bytes<int> Indices;

        public Triangle(int v1, int v2, int v3, NativeList<Vector3> vertices)
        {
            A = vertices[v1];
            B = vertices[v2];
            C = vertices[v3];
            Area = CalculateTriangleArea(A, B, C);
            InPoint = GetIncenter(A, B, C);

            Indices = new FixedList32Bytes<int>();
            Indices.Add(v1);
            Indices.Add(v2);
            Indices.Add(v3);

        }

        public static float CalculateTriangleArea(Vector3 a, Vector3 b, Vector3 c)
        {
            var ab = b - a;
            var ac = c - a;
            var crossProduct = Vector3.Cross(ab, ac);
            var area = crossProduct.magnitude * 0.5f;

            return area;
        }
        public static Vector3 GetIncenter(Vector3 a, Vector3 b, Vector3 c)
        {
            var abLength = Vector3.Distance(a, b);
            var bcLength = Vector3.Distance(b, c);
            var caLength = Vector3.Distance(c, a);

            var perimeter = abLength + bcLength + caLength;

            return (a * bcLength + b * caLength + c * abLength) / perimeter;
        }
    }
    #endregion

    public enum BlockFace : byte { Top = 0, Front = 1, Right = 2, Back = 3, Left = 4, Bot = 5 }

    #region KEY SCRIPTABLE
    public abstract class KeyScriptable : ScriptableObject
    {
        public int ID;
        public int GetID() => ID;

#if UNITY_EDITOR
        public void RandomizeID()
        {
            ID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static bool Create<T>(out T asset) where T : KeyScriptable
        {
            asset = ScriptableObject.CreateInstance<T>();

            var path = EditorUtility.SaveFilePanelInProject("Save Asset", "New.asset", "asset", "POOK");
            if (string.IsNullOrEmpty(path))
                return false;

            AssetDatabase.CreateAsset(asset, path);
            asset.RandomizeID();

            return true;
        }
        public static bool Load<T>(out T asset) where T : KeyScriptable
        {
            asset = null;

            var path = EditorUtility.OpenFilePanel("Load Asset", Application.dataPath, "asset");
            if (string.IsNullOrEmpty(path))
                return false;

            asset = AssetDatabase.LoadAssetAtPath<T>(FileUtil.GetProjectRelativePath(path));

            return asset;
        }
#endif
    }
    #endregion

    #region STREAMING SPRITES
    [Serializable]
    public class StreamingSprites
    {
        public int SpriteDensity;
        public Texture2D Texture;
        public Texture2D[] DefaultSprites;
        public TMP_SpriteAsset Asset;

        public bool[] TextureMap;
        public List<int> ReservedKeys = new List<int>();
        public Dictionary<int, (int, List<GameObject>)> HashSprite = new Dictionary<int, (int, List<GameObject>)>();

        public int WidthSprites => Texture.width / SpriteDensity;
        public int HeightSprites => Texture.height / SpriteDensity;

        public void Prepare()
        {
            TextureMap = new bool[SpriteDensity * SpriteDensity];

            for (int d = 0; d < DefaultSprites.Length; d++)
                Draw(DefaultSprites[d], d, d);
        }
        public void ReserveKey(int key) => ReservedKeys.Add(key);
        public void Draw(Texture2D smile, int hash, int id = -1)
        {
            if (IsKeyReserved(hash))
                UnreserveKey(hash);

            if (id < 0)
            {
                var got = false;
                for (int t = DefaultSprites.Length; t < TextureMap.Length; t++)
                    if (!TextureMap[t])
                    {
                        got = TextureMap[t] = true;
                        id = t;

                        break;
                    }

                if (!got)
                    id = UnityEngine.Random.Range(DefaultSprites.Length, SpriteDensity * SpriteDensity);
            }

            HashSprite[hash] = (id, new List<GameObject>());

            smile = DataUtil.ResizeBilinear(smile, WidthSprites, HeightSprites);
            var x = WidthSprites * (id % SpriteDensity);
            var y = Texture.height - HeightSprites * (id / SpriteDensity + 1);

            Texture.SetPixels32(x, y, WidthSprites, HeightSprites, smile.GetPixels32());
            Texture.Apply();
            Asset.UpdateLookupTables();
        }
        public void RemoveRange(List<int> toRemove, GameObject holder)
        {
            for (int s = 0; s < toRemove.Count; s++)
            {
                var key = toRemove[s];
                if (IsNonDefKey(key))
                {
                    var smile = HashSprite[key];
                    if (smile.Item2.Contains(holder))
                    {
                        smile.Item2.Remove(holder);

                        if (smile.Item2.Count == 0)
                        {
                            TextureMap[smile.Item1] = false;

                            HashSprite.Remove(key);
                        }
                        else
                            HashSprite[key] = smile;
                    }
                }
            }
        }
        public int GetSpriteID(int key, GameObject requester = null)
        {
            var smile = HashSprite[key];

            if (IsNonDefKey(key) &&
                 requester &&
                !smile.Item2.Contains(requester))
            {
                smile.Item2.Add(requester);

                HashSprite[key] = smile;
            }

            return smile.Item1;
        }
        public bool HasSprite(int hash) => HashSprite.ContainsKey(hash);
        public bool IsKeyReserved(int key) => ReservedKeys.Contains(key);

        bool IsNonDefKey(int key) => key < 0 || key >= DefaultSprites.Length;
        void UnreserveKey(int key) => ReservedKeys.Remove(key);
    }
    #endregion
}