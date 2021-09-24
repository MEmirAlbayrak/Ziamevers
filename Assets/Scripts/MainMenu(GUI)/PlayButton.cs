using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour
{
    public Canvas cv;
    public Canvas settingsCanvas;
    void Start()
    {
        cv = GetComponent<Canvas>();
        settingsCanvas = GameObject.Find("SettingCanvas").GetComponent<Canvas>();
        settingsCanvas.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void playButton()
    {
        SceneManager.LoadScene(1);
    }
    public void settingsButton()
    {
        cv.enabled = false;
        settingsCanvas.enabled = true;
    }
    public void BackButton()
    {
        settingsCanvas.enabled = false;
        cv.enabled = true;
    }
    public void exit()
    {
        Application.Quit();
    }
}
