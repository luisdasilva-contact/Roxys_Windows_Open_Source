using System.Collections.Generic;
using UnityEngine;

/*
 * Manages all audio for the scene; commonly referred to as audioMaster. Audio is imported into the scene
   by dropping audio files into the Assets folder, then adding each audio file as an AudioSource to the AudioMaster
   in the scene view.
 */ 
public class SFX : MonoBehaviour
{
    public Dictionary<string, AudioSource> allAudioSources;
    public bool muted;
    public List<AudioFile> allAudioFiles;
    public AudioSource currentlyPlaying;
    // public medal SFX because these aren't accessed via MediaFile and ContentWindowFunctions
    public AudioSource NGMedalOpen;
    public AudioSource NGMedalClose;

    /*
     * Initializes all functionality for the audioMaster.
     */ 
    public void Start()
    {   
        allAudioFiles = new List<AudioFile>();
        allAudioSources = new Dictionary<string, AudioSource>();
        muted = false;
        AudioSource[] allPrivSounds = GetComponents<AudioSource>();

        foreach (AudioSource amSound in allPrivSounds)
        {
            allAudioSources.Add(amSound.clip.name, amSound);
            allAudioSources[amSound.clip.name].Stop();
            amSound.Stop();
        }
    }

    /*
     * Plays the sound from the AudioSource passed in.
     * @param {AudioSource} sound The sound to play.
     */ 
    public void PlaySound(AudioSource sound)
    {
        if (!muted)
        {
            StopCurrentPlaying();
            currentlyPlaying = sound;
            sound.Play();
        }        
    }

    /*
     * Stops whichever sound is currently playing.
     */ 
    public void StopCurrentPlaying()
    {
        if (currentlyPlaying)
        {
            currentlyPlaying.Stop();
            currentlyPlaying = null;
        }
    }

    /*
     * Toggle mute functionality for the game.
     */ 
    public void MuteGame()
    {
        muted = !muted;
        if (muted)
        {
            StopCurrentPlaying();
        }
    }
}
