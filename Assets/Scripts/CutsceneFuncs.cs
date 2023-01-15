using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;

/*
 * Handles functionality related to cutscenes.
 */ 
public class CutsceneFuncs : MonoBehaviour
{
    public GameObject skippingTextObj;
    private double currentTime;
    private GameObject vidPlayerObj;    
    private float skipHoldTimer;
    private float? vidTime;    
    private readonly float skipTime = 1f; // # of sec to hold mouse to skip cutscene

    private void Awake()
    {
        vidPlayerObj = GameObject.Find("vidPlayer");
        UtilitiesFuncs.FlushVideoPlayer(vidPlayerObj.GetComponent<VideoPlayer>());

        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            NGAPI.initNGAPI();
          //  NGIO.CancelLogin(); // useful in development for forcing to restart login, for debugging NG API funcs
        }
    }

    private void Update()
    {//
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            StartCoroutine(NGIO.GetConnectionStatus(NGAPI.OnConnectionStatusChanged)); // for debugging NG API status as it loads in the background
        }

        if (vidPlayerObj.GetComponent<VideoPlayer>().isPrepared)
        {
            vidTime ??= (vidPlayerObj.GetComponent<VideoPlayer>().frameCount /
                 vidPlayerObj.GetComponent<VideoPlayer>().frameRate) - 0.06f;

            currentTime = vidPlayerObj.GetComponent<VideoPlayer>().time;

            if (currentTime >= vidTime)
            {
                ManageNextSceneInit();                
            }

            if (Input.GetMouseButton(0))
            {
                UpdateSkipOpacity();

                if (skipHoldTimer == 0)
                {
                    skipHoldTimer = Time.time + skipTime;
                }
                if (Time.time >= skipHoldTimer)
                {
                    ManageNextSceneInit();
                }
            } 
            else if (Input.GetMouseButtonUp(0))
            {
                if (skipHoldTimer != 0)
                {
                    skipHoldTimer = 0;
                    UpdateSkipOpacity();
                }
            }

        }        
    }

    /*
     * Manages loading and initialization between different scenes.
     */
    private void ManageNextSceneInit()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        if (SceneManager.GetActiveScene().buildIndex == 3)
        {
            SceneInitFuncs.PostGameLoad();
        }
    }

    /*
     * Updates opacity for the "skip" button. Functionality to skip cutscene managed in Update loop.
     */ 
    private void UpdateSkipOpacity()
    { 
        Text textComponent = skippingTextObj.GetComponent<Text>();
        Color newAlpha = new(textComponent.color.r, textComponent.color.g, textComponent.color.b, textComponent.color.a);
        if (skipHoldTimer == 0)
        {
            newAlpha.a = 0;
        } else { 
            newAlpha.a = Time.time / skipHoldTimer;
        }
        textComponent.color = newAlpha;
    }
}
