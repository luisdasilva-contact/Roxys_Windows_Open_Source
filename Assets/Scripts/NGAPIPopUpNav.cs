using UnityEngine;

/*
 * Manages functionality for the buttons that appear with the NG API login pop-up.
 */ 

public class NGAPIPopUpNav : MonoBehaviour
{
    
    public void OnMouseUpAsButton()
    {
        if (gameObject.name == "confirmLogin")
        {
            if (Screen.fullScreen)
            { // in very specific cases (i.e. itch.io on mobile), game will freeze if a new tab is opened, preventing the login signal from being received
                Screen.fullScreen = false;
            }

            if (!NGIO.loginPageOpen)
            {
                NGIO.OpenLoginPage();
            } else
            {
                UnityEngine.Debug.Log("Detected login page already open. Session check: " + NGIO.hasSession + ", user check: " + NGIO.hasUser + ", last connection check: " + NGIO.lastConnectionStatus);
            }
            
            GameObject.Find("contentBG").GetComponent<ContentWindowFunctions>().bodyComponent.text = "Logging in, hold on a sec...";
        }
        else if (gameObject.name == "denyLogin")
        {
            GameObject.Find("contentBG").GetComponent<ContentWindowFunctions>().bodyComponent.text = "No worries, you can just close this window and start the game!";
            Destroy(GameObject.Find("confirmLogin"));
            Destroy(GameObject.Find("denyLogin"));
        }
    }
}
