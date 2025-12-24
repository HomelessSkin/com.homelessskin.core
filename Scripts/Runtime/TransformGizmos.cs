#if UNITY_EDITOR
using UnityEngine;

namespace Game.Test
{
    public class TransformGizmos : MonoBehaviour
    {
        [SerializeField] float SphereRadius;
        [SerializeField] Color SphereColor;
        [SerializeField] Transform[] Transforms;

        void OnDrawGizmos()
        {
            Gizmos.color = SphereColor;
            if (Transforms != null)
                for (int t = 0; t < Transforms.Length; t++)
                    Gizmos.DrawWireSphere(Transforms[t].position, SphereRadius);
        }
    }
}
#endif