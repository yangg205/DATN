using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class StatItemUI
{
    public Image icon;
    public TMP_Text valueText;
}

public class StatPanelController : MonoBehaviour
{
    public StatItemUI hpStat;
    public StatItemUI mpStat;
    public StatItemUI staStat;
    public StatItemUI damageStat;
    public StatItemUI atkSpeedStat;
    public StatItemUI critStat;
    public StatItemUI damageCritStat;
    public StatItemUI magicStat;
    public StatItemUI defenseStat;

    [Header("Icons")]
    public Sprite hpIcon;
    public Sprite mpIcon;
    public Sprite staIcon;
    public Sprite damageIcon;
    public Sprite atkSpeedIcon;
    public Sprite critIcon;
    public Sprite damageCritIcon;
    public Sprite magicIcon;
    public Sprite defenseIcon;

    void Start()
    {
        // Gán icon
        hpStat.icon.sprite = hpIcon;
        mpStat.icon.sprite = mpIcon;
        staStat.icon.sprite = staIcon;
        damageStat.icon.sprite = damageIcon;
        atkSpeedStat.icon.sprite = atkSpeedIcon;
        critStat.icon.sprite = critIcon;
        damageCritStat.icon.sprite = damageCritIcon;
        magicStat.icon.sprite = magicIcon;
        defenseStat.icon.sprite = defenseIcon;

        // Gán giá trị mẫu
        UpdateStats(100, 80, 60, 40, 1.2f, 15, 200, 50, 25);
    }

    public void UpdateStats(int hp, int mp, int sta, int damage, float atkSpeed, int crit, int damageCrit, int magic, int defense)
    {
        hpStat.valueText.text = hp.ToString();
        mpStat.valueText.text = mp.ToString();
        staStat.valueText.text = sta.ToString();
        damageStat.valueText.text = damage.ToString();
        atkSpeedStat.valueText.text = atkSpeed.ToString("0.0");
        critStat.valueText.text = crit + "%";
        damageCritStat.valueText.text = damageCrit.ToString();
        magicStat.valueText.text = magic.ToString();
        defenseStat.valueText.text = defense.ToString();
    }
}
