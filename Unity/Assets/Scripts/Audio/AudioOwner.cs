using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Entities;

public enum PlaybackMessageType { Played, Stopped, Muted, Unmuted }

public class AudioOwner : MonoBehaviour
{
    [SerializeField] AudioContainer _audioContainer;
    public AudioContainer AudioContainer { get { return _audioContainer; } }
    public Entity GameEntity { get; private set; }

    bool _voicesCreated;
    bool _isApplicationQuitting;

    public delegate void PlaybackMessage();
    public event PlaybackMessage OnAudioPlayed;
    public event PlaybackMessage OnAudioStopped;
    /// <summary>
    /// Only will be called when there's only one instance allowed.
    /// </summary>
    public event PlaybackMessage OnAudioMuted;
    /// <summary>
    /// Only will be called when there's only one instance allowed and one voice involved.
    /// </summary>
    public event PlaybackMessage OnAudioUnmuted;

    void Awake()
    {
        if (BootstrapAudio.IfAudioServiceInitialized())
        {
            RegisterToEntityMananger();
            HookWithAudioContainer();
        }
        else //if AudioService is not initialized yet, then call these functions right after it's initialized.
        {
            BootstrapAudio.OnAudioServiceInitialized += RegisterToEntityMananger;
            BootstrapAudio.OnAudioServiceInitialized += HookWithAudioContainer;
            //Debug.Log(gameObject.name + " subscribed to BootstrapAudio.");
        }
    }

    void OnDisable()
    {
        if (_isApplicationQuitting) //so that the DeRegister won't be called after exiting play mode and throw exceptions.
            return;
        AudioService.DeRegisterAudioOwner(this);
    }

    void OnApplicationQuit()
    {
        _isApplicationQuitting = true;
    }

    void HookWithAudioContainer()
    {
        _audioContainer.OnInstancePlayed += OnPlayed;
        _audioContainer.OnInstanceStopped += OnStopped;

        if (_audioContainer.InstanceLimit == 1)
        {
            _audioContainer.OnInstanceMuted += OnMuted;
            _audioContainer.OnInstanceUnmuted += OnUnmuted;
        }

        _voicesCreated = true;
    }

    void RegisterToEntityMananger() { GameEntity = AudioService.RegisterAudioOwner(this); }

    public void Play()
    {
        if (_voicesCreated)
            AudioService.Play(this);
        else
            BootstrapAudio.OnAudioServiceInitialized += Play; //will play right after the AudioService is initialized and voices are created;
    }

    public void Stop()
    {
        if (_voicesCreated)
            AudioService.Stop(this);
    }

    public void Mute()
    {
        if (_voicesCreated)
            AudioService.Mute(this);
    }

    public void Unmute()
    {
        if (_voicesCreated)
            AudioService.Unmute(this);
    }

    public int GetClipID(int index) { return AudioContainer.GetAudioElements[index].GetAudioAsset.GetClipID; }
    public float GetSpatialBlend(int index) { return AudioContainer.GetAudioElements[index].GetSpatialBlend; }
    public bool GetLoop(int index) { return AudioContainer.GetAudioElements[index].GetLoop; }

    void OnPlayed() { OnAudioPlayed?.Invoke(); } //whenever an instance gets fired, the AudioOwner is considered played.
    void OnStopped()
    {
        if (!_audioContainer.GetInstanceUsage.ContainsValue(true)) //it needs all instances to be stopped for the whole AudioOwner to be considered stopped.
            OnAudioStopped?.Invoke();
    }
    void OnMuted() { OnAudioMuted?.Invoke(); }
    void OnUnmuted() { OnAudioUnmuted?.Invoke(); }
}
