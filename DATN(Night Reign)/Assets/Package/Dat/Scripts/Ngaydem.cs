using UnityEngine;

public class Ngaydem : MonoBehaviour
{
    [Header("Time Settings")]
    public float dayDurationInSeconds = 120f; // Thời gian 1 vòng ngày đêm
    private float timePercent; // 0 -> 1

    [Header("Sun Settings")]
    public Light sunLight;
    public Gradient sunColorOverTime;
    public AnimationCurve sunIntensityOverTime;

    [Header("Skybox Settings")]
    public Material proceduralSkybox;
    public Gradient skyTintOverTime;
    public Gradient groundColorOverTime;
    public Gradient fogColorOverTime;

    void Update()
    {
        timePercent += Time.deltaTime / dayDurationInSeconds;
        if (timePercent >= 1f)
            timePercent = 0f;

        UpdateSun();
        UpdateSkybox();
    }

    void UpdateSun()
    {
        if (sunLight == null) return;

        float sunAngle = timePercent * 360f - 90f;
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        sunLight.color = sunColorOverTime.Evaluate(timePercent);
        sunLight.intensity = sunIntensityOverTime.Evaluate(timePercent);
    }

    void UpdateSkybox()
    {
        if (proceduralSkybox == null) return;

        RenderSettings.skybox = proceduralSkybox;

        proceduralSkybox.SetColor("_SkyTint", skyTintOverTime.Evaluate(timePercent));
        proceduralSkybox.SetColor("_GroundColor", groundColorOverTime.Evaluate(timePercent));
        RenderSettings.fogColor = fogColorOverTime.Evaluate(timePercent);
    }
}
