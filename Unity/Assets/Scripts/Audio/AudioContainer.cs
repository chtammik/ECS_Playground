using UnityEngine;
using System;
using System.Collections;
using Unity.Entities;

[CreateAssetMenu(fileName = "AudioContainer")]
public class AudioContainer : ScriptableObject
{
    [SerializeField] int _instanceMaxLimit; //TODO: make this actually useful.
    [SerializeField] AudioElement[] _audioElements;

    public AudioElement[] GetAudioElements { get { return _audioElements; } }
    public int VoiceCount { get { return _audioElements.Length; } }
}

[Serializable]
public class AudioElement
{
    [SerializeField] AudioAsset _audioAssets;
    [SerializeField] [Range(0, 1)] float _spatialBlend;
    [SerializeField] bool _loop;

    public AudioAsset GetAudioAsset { get { return _audioAssets; } }
    public float GetSpatialBlend { get { return _spatialBlend; } }
    public bool GetLoop { get { return _loop; } }
}
