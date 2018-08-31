using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SoundBank")]
public class SoundBank : ScriptableObject
{
    [SerializeField] AudioClip[] _clips;
    public AudioClip[] Clips { get { return _clips; } }
    public float[] Lengths { get; private set; }

    void OnEnable()
    {
        if (_clips == null)
            _clips = Resources.LoadAll<AudioClip>("Clips");

        Lengths = new float[Clips.Length];
        for (int i = 0; i < Clips.Length; i++)
        {
            Lengths[i] = Clips[i].length;
        }
    }
}

