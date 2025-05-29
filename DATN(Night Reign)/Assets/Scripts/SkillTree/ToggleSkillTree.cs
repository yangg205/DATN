using UnityEngine;

public class ToggleSkillTree : MonoBehaviour
{
    public GameObject skillTreeUI;
    void Start()
    {
        skillTreeUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        ToggleSkill();
    }
    private void ToggleSkill()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            skillTreeUI.SetActive(!skillTreeUI.activeSelf);
        }
    }
}
