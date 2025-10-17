using UnityEngine;

namespace Core.Rendering
{
    [CreateAssetMenu(fileName = "_Variant", menuName = "Presentation/AnimationVariant")]
    public class AnimationVariant : ScriptableObject
    {
        public int ID;

        public Frame[] Frames;

        [System.Serializable]
        public struct Frame
        {
            public float Length;
            public RenderableObject Renderable;
        }
    }
}