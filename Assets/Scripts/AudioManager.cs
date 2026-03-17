using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Source")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("Audio Clip")]
    public AudioClip Background;
    public AudioClip Victory;
    public AudioClip Dead;
    public AudioClip HammerHit;
    public AudioClip HammerSwing;
    public AudioClip Heal;
    public AudioClip Hurt;
    public AudioClip ItemGet;
    public AudioClip Jump;
    public AudioClip Yay;
    public AudioClip Zombie;
    public AudioClip ZombieDead;
    public AudioClip ZombieHurt;

    private void Start()
    {
        musicSource.clip = Background;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }
}