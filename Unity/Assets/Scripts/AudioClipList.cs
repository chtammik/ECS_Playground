using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioClipList")]
public class AudioClipList : ScriptableObject {

    //public List<AudioClip> AllClips = new List<AudioClip>();
    public AudioClip[] clips;

    void OnEnable()
    {
        clips = Resources.LoadAll<AudioClip>("Clips");
    }
}

