using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using NewgroundsIO;


/*
 * For functionality related to initializing the gameplay loop, as well as the post-game.
 */ 
public static class SceneInitFuncs 
{
    public static GameMaster gm = GameObject.Find("GameMaster").GetComponent<GameMaster>();
    private static readonly float minSpawnRangeX = -1.2f;
    private static readonly float maxSpawnRangeX = 1.5f;
    private static readonly float minSpawnRangeY = -0.8f;
    private static readonly float maxSpawnRangeY = 0.5f;

   /*
    * Handles functions that store the current state of all files ahead of the ending cutscene, allowing them to be 
      unloaded in their exact place when the post-game begins.
    */ 
    public static void EndingCutsceneInit()
    {
        // Gathers all objects and deactivates them ahead of the ending scene.
        GameObject[] allObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (GameObject go in allObjects)
            if (go.activeInHierarchy)
            {
                gm.masterTransitionList.Add(go);

                if (go.name == "mainCanvas") // All files are children of the mainCanvas
                {
                    List<GameObject> canvasGOs = new();
                    for (int i = 0; i < go.transform.childCount; i++)
                    {
                        canvasGOs.Add(go.transform.GetChild(i).gameObject);
                    }

                    foreach (GameObject mediaFileChildOfCanvas in canvasGOs)
                    {
                        if (mediaFileChildOfCanvas.GetComponent<FolderFile>() != null) // specifically looking for folders in this loop
                        {
                            foreach (MediaFile file in mediaFileChildOfCanvas.GetComponent<FolderFile>().objectsInFolder)
                            {
                                FolderMaster.allMediaFilesInFoldersForPreservation.Add(file);
                                /* needs to be done this way instead of using dontdestroy because these class instances (i.e. textfile, AudioFile, etc) are not attached
                                 to gameobjects. They will be reset on the scene change if not added to a static class, and FolderMaster was the most appropriate, considering these properties are only for files in folders.*/                                
                            }
                        }
                    }
                }
                UnityEngine.Object.DontDestroyOnLoad(go);
                go.SetActive(false);
            }

         SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    /*
     * Sets up scene in the post-game.
     */ 
    public static void PostGameInit()
    {
        gm.inPostGame = true;
        var gameObjectList = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var go in gameObjectList)
        {
            if (go.name == "vidPlayer")
            {
                go.SetActive(true);
                UtilitiesFuncs.FlushVideoPlayer(go.GetComponent<VideoPlayer>());
                continue;
            }

            if (go.GetComponent<FolderFile>() != null)
            {
                go.GetComponent<FolderFile>().objectsInFolder.Clear();
                // clears folders ahead of re-initialization in next block of code
            }
        }

        // Re-initializes all files in folders.
        foreach (MediaFile fileFromFolder in FolderMaster.allMediaFilesInFoldersForPreservation)
        {
            var FolderFileObjs = UnityEngine.Object.FindObjectsOfType<FolderFile>();
            FolderFile folderToAddTo = FolderFileObjs.SingleOrDefault(item => item.title == fileFromFolder.location);
            folderToAddTo.AddFileToFolder(fileFromFolder);
        }
    }

    /*
     * Initializes the instructions seen when the game begins.
     */ 
    public static void InstructionsInit()
    {
        GameObject instructionsGO = MonoBehaviour.Instantiate(gm.mediaPrefab, new Vector3(-999, 999), Quaternion.identity);
        TextFile instructions = instructionsGO.AddComponent<TextFile>();

        string instructionsString = "- Welcome to Roxy's Windows! - \nThis game can be played on both PC and mobile devices (but YMMV, iUsers)\n\n---CONTROLS---\nTap/left click with a mouse - open a file\nHold – move a file (max of 24 files per folder)\n\n---OBJECT---\nRoxy's Windows is an interactive story based game where you're free to take things at your own pace.\nLearn more about Roxy and her friends through the files on her computer organizing them as you please.\nYou may even be rewarded for your efforts if you spend enough time browsing. ;)";

        string[] splitInstructions = instructionsString.Split("\n");
        List<string> instructionsText = new(splitInstructions);

        instructions.InitializeTxtFile("How to Play", instructionsText);
        instructions.fileHasBeenOpened = true;
        gm.OpenWindow(instructions);
    }


