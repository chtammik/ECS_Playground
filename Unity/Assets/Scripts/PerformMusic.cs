using UnityEngine;
using System.Collections;

public class PerformMusic : MonoBehaviour
{
    [SerializeField] KeyCode _playAndStopKeyCode;
    [SerializeField] KeyCode _playAndMuteKeyCode;
    bool _performing;
    bool _muted;

    AudioOwner _audioOwner;

    void Awake()
    {
        _audioOwner = GetComponent<AudioOwner>();
        _audioOwner.OnAudioPlayed += IsPerforming;
        _audioOwner.OnAudioStopped += IsNotPerforming;
        _audioOwner.OnAudioMuted += IsMuted;
        _audioOwner.OnAudioUnmuted += IsNotMuted;
    }

    void Start()
    {
        _audioOwner.Play();
        _performing = true;
    }

    void OnDisable()
    {
        _audioOwner.OnAudioPlayed -= IsPerforming;
        _audioOwner.OnAudioStopped -= IsNotPerforming;
    }

    void Update()
    {
        if (Input.GetKeyDown(_playAndStopKeyCode))
        {
            if (_performing)
                _audioOwner.Play();
            else
                _audioOwner.Stop();
        }

        if (Input.GetKeyDown(_playAndMuteKeyCode))
        {
            if (_muted)
                _audioOwner.Unmute();
            else
                _audioOwner.Mute();
        }
    }

    void IsPerforming() { _performing = true; }
    void IsNotPerforming() { _performing = false; }
    void IsMuted() { _muted = true; }
    void IsNotMuted() { _muted = false; }

}
