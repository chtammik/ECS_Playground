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
    public AudioInstance[] AudioInstances { get; private set; }
    public Dictionary<AudioInstance, InstanceState> InstanceOccupation { get; set; }

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
            CreateAudioInstances();
        }
        else //if AudioService is not initialized yet, then call these functions right after it's initialized.
        {
            BootstrapAudio.OnAudioServiceInitialized += RegisterToEntityMananger;
            BootstrapAudio.OnAudioServiceInitialized += CreateAudioInstances;
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

    void CreateAudioInstances()
    {
        AudioInstances = new AudioInstance[AudioContainer.InstanceLimit];
        InstanceOccupation = new Dictionary<AudioInstance, InstanceState>(AudioContainer.InstanceLimit);

        for (int i = 0; i < AudioInstances.Length; i++)
        {
            AudioInstances[i] = new AudioInstance();
            AudioInstances[i].CreateVoiceEntities(this);
            AudioInstances[i].OnInstancePlayed += OnPlayed;
            AudioInstances[i].OnInstanceStopped += OnStopped;
            InstanceOccupation.Add(AudioInstances[i], InstanceState.Stopped);
        }

        if (AudioInstances.Length == 1)
        {
            AudioInstances[0].OnInstanceMuted += OnMuted;
            AudioInstances[0].OnInstanceUnmuted += OnUnmuted;
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

    void OnPlayed() { OnAudioPlayed(); } //whenever an instance gets fired, the AudioOwner is considered played.
    void OnStopped()
    {
        if (!InstanceOccupation.ContainsValue(InstanceState.PartiallyMuted) && !InstanceOccupation.ContainsValue(InstanceState.FullyMuted)) //it needs all instances to be stopped for the whole AudioOwner to be considered stopped.
            OnAudioStopped();
    }
    void OnMuted() { OnAudioMuted(); }
    void OnUnmuted() { OnAudioUnmuted(); }
}
