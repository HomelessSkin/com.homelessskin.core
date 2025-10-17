#if UNITY_EDITOR
using Core.Util;

using UnityEditor;

using UnityEngine;
using UnityEngine.Rendering;

namespace Core.Rendering
{
    [CustomEditor(typeof(RenderableObject))]
    public class RenderableObjectEditor : KeyScriptableEditor
    {
        float Zoom = 1f;
        Vector2 Dir = new Vector2(120f, -20f);

        Material TargetMaterial;
        Material DefaultMaterial;
        Material RenderMaterial;

        PreviewRenderUtility RenderUtility;
        RenderableObject Target => (RenderableObject)target;

        void OnEnable()
        {
            RenderUtility = new PreviewRenderUtility();

            RenderUtility.camera.fieldOfView = 30f;
            RenderUtility.camera.nearClipPlane = 0.01f;
            RenderUtility.camera.farClipPlane = 1000f;
            RenderUtility.camera.backgroundColor = Color.gray * 0.2f;
            RenderUtility.camera.clearFlags = CameraClearFlags.SolidColor;

            RenderUtility.lights[0].intensity = 1f;
            RenderUtility.lights[0].transform.rotation = Quaternion.Euler(30f, 30f, 0f);

            RenderMaterial = DefaultMaterial = GraphicsSettings.currentRenderPipeline.defaultMaterial;
            if (Target.Material)
                RenderMaterial = TargetMaterial = Target.Material;
        }
        void OnDisable()
        {
            if (RenderUtility != null)
            {
                RenderUtility.Cleanup();
                RenderUtility = null;
            }
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (!Target.Mesh)
            {
                EditorGUI.DropShadowLabel(r, "No Mesh Assigned");

                return;
            }

            if (Event.current.type == EventType.Repaint)
            {
                RenderUtility.BeginPreview(r, background);
                DrawPreview();

                var result = RenderUtility.EndPreview();
                GUI.DrawTexture(r, result, ScaleMode.StretchToFill, false);
            }

            HandlePreviewInput(r);
        }
        public override void OnPreviewSettings()
        {
            if (GUILayout.Button("Reset View", EditorStyles.miniButton))
            {
                Zoom = 1f;
                Dir = new Vector2(120f, -20f);
            }

            if (TargetMaterial && GUILayout.Button("Switch Material", EditorStyles.miniButton))
                RenderMaterial = RenderMaterial == TargetMaterial ? DefaultMaterial : TargetMaterial;
        }

        public override bool HasPreviewGUI() => true;
        public override GUIContent GetPreviewTitle() => new GUIContent("Mesh Preview");

        void DrawPreview()
        {
            var rotation = Quaternion.Euler(Dir.y, Dir.x, 0f);

            var bounds = Target.Mesh.bounds;
            var size = bounds.size.magnitude;
            if (size < 0.1f)
                size = 1f;

            var distance = size * 3f * Zoom;
            var cameraPosition = rotation * new Vector3(0, 0, -distance);
            var center = bounds.center;

            RenderUtility.camera.transform.position = center + cameraPosition;
            RenderUtility.camera.transform.LookAt(center);

            RenderUtility.DrawMesh(Target.Mesh, Vector3.zero, Quaternion.identity, RenderMaterial, 0);
            RenderUtility.camera.Render();
        }
        void HandlePreviewInput(Rect r)
        {
            var e = Event.current;

            if (r.Contains(e.mousePosition))
            {
                switch (e.type)
                {
                    case EventType.MouseDrag:
                    if (e.button == 0)
                    {
                        Dir.x += e.delta.x * 0.5f;
                        Dir.y += e.delta.y * 0.5f;
                        Dir.y = Mathf.Clamp(Dir.y, -89f, 89f);

                        e.Use();
                    }
                    else if (e.button == 2)
                        e.Use();
                    break;

                    case EventType.ScrollWheel:
                    Zoom *= 1f + e.delta.y * 0.01f;
                    Zoom = Mathf.Clamp(Zoom, 0.1f, 10f);

                    e.Use();
                    break;
                }
            }
        }
    }
}
#endif