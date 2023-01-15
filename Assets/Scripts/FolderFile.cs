using System.Collections.Generic;
using UnityEngine;

/*
 * Class representing folders within the game.
 */
public class FolderFile : MediaFile
{
    public List<MediaFile> objectsInFolder;
    public GameObject hoverHighlight;
    public GameObject highlightInstc;

    /*
     * Initializes variables and icons related to folders, derived from the base MediaFile class. 
     * @param {string} titleArg The title of thefolder.
     * @param {List<MediaFile>} List of MediaFiles to insert into the folder at start, if any.
     */
    public void InitializeFolderFile(string titleArg, List<MediaFile> objectsInFolderArg)
    {
        title = titleArg;
        mediaType = "folder";
        objectsInFolder = objectsInFolderArg;
        fileIcon = gm.IconMasterScript.folderFullIcon;
        location = null;
        hoverHighlight = gm.objHighlight;
        highlightInstc = null;
        InitVisuals();
    }

    private void OnMouseUp()
    {
        OnMouseUpBehavior(this);
    }

    /*
     * Running hitbox detection in update look because OnMouseOver cannot detect this object's folder icon
       if the user is currently dragging a file.
     */
    private void Update()
    {
        HitBoxCheck();      

        if ((gm.isFileBeingDragged))
        {
            var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (GetComponent<Collider2D>().bounds.IntersectRay(mouseRay))
            {
                // check if file being dragged is folder; if so, don't create a highlight because folders cannot be dropped into folders
                GameObject isFolderCheck = (GameObject)gm.draggedFile;
                if (isFolderCheck.GetComponent<MediaFile>().mediaType != "folder")
                {
                    if (highlightInstc == null)
                    {
                        highlightInstc = Instantiate(hoverHighlight, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0), Quaternion.identity) as GameObject;
                        highlightInstc.transform.parent = gameObject.transform;
                        highlightInstc.transform.localScale = new Vector3(gameObject.transform.localScale.x * 0.75f, gameObject.transform.localScale.y * 0.75f, 0f);
                    }

                    if (gm.readyToDropFileIntoFolder == false)
                    {
                        gm.readyToDropFileIntoFolder = true;
                        gm.folderToDropInto = gameObject;
                    }
                }
            } 
            else
            { // basically, if dragging away from this folder
                if (highlightInstc != null) // highlight instance check basically prevents this check from being run over and over, and thus, preventing the other folders on screen from 
                    // turning this functionality off for the folder that's about to receive the file
                {
                    DestroyHighlight();
                }
            }
        } else
        { // for destroying the highlight after a file is dropped into the folder OR after you've dragged the file itself around
            if (highlightInstc != null)
            {
                DestroyHighlight();
            }
        }

        }

    /*
     * Adds a file to this folder.
     * @param {MediaFile} fileToAdd The file to add to the folder.
     */ 
    public void AddFileToFolder(MediaFile fileToAdd)
    {
        fileToAdd.location = title;
        objectsInFolder.Add(fileToAdd);        
    }

    /*
     * Destroys the highlight that appears when a file is dragged over a folder.
     */ 
    public void DestroyHighlight()
    {
        Destroy(highlightInstc);
        highlightInstc = null;
        gm.folderToDropInto = null;
        gm.readyToDropFileIntoFolder = false;
    }

    /*
     * Adds a MediaFile to this folder at a specified index in the objectsInFolder list.
     * @param {MediaFile} fileToAdd The file to add to the folder.
     * @param {int} indexInObjList The exact index to insert the file into.
     */ 
    public void AddFileToFolderAtIndex(MediaFile fileToAdd, int indexInObjList = 0)
    {
        fileToAdd.location = title;
        objectsInFolder.Insert(indexInObjList, fileToAdd);
    }    
}
