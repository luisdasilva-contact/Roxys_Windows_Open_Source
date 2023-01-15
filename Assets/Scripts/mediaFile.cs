using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

/*
 * Base class representing data and functions for all files in the game, including: text, image, audio, video, and folders.
 */ 
public class MediaFile : MonoBehaviour
{
    public static GameMaster gm;
    public string title;
    public string location;
    private Vector3 _filePhysicalLocation;
    private Vector3 _orgMousePos;
    public string mediaType; // can be text, video, image, audio, or folder
    public Sprite fileIcon; // i.e. folder icon, docs icon, img icon, etc
    public bool fileHasBeenOpened;
    private static int lastHighestSortingOrder = 0; // used for stacking files one on top of another
    protected BoxCollider2D hitBox;

    
    void Awake()
    {
        if (gm == null)
        {
            gm = GameObject.Find("GameMaster").GetComponent<GameMaster>();            
        }
        
        if (hitBox == null)
        {
            hitBox = gameObject.GetComponent<BoxCollider2D>();
        }
        location = null;
    }

    
    private void Update()
    {
        HitBoxCheck();        
    }    

    /*
     * Sets transparency of a file while user is dragging it, and ensures it appears over other files. 
     */ 
    private void OnMouseDrag()
    {
        gm.draggedFile = gameObject;
        Vector3 curMousePos = UtilitiesFuncs.getMousePosIn2DWorldSpace();
        float dist = Vector3.Distance(curMousePos, _orgMousePos);
        SetContentLayer("overAll");

        // Threshold to distinguish between a tap and a drag
        if (dist >= gm.dragThreshold)
        {
            gm.isFileBeingDragged = true;
            transform.position = UtilitiesFuncs.getMousePosIn2DWorldSpace() + gm.curDragOffset;
            transform.SetAsFirstSibling();
            transform.position = new Vector3(transform.position.x, transform.position.y, 70); // making sure this gets drawnd on top
        }
    }

    /*
     * Handles visual flourishes upon clicking a file, and prepares a file for opening if it's within a folder. 
     */ 
    private void OnMouseDown()
    {
        if (location == null)// to prevent blinking after clicking on file in folder
        {
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f);
        }

