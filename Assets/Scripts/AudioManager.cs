using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    public AudioClip Ammo;
    public AudioClip Background;
    public AudioClip Victory;
    public AudioClip Dead;
    public AudioClip Gun;
    public AudioClip HammerHit;
    public AudioClip HammerSwing;
    public AudioClip Heal;
    public AudioClip Hurt;
    public AudioClip Jump;
    public AudioClip Yay;
    public AudioClip Zombie;
    public AudioClip ZombieDead;
    public AudioClip ZombieHurt;

    public static AudioManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        musicSource.clip = Background;
        musicSource.Play();
        musicSource.loop = true;
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }

    public void StopBGM()
    {
        if (musicSource.isPlaying)
            musicSource.Stop();
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.Play();
        musicSource.loop = true;
    }
}