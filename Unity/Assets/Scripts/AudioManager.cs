using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public AudioClipList ClipList;

    void Awake()
    {
        if (ClipList == null)
            ClipList = FindObjectOfType<AudioClipList>();
    }
}
