using UnityEngine;

namespace AG
{
    public class DestroyAfterCastingSpell : MonoBehaviour
    {
        public float lifeTime = 3f; // tồn tại 5 giây

        private void Start()
        {
            Destroy(gameObject, lifeTime);
        }
    }
}

