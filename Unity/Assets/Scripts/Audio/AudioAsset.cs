using UnityEngine;
using System;

[CreateAssetMenu(fileName = "AudioAsset")]
public class AudioAsset : ScriptableObject
{
    [SerializeField] AudioClip _audioClip;
    [SerializeField] SoundBank _soundBank;
    public int ClipID { get; private set; } = -1;

    void OnValidate()
    {
        try
        {
            foreach (AudioClip clip in _soundBank.Clips)
            {
                if (_audioClip == clip)
                {
                    ClipID = Array.IndexOf(_soundBank.Clips, clip);
                    break;
                }
            }
            if (ClipID == -1)
                Debug.LogError(name + "'s audio clip is not found in the defined list.");
        }
        catch
        {
            Debug.LogError(name + "'s audio clip is not found in the defined list.");
        }

    }
}
