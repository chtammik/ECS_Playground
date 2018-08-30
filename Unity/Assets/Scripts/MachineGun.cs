using UnityEngine;
using System.Collections;

public class MachineGun : MonoBehaviour
{
    AudioOwner _audioOwner;
    [SerializeField] KeyCode _fireKey;

    // Use this for initialization
    void Start()
    {
        _audioOwner = GetComponent<AudioOwner>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(_fireKey))
            _audioOwner.Play();
    }
}
