#if UNITY_EDITOR
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;
using UnityEngine.Rendering;

namespace Core.Rendering
{
    [InitializeOnLoad]
    public class RenderableObjectDragHandler
    {
        static RenderableObjectDragHandler()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
        }

        static void OnSceneGUI(SceneView sceneView) => HandleDragAndDrop(Event.current, sceneView);
        static void OnHierarchyGUI(int instanceID, Rect selectionRect) => HandleDragAndDrop(Event.current, null);
        static void HandleDragAndDrop(Event currentEvent, SceneView sceneView)
        {
            if (currentEvent.type != EventType.DragUpdated &&
                 currentEvent.type != EventType.DragPerform &&
                 currentEvent.type != EventType.DragExited)
                return;

            var assets = new List<RenderableObject>();
            foreach (var drag in DragAndDrop.objectReferences)
                if (drag is RenderableObject renderable)
                    assets.Add(renderable);

            if (assets.Count == 0)
                return;

            switch (currentEvent.type)
            {
                case EventType.DragUpdated:
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                currentEvent.Use();

                break;
                case EventType.DragPerform:
                DragAndDrop.AcceptDrag();

                currentEvent.Use();

                foreach (var renderable in assets)
                    Create(renderable);

                break;

                case EventType.DragExited:
                currentEvent.Use();

                break;
            }
        }
        static void Create(RenderableObject renderable)
        {
            if (!renderable.Mesh)
            {
                Debug.LogWarning($"RenderableObject {renderable.name} has no mesh assigned!");

                return;
            }

            var GO = new GameObject(renderable.name);
            GO.AddComponent<MeshFilter>().mesh = renderable.Mesh;

            if (renderable.DragMaterial)
                GO.AddComponent<MeshRenderer>().material = renderable.DragMaterial;
            else if (!renderable.Material)
                GO.AddComponent<MeshRenderer>().material = GraphicsSettings.currentRenderPipeline.defaultMaterial;
            else
                GO.AddComponent<MeshRenderer>().material = renderable.Material;

            Undo.RegisterCreatedObjectUndo(GO, "Create " + GO.name);

            Selection.activeGameObject = GO;
        }
    }
}
#endif