using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    public float moveUpSpeed = 1f;
    public float fadeOutSpeed = 2f;
    private TextMesh textMesh;
    private Color textColor;
    private float lifetime = 1.5f;
    private float timer;

    void Awake()
    {
        textMesh = GetComponent<TextMesh>();
    }

    public void Setup(float damageAmount)
    {
        textMesh.text = damageAmount.ToString();
        textColor = textMesh.color;
        textColor.a = 1f;
        textMesh.color = textColor;
        timer = 0f;
        gameObject.SetActive(true);
    }

    void LateUpdate()
    {
        transform.LookAt(Camera.main.transform);
        transform.Rotate(0, 180, 0);
    }

    void Update()
    {
        transform.position += Vector3.up * moveUpSpeed * Time.deltaTime;

        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, timer / lifetime);
        textColor.a = alpha;
        textMesh.color = textColor;

        if (timer >= lifetime)
        {
            DamagePopupPool.Instance.ReturnToPool(this);
        }
    }
}
