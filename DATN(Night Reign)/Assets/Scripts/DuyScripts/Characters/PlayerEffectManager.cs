using System.Collections;
using UnityEngine;

public class PlayerEffectManager : MonoBehaviour
{
    public WeaponFX rightWeaponFX;
    public WeaponFX leftWeaponFX;

    private GameObject currentChargeVFX;

    public virtual void PlayWeaponFX(bool isLeft)
    {
        Debug.Log($"[EffectManager] → PlayWeaponFX (isLeft: {isLeft})");

        if (!isLeft)
        {
            if (rightWeaponFX != null)
            {
                rightWeaponFX.PlayWeaponFX();
            }
            else
            {
                Debug.LogWarning("[EffectManager] → rightWeaponFX is NULL!");
            }
        }
        else
        {
            if (leftWeaponFX != null)
            {
                leftWeaponFX.PlayWeaponFX();
            }
            else
            {
                Debug.LogWarning("[EffectManager] → leftWeaponFX is NULL!");
            }
        }
    }

    public virtual void StopWeaponFX(bool isLeft)
    {
        Debug.Log($"[EffectManager] → StopWeaponFX (isLeft: {isLeft})");

        if (!isLeft)
        {
            if (rightWeaponFX != null)
            {
                StartCoroutine(DelayedStop(rightWeaponFX));
            }
        }
        else
        {
            if (leftWeaponFX != null)
            {
                StartCoroutine(DelayedStop(leftWeaponFX));
            }
        }
    }

    private IEnumerator DelayedStop(WeaponFX fx)
    {
        yield return new WaitForSeconds(0.15f); // delay để trail nhìn mượt hơn khi combo
        fx.StopWeaponFX();
    }

    public void PlayChargeVFX(GameObject vfxPrefab, Transform attachPoint)
    {
        if (vfxPrefab == null || attachPoint == null) return;

        // Xóa cái cũ nếu còn
        if (currentChargeVFX != null)
            Destroy(currentChargeVFX);

        // Tạo mới
        currentChargeVFX = Instantiate(vfxPrefab, attachPoint.position, attachPoint.rotation, attachPoint);
    }

    public void StopChargeVFX()
    {
        if (currentChargeVFX != null)
        {
            Destroy(currentChargeVFX);
            currentChargeVFX = null;
        }
    }
}


