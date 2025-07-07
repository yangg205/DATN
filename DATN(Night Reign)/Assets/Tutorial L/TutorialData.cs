using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Video;

[CreateAssetMenu(fileName = "NewTutorial", menuName = "Tutorial/Tutorial Data")]
public class TutorialData : ScriptableObject
{
    [TextArea]
    public string tutorialText;

    public Sprite tutorialSprite;
    public VideoClip tutorialVideo;

    [Header("Icon UI kéo tay (Scene Object)")]
    public List<GameObject> manualIconObjects = new List<GameObject>();

    public bool onlyTriggerOnce = true;
}
