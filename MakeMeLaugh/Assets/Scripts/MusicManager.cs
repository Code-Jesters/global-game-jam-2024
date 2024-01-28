using UnityEngine;

public class MusicManager : MonoBehaviour
{
    static public MusicManager Instance;

    // audio player
    public AudioSource audioSource;

    // music files
    public AudioClip mainBattle; // battle-furious
    public AudioClip tickleFight; // 8-bit battle
    public AudioClip wandering; // ambient 8
    public AudioClip defeat; // ambient 4
    public AudioClip victory; // the mountains loop

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        PlayMusicClip(wandering);
    }

    void PlayMusicClip(AudioClip musicClip, float volume = 1.0f) // one at a time
    {
        if (audioSource.clip != musicClip)
        {
            if (audioSource.clip && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            audioSource.clip = musicClip;
        }
        if (!audioSource.isPlaying)
        {
            audioSource.volume = volume;
            audioSource.Play();
        }
    }

    void StopMusic(AudioClip musicClip)
    {
        if (audioSource.clip == musicClip)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            audioSource.clip = null;
        }
    }

    void Update()
    {
        //audioSource;
    }
}
