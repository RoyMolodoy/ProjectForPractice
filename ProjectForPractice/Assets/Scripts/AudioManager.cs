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
        aSourse.Play();
        Debug.Log("Jump sound played");
    }   
}
