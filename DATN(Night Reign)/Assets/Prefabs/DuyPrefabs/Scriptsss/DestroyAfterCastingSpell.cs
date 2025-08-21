using UnityEngine;

namespace AG
{
    public class DestroyAfterCastingSpell : MonoBehaviour
    {
        CharacterManager characterCastingSpell;
        private void Awake()
        {
            characterCastingSpell = GetComponentInParent<CharacterManager>();
        }

        private void Update()
        {
            if(characterCastingSpell.isFiringSpell)
            {
                Destroy(gameObject);
            }
        }
    }
}

