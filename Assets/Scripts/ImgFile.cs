using UnityEngine;

/*
 * Class representing image files within the game.
 */

public class ImgFile : MediaFile
{

    public Sprite image;
    /*
     * Initializes variables and icons related to image files, derived from the base MediaFile class.
     * @param {string} titleArg The title of the image file.
     */
    public void InitializeImgFile(string titleArg, Sprite imgArg)
    {
        title = titleArg;
        mediaType = "image";
        fileIcon = gm.IconMasterScript.imgIcon;
        image = imgArg;
        location = null;
        InitVisuals();
    }
    /*
     * Performs functions for when the mouse is lifted up, handled by the base MediaFile class.
     */
    private void OnMouseUp()
    {
        OnMouseUpBehavior(this);
    }
}
