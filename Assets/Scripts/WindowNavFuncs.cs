using UnityEngine;

/*
 * Handles navigation functionality related to the content window, including the close button and back button.
 */ 
public class WindowNavFuncs : MonoBehaviour
{
    public static GameMaster gm;
    void Awake()
    {
        if (gm == null)
        {
            gm = GameObject.Find("GameMaster").GetComponent<GameMaster>();
        }
    }
    private void OnMouseUpAsButton()
    {
        var curFolder = gm.textWindow.GetComponent<ContentWindowFunctions>().currentlyOpenedFolder;

        if (gm.textWindow.GetComponent<ContentWindowFunctions>().activeFile.mediaType == "video")
        {
            gm.textWindow.GetComponent<ContentWindowFunctions>().closingVidBeforeCompletion = true;
        }
        gm.textWindow.GetComponent<ContentWindowFunctions>().readyToClose = true;
        gm.textWindow.GetComponent<ContentWindowFunctions>().CleanupBeforeClose();


        if (gm.manuallyLoggingIntoNGAPI == true)
        {
            // user said "no" to logging into NG API at start of game, need to ensure they see instructions
            SceneInitFuncs.InstructionsInit();
            

            if (GameObject.Find("confirmLogin"))
            { 
                // user can close out window during prompt, want to account for that as well
                MonoBehaviour.Destroy(GameObject.Find("confirmLogin"));
                MonoBehaviour.Destroy(GameObject.Find("denyLogin"));
            }
            gm.manuallyLoggingIntoNGAPI = false;

        }
        // Back button navigation is so similar to close button, code is shared between the two
        if (gameObject.name.Contains("backToFolderBtn"))
        {
           gm.OpenWindow(gm.textWindow.GetComponent<ContentWindowFunctions>().currentlyOpenedFolder);
        } else
        {
            // Checking if folder is empty after closing; if so, swapping to "empty folder" icon for the folder
            if (curFolder)
            {
                if (curFolder.objectsInFolder.Count == 0 && curFolder.title != "Recycle Bin")
                {
                    curFolder.GetComponent<SpriteRenderer>().sprite = gm.IconMasterScript.folderIcon;
                }
            }
        }        
    }
}
