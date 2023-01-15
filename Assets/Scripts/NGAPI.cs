using NewgroundsIO.objects;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking.Types;

/*
 * Note: Much of this code is derived directly from the NG API, available here: https://github.com/PsychoGoldfishNG/NewgroundsIO-Unity/
 */

public static class NGAPI
{
    private static readonly string AppID = "Your NG App ID here!";
    private static readonly string AesKey = "Your NG AesKey here!";
    public static int finishGameMedalID = 1;
    public static int ViewAllFilesMedalID = 1;
    public static int carlProblemMedalID = 1;
    public static bool userDeniedLogin = false;

    public static void initNGAPI()
    {
        Dictionary<string, object> options = new() {
        { "version", "1.0.0" },
            { "preloadMedals", true }
        };

        NGIO.Init(AppID, AesKey, options);        
    }

    public static void OnConnectionStatusChanged(string status)
    {
        // copied directly from NG API docs 
        UnityEngine.Debug.Log("NG API status: " + status);
        switch (status)
        {
            case NGIO.STATUS_LOGIN_REQUIRED:
                /**
                    * We have a valid session ID, but the player isn't logged in.
                    * Show a 'Log In' button, and a message about how the player
                    * needs to sign in to use certain features.
                    *
                    * The 'Log In' button should call NGIO.OpenLoginPage();
                    *
                    * It is also good practice to provide a 'No Thanks' button
                    * for players who don't want to sign in.
                    *
                    * The 'No Thanks' button should call NGIO.SkipLogin();
                    */
                UnityEngine.Debug.Log("login status required");
                break;

            case NGIO.STATUS_READY:

                /**
                    * The user has either logged in (or declined to do so), and everything else 
                    * has finished preloading.
                    */
                UnityEngine.Debug.Log("NG API ready");

                if (NGIO.hasUser)
                {
                    /**
                        * The user is signed in!
                        * If they selected the 'remember me' option, their session id will be saved automatically!
                        * 
                        * Show a friendly welcome message! You can get their user name via:
                        *   NGIO.user.name
                        */
                    UnityEngine.Debug.Log("User found");
                    if (GameObject.Find("GameMaster").GetComponent<GameMaster>().manuallyLoggingIntoNGAPI)
                    {
                        GameObject.Find("contentBG").GetComponent<ContentWindowFunctions>().titleComponent.text = "Hey there, " + NGIO.user.name + "!";
                        GameObject.Find("contentBG").GetComponent<ContentWindowFunctions>().bodyComponent.text = "You're all set! You can close this window and start the game!";
                        MonoBehaviour.Destroy(GameObject.Find("confirmLogin"));
                        MonoBehaviour.Destroy(GameObject.Find("denyLogin"));
                    }
                }
                break;

            case NGIO.STATUS_LOGIN_SUCCESSFUL:
                UnityEngine.Debug.Log("login success");
                if (GameObject.Find("GameMaster"))
                {
                    if (GameObject.Find("GameMaster").GetComponent<GameMaster>().manuallyLoggingIntoNGAPI)
                    {
                        GameObject.Find("contentBG").GetComponent<ContentWindowFunctions>().titleComponent.text = "Hey there, " + NGIO.user.name + "!";
                        GameObject.Find("contentBG").GetComponent<ContentWindowFunctions>().bodyComponent.text = "You're all set! You can close this window and start the game!";
                        MonoBehaviour.Destroy(GameObject.Find("confirmLogin"));
                        MonoBehaviour.Destroy(GameObject.Find("denyLogin"));
                    }
                }
                    
                break;


            case NGIO.STATUS_LOGIN_CANCELLED:
                userDeniedLogin = true;
                GameObject.Find("contentBG").GetComponent<ContentWindowFunctions>().bodyComponent.text = "Changed your mind? No worries, you can just close this window and start the game!";
                MonoBehaviour.Destroy(GameObject.Find("confirmLogin"));
                MonoBehaviour.Destroy(GameObject.Find("denyLogin"));
                break;
        }        
    }

    public static void OnMedalUnlocked(NewgroundsIO.objects.Medal medal)
    {
        /**
     * Show a medal popup.  You can get medal information like so:
     *   medal.id
     *   medal.name
     *   medal.description
     *   medal.value
     *   medal.icon  (note, these are usually .webp files, and may not work in Unity)
     */
        UnityEngine.Debug.Log("Unlocking medal w this name: " + medal.name);
    }
}
