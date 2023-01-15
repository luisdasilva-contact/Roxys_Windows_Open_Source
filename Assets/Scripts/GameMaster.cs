using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Linq;
using NewgroundsIO.objects;

/*
 * Serves as a central hub for game logic during the main gameplay loop. 
 */ 
public class GameMaster : MonoBehaviour
{
    public static GameMaster gm;
    public GameObject audioMaster;
    public GameObject mediaPrefab;
    public GameObject IconMasterPrefab;
    public GameObject objHighlight;
    public GameObject imgCanv;
    public GameObject textWindow;
    public GameObject FullScreenButton;
    public GameObject muteAudio;
    public GameObject medalPrefab;
    public GameObject NGAPIButtonPrefab;

    [HideInInspector] public bool isFileBeingDragged;
    [HideInInspector] public SFX sfx;
    [HideInInspector] public GameObject draggedFile;
    [HideInInspector] public bool readyToDropFileIntoFolder;
    [HideInInspector] public GameObject folderToDropInto;
    [HideInInspector] public string openFileType;
    [HideInInspector] public bool isFolderOpen;    
    [HideInInspector] public IconMaster IconMasterScript;
    [HideInInspector] public int totalFilesRead;
    [HideInInspector] public List<GameObject> masterTransitionList;
    [HideInInspector] public bool inPostGame;
    [HideInInspector] public Vector3 curDragOffset;
    [HideInInspector] public float dragThreshold;
    [HideInInspector] public bool manuallyLoggingIntoNGAPI;
    [HideInInspector] public bool medalIsUnlocking; // to prevent the animation from being called every frame is medal unlocked from an update loop

    private int totalFiles;
    private const int FilesToViewToEndGame = 85;

    /*
     * Functions run when the gameplay loop begins, oriented around initializing the scene and variables.
     */ 
    void Start()
    {
        inPostGame = false;
        masterTransitionList = new List<GameObject>();
        imgCanv = GameObject.Find("imageCanvas");
        sfx = audioMaster.GetComponent<SFX>();
        GameObject vidRaw = GameObject.Find("vidRawImg");        
        vidRaw.GetComponent<RawImage>().enabled = false;
        draggedFile = null;
        IconMasterScript = IconMasterPrefab.GetComponent<IconMaster>();
        isFolderOpen = false;
        folderToDropInto = null;
        textWindow.SetActive(false);
        isFileBeingDragged = false;
        openFileType = "";
        SceneInitFuncs.CreateInitDesktopFiles();
        totalFiles = GetTotalNoOfFiles();
        totalFilesRead = 0;
        curDragOffset = new Vector3(0, 0, 0);
        dragThreshold = 0.04f;
        manuallyLoggingIntoNGAPI = false;
        medalIsUnlocking = false;
        

        if (NGIO.hasUser == false)
        {
            manuallyLoggingIntoNGAPI = true;
            SceneInitFuncs.NGAPIPopUp();
        } else
        {
            SceneInitFuncs.InstructionsInit();
        }
    }


    private void Update()
    {
        StartCoroutine(NGIO.GetConnectionStatus(NGAPI.OnConnectionStatusChanged));
        NGIO.KeepSessionAlive(); // keeps session in NG API alive if no activity for 30 sec

        // Code below rotates screen if the user's device rotates.
        if (GameObject.Find("mainCanvas").GetComponent<RectTransform>().sizeDelta.x <= GameObject.Find("mainCanvas").GetComponent<RectTransform>().sizeDelta.y) // if portrait
        {
            if (GameObject.Find("mainCanvas").GetComponent<CanvasScaler>().matchWidthOrHeight != 0)
            {
                GameObject.Find("mainCanvas").GetComponent<CanvasScaler>().matchWidthOrHeight = 0;
                GameObject.Find("imgCanvas").GetComponent<CanvasScaler>().matchWidthOrHeight = 0;
            }
        }
        else // if landscape
        {
            if (GameObject.Find("mainCanvas").GetComponent<CanvasScaler>().matchWidthOrHeight != 1)
            {
                GameObject.Find("mainCanvas").GetComponent<CanvasScaler>().matchWidthOrHeight = 1;
                GameObject.Find("imgCanvas").GetComponent<CanvasScaler>().matchWidthOrHeight = 1;
            }
        }

        // Funcs below trigger functionality to navigate between cutscenes and the post-game.
        if (totalFilesRead >= FilesToViewToEndGame && textWindow.activeSelf == false && SceneManager.GetActiveScene().buildIndex == 2)
        {
            SceneInitFuncs.EndingCutsceneInit();
        }

        if (SceneManager.GetActiveScene().buildIndex == 4 && inPostGame == false)
        {
            SceneInitFuncs.PostGameInit();
        }         

        if (SceneManager.GetActiveScene().buildIndex == 4)
        {
            if (totalFilesRead >= 85)
            {
                if (!medalIsUnlocking)
                {
                    medalPrefab.GetComponent<MedalFuncs>().UnlockMedal("win"); // unlocking in scene 4 as to not distract from the cutscene
                }                
            }

            // nesting so getTotalNoOFiles is not run every frame
            if (totalFilesRead >= totalFiles)
            {
                if (!medalIsUnlocking)
                {
                    medalPrefab.GetComponent<MedalFuncs>().UnlockMedal("oneHundredPercent");
                }               
            }
        }
    }

    /*
     * Retrieves the total number of files in the game. Does not include folders.
     */ 
    public int GetTotalNoOfFiles()
    {
        int runningNo = 0;
        runningNo += TextMaster.allTextFiles.Count;
        runningNo += ImgMaster.allSprites.Count;
        runningNo += sfx.allAudioFiles.Count;
        runningNo += VidMaster.videoURLs.Count;
        return runningNo;
    }

    /*
     * Opens a window containing the content passed in.
     * @param {MediaFile} contentFile The file to open. Will be a text file, image file, video file, audio file, or folder.
     */ 
    public void OpenWindow(MediaFile contentFile)
    {
        ContentWindowFunctions twScript = textWindow.GetComponent<ContentWindowFunctions>();

        if (textWindow.activeSelf)
        {
            isFolderOpen = false;            
            twScript.ClearGrid();
        } else
        {
            textWindow.SetActive(true);
        }

        twScript.activeFile = contentFile;

        // Initiates back button if the file is in a folder.
        if (contentFile.mediaType != "folder" && contentFile.location != null)
        {
            GameObject btn = Instantiate(textWindow.GetComponent<ContentWindowFunctions>().backToFolderBtn, new Vector3(GameObject.Find("closeWindowObj").transform.position.x - (GameObject.Find("closeWindowObj").transform.lossyScale.x * 5),
                 GameObject.Find("closeWindowObj").transform.position.y, 0), Quaternion.identity);
            btn.transform.SetParent(GameObject.Find("contentBG").transform, true);
            btn.transform.localScale = new Vector3(GameObject.Find("closeWindowObj").transform.localScale.x, GameObject.Find("closeWindowObj").transform.localScale.y, 0);
        }

        switch (contentFile.mediaType)
        {
            case "text":
                twScript.StartDialogue((TextFile)contentFile);
                break;
            case "image":
                twScript.DisplayImage((ImgFile)contentFile);
                break;
            case "video":
                twScript.PlayVid((VidFile)contentFile);
                break;
            case "audio":
                twScript.HandleAudio((AudioFile)contentFile);
                break;
            case "folder":
                twScript.OpenFolderContents((FolderFile)contentFile);
                break;
        }
    }
}
