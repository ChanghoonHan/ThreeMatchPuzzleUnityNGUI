using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeButton : MonoBehaviour
{
    [Header("Set in Inspector")]
    public string ChangeSceneName;

    [Header("Set Dynamically")]
    private UIButton thisUIButton;

    // Start is called before the first frame update
    void Start()
    {
        thisUIButton = GetComponent<UIButton>();
        thisUIButton.onClick.Add(new EventDelegate(ChangeScene));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ChangeScene()
    {
        if (ChangeSceneName == "Exit")
        {
            Application.Quit();

            return;
        }

        SceneManager.LoadScene(ChangeSceneName);
    }
}
