using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/*
 * Class of general Unity-focused utilities, re-usable for other projects.
 */

public static class UtilitiesFuncs
{
    /*
     * Retrieves the mouse's current position on the screen, converts it to world space, and sets z index to 0 to simplify functionality for 2D projects.
     */ 
    public static Vector3 getMousePosIn2DWorldSpace()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        return mousePos;
    }
    /*
     * Checks if a GameObject is off-screen, outside of the camera's view.
     * @param {GameObject} checkIfOffScreen The game object to check for being off-screen.
     */
    public static bool CheckOffScreen(GameObject checkForOffScreen)
    {
        Vector2 mainCanvasSize = new(Camera.main.scaledPixelWidth, Camera.main.scaledPixelHeight);
        // mainCanvasSize represents the "actual" size of the camera/canvas, including black borders on the top/side of screen
        float canvSizeX, canvSizeY;
        Rect canvasRectConverted;

        // Creates a rectangle based upon the in-game desktop, excluding black borders if there are any.
        if (mainCanvasSize.x == GameObject.Find("mainCanvas").GetComponent<CanvasScaler>().referenceResolution.x &&
            mainCanvasSize.y == GameObject.Find("mainCanvas").GetComponent<CanvasScaler>().referenceResolution.y)
        {
            // If game is being played at standard 960 x 540 res, most likely on desktop
            canvasRectConverted = new Rect(0, 0, mainCanvasSize.x, mainCanvasSize.y);
        } else if (mainCanvasSize.x < mainCanvasSize.y)
        { 
            // if portrait, i.e. phone in portrait mode
            canvSizeX = mainCanvasSize.x;
            canvSizeY = canvSizeX * (9f / 16f);
            canvasRectConverted = new Rect(0, (mainCanvasSize.y / 2f) - (canvSizeY / 2f), canvSizeX, canvSizeY);
        } else
        { 
            // if landscape, i.e. phone in landscape mode
            canvSizeY = mainCanvasSize.y;
            canvSizeX = canvSizeY * (16f / 9f);
            canvasRectConverted = new Rect((mainCanvasSize.x / 2f) - (canvSizeX / 2f), 0, canvSizeX, canvSizeY);
        }

        Rect objSpriteRectConverted = GetScreenSizeOfGOWithRectTransform(checkForOffScreen);

        return SourceRectOutsideTargetRect(objSpriteRectConverted, canvasRectConverted);
    }

    /*
     * Checks if targetGameObject overlaps any of the sourceGameObjects.
     * @param {GameObject} targetGameObject The object to check for overlapping with any of the sourceGameObjects.
     * @param {List<GameObject>} sourceGameObjects The list of GameObjects to check for overlap with targetGameObject.
     */
    public static bool CheckGameObjectsOverlap(GameObject targetGameObject, List<GameObject> sourceGameObjects)
    {
        foreach (GameObject sourceObj in sourceGameObjects)
        {
            Rect objRectConverted = GetScreenSizeOfGOWithRectTransform(sourceObj);
            Rect objSpriteRectConverted = GetScreenSizeOfGOWithRectTransform(targetGameObject);
            if (SourceRectIntersectsTargetRect(objSpriteRectConverted, objRectConverted))
            {
                return true;
            }
        }

        return false;
    }

    /*
     * Checks if the mouse is over the given GameObject. Uses the raycast method because sliced sprites are unable to reliably
       return their size.
     * @param {GameObject} objToCheck GameObject to check for overlap with the mouse. 
     */
    public static bool CheckMousePosOverlapsGameObject(GameObject objToCheck)
    {
        var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (objToCheck == null)
        {
            return false;
        }
        return (objToCheck.GetComponent<BoxCollider2D>().bounds.IntersectRay(mouseRay));
    }

    /*
     * Gets the size, in pixels, of a Game Object with a Rect Transform.
     * @param {GameObject} sourceObject The GameObject with the Rect Transform that will have its size in pixels returned.
     */ 
    private static Rect GetScreenSizeOfGOWithRectTransform(GameObject sourceObject)
    {
        RectTransform sourceObjectRT = sourceObject.GetComponent<RectTransform>();
        Rect sourceObjectRect = new(Camera.main.WorldToScreenPoint(sourceObject.transform.position).x - (sourceObjectRT.sizeDelta.x * sourceObjectRT.localScale.x) /2f,
            Camera.main.WorldToScreenPoint(sourceObject.transform.position).y - (sourceObjectRT.sizeDelta.y * sourceObjectRT.localScale.y ) /2f,
            (sourceObjectRT.sizeDelta.x * sourceObjectRT.localScale.x),
            (sourceObjectRT.sizeDelta.y * sourceObjectRT.localScale.y));
        return sourceObjectRect;
    }

    /*
     * Checks if the first rectangle is entirely outside of the second rectangle arg.
     * @param {Rect} sourceRect the rectangle that targetRect will be checked against.
     * @param {Rect} targetRect The rectangle that sourceRect will check for being outside of. 
     */
    public static bool SourceRectOutsideTargetRect(Rect sourceRect, Rect targetRect)
    {
        if (sourceRect.x < targetRect.x ||
            sourceRect.x + sourceRect.width > targetRect.width + targetRect.x ||
            sourceRect.y < targetRect.y ||
            sourceRect.y + sourceRect.height > targetRect.height + targetRect.y)
        {
            return true;
        } else
        {
            return false;
        }
    }

    /*
     * Checks if the first rectangle arg intersects at any point with the second rectangle arg. Note that
       this will also return true if the targetRect fully encases the sourceRect.
     * @param {Rect} sourceRect the rectangle that targetRect will be checked against.
     * @param {Rect} targetRect The rectangle that sourceRect will check for intersection. 
     */ 
    private static bool SourceRectIntersectsTargetRect(Rect sourceRect, Rect targetRect)
    {
        if (sourceRect.x > targetRect.x + targetRect.width ||
            targetRect.x > sourceRect.x + sourceRect.width)
        {
            return false;
        } else if (sourceRect.y > targetRect.y + targetRect.height ||
            targetRect.y > sourceRect.y + sourceRect.height)
        {
            return false;
        } else
        {
            return true;
        }
    }

    /*
     * Shifts a Sprite to a given color, then reverts back to the original color.
     * @param {SpriteRenderer} sr The SpriteRenderer containing the sprite that will have its color shifted.
     * @param {float} blinkSpeed The speed at which the color will shift, then shift back. The lower the value, the quicker the speed.
     * @param {Color} colorToFadeTo The color the object will shift to.
     */
    public static IEnumerator BlinkColor(SpriteRenderer sr, float blinkSpeed, Color colorToFadeTo)
    {
        Color origColor = sr.material.color;
        float elapsedTime = 0f;
        
        while (elapsedTime < blinkSpeed) {
            elapsedTime += Time.deltaTime;

            if ((elapsedTime /blinkSpeed) >= 0.5f)
            {
                sr.material.color = Color.Lerp(colorToFadeTo, origColor, (elapsedTime / blinkSpeed));
            } else
            {
                sr.material.color = Color.Lerp(origColor, colorToFadeTo, (elapsedTime / blinkSpeed));
            }
            
            yield return null;
        }
    }

    /*
     * Cleans video player of the video that was previously played; Unity-specific quirk where
       a video player that is being re-used will play a single frame from the previous video before
       a new one can begin.
     * @param {VideoPlayer} vp The VideoPlayer to clean out.
     */

    public static void  FlushVideoPlayer(VideoPlayer vp)
    {
        RenderTexture.active = vp.targetTexture;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = null;
    }
}
