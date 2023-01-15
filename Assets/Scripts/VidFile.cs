/*
 * Class representing video files within the game.
 */
public class VidFile : MediaFile
{
    public string url;

    /*
     * Initializes variables and icons related to video files, derived from the base MediaFile class.
     * @param {string} titleArg The title of the video file.
     * @param {string} urlArg The URL the video is hosted at.
     */
    public void InitializeVidFile(string titleArg, string urlArg)
    {
        title = titleArg;
        mediaType = "video";
        url = urlArg;
        fileIcon = gm.IconMasterScript.vidIcon;
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
