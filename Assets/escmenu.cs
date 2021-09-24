using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class escmenu : MonoBehaviour
{
    public Canvas Esc;

    public void play()
    {
        Esc.enabled = false;
        Time.timeScale = 1;
    }
    public void menu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void exit()
    {
        Application.Quit();
    }

}
