using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using ND;

public class AutoOutline : MonoBehaviour
{
    [SerializeField] int characterId;
    private Outline outline;
    private bool isSelected = false;
    [SerializeField] bool status= false;
    SignalRClient signalRClient;
    int playerId = PlayerPrefs.GetInt("PlayerId", 0);
    
    PlayerStats playerStats;

    private void Start()
    {
        signalRClient = FindAnyObjectByType<SignalRClient>();
        outline = GetComponent<Outline>();    
    }

    private void Update()
    {
        //if (status == true)
        //{
        //    if (outline != null)
        //    {
        //        outline.enabled = true; // Hiển thị sẵn Outline
        //        outline.effectColor = Color.yellow; // Có thể đổi màu nếu muốn khác lúc hover
        //    }
        //}
        SelectedCharacter();
    }
    private async void SelectedCharacter()
    {
        var result = await signalRClient.SelectedCharacter(playerId, characterId);
        if (result.status)
        {
            if (outline != null)
            {
                outline.enabled = true; // Hiển thị sẵn Outline
                outline.effectColor = Color.yellow; // Có thể đổi màu nếu muốn khác lúc hover
            }
        }
        else
        {
            outline.enabled = false; // Hiển thị sẵn Outline
            outline.effectColor = Color.yellow; // Có thể đổi màu nếu muốn khác lúc hover
        }
        
    }
    public void Deselect()
    {
        isSelected = false;
        if (outline != null)
        {
            outline.effectColor = Color.yellow; // Quay về màu hover ban đầu
        }
    }
    public async void Continue()
    {
        var result = await signalRClient.ContinueGame(playerId,characterId);
        if (result.status)
        {

        }
    } 
}
