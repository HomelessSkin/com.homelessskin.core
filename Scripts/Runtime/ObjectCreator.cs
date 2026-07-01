#if UNITY_EDITOR
using Unity.Collections;
using Unity.Mathematics;
using Unity.Scenes;

using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;
using UnityEngine.SceneManagement;

using static Unity.Mathematics.math;

using Random = Unity.Mathematics.Random;

namespace Core
{
    public class ObjectCreator : MonoBehaviour
    {
        [SerializeField] int GridHorizontalBorder = 3;
        [SerializeField] int GridVerticalBorder = 3;
        [SerializeField] int GridHorizontalOffset = 0;
        [SerializeField] int GridVerticalOffset = 0;
        [SerializeField] Poisson Poisson;
        [SerializeField] SubScene Scene;
        [SerializeField] int ParentIndex;
        [SerializeField] GameObject Prefab;

        [Space]
        [SerializeField] Bounds[] Obstacles;

        [Space]
        [SerializeField] Mesh Preview;
        [SerializeField] NativeArray<float3> Grid;

        public void SetSeed(uint seed) => Poisson.Seed = seed;
        public void CreateGridus()
        {
            var parent = Scene.EditingScene.GetRootGameObjects()[ParentIndex].transform;

            var cx = Poisson.GridWidth / Poisson.CellWidth + GridHorizontalBorder;
            var cz = Poisson.GridHeight / Poisson.CellWidth + GridVerticalBorder;
            var half = Poisson.CellWidth / 2f;
            var settings = new ConvertToPrefabInstanceSettings
            {

            };

            for (int x = -GridHorizontalBorder; x < cx; x++)
                for (int z = -GridVerticalBorder; z < cz; z++)
                {
                    var newGO = Instantiate(Prefab);

                    SceneManager.MoveGameObjectToScene(newGO, Scene.EditingScene);
                    PrefabUtility.ConvertToPrefabInstance(newGO, Prefab, settings, InteractionMode.AutomatedAction);

                    newGO.transform.SetParent(parent, false);
                    newGO.transform.localPosition =
                        new Vector3
                        (
                            Poisson.CellWidth * x + half + x * GridHorizontalOffset,
                            0f,
                            Poisson.CellWidth * z + half + z * GridVerticalOffset
                        );
                }

            EditorSceneManager.MarkSceneDirty(Scene.EditingScene);
        }
        public void CalculatePoisson()
        {
            Poisson.Seed = new Random(Poisson.Seed).NextUInt();

            if (Grid.IsCreated)
                Grid.Dispose();
            Grid = Poisson.GetGrid_Job(Obstacles);
        }
        public void Spawn()
        {
            if (!Grid.IsCreated)
                return;

            var parent = Scene.EditingScene.GetRootGameObjects()[ParentIndex].transform;
            var settings = new ConvertToPrefabInstanceSettings
            {

            };

            for (int g = 0; g < Grid.Length; g++)
            {
                var pos = Grid[g];
                if (length(pos) > 0f)
                {
                    var newGO = Instantiate(Prefab);

                    SceneManager.MoveGameObjectToScene(newGO, Scene.EditingScene);
                    PrefabUtility.ConvertToPrefabInstance(newGO, Prefab, settings, InteractionMode.AutomatedAction);

                    newGO.transform.SetParent(parent, false);
                    newGO.transform.localPosition = pos;
                }
            }

            Grid.Dispose();

            EditorSceneManager.MarkSceneDirty(Scene.EditingScene);
        }
        public void DestroyChildren()
        {
            var parent = Scene.EditingScene.GetRootGameObjects()[ParentIndex].transform;
            var children = parent.GetComponentsInChildren<Transform>();
            for (int c = 0; c < children.Length; c++)
            {
                if (children[c] == parent)
                    continue;

                DestroyImmediate(children[c].gameObject);
            }
        }

        void OnDrawGizmos()
        {
            if (!Preview || !Grid.IsCreated)
                return;

            var a = 0;
            for (int g = 0; g < Grid.Length; g++)
            {
                var pos = Grid[g];
                if (length(pos) > 0f)
                {
                    a++;

                    Gizmos.DrawWireMesh(Preview, 0, pos);
                }
            }

            Handles.Label(transform.position, $"{a}");
        }
        void OnDestroy()
        {
            if (Grid.IsCreated)
                Grid.Dispose();
        }
    }
}
#endif