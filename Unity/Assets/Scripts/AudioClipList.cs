using UnityEngine;

[CreateAssetMenu(fileName = "AudioClipList")]
public class AudioClipList : ScriptableObject {

    public AudioClip[] clips;

    void OnEnable()
    {
        clips = Resources.LoadAll<AudioClip>("Clips");
    }
}

