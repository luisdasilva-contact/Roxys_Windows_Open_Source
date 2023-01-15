using NewgroundsIO.objects;
using System.Collections.Generic;
using UnityEngine;

/*
 * Handles functionality related to Newgrounds medals; interacts with the NGAPI class.
 */ 
public class MedalFuncs : MonoBehaviour
{
    public AnimationClip secretMedalAnim;
    public AnimationClip winMedalAnim;
    public AnimationClip oneHundredPercentMedalAnim;
    public bool medalIsUnlocking = false; 

    /*
     * Plays Newgrounds' sound effect for a medal pop-up opening.
     */ 
    public void PlayMedalOpenSound()
    {
        GameObject.Find("audioMaster").GetComponent<SFX>().PlaySound(
            GameObject.Find("audioMaster").GetComponent<SFX>().NGMedalOpen);
    }

    /*
     * Plays Newgrounds' sound effect for a medal pop-up closing.
     */
    public void PlayMedalCloseSound()
    {
        GameObject.Find("audioMaster").GetComponent<SFX>().PlaySound(
            GameObject.Find("audioMaster").GetComponent<SFX>().NGMedalClose);
    }

    /*
     * Plays the animation for a Newgrounds medal.
     * @param {string} medalName The name of the medal to unlock.
     */
    public void PlayUnlockMedalAnim(string medalName)
    {
            Animator animator = gameObject.GetComponent<Animator>();
            AnimationClip anim = medalName switch
            {
                "secret" => secretMedalAnim,
                "win" => winMedalAnim,
                "oneHundredPercent" => oneHundredPercentMedalAnim,
                _ => null,
            };
            AnimatorOverrideController aoc = new(animator.runtimeAnimatorController);
            var anims = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            foreach (var a in aoc.animationClips)
                anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, anim));
            aoc.ApplyOverrides(anims);
            animator.runtimeAnimatorController = aoc;               
    }

    /*
     * Utilized by the animation controller to destroy the medal GameObject when the animation is completed.
     */
    public void DestroySelfOnAnimComplete()
    {        
        GameObject.Find("GameMaster").GetComponent<GameMaster>().medalIsUnlocking = false;
        Destroy(gameObject);
    }

    /*
     * Communicates with Newgrounds' API to unlock a medal on the user's account.
     * @param {string} medalName The name of the medal to unlock.
     */
    public void UnlockMedal(string medalName)
    {
        if (NGIO.hasUser)
        {            
            var medalID = medalName switch
            {
                "secret" => NGAPI.carlProblemMedalID,
                "win" => NGAPI.finishGameMedalID,
                "oneHundredPercent" => NGAPI.ViewAllFilesMedalID,
                _ => 0,
            };
            Medal medalToUnlock = NGIO.GetMedal(medalID);
            if (!medalToUnlock.unlocked) // user has never unlocked medal before
           {
                GameObject.Find("GameMaster").GetComponent<GameMaster>().medalIsUnlocking = true;
                GameObject medalGameObject = Instantiate(GameObject.Find("GameMaster").GetComponent<GameMaster>().medalPrefab, new Vector3(0, 0, 0), Quaternion.identity);

                medalGameObject.transform.SetParent(GameObject.Find("mainCanvas").transform);
                medalGameObject.transform.localPosition = new Vector3(-297, 189, 0);
                medalGameObject.transform.localScale = new Vector3(135, 135, 1);
                medalGameObject.GetComponent<MedalFuncs>().PlayUnlockMedalAnim(medalName);
                medalGameObject.GetComponent<MedalFuncs>().StartCoroutine(NGIO.UnlockMedal(medalToUnlock.id, NGAPI.OnMedalUnlocked));
            }          
        }
    }
}