    /*
     * Initializes all desktop files at the beginning of the game.
     */ 
    public static void CreateInitDesktopFiles()
    {
        CreateFolderFilesOnDesktop();
        CreateTextFilesOnDesktop();
        CreateImgFilesOnDesktop();
        CreateAudioFilesOnDesktop();
        CreateVidFilesOnDesktop();
        SpecialCreate();
    }

    /*
     * Initializes creation of all folders for the desktop.
     */ 
    private static void CreateFolderFilesOnDesktop()
    {
        foreach (string folderTitle in FolderMaster.folderTitles)
        {
            GameObject newFolderFile = MonoBehaviour.Instantiate(gm.mediaPrefab, new Vector3(Random.Range(minSpawnRangeX, maxSpawnRangeX),Random.Range(minSpawnRangeY, maxSpawnRangeY), 0), Quaternion.identity);
            newFolderFile.transform.SetParent(GameObject.Find("mainCanvas").transform);
            newFolderFile.AddComponent<FolderFile>();
            newFolderFile.GetComponent<FolderFile>().InitializeFolderFile(folderTitle, new List<MediaFile>());
            FolderMaster.allFolders.Add(newFolderFile.GetComponent<FolderFile>());
        }
    }

    /*
     * Initiates all text files on the desktop. Each time the game starts, random values are used to determine which files are in folders and which 
       are scattered about on the desktop.
     */ 
    private static void CreateTextFilesOnDesktop()
    {
        int SophieCounter = 0;
        int SophieMax = Random.Range(16, 20);
        int MarieCounter = 0;
        int MarieMax = Random.Range(16, 20);
        int PaulCounter = 0;
        int PaulMax = Random.Range(16, 20);
        int randomCounter = 0;
        int randomCounterMax = Random.Range(7, 14);

        foreach (KeyValuePair<string, string> f in TextMaster.textFileContents)
        {

            GameObject newTxtFile = MonoBehaviour.Instantiate(gm.mediaPrefab, new Vector3(Random.Range(minSpawnRangeX, maxSpawnRangeX), 
                Random.Range(minSpawnRangeY, maxSpawnRangeY), 0), Quaternion.identity);
            newTxtFile.AddComponent<TextFile>();
            newTxtFile.transform.SetParent(GameObject.Find("mainCanvas").transform);
            string[] split = f.Value.Split("\n");
            List<string> fileLines = new(split);
            newTxtFile.GetComponent<TextFile>().InitializeTxtFile(f.Key, fileLines);
            TextMaster.allTextFiles.Add(newTxtFile.GetComponent<TextFile>());

            if (f.Key.Contains("Sophie"))
            {
                if (SophieCounter <= SophieMax)
                {
                    FolderMaster.allFolders.Find(item => item.title == "Sophie logs").AddFileToFolder(
                        newTxtFile.GetComponent<TextFile>());
                    MonoBehaviour.Destroy(newTxtFile);
                    SophieCounter++;
                }
            }
            else if (f.Key.Contains("Marie"))
            {
                if (MarieCounter <= MarieMax)
                {
                    FolderMaster.allFolders.Find(item => item.title == "Marie logs").AddFileToFolder(
                        newTxtFile.GetComponent<TextFile>());
                    MonoBehaviour.Destroy(newTxtFile);
                    MarieCounter++;
                }
            }
            else if (f.Key.Contains("Paul"))
            {
                if (PaulCounter <= PaulMax)
                {
                    FolderMaster.allFolders.Find(item => item.title == "Paul logs").AddFileToFolder(
                        newTxtFile.GetComponent<TextFile>());
                    MonoBehaviour.Destroy(newTxtFile);
                    PaulCounter++;
                }
            }
            else if (f.Key.ToString() == "DO NOT OPEN")
            {
                // putting "do not open" in the recycle bin at start
                FolderMaster.allFolders.Find(item => item.title == "Recycle Bin").AddFileToFolder(
                        newTxtFile.GetComponent<TextFile>());
                MonoBehaviour.Destroy(newTxtFile);
            }
            else
            {
                if (randomCounter <= randomCounterMax)
                {
                    FolderMaster.allFolders.Find(item => item.title == "etcetcetc").AddFileToFolder(
                        newTxtFile.GetComponent<TextFile>());
                    MonoBehaviour.Destroy(newTxtFile);
                    randomCounter++;
                }
            }
        }
    }

