using UnityEngine;

public class UIButtonLocalizer : MonoBehaviour
{
    [Header("Button cần dịch")]
    public GameObject buttonContinue;
    public GameObject buttonNewgame;
    public GameObject buttonRank;
    public GameObject buttonSettings;
    public GameObject buttonExit;

    private void Start()
    {
        buttonContinue.SetLocalizationKey("btn_continue");
        buttonNewgame.SetLocalizationKey("btn_newgame");
        buttonRank.SetLocalizationKey("btn_rank");
        buttonSettings.SetLocalizationKey("btn_settings");
        buttonExit.SetLocalizationKey("btn_exit");
    }
}
