using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Audio;
public class Settingsmenu : MonoBehaviour
{

    public AudioMixer audioMixer;
    public void SetVolume (float volume)
    {
        Debug.Log(volume);
        audioMixer.SetFloat("volume", volume);
    }

    public void SetQuality (int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }
}