        _orgMousePos = UtilitiesFuncs.getMousePosIn2DWorldSpace();
        _filePhysicalLocation = gameObject.GetComponent<SpriteRenderer>().transform.position;
        gm.curDragOffset = transform.position - UtilitiesFuncs.getMousePosIn2DWorldSpace();
        gm.textWindow.GetComponent<ContentWindowFunctions>().readyToOpen = true;
    }

    /*
     * Turns off hitbox for items that are on the desktop when a folder is open; prevents clicking through content window.
     */
    protected void HitBoxCheck()
    {
        if (gm.textWindow.activeSelf && location == null && hitBox.enabled)
        {
            hitBox.enabled = false;
        }
        else if (!gm.textWindow.activeSelf && !hitBox.enabled)
        {
            hitBox.enabled = true;
        }
    }

    /*
     * Initializes all visual components for the file, including the icon and file title.
     */ 
    public void InitVisuals()
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = fileIcon;
        gameObject.GetComponentInChildren<Text>().text = title;
        gameObject.GetComponentInChildren<Text>().color = new Color(1f, 1f, 1f, 1f);
        gameObject.transform.Find("Canvas").transform.Find("text_BG").localScale = new Vector2(gameObject.GetComponentInChildren<Text>().preferredWidth * 0.105f,
            gameObject.GetComponentInChildren<Text>().preferredHeight * 0.105f);        
        gameObject.GetComponent<SortingGroup>().sortingLayerName = "fileLayer";
        gameObject.GetComponent<SortingGroup>().sortingOrder = lastHighestSortingOrder;
        gameObject.transform.Find("Canvas").GetComponent<Canvas>().sortingLayerName = "fileLayer";
        gameObject.transform.Find("Canvas").GetComponent<Canvas>().sortingOrder = lastHighestSortingOrder;
        lastHighestSortingOrder+= 2;
    }

    /*
     * Checks if the object is in an invalid location, such as off-screen or underneath the UI icons.
     */ 
    private bool IsInvalidLocation()
    {
        List<GameObject> uiGOs = new()
        {
            gm.FullScreenButton,
            gm.muteAudio
        }; 

        return (UtilitiesFuncs.CheckOffScreen(gameObject) ||
            UtilitiesFuncs.CheckGameObjectsOverlap(gameObject, uiGOs));
    }

    /*
     * Runs validation checks before adding a file to a folder, ensuring that a file passes all conditions to be dropped in. If the checks all pass,
       the file is added. If not, it is reset to its original position.
     * @param {MediaFile} fileToAddToFolder The file the user is attempting to add to a folder.
     */ 
    private void DropFileIntoFolderChecks(MediaFile fileToAddToFolder)
    {
        if (fileToAddToFolder.mediaType != "folder")
        {
            if (gm.folderToDropInto.GetComponent<FolderFile>().objectsInFolder.Count >= 24)
            {
                ResetLocationUponInvalidAttempt();
            }
            else
            {
                if (fileToAddToFolder.title == gm.draggedFile.GetComponent<MediaFile>().title && gm.isFileBeingDragged)
                {
                    gm.folderToDropInto.GetComponent<FolderFile>().AddFileToFolder(this);
                    location = gm.folderToDropInto.GetComponent<FolderFile>().title;

                    // medal check for Carl
                    if (location == "Recycle Bin" && title == "The Carl Problem")
                    {
                        gm.medalPrefab.GetComponent<MedalFuncs>().UnlockMedal("secret");
                    }

                    gm.isFileBeingDragged = false;
                    gm.draggedFile = null;
                    Destroy(gameObject);
                }
            }
        }
    }

    /*
     * Integrates  what would typically be used with "OnMouseUpAsButton" function because dragging a mediaFile over another mediaFile's collider 
       (including the recycle bin and folders) will cancel out the OnMouseUpAsButton function.
     * @param {MediaFile} contentFile The current file that was being dragged.
     */ 
    protected void OnMouseUpBehavior(MediaFile contentFile)
    {
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1f); // Resets color after transparency effect while dragging
        if (!gm.isFolderOpen)
        {            
            DropFileOnTop();
        }

       if (IsInvalidLocation())
       {             
            ResetLocationUponInvalidAttempt();
            return;
       }

        if (gm.isFileBeingDragged == false) // if the file was just clicked to be opened, as opposed to being dragged around
        {
            if (gm.textWindow.GetComponent<ContentWindowFunctions>().readyToOpen) // if content window is ready to open a file, open this mediafile. If it's not a folder, also add to totalfilesread.
            {
                if (fileHasBeenOpened == false && mediaType != "folder")
                {
                    gm.totalFilesRead++;
                }

                gm.OpenWindow(contentFile);
            }
            else
            {
                gm.textWindow.GetComponent<ContentWindowFunctions>().readyToOpen = true; // setting true if content window not ready. Functionality used so files won't flicker open/closed upon mouse click.
            }
        }
        else // if the file was being dragged around.
        {
            if (gm.readyToDropFileIntoFolder)
            {
                DropFileIntoFolderChecks(contentFile);
            }
            else // not ready to drop into folder
            {
                if (gm.isFolderOpen && gm.isFileBeingDragged) // used in the event that the contentwindow is open and you're dragging a file; used to check if you're good to drop a file back onto the desktop
                {
                    gm.textWindow.GetComponent<BoxCollider2D>().enabled = true;
                    if (!UtilitiesFuncs.CheckMousePosOverlapsGameObject(gm.textWindow)) { // valid to drop file back onto desktop

                        FolderFile fromList = FolderMaster.allFolders.Find(x => x.title == location);
                        fromList.objectsInFolder.Remove(fromList.objectsInFolder.Find(x => x.title == title));
                        location = null;
                        gameObject.transform.position = UtilitiesFuncs.getMousePosIn2DWorldSpace();
                        gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y, 0f);

                        GetComponent<RectTransform>().localScale = new Vector3(13.5f, 13.5f, 0f);
                        gm.textWindow.GetComponent<ContentWindowFunctions>().objsDrawnOnGrid.Remove(
                        gm.textWindow.GetComponent<ContentWindowFunctions>().objsDrawnOnGrid.Find(x => x.GetComponent<MediaFile>().title == title));
                    } else
                    {
                        ResetLocationUponInvalidAttempt();
                    }
                    gm.textWindow.GetComponent<BoxCollider2D>().enabled = false;
                }
            }
        }
        gm.isFileBeingDragged = false;
        gm.draggedFile = null;
        if (location == null)
        {
            SetContentLayer("fileLayer");
        } else
        {
            SetContentLayer("textWindowLayer");
        }
    }

    /*
     * If a file has been dragged to an invalid location, it is reset to its location before the user began dragging it. It will also briefly blink red.
     */ 
    private void ResetLocationUponInvalidAttempt() 
    {
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1f);
        gameObject.transform.position = _filePhysicalLocation; // was _orgMousePos
        gameObject.transform.localPosition = new Vector3(gameObject.transform.localPosition.x, gameObject.transform.localPosition.y, 0f); // z axis is finnecky when resetting position
        StartCoroutine(UtilitiesFuncs.BlinkColor(GetComponent<SpriteRenderer>(), 0.5f, new Color(236f / 255f, 77f / 255f, 67f / 255f)));
        gm.isFileBeingDragged = false;
        gm.draggedFile = null;
    }

    /*
     * Sets the content layer for the object's Canvas, Sprite, and Sorting Group.
     * @param {string} layer The layer the object's components will be set to.
     */ 
    public void SetContentLayer(string layer)
    {
        GetComponentInChildren<Canvas>().sortingLayerName = layer;
        GetComponentInChildren<Canvas>().GetComponentInChildren<SpriteRenderer>().sortingLayerName = layer;
        GetComponent<SpriteRenderer>().sortingLayerName = layer;
        GetComponentInChildren<SortingGroup>().sortingLayerName = layer;
    }

    /*
     * Sets both the content layer for the object's Canvas, Sprite, and Sorting Group, as well as a specified sorting order.
     * @param {string} layer The layer the object's components will be set to.
     * @param {int} sortingOrder The Sorting Order for the Sprite Renderer and Canvas.
     */
    public void SetContentLayerAndSortingOrder(string layer, int sortingOrder)
    {
        SetContentLayer(layer);
        GetComponent<SortingGroup>().sortingOrder = sortingOrder;
        GetComponent<SpriteRenderer>().sortingOrder = 2;
        GetComponentInChildren<Canvas>().sortingOrder = 2;
    }

    /*
     * Drops the gameObject on top of the other files in the scene.
     */ 
    private void DropFileOnTop()
    {
        gameObject.GetComponent<SortingGroup>().sortingLayerName = "fileLayer";
        gameObject.GetComponent<SortingGroup>().sortingOrder = lastHighestSortingOrder;
        gameObject.transform.Find("Canvas").GetComponent<Canvas>().sortingLayerName = "fileLayer";
        gameObject.transform.Find("Canvas").GetComponent<Canvas>().sortingOrder = lastHighestSortingOrder;
        lastHighestSortingOrder++;
    }
}
