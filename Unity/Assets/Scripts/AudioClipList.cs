using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AudioClipList")]
public class AudioClipList : ScriptableObject {

    public AudioClip[] Clips { get; private set; }
    public float[] Lengths { get; private set; }

    void OnEnable()
    {
        Clips = Resources.LoadAll<AudioClip>("Clips");
        Lengths = new float[Clips.Length];
        for (int i = 0; i < Clips.Length; i++)
        {
            Lengths[i] = Clips[i].length;
        }
    }
}

