using UnityEngine;

public class RainLoopController : MonoBehaviour
{
    public float rainDuration = 10f;
    public float clearDuration = 5f;

    private ParticleSystem[] allRainParticles;
    private bool isRaining = true;
    private float timer = 0f;

    void Start()
    {
        // Tìm tất cả ParticleSystem có tag "Rain"
        GameObject[] rainObjects = GameObject.FindGameObjectsWithTag("Rain");
        allRainParticles = new ParticleSystem[rainObjects.Length];

        for (int i = 0; i < rainObjects.Length; i++)
        {
            allRainParticles[i] = rainObjects[i].GetComponent<ParticleSystem>();
            if (allRainParticles[i] != null)
                allRainParticles[i].Play();
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (isRaining && timer >= rainDuration)
        {
            foreach (var ps in allRainParticles)
            {
                if (ps != null) ps.Stop();
            }
            isRaining = false;
            timer = 0f;
        }
        else if (!isRaining && timer >= clearDuration)
        {
            foreach (var ps in allRainParticles)
            {
                if (ps != null) ps.Play();
            }
            isRaining = true;
            timer = 0f;
        }
    }
}
