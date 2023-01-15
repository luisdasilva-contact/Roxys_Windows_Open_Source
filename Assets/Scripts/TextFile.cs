using System.Collections.Generic;
/*
 * Class representing text files within the game.
 */

public class TextFile : MediaFile
{
    public List<string> lines; // The content of the text file separated by line. Used for displaying text one line at a time, progressed forward with user input.

    /*
     * Initializes variables and icons related to text files, derived from the base MediaFile class.
     * @param {string} titleArg The title of the text file.
     * @param {string} linesArg The individual lines that comprise the text file.
     */
    public void InitializeTxtFile(string titleArg, List<string> linesArg)
    {
        title = titleArg;
        lines = linesArg;
        mediaType = "text";
        fileIcon = gm.IconMasterScript.txtIcon;
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
