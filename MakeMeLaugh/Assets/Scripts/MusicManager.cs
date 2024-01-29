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

    float musicVolume = 0.1f;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = musicVolume;

        PlayMusicClip(wandering);
    }

    public void PlayMusicClip(AudioClip musicClip) // one at a time
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
            audioSource.Play();
        }
    }

    public void StopMusic(AudioClip musicClip)
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

    public void SetMusicVolume(float volume) // between 0.0f and 1.0f
    {
        audioSource.volume = volume;
    }

    void Update()
    {
        //audioSource;
    }
}
