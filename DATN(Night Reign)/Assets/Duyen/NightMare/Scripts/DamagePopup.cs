using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    public float moveUpSpeed = 1f;
    public float fadeOutSpeed = 2f;
    private TextMesh textMesh;
    private Color textColor;

    void Awake()
    {
        textMesh = GetComponent<TextMesh>();
        textColor = textMesh.color;
    }

    public void Setup(int damageAmount)
    {
        textMesh.text = damageAmount.ToString();
    }
    void LateUpdate()
    {
        transform.LookAt(Camera.main.transform);
        transform.Rotate(0, 180, 0); // Quay lại mặt đúng hướng camera
    }

    void Update()
    {
        // Di chuyển lên
        transform.position += Vector3.up * moveUpSpeed * Time.deltaTime;

        // Mờ dần
        textColor.a -= fadeOutSpeed * Time.deltaTime;
        textMesh.color = textColor;

        if (textColor.a <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
