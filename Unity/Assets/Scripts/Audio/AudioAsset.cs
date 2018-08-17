using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "AudioAsset")]
public class AudioAsset : ScriptableObject
{
    [SerializeField] int _clipID;
    public int GetClipID { get { return _clipID; } }
}
