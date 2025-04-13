using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NewMonoBehaviourScript : SimulationBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log(Runner);
            Runner.LoadScene(SceneRef.FromIndex(1), LoadSceneMode.Single); // Scene 2   
        }
    }
}
