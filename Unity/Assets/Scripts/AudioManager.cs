using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public AudioClipList ClipList;

    // Use this for initialization
    void Awake()
    {
        if (ClipList == null)
            ClipList = FindObjectOfType<AudioClipList>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
