using UnityEngine;

public class BossCombat : MonoBehaviour
{
    public void AttackNormal()
    {
        Debug.Log("[BossCombat] Normal melee attack triggered.");
        // Trigger animation/mechanic
    }

    public void CastSpell()
    {
        Debug.Log("[BossCombat] Spell cast triggered.");
        // Instantiate spell prefab / play effect
    }

    public void Evade()
    {
        Debug.Log("[BossCombat] Evade triggered.");
        // Add dash or animation
    }
}