    /*
     * Initiates all image files on the desktop. A random number are also sorted between the desktop and the "my photo stuff" folder.
     */ 
    private static void CreateImgFilesOnDesktop()
    {
        int randomCounter = 0;
        int randomCounterMax = UnityEngine.Random.Range(15, 20);

        ImgMaster.InitializeAllSpritesList();
        foreach (Sprite sprite in ImgMaster.allSprites)
        {
            GameObject newImgFile = MonoBehaviour.Instantiate(gm.mediaPrefab, new Vector3(UnityEngine.Random.Range(minSpawnRangeX, maxSpawnRangeX), 
                UnityEngine.Random.Range(minSpawnRangeY, maxSpawnRangeY), 0), Quaternion.identity);
            newImgFile.transform.SetParent(GameObject.Find("mainCanvas").transform);
            newImgFile.AddComponent<ImgFile>();
            newImgFile.GetComponent<ImgFile>().InitializeImgFile(sprite.name, sprite);

            if (sprite.name.Contains("MarieFood"))
            {
                FolderMaster.allFolders.Find(item => item.title == "Marie's Food").AddFileToFolder(
                        newImgFile.GetComponent<ImgFile>());
                MonoBehaviour.Destroy(newImgFile);
                continue;
            } else if (sprite.name.Contains("Thrift"))
            {
                FolderMaster.allFolders.Find(item => item.title == "Paul's Thrift Finds").AddFileToFolder(
                        newImgFile.GetComponent<ImgFile>());
                MonoBehaviour.Destroy(newImgFile);
                continue;
            }

            if (randomCounter <= randomCounterMax)
            {
                FolderMaster.allFolders.Find(item => item.title == "My photo stuff").AddFileToFolder(
                newImgFile.GetComponent<ImgFile>());
                MonoBehaviour.Destroy(newImgFile);
                randomCounter++;
                continue;
            }
        }
    }

    /*
     * Initiates all audio files on the desktop.
     */
    private static void CreateAudioFilesOnDesktop()
    {
        foreach (KeyValuePair<string, AudioSource> kvp in gm.sfx.allAudioSources)
        {
            // Creating a temp gameObject because AudioFile derives from MediaFile, which itself derives from Monobehaviors, which can't be instantiated w/ "new" keyword
            GameObject newAudioFile = MonoBehaviour.Instantiate(gm.mediaPrefab, new Vector3(UnityEngine.Random.Range(minSpawnRangeX, maxSpawnRangeX), 
                UnityEngine.Random.Range(minSpawnRangeY, maxSpawnRangeY), 0), Quaternion.identity);
            newAudioFile.transform.SetParent(GameObject.Find("mainCanvas").transform);
            newAudioFile.AddComponent<AudioFile>();
            newAudioFile.GetComponent<AudioFile>().InitializeAudioFile(kvp.Key);
            gm.sfx.allAudioFiles.Add(newAudioFile.GetComponent<AudioFile>());
            FolderMaster.allFolders.Find(item => item.title == "Music").AddFileToFolder(newAudioFile.GetComponent<AudioFile>());
            MonoBehaviour.Destroy(newAudioFile);
        }
    }

    /*
     * Initiates all video files on the desktop.
     */
    private static void CreateVidFilesOnDesktop()
    {
        foreach (KeyValuePair<string, string> kvp in VidMaster.videoURLs)
        {
            GameObject newVidFile = MonoBehaviour.Instantiate(gm.mediaPrefab, new Vector3(UnityEngine.Random.Range(minSpawnRangeX, maxSpawnRangeX), UnityEngine.Random.Range(minSpawnRangeY, maxSpawnRangeY), 0), Quaternion.identity);
            newVidFile.transform.SetParent(GameObject.Find("mainCanvas").transform);
            newVidFile.AddComponent<VidFile>();
            newVidFile.GetComponent<VidFile>().InitializeVidFile(kvp.Key, kvp.Value);
        }
    }    

