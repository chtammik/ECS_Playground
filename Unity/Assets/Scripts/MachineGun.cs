using UnityEngine;
using System.Collections;

public class MachineGun : MonoBehaviour
{
    AudioUser _audioUser;
    [SerializeField] KeyCode _fireKey;

    // Use this for initialization
    void Start()
    {
        _audioUser = GetComponent<AudioUser>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(_fireKey))
            _audioUser.Play();
    }
}
