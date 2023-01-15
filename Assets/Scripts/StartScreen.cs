using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * Handles functionality for the game's title screen.
 */ 
public class StartScreen : MonoBehaviour
{
    /*
     * Sets fullscreen on mobile platforms as soon as the game begins.
     */ 
    private void OnMouseUpAsButton()
    {
        if (Application.isMobilePlatform)
        {
            Screen.fullScreen = !Screen.fullScreen;            
        }

        StartCoroutine(GoToIntroCutscene());        
    }

    /*
     * Starts the first cutscene. Used as IEnumerator so the program must wait 1 frame before starting the scene, 
      because Screen.fullScreen does not take effect until the end of the frame it's called on.
     */
    IEnumerator GoToIntroCutscene()
    {
        yield return new WaitForEndOfFrame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        
    }
}
