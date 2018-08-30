using UnityEngine;
using System;
using System.Collections.Generic;
using Unity.Entities;

[CreateAssetMenu(fileName = "AudioContainer")]
public class AudioContainer : ScriptableObject
{
    [SerializeField] int _instanceLimit;
    [SerializeField] AudioElement[] _audioElements;
    [NonSerialized] Entity[] _instanceEntities;
    [NonSerialized] Entity[] _voiceEntities;

    public AudioElement[] GetAudioElements { get { return _audioElements; } }
    public int VoiceCount { get { return _audioElements.Length; } }
    public int InstanceLimit { get { return _instanceLimit; } }
    public Entity[] GetInstanceEntities { get { return _instanceEntities; } }
    public Entity[] GetVoiceEntities { get { return _voiceEntities; } }
    public Dictionary<Entity, int> InstanceIndex { get; private set; }

    public delegate void PlaybackMessage(Entity instanceEntity);
    public event PlaybackMessage OnInstancePlayed;
    public event PlaybackMessage OnInstanceStopped;
    public event PlaybackMessage OnInstanceMuted;
    public event PlaybackMessage OnInstanceUnmuted;

    void OnEnable()
    {
        AudioService.RegisterAudioContainer(this);
    }

    public void CreateInstanceEntites(EntityManager entityManager)
    {
        _instanceEntities = new Entity[_instanceLimit];
        _voiceEntities = new Entity[VoiceCount * _instanceLimit];
        InstanceIndex = new Dictionary<Entity, int>(_instanceLimit);
        for (int i = 0; i < _instanceEntities.Length; i++)
        {
            _instanceEntities[i] = entityManager.CreateEntity(typeof(InstanceHandle));
            AudioMessageSystem.AddAudioContainer(_instanceEntities[i], this);
            InstanceIndex.Add(_instanceEntities[i], i);
            for (int j = 0; j < VoiceCount; j++)
            {
                Entity voiceEntity = entityManager.CreateEntity(typeof(VoiceHandle));
                _voiceEntities[i * VoiceCount + j] = voiceEntity;
                entityManager.SetComponentData(voiceEntity, new VoiceHandle(_instanceEntities[i], voiceEntity));
            }
        }
    }

    public void OnPlayed(Entity instanceEntity) { OnInstancePlayed?.Invoke(instanceEntity); }
    public void OnStopped(Entity instanceEntity) { OnInstanceStopped?.Invoke(instanceEntity); }
    public void OnMuted(Entity instanceEntity) { OnInstanceMuted?.Invoke(instanceEntity); }
    public void OnUnmuted(Entity instanceEntity) { OnInstanceUnmuted?.Invoke(instanceEntity); }
}

[Serializable]
public class AudioElement
{
    [SerializeField] AudioAsset _audioAssets;
    [SerializeField] [Range(0, 1)] float _spatialBlend;
    [SerializeField] bool _loop;

    public AudioAsset GetAudioAsset { get { return _audioAssets; } }
    public float GetSpatialBlend { get { return _spatialBlend; } }
    public bool GetLoop { get { return _loop; } }
}
