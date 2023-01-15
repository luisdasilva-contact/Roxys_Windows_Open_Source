using UnityEngine;
/*
 * Assigned to audioToggle object in Scene View, enabling functionality for muting/unmuting audio.
 */

public class AudioToggle : MonoBehaviour
{
    public static SFX am;
    public Sprite mute;
    public Sprite unmute;

    /*
     * Called when the script instance is loaded. Sets audioMaster for this object.
     */ 
    void Awake()
    {
        if (am == null)
        {
            am = GameObject.Find("audioMaster").GetComponent<SFX>();
        }
    }

    /*
     * Activates mute toggle in audioMaster and displays mute/unmute graphic on GameObject.
     */ 
    private void OnMouseUpAsButton()
    {
        am.MuteGame();

        if (am.muted)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = mute;
        } 
        else
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = unmute;
        }
    }
};
