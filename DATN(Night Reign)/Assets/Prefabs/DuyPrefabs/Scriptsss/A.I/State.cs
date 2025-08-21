using UnityEngine;

namespace AG
{
    public abstract class State : MonoBehaviour
    {
        public abstract State Tick(EnemyManager enemyManager, EnemyStats enemyStats, EnemyAnimatorManager enemyAnimatorManager);
    }
}

