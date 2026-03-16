#if UNITY_EDITOR
using Unity.Scenes;

using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;
using UnityEngine.SceneManagement;

using static Unity.Mathematics.math;

namespace Core
{
    public class ObjectCreator : MonoBehaviour
    {
        [SerializeField] int GridHorizontalBorder = 3;
        [SerializeField] int GridVerticalBorder = 3;
        [SerializeField] Poisson Poisson;
        [SerializeField] SubScene Scene;
        [SerializeField] int ParentIndex;
        [SerializeField] GameObject Prefab;
        [Space]
        [SerializeField] Bounds[] Obstacles;

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
                    newGO.transform.localPosition = new Vector3(Poisson.CellWidth * x + half, 0f, Poisson.CellWidth * z + half);
                }

            EditorSceneManager.MarkSceneDirty(Scene.EditingScene);
        }
        public void CreatePoissonus()
        {
            var parent = Scene.EditingScene.GetRootGameObjects()[ParentIndex].transform;

            var grid = Poisson.GetGrid_Job(Obstacles);
            for (int g = 0; g < grid.Length; g++)
            {
                var pos = grid[g];
                if (length(pos) > 0f)
                {
                    var newGO = Instantiate(Prefab);

                    SceneManager.MoveGameObjectToScene(newGO, Scene.EditingScene);

                    newGO.transform.SetParent(parent, false);
                    newGO.transform.localPosition = pos;
                }
            }
            grid.Dispose();

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
    }
}
#endif