using UnityEngine;

/*
 * Handles functionality for the forward/back buttons for multi-paged Text Files.
 */ 

public class TextPageNav : MonoBehaviour
{
    public static GameMaster gm;
    private static bool canChangePage;
    void Awake()
    {
        if (gm == null)
        {
            gm = GameObject.Find("GameMaster").GetComponent<GameMaster>();
        }
    }

    private void OnMouseDown()
    {
        canChangePage = true; // toggle to prevent the function from running multiple times per button press
    }

    private void OnMouseUpAsButton()
    {
        ContentWindowFunctions contentWindowFuncs = gm.textWindow.GetComponent<ContentWindowFunctions>();
        contentWindowFuncs.StopAllCoroutines(); // to stop co-routine if line is currently in processing of being typed out

        if (gameObject.name.Contains("txtNavBack"))
        {
            if (contentWindowFuncs.fullPgIndex > 0 && canChangePage)
            {
                canChangePage = false;
                gm.textWindow.GetComponent<ContentWindowFunctions>().fullPgIndex =
                                gm.textWindow.GetComponent<ContentWindowFunctions>().fullPgIndex - 1;
                gm.textWindow.GetComponent<ContentWindowFunctions>().WriteTextAlreadyRead();
            }
            
        }
        else if (gameObject.name.Contains("txtNavForward"))
        {
            canChangePage = false;
            gm.textWindow.GetComponent<ContentWindowFunctions>().fullPgIndex++;
            gm.textWindow.GetComponent<ContentWindowFunctions>().WriteTextAlreadyRead();
        }
    }
}
