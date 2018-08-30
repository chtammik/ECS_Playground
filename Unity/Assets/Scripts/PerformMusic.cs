using UnityEngine;
using System.Collections;

public class PerformMusic : MonoBehaviour
{
    [SerializeField] KeyCode _playAndStopKeyCode;
    [SerializeField] KeyCode _muteAndUnmuteKeyCode;
    bool _performing;
    bool _muted;

    AudioUser _audioUser;

    void Awake()
    {
        _audioUser = GetComponent<AudioUser>();
        _audioUser.OnAudioPlayed += IsPerforming;
        _audioUser.OnAudioStopped += IsNotPerforming;
        _audioUser.OnAudioStopped += IsNotMuted;
        _audioUser.OnAudioMuted += IsMuted;
        _audioUser.OnAudioUnmuted += IsNotMuted;
    }

    void Start()
    {
        _audioUser.Play();
    }

    void OnDisable()
    {
        _audioUser.OnAudioPlayed -= IsPerforming;
        _audioUser.OnAudioStopped -= IsNotPerforming;
    }

    void Update()
    {
        if (Input.GetKeyDown(_playAndStopKeyCode))
        {
            if (_performing)
                _audioUser.Stop();
            else
                _audioUser.Play();
        }

        if (Input.GetKeyDown(_muteAndUnmuteKeyCode))
        {
            if (_muted)
                _audioUser.Unmute();
            else
                _audioUser.Mute();
        }
    }

    void IsPerforming() { _performing = true; }
    void IsNotPerforming() { _performing = false; }
    void IsMuted() { _muted = true; }
    void IsNotMuted() { _muted = false; }

}
