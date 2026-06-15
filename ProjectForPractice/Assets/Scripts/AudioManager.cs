using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] public AudioSource aSourse;

    [SerializeField] public AudioClip JumpAudio;
    [SerializeField] public AudioClip Atack1Audio;
    [SerializeField] public AudioClip Atack2Audio;
    [SerializeField] public AudioClip AtackHitAudio;
    [SerializeField] public AudioClip StepsAudio;
    [SerializeField] public AudioClip DashAudio;

    [SerializeField] public Vector2 PitchSwordSound = new Vector2(0.9f, 1.1f);

    // Start is called before the first frame update
    void Start()
    {
        aSourse = gameObject.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void JumpSound()
    {
        aSourse.clip = JumpAudio;
        aSourse.pitch = UnityEngine.Random.Range(PitchSwordSound.x, PitchSwordSound.y);
        aSourse.Play();
        Debug.Log("Jump sound played");
    }   

    public void Atack1Sound()
    {
        aSourse.clip = Atack1Audio;
        aSourse.pitch = UnityEngine.Random.Range(PitchSwordSound.x, PitchSwordSound.y);
        aSourse.Play();
        Debug.Log("Atack1 sound played");
    }
    public void Atack2Sound()
    {
        aSourse.clip = Atack2Audio;
        aSourse.pitch = UnityEngine.Random.Range(PitchSwordSound.x, PitchSwordSound.y);
        aSourse.Play();
        Debug.Log("Atack1 sound played");
    }

    public void DashSound()
    {
        aSourse.clip = DashAudio;
        aSourse.pitch = UnityEngine.Random.Range(PitchSwordSound.x, PitchSwordSound.y);
        aSourse.Play();
        Debug.Log("Dash sound played");
    }
    public void AtackHitSound()
    {
        aSourse.clip = AtackHitAudio;
        aSourse.pitch = UnityEngine.Random.Range(PitchSwordSound.x, PitchSwordSound.y);
        aSourse.Play();
        Debug.Log("AtackHit sound played");
    }
}
