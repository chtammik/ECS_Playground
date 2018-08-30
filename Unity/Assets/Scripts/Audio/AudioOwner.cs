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
    public List<Entity> OccupiedInstances { get; private set; }

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
    /// Only will be called when there's only one instance allowed.
    /// </summary>
    public event PlaybackMessage OnAudioUnmuted;

    public int GetClipID(int index) { return AudioContainer.GetAudioElements[index].GetAudioAsset.GetClipID; }
    public float GetSpatialBlend(int index) { return AudioContainer.GetAudioElements[index].GetSpatialBlend; }
    public bool GetLoop(int index) { return AudioContainer.GetAudioElements[index].GetLoop; }

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
        OccupiedInstances = new List<Entity>(_audioContainer.InstanceLimit);
        _audioContainer.OnInstancePlayed += OnPlayed;
        _audioContainer.OnInstanceStopped += OnStopped;
        _audioContainer.OnInstanceMuted += OnMuted;
        _audioContainer.OnInstanceUnmuted += OnUnmuted;

        _voicesCreated = true;
    }

    void RegisterToEntityMananger() { GameEntity = AudioService.RegisterAudioOwner(this); }

    public void Play()
    {
        if (_voicesCreated)
        {
            Entity instanceEntity = AudioService.Play(this);
            if (instanceEntity != Entity.Null)
                OccupiedInstances.Add(instanceEntity);
        }
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

    void OnPlayed(Entity instanceEntity) //whenever an instance gets fired, the AudioOwner is considered played.
    {
        if (OccupiedInstances.Contains(instanceEntity))
            OnAudioPlayed?.Invoke();
    }
    void OnStopped(Entity instanceEntity)
    {
        if (OccupiedInstances.Contains(instanceEntity))
        {
            OccupiedInstances.Remove(instanceEntity);
            if (OccupiedInstances.Count == 0) //it needs all instances to be stopped for the whole AudioOwner to be considered stopped.
                OnAudioStopped?.Invoke();
        }
    }
    void OnMuted(Entity instanceEntity)
    {
        if (OccupiedInstances.Count == 1 && OccupiedInstances.Contains(instanceEntity))
            OnAudioMuted?.Invoke();
    }
    void OnUnmuted(Entity instanceEntity)
    {
        if (OccupiedInstances.Count == 1 && OccupiedInstances.Contains(instanceEntity))
            OnAudioUnmuted?.Invoke();
    }
}
