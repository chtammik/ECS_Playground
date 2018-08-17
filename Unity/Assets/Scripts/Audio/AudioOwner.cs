using UnityEngine;
using System.Collections;
using Unity.Entities;

public enum PlaybackMessageType { Played, Stopped, Muted, Unmuted }

public class AudioOwner : MonoBehaviour
{
    [SerializeField] AudioContainer _audioContainer;
    public AudioContainer AudioContainer { get { return _audioContainer; } }
    public Entity GameEntity { get; private set; }
    public Entity[] VoiceEntities { get; private set; }
    public int[] VoiceEntityIndex { get; private set; }
    bool _voicesCreated;
    bool _isApplicationQuitting;

    public delegate void PlaybackMessage();
    public event PlaybackMessage OnAudioPlayed;
    public event PlaybackMessage OnAudioStopped;
    public event PlaybackMessage OnAudioMuted;
    public event PlaybackMessage OnAudioUnmuted;

    void Awake()
    {
        if(BootstrapAudio.IfAudioServiceInitialized())
        {
            RegisterToEntityMananger();
            CreateVoiceEntities();
        }
        else //if AudioService is not initialized yet, then call these functions right after it's initialized.
        {
            BootstrapAudio.OnAudioServiceInitialized += RegisterToEntityMananger;
            BootstrapAudio.OnAudioServiceInitialized += CreateVoiceEntities;
            Debug.Log(gameObject.name + " subscribed to BootstrapAudio.");
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

    void CreateVoiceEntities()
    {
        VoiceEntities = new Entity[AudioContainer.VoiceCount];
        VoiceEntityIndex = new int[AudioContainer.VoiceCount];

        for (int i = 0; i < VoiceEntities.Length; i++)
        {
            VoiceEntities[i] = AudioService.CreateVoiceEntity(GameEntity);
            VoiceEntityIndex[i] = VoiceEntities[i].Index;
            AudioMessageSystem.AddNewAudioOwner(VoiceEntities[i], this);
        }
        _voicesCreated = true;
        Debug.Log(gameObject.name + "'s VoiceHandles' length is " + VoiceEntities.Length);
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

    public void BroadcastPlaybackStatus(PlaybackMessageType playback)
    {
        switch (playback)
        {
            case PlaybackMessageType.Played:
                OnAudioPlayed();
                return;
            case PlaybackMessageType.Stopped:
                OnAudioStopped();
                return;
            case PlaybackMessageType.Muted:
                OnAudioMuted();
                return;
            case PlaybackMessageType.Unmuted:
                OnAudioUnmuted();
                return;
        }
    }

    public int GetClipID(int index) { return AudioContainer.GetAudioElements[index].GetAudioAsset.GetClipID; }
    public float GetSpatialBlend(int index) { return AudioContainer.GetAudioElements[index].GetSpatialBlend; }
    public bool GetLoop(int index) { return AudioContainer.GetAudioElements[index].GetLoop; }
}