    /*
     * Initializes files with unique icons and/or locations on-screen, such as Ultrakill and the Recycle Bin.
     */ 
    private static void SpecialCreate()
    {
        GameObject[] allObjectsInScene = UnityEngine.Object.FindObjectsOfType<GameObject>();

        foreach (GameObject go in allObjectsInScene)
        {
            if (go.activeInHierarchy && go.GetComponent<VidFile>() != null)
            {
                if (go.GetComponent<VidFile>().title == "Ultrakill")
                {
                    go.GetComponent<SpriteRenderer>().sprite = gm.IconMasterScript.UltrakillIcon;
                }
            } else if (go.activeInHierarchy && go.GetComponent<FolderFile>() != null)
            {
                if (go.GetComponent<FolderFile>().title == "Recycle Bin")
                {
                    go.GetComponent<SpriteRenderer>().sprite = gm.IconMasterScript.recycleBinIcon;
                    go.transform.position = new Vector3(-1.5f, 0.8f, 0);
                }
            }
        }
    }

    /*
     * Manages object unloading in the post-game.
     */ 
    public static void PostGameLoad()
    {
        Camera.allCameras[0].enabled = false;
        EventSystem.current.enabled = false;

        var foundGM = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject go in foundGM)
        {
            if (go.GetComponent<GameMaster>() != null) // Retrieving GameMaster
            {
                go.SetActive(true);
                foreach (GameObject transitionGO in go.GetComponent<GameMaster>().masterTransitionList)
                {
                    if (transitionGO.name != "Main Camera")
                    {
                        transitionGO.SetActive(true);
                        if (transitionGO.name == "audioMaster")
                        {
                            foreach (AudioSource audioComp in transitionGO.GetComponents<AudioSource>())
                                audioComp.Stop();
                        }
                    }
                    else
                    {
                        transitionGO.tag = "MainCamera";
                        transitionGO.SetActive(true);
                    }
                    
                }
            }
        }
    }

    /*
     * Creates and manages the popup to ask the user if they'd like to log in to their Newgrounds account
       to earn medals.
     */ 
    public static void NGAPIPopUp()
    {
        GameObject APIPopup = MonoBehaviour.Instantiate(gm.mediaPrefab, new Vector3(-999, 999), Quaternion.identity);
        TextFile APIText = APIPopup.AddComponent<TextFile>();

        string instructionsString = "It looks like you're not logged into Newgrounds! \n\nIf you log in, you can earn achievements for playing. Want to open a window to sign in?\n\n\n\n";

        string[] splitInstructions = instructionsString.Split("\n");
        List<string> instructionsText = new(splitInstructions);

        APIText.InitializeTxtFile("Log In to Newgrounds?", instructionsText);
        APIText.fileHasBeenOpened = true;
        gm.OpenWindow(APIText);

        // make new components similar to arrows for text nav
        GameObject confirmNGAPI = MonoBehaviour.Instantiate(gm.NGAPIButtonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        GameObject denyNGAPI = MonoBehaviour.Instantiate(gm.NGAPIButtonPrefab, new Vector3(0, 0, 0), Quaternion.identity);

        confirmNGAPI.name = "confirmLogin";
        denyNGAPI.name = "denyLogin";

       confirmNGAPI.transform.SetParent(gm.textWindow.transform, true);
        denyNGAPI.transform.SetParent(gm.textWindow.transform, true);

       confirmNGAPI.transform.localPosition = new Vector3(confirmNGAPI.GetComponent<SpriteRenderer>().localBounds.extents.x / -2, confirmNGAPI.transform.localPosition.y * 1.15f, 0);
        denyNGAPI.transform.localPosition = new Vector3(confirmNGAPI.GetComponent<SpriteRenderer>().localBounds.extents.x / 2, denyNGAPI.transform.localPosition.y * 1.15f, 0);
        denyNGAPI.GetComponent<SpriteRenderer>().sprite = gm.IconMasterScript.NGAPIDenyIcon;
    }
}
