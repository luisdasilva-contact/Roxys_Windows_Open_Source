using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ContentWindowFunctions : MonoBehaviour
{
    public static GameMaster gm;
    public Text titleComponent;
    public Text bodyComponent;
    public GameObject closeBtn;
    public GameObject backToFolderBtn;
    public GameObject textNavArrow;
    public GameObject txtNavBack;
    public GameObject txtNavForward;    
    public float textSpeed; // the lower the number, the faster this goes
    private int index;
    public List<string> lines;
    public bool readyToOpen;
    public bool readyToClose;
    public bool canProceedWText;
    public GameObject videoPlayerObj;
    public int wordCounter;
    public int wordLineLimit; // max # of lines per content page window
    public int pageNoInCurText; // ; total # of pages needed to display text in the current text file; based on 0-index
    public int fullPgIndex; // in comparison to fullPgIndex, what the current page is
    public bool txtFileAlreadyRead;
    public List<GameObject> objsDrawnOnGrid;
    public FolderFile currentlyOpenedFolder;
    public MediaFile activeFile;
    public float defaultWidth;
    public float defaultHeight;
    public float defaultYPos;
    public bool closingVidBeforeCompletion; // To check if a video is being closed before it's completed; prevents stack overflow for cleanup funcs related to vids


    // Initializing all values above.
    void Start()
    {
        if (gm == null)
        {
            gm = GameObject.Find("GameMaster").GetComponent<GameMaster>();
        }

        if (videoPlayerObj == null)
        {
            videoPlayerObj = GameObject.Find("vidPlayer");
        }
        
        lines = null;
        readyToClose = false;
        readyToOpen = true;
        wordCounter = 0;
        wordLineLimit = 10; 
        pageNoInCurText = 0;
        fullPgIndex = 0;
        txtFileAlreadyRead = false;
        objsDrawnOnGrid = new List<GameObject>();
        currentlyOpenedFolder = null;
        activeFile = null;
        textSpeed = 0.03f;
        defaultWidth = 24.25f;
        defaultHeight = 12.85f;
        defaultYPos = 250;
        closingVidBeforeCompletion = false;
    }
    
    /*
     * Manages variable cleanup before closing the content window.
     */ 
    public void CleanupBeforeClose()
    {
        if (readyToClose)
        {
            if (GameObject.Find("backToFolderBtn(Clone)") != null)
            {
                Destroy(GameObject.Find("backToFolderBtn(Clone)"));
            }

            if (txtNavBack)
            {
                Destroy(txtNavBack);
                txtNavBack = null;                
            }

            if (txtNavForward)
            {
                Destroy(txtNavForward);
                txtNavForward = null;                
            }

            switch (activeFile.mediaType)
            {
                case "text":
                    fullPgIndex = 0;
                    pageNoInCurText = 0;
                    break;
                case "video": 
                    // if file has run its natural course, do not want to run endVid because it has already been auto-called. 
                    if (closingVidBeforeCompletion)
                    {
                        EndVid(videoPlayerObj.GetComponent<VideoPlayer>());
                        closingVidBeforeCompletion = false;
                    }                    
                    
                    break;
                case "audio":
                    CancelInvoke();
                    break;
                case "folder":
                    ClearGrid();
                    currentlyOpenedFolder = null;
                    objsDrawnOnGrid = new List<GameObject>();
                    break;
            }

            activeFile.fileHasBeenOpened = true;
            AdjustContentWindowSize(defaultWidth, defaultHeight); // Resetting window size 
            GameObject.Find("imgHolder").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            GameObject.Find("contentBG").GetComponent<RectTransform>().transform.localPosition = new Vector3(0, 250, 0); // setting this back to default pos
            CloseWindow();
        }
    }

    // Update function to continue writing text as user taps on a newly-opened text file
    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (CheckMouseOverUIElements() == false)
            {
                if (gm.isFolderOpen == false)
                {
                    if (!readyToClose)
                    {
                        if (activeFile.mediaType == "text")
                        {
                            canProceedWText = true;
                            if (txtFileAlreadyRead)
                            {
                                if (fullPgIndex < pageNoInCurText)
                                {
                                    fullPgIndex++;
                                } 
                                if (!gm.manuallyLoggingIntoNGAPI) // catching a scenario where incorrect text could be displayed during NG API login process
                                {
                                    WriteTextAlreadyRead();
                                }                                
                            }
                            else
                            {
                                TextLoop();
                            }
                        }
                    }
                }
            }            
        }
    }
    
    

    /*
     * Manages writing text to the content window when a text file is active.
     */ 
    public void TextLoop()
    {
        canProceedWText = false;
        int curLineChars = lines[index].Length;
        int newLineCountInBody = Regex.Matches(bodyComponent.text, "\n").Count;
        /* basically checking last X number of characters from the bodyComponent to see if they match 
         the current index. If they do, ready for next line! If not, want to wrap up the line, THEN do the NextLine.*/
        string curTxtLastLine;
        int bodyTxtVsTotal = bodyComponent.text.Length - curLineChars;

        if (bodyTxtVsTotal >= 0)
        {
        curTxtLastLine = bodyComponent.text[^curLineChars..];
        } else
        {
        curTxtLastLine = null;
        }

        if (curTxtLastLine == lines[index])
        {
            NextLine();
        } else if (newLineCountInBody == wordLineLimit)
        {           
            // to catch a scenario in which a user will hit the back button on a text doc before the current page is complete
            wordCounter = wordLineLimit; // forcing a new page??
            index = ((fullPgIndex * wordLineLimit) + wordLineLimit) - 1;
            NextLine();
        }
        else // this is to skip to the end of the line if the user clicks ahead before the typing animation is complete
        {
            int lastNewLine = bodyComponent.text.LastIndexOf('\n');
            bodyComponent.text = bodyComponent.text[..(lastNewLine + 1)];
            bodyComponent.text += lines[index];
            StopAllCoroutines();
        }        
    }


    /*
     * Serves as the entry point for text files.
     * @param {TextFile} txtContents The text file to read and write.
     */ 
    public void StartDialogue(TextFile txtContents)
    {
        readyToClose = false;
        titleComponent.text = txtContents.title;
        lines = txtContents.lines;

        if (lines.Count < wordLineLimit)
        {
            AdjustContentWindowSize(defaultWidth, lines.Count + 4);           
        }        

        if (((float)lines.Count / wordLineLimit) % 1 == 0)
        {
            pageNoInCurText = (Math.Abs(lines.Count / wordLineLimit)) - 1;
        } else
        {
            pageNoInCurText = Math.Abs(lines.Count / wordLineLimit);
        }



        if (txtContents.fileHasBeenOpened == true)
        {
            txtFileAlreadyRead = true;            
            
            if (lines.Count > wordLineLimit)
            {
                CreateTextNavButton("forward");
            }
            WriteTextAlreadyRead();
        }
        else
        {
            index = 0;
            lines = txtContents.lines;
            if (txtContents.location == null)
            {
                txtContents.fileHasBeenOpened = true;
            } else
            {
                currentlyOpenedFolder.objectsInFolder.Find(x => x.title == activeFile.title).fileHasBeenOpened = true; 
            }            
            wordCounter++;
            StartCoroutine(TypeLine());
        }
    }
    
    /*
     * Creates a navigation button for Text Files with more than one page. 
     * @param {string} direction The direction the button will go in, either forward or back.
     */ 
    public void CreateTextNavButton(string direction)
    {
        // direction = forward or back

        if (direction == "forward")
        {
            txtNavForward = Instantiate(textNavArrow, new Vector3(0, 0, 0), Quaternion.identity);
            txtNavForward.transform.SetParent(gameObject.transform, true);
            txtNavForward.transform.localScale = new Vector3(0.15f, 0.15f, 0);

            // some positioning of the forward button is relative to the back button to account for canvas scaling; makes sense to just create a temp GO for pos calculations then immediately destroy
            GameObject tempNav = Instantiate(textNavArrow, new Vector3(0, 0, 0), Quaternion.identity);
           tempNav.transform.SetParent(gameObject.transform, true);
            tempNav.transform.localScale = new Vector3(0.15f, 0.15f, 0);
            tempNav.transform.localPosition = new Vector3(gameObject.transform.localScale.x * -0.25f, gameObject.transform.lossyScale.y - (gameObject.GetComponent<RectTransform>().sizeDelta.y * 0.85f), 0);

            if (GameObject.Find("mainCanvas").GetComponent<CanvasScaler>().matchWidthOrHeight == 0)
            { // device in portrait mode
                txtNavForward.transform.localPosition = new Vector3(tempNav.transform.localPosition.x - (tempNav.GetComponent<SpriteRenderer>().bounds.center.x * 2.75f),
                gameObject.transform.lossyScale.y - (gameObject.GetComponent<RectTransform>().sizeDelta.y * 0.85f), 0);
            } else
            {
                txtNavForward.transform.localPosition = new Vector3(tempNav.transform.localPosition.x - (tempNav.GetComponent<SpriteRenderer>().bounds.center.x * 1.25f),
                gameObject.transform.lossyScale.y - (gameObject.GetComponent<RectTransform>().sizeDelta.y * 0.85f), 0);
            }
            
            Destroy(tempNav);
            txtNavForward.name = "txtNavForward";
            txtNavForward.GetComponent<SpriteRenderer>().flipX = true;
        } 
        else if (direction == "back")
        {
            txtNavBack = Instantiate(textNavArrow, new Vector3(0, 0, 0), Quaternion.identity);
            txtNavBack.transform.SetParent(gameObject.transform, true);
            txtNavBack.transform.localScale = new Vector3(0.15f, 0.15f, 0);
            txtNavBack.transform.localPosition = new Vector3(gameObject.transform.localScale.x * -0.25f, gameObject.transform.lossyScale.y - (gameObject.GetComponent<RectTransform>().sizeDelta.y * 0.85f), 0);
            txtNavBack.name = "txtNavBack";
        }
    }

    /*
     * Used when a text file is active, types out the current line character by character.
     */ 
    IEnumerator TypeLine()
    { // for automatically typing out a line
        foreach (char c in lines[index].ToCharArray())
        {
            bodyComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    /*
     * After a line of text has been written, this is used to write the next line.
     */ 
    public void NextLine()
    {
        if (index < lines.Count - 1)
        {
            if (wordCounter < wordLineLimit)
            {
                bodyComponent.text += Environment.NewLine;
                wordCounter++;
            }
            else // starting blank page
            {
                fullPgIndex++;
                if (txtNavBack == null)
                {
                    CreateTextNavButton("back");
                }
                
                wordCounter = 1;
                bodyComponent.text = "";
            }
            index++;
            StartCoroutine(TypeLine());
        } else if (index == lines.Count - 1)
        {
            txtFileAlreadyRead = true;

            if (activeFile.location != null)
            {
                // the file has been opened from a folder, and needs to be marked as read
                MediaFile countAsRead = currentlyOpenedFolder.objectsInFolder.Find(x => x.title == activeFile.title); 
                countAsRead.fileHasBeenOpened = true;
            }
        }
    }

    /*
     * If a text file has already been read, the document appears all at once, not line by line.
     */ 
    public void WriteTextAlreadyRead()
    {
        if (lines.Count > wordLineLimit)
        {
            // managing creation of buttons as user flips through pages
            if (fullPgIndex == 0)
            {
                if (txtNavBack)
                {
                    Destroy(txtNavBack);
                    txtNavBack = null;
                }

                if (txtNavForward == null)
                {
                    CreateTextNavButton("forward");
                }

            }
            if (fullPgIndex == pageNoInCurText)
            {
                if (txtNavForward)
                {
                    Destroy(txtNavForward);
                    txtNavForward = null;
                }

                if (txtNavBack == null)
                {
                    CreateTextNavButton("back");
                }

            }

            if (fullPgIndex < pageNoInCurText)
            {
                if (fullPgIndex != 0)
                {
                    if (txtNavBack == null)
                    {
                        CreateTextNavButton("back");
                    }

                    if (txtNavForward == null)
                    {
                        CreateTextNavButton("forward");
                    }
                }
            }
        }

        int loopVal = wordLineLimit;
        bodyComponent.text = "";
        if (pageNoInCurText != fullPgIndex) // if not on last page
        {
            for (int i = 0; i < loopVal; i++)
            {
                bodyComponent.text += lines[i + (fullPgIndex * wordLineLimit)] + Environment.NewLine;
            }


            canProceedWText = false;
        } 
        else
        { // on last page
            txtFileAlreadyRead = true;
            loopVal = lines.Count % wordLineLimit;

            if (loopVal == 0)
            {
                loopVal = wordLineLimit; // if modulo is 0, means the # of lines left on the page is exactly the same as the wordLineLimit val
            }

            for (int i = 0; i < loopVal; i++)
            {
                bodyComponent.text += lines[i + (fullPgIndex * wordLineLimit)] + Environment.NewLine;
            }
        }
    }
    
   
    /*
     * Opens the contents of a folder, showcasing the files within.
     * @param {FolderFile} folderArg The folder to open.
     */ 
    public void OpenFolderContents(FolderFile folderArg)
    {
        titleComponent.text = folderArg.title;
        gm.isFolderOpen = true;
        currentlyOpenedFolder = folderArg;
                
        if (folderArg.objectsInFolder.Count > 0)
        {
            foreach (MediaFile f in folderArg.objectsInFolder)
            {
                GameObject newFileForFolder = Instantiate(gm.mediaPrefab);
                Type fType = f.GetType();

                // using if-else chain here instead of switch because constant vals are expected in switch cases
                if (fType == typeof(TextFile))
                {
                    TextFile content = (TextFile)f;
                    newFileForFolder.AddComponent<TextFile>();
                    newFileForFolder.GetComponent<TextFile>().InitializeTxtFile(content.title, content.lines);

                } else if (fType == typeof(ImgFile))
                {
                    ImgFile content = (ImgFile)f;
                    newFileForFolder.AddComponent<ImgFile>();
                    newFileForFolder.GetComponent<ImgFile>().InitializeImgFile(content.title, content.image) ;
                } else if (fType == typeof(AudioFile))
                {
                    AudioFile content = (AudioFile)f;
                    newFileForFolder.AddComponent<AudioFile>();
                    newFileForFolder.GetComponent<AudioFile>().InitializeAudioFile(content.title);
                }
                else if (fType == typeof(VidFile))
                {
                    VidFile content = (VidFile)f;
                    newFileForFolder.AddComponent<VidFile>();
                    newFileForFolder.GetComponent<VidFile>().InitializeVidFile(content.title, content.url);
                }

                objsDrawnOnGrid.Add(newFileForFolder);
                newFileForFolder.GetComponent<MediaFile>().location = f.location;
                newFileForFolder.GetComponent<MediaFile>().fileHasBeenOpened = f.fileHasBeenOpened;


            }
            GridLayout(objsDrawnOnGrid);
            readyToClose = true;
        }         
    }

    /*
     * Displays a list of GameObjects with MediaFiles on a grid, used for creating a layout when a folder is opened.
     * @param {List<GameObject>} A list of GameObjects containing MediaFiles to display in the folder.
     */ 
    public void GridLayout(List<GameObject> gameObjectsToLayout)
    {
        float x_Start, y_Start, x_Space, y_Space;
        int columnLength = 8; // # of objects in each row
        int i = 0;

        if (Camera.main.pixelWidth <= Camera.main.pixelHeight) // if portrait
        {
            x_Start = gameObject.transform.position.x - 0.35f;
            y_Start = gameObject.transform.position.y - 0.15f;            
            x_Space = 0.1f; // pixels between objects
            y_Space = 0.13f;
        }
        else // landscape
        {
            x_Start = gameObject.transform.position.x - 1.4f; 
            y_Start = gameObject.transform.position.y - 0.55f;
            x_Space = 0.4f; // pixels between objects
            y_Space = 0.27f;            
        }

        foreach (GameObject gameObj in gameObjectsToLayout)
        {
            gameObj.transform.position = new Vector3(x_Start + (x_Space * (i % columnLength)), y_Start + (-y_Space * (i / columnLength)));
            gameObj.transform.SetParent(GameObject.Find("mainCanvas").transform);
            gameObj.GetComponent<RectTransform>().localScale = new Vector3(9f, 9f, 0f);

            // attempting rect change
            gameObj.GetComponent<RectTransform>().localPosition = new Vector3(gameObj.GetComponent<RectTransform>().localPosition.x, gameObj.GetComponent<RectTransform>().localPosition.y, 0f);
            gameObj.GetComponent<MediaFile>().SetContentLayerAndSortingOrder("textWindowLayer", 2);
            i++;
        }
    }

    /*
     * Clears out all objects created by the GridLayout function.
     */ 
    public void ClearGrid()
    {
        foreach(GameObject g in objsDrawnOnGrid)
        {
            Destroy(g);
        }
        objsDrawnOnGrid = new List<GameObject>();
    }

    /*
     * Plays the given video file.
     * @param {VidFile} vidToPlay the video file containing the URL to play.
     */ 
    public void PlayVid(VidFile vidToPlay)
    {
        titleComponent.text = "Loading...";

        videoPlayerObj.GetComponent<VideoPlayer>().url = vidToPlay.url;
        UtilitiesFuncs.FlushVideoPlayer(videoPlayerObj.GetComponent<VideoPlayer>());
        videoPlayerObj.GetComponent<VideoPlayer>().prepareCompleted += PlayVidOnPrepComplete;
        videoPlayerObj.GetComponent<VideoPlayer>().loopPointReached += EndVid;
    }

    /*
     * Starts the video when it has finished loading from the URL and is ready to play.
     * @param {VideoPlayer} vp The VideoPlayer object on which the video will be played.
     */ 
    public void PlayVidOnPrepComplete(VideoPlayer vp)
    {
        if (titleComponent && activeFile) // won't always have this component because this func is also used for playing cutscenes; same for active file
        {
            titleComponent.text = activeFile.title;

            if (GameObject.Find("vidRawImg").GetComponent<RawImage>().enabled == false)
            {
                GameObject.Find("vidRawImg").GetComponent<RawImage>().enabled = true;
            }
        }
        
    }

    /*
     * Ends the video currently being played.
     * @param {VideoPlayer} vp The The VideoPlayer object on which the video was being played.
     */
    public void EndVid(VideoPlayer vp)
    {
        if (activeFile) // catching scenario where this func can run if there's no active file; appears to be Unity Engine bug
        {
            vp.Stop();
            vp.frame = 1;
            GameObject vidRaw = GameObject.Find("vidRawImg");
            vidRaw.GetComponent<RawImage>().enabled = false;
            readyToClose = true;

            if (!closingVidBeforeCompletion)
            {
                CleanupBeforeClose();
            }
        }
          
    }

    /*
     * Performs key cleanup necessary to close the content window.
     */
    public void CloseWindow()
    {

        if (gm.isFolderOpen == true)
        {
            gm.isFolderOpen = false;
        }

        readyToClose = false;

        if (activeFile.mediaType == "text")
        {
            wordCounter = 0;
            fullPgIndex = 0;
            pageNoInCurText = 0;
            txtFileAlreadyRead = false;
            bodyComponent.text = string.Empty;
            index = 0; // to restart the text script
        }
        else if (activeFile.mediaType == "image")
        {
            GameObject holder = GameObject.Find("imgHolder");
            holder.GetComponent<SpriteRenderer>().sprite = null;
        }
        else if (activeFile.mediaType == "audio")
        {
            bodyComponent.text = "";
        }
        
        activeFile = null;
        gameObject.SetActive(false);
        readyToOpen = false; // so you can't click on the doc again and immediately open again by mistake
    }

    /*
     * Plays audio and showcases the timestamp of the audio clip.
     * @param {AudioFile} track The track to play.
     */ 
    public void HandleAudio(AudioFile track)
    {
        titleComponent.text = track.title;
        if (!gm.sfx.muted)
        {
            AudioSource curTrack = gm.audioMaster.GetComponent<SFX>().allAudioSources[track.title];
            gm.audioMaster.GetComponent<SFX>().PlaySound(curTrack);
            InvokeRepeating(nameof(UpdateBodyWithAudioLen), 0f, 0.06f); 
        } else
        {
            bodyComponent.text = "Audio is muted!";
        }        
    }

    /*
     * In the content window, shows a timestamp for the song that is playing.
     */ 
    public void UpdateBodyWithAudioLen()
    {
            float curMin = Mathf.FloorToInt((gm.audioMaster.GetComponent<SFX>().currentlyPlaying.time) / 60);
            float curSec = Mathf.FloorToInt((gm.audioMaster.GetComponent<SFX>().currentlyPlaying.time) % 60);
            string current = string.Format("{0:00} : {1:00}", curMin, curSec);
            float totMin = Mathf.FloorToInt((gm.audioMaster.GetComponent<SFX>().currentlyPlaying.clip.length) / 60);
            float totSec = Mathf.FloorToInt((gm.audioMaster.GetComponent<SFX>().currentlyPlaying.clip.length) % 60);
            string total = string.Format("{0:00} : {1:00}", totMin, totSec);
            bodyComponent.text = current + " / " + total;
    }

    /*
     * Displays an image file. Also manages content window resizing to adapt to the size of the image.
     * @param {ImgFile} imgToDisplay The image to display.
     */ 
    public void DisplayImage(ImgFile imgToDisplay)
    {
        titleComponent.text = imgToDisplay.title;
        float camX = Camera.main.scaledPixelWidth;
        float camY = Camera.main.scaledPixelHeight;
        float myMaxWidth, myMaxHeight, myBestRatio, newSizeX, newSizeY;
        float paddingValX = 0.62f, paddingValY = 0.62f;
        GameObject holder = GameObject.Find("imgHolder");
        holder.GetComponent<SpriteRenderer>().sprite = imgToDisplay.image;
        Vector3 spriteSize = holder.GetComponent<SpriteRenderer>().sprite.bounds.size;

        
        if (camX <= camY)
        {
            myMaxWidth = camX;
            myMaxHeight = camX * (9f / 16f);
        } else // if portrait; remember, can't do straight 16:9 conversion because many devices aren't 16:9
        {
            myMaxHeight = camY;
            myMaxWidth = camY * (16f / 9f);
        }

        myBestRatio = Math.Min((myMaxWidth / spriteSize.x) * (GameObject.Find("imgCanvas").GetComponent<CanvasScaler>().referenceResolution.x / myMaxWidth), (myMaxHeight / spriteSize.y) * (GameObject.Find("imgCanvas").GetComponent<CanvasScaler>().referenceResolution.y / myMaxHeight));

        holder.GetComponent<RectTransform>().localScale = new Vector3(myBestRatio * paddingValX, myBestRatio * paddingValY, 0f);

        if (imgToDisplay.location == null) // scaling a little differently for content in a folder so the X button doesn't bounce around
        {
            spriteSize = holder.GetComponent<SpriteRenderer>().sprite.bounds.size;

            // giving the image some padding on the contentBG
            newSizeX = holder.GetComponent<RectTransform>().localScale.x * spriteSize.x / GameObject.Find("contentBG").GetComponent<RectTransform>().localScale.x * (float)Math.Pow((paddingValX * 2f), 2f);
            newSizeY = holder.GetComponent<RectTransform>().localScale.y * spriteSize.y / GameObject.Find("contentBG").GetComponent<RectTransform>().localScale.y * (float)Math.Pow((paddingValY * 2f), 2f);
            


            if (titleComponent.preferredWidth > (newSizeX * GameObject.Find("contentBG").GetComponent<RectTransform>().localScale.x)) // accomodating for the width of the title text
            {
                newSizeX = titleComponent.preferredWidth / GameObject.Find("contentBG").GetComponent<RectTransform>().localScale.x;
            }

            // check for unusually wide or tall images; if so, setting height to default
            // 2nd pass on 12/22: is this really needed? Why should it be, shouldn't the content window do this work?
            
            if (((spriteSize.x < spriteSize.y) &&
                (spriteSize.x / spriteSize.y) < 0.65f) ||
                ((spriteSize.x > spriteSize.y) &&
                (spriteSize.y / spriteSize.x) < 0.65f))
            {
                newSizeY = defaultHeight;
            }
            
        }
        else
        {
            newSizeX = defaultWidth;
            newSizeY = defaultHeight;
        }

        AdjustContentWindowSize(newSizeX, newSizeY);

        
        float actualHeight = spriteSize.y * holder.GetComponent<RectTransform>().localScale.y / (GameObject.Find("imgCanvas").GetComponent<CanvasScaler>().referenceResolution.y / myMaxHeight); // actual height in px of the imgCanvas

        float topBorder = (myMaxHeight - actualHeight) / 2; //this is for finding the top of the imgCanvas as a y coord
        float actualCenter = (actualHeight / 2f) + topBorder;
        float targetCenter = actualCenter - ((0.255f * myMaxHeight) - topBorder); // doing this calculation because the blue bar for the title is included, and takes up 25.5% of the content box
        float screenRatio = myMaxHeight / GameObject.Find("imgCanvas").GetComponent<CanvasScaler>().referenceResolution.y;

        if (topBorder < (0.255f * myMaxHeight))
        {
            holder.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, ((GameObject.Find("imgCanvas").GetComponent<CanvasScaler>().referenceResolution.y / 2) - (targetCenter / screenRatio)) * -1f);
        }

    }

    /*
     * Sets the set of the content window's Rect Transform and Sprite Renderer.
     * @param {float} newWidth The new width value of the window.
     * @param {float} newHeight The new height value of the window.
     */
    public void AdjustContentWindowSize(float newWidth, float newHeight)
    {
        GameObject.Find("contentBG").GetComponent<RectTransform>().sizeDelta = new Vector2(newWidth, newHeight);
        GameObject.Find("contentBG").GetComponent<SpriteRenderer>().size = new Vector2(newWidth, newHeight);
    }

    /*
     * Returns true if any UI elements are being touched. Used in ContentWindowFunction's main loop to verify if it is ok to write Text File contents.
     */
    private bool CheckMouseOverUIElements()
    {
        if (UtilitiesFuncs.CheckMousePosOverlapsGameObject(closeBtn) ||
            UtilitiesFuncs.CheckMousePosOverlapsGameObject(txtNavBack) ||
            UtilitiesFuncs.CheckMousePosOverlapsGameObject(txtNavForward) ||
            UtilitiesFuncs.CheckMousePosOverlapsGameObject(gm.FullScreenButton) ||
            UtilitiesFuncs.CheckMousePosOverlapsGameObject(gm.muteAudio))
        {
            return true;
        }

        GameObject backToFolderClone = GameObject.Find("backToFolderBtn(Clone)");
        if (backToFolderClone != null)
        {
            if (UtilitiesFuncs.CheckMousePosOverlapsGameObject(backToFolderClone))
            {
                return true;
            }
        }
        return false;
    }
}
