using UnityEngine;

namespace Core
{
    public class SelfDestruct : MonoBehaviour
    {
        [SerializeField] Destroyable Destroyable;

        void Update()
        {
            switch (Destroyable.DestroyBy)
            {
                case Destroyable.Type.Timer:
                Destroyable.Value += Time.deltaTime;
                break;

                case Destroyable.Type.Y:
                Destroyable.Value = transform.position.y;
                break;
            }

            if (Destroyable.IsDone())
                Destroy(gameObject);
        }
    }
}