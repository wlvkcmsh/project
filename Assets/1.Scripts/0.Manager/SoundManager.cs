﻿using UnityEngine;
using System.Collections;

/// <summary>
/// Class in charge to play musics and fx
/// 
/// Script attached to the "SoundManager" GameObject. In charge to play musics and sound effects.
/// 
/// To Change a background music: Find the GameObject "Main Camera", and find the GameObject "SoundManager" and add your Audioclip music in the "Music Game" field. Same thing for the Music Menu; and for the FX sounds.
/// </summary>
public class SoundManager : MonoBehaviour
{
	/// <summary>
	/// Reference to the audio source use for music
	/// </summary>
	public AudioSource music;
	/// <summary>
	/// Reference to the audio source use for fx
	/// </summary>
	public AudioSource fx;

	/// <summary>
	/// Reference to the music use during the game
	/// </summary>
	public AudioClip musicGame;
	/// <summary>
	/// Reference to the music use in the menu
	/// </summary>
	public AudioClip beepFX;

	/// <summary>
	/// Reference to the music use when the player touch an obstacle 
	/// </summary>
	public AudioClip failFX;

	/// <summary>
	/// Reference to the fx played when the player jumps
	/// </summary>
	public AudioClip successFX;




    private static SoundManager s_Instance = null;

    public static SoundManager Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance //= new SoundManager();
                    = FindObjectOfType(typeof(SoundManager)) as SoundManager;
            }
            return s_Instance;
        }
    }

    void Awake()
    {
        if (s_Instance != null)
        {
            Debug.LogError("Cannot have two instances of SoundManager.");
            return;
        }
        s_Instance = this;
        //신넘어가도 유지시키는 함수
        DontDestroyOnLoad(this);
        Debug.Log("SoundManager Awake");




    }
    void Start()
	{
		PreparePoolSounds();
	}
	/// <summary>
	/// Spawn the sound prefabs from the ObjectPooling. 
	/// </summary>
	public void PreparePoolSounds()
	{
		fx.PlayOneShot (beepFX,0);
		fx.PlayOneShot (failFX,0);
		fx.PlayOneShot (successFX,0);
	}
	/// <summary>
	/// Play the music game
	/// </summary>
	public void PlayMusicGame()
	{
		PlayMusic (musicGame);
	}
	/// <summary>
	/// Play an audioclip to be used with music audio source
	/// </summary>
	private void PlayMusic(AudioClip a)
	{
		if (music != null && music.clip != null)
			music.Stop ();


		music.clip = a;
		music.Play ();
	}
	/// <summary>
	/// Play an audioclip to be used with fx audio source
	/// </summary>
	private void playFX(AudioClip a)
	{
		if (fx != null && fx.clip != null)
			fx.Stop ();

		fx.PlayOneShot (a);
	}
	/// <summary>
	/// Play the "bip" sound.
	/// </summary>
	public void PlaySoundBeep()
	{
		playFX(beepFX);
	}
	/// <summary>
	/// Play the "Fail" sound.
	/// </summary>
	public void PlaySoundFail()
	{
		playFX(failFX);
	}
	/// <summary>
	/// Play the "Success" sound.
	/// </summary>
	public void PlaySoundSuccess()
	{
		playFX(successFX);
	}

	public void MuteAllMusic()
	{
		music.Pause();
		fx.Pause();
	}

	public void UnmuteAllMusic()
	{
		music.Play();
		fx.Play();
	}






}
