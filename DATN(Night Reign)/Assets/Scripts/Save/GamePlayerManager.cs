using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int playerCharacterId { get; set; }
    public int playerId { get; set; }
    public int characterId { get; set; }
    public Vector3 PlayerPosition { get; set; }
    public int PlayerHP { get; set; }
    public int PlayerEXP { get; set; }
    public int PlayerLevel { get; set; }
    public int totalPoint { get; set; }
    public int totalCoin { get; set; }
    public int skillPoint { get; set; }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
