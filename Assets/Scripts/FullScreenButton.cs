using UnityEngine;

/*
 * Manages the toggle for fullscreen vs. windowed mode.
 */ 
public class FullScreenButton : MonoBehaviour
{
    /*
     * Toggles between fullscreen and windowed mode.
     */ 
    private void OnMouseUpAsButton()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }
}
