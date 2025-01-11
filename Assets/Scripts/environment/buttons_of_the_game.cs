using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class buttons_of_the_game : MonoBehaviour
{
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void Resume(GameObject myObject)
    {
        Time.timeScale = 1f;
        myObject.SetActive(!myObject.activeSelf);


    }
    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    public void NextLevel()
    {   
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }


}
