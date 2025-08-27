using UnityEngine;

public class SnowstormController : MonoBehaviour
{
    [Header("Snowstorm Settings")]
    public float stormDuration = 10f;   // Thời gian có bão
    public float calmDuration = 20f;     // Thời gian yên tĩnh
    public float fadeTime = 2f;         // Thời gian chuyển đổi mượt

    private ParticleSystem[] allSnowParticles;
    private bool isStorming = true;
    private float timer = 0f;

    void Start()
    {
        // Tìm tất cả particle có tag "Snowstorm"
        GameObject[] snowObjects = GameObject.FindGameObjectsWithTag("Snowstorm");
        allSnowParticles = new ParticleSystem[snowObjects.Length];

        for (int i = 0; i < snowObjects.Length; i++)
        {
            allSnowParticles[i] = snowObjects[i].GetComponent<ParticleSystem>();
            if (allSnowParticles[i] != null)
                allSnowParticles[i].Play();
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (isStorming && timer >= stormDuration)
        {
            StartCoroutine(FadeSnow(false)); // tắt bão
            isStorming = false;
            timer = 0f;
        }
        else if (!isStorming && timer >= calmDuration)
        {
            StartCoroutine(FadeSnow(true)); // bật bão
            isStorming = true;
            timer = 0f;
        }
    }

    private System.Collections.IEnumerator FadeSnow(bool turnOn)
    {
        foreach (var ps in allSnowParticles)
        {
            if (ps == null) continue;

            var emission = ps.emission;
            float startRate = emission.rateOverTime.constant;
            float targetRate = turnOn ? 50f : 0f; // tuỳ chỉnh mật độ tuyết
            float t = 0f;

            while (t < fadeTime)
            {
                t += Time.deltaTime;
                float newRate = Mathf.Lerp(startRate, targetRate, t / fadeTime);
                emission.rateOverTime = newRate;
                yield return null;
            }

            emission.rateOverTime = targetRate;
        }
    }
}
