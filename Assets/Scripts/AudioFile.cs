/*
 * Class representing audio files within the game.
 */ 
public class AudioFile : MediaFile
{   
    /*
     * Initializes variables and icons related to audio files, derived from the base MediaFile class. Traditional class constructor is
       not used because Unity's AddComponent function for GameObjects does not allow constructor arguments. Also, note that songs themselves 
       are not associated with the audioFile object, as the audioMaster object and SFX class retrieve AudioSources via the song's title. 
       This is for the sake of aligning my code as closely as possible with standard methods of playing audio in Unity.
     * @param {string} titleArg The title of the audio file. Usually the song title.
     */     
    public void InitializeAudioFile(string titleArg)
    {
        title = titleArg;
        mediaType = "audio";
        fileIcon = gm.IconMasterScript.audioIcon;
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
