using UnityEngine;
using System.Collections.Generic;
using Unity.Entities;
using System;
using Unity.Transforms;

public sealed class AudioService
{
    static int s_counter;
    static SoundBank s_soundBank;
    static EntityManager s_entityManager;
    static HashSet<AudioContainer> s_audioContainers = new HashSet<AudioContainer>();

    public static void Initialize(SoundBank soundBank, EntityManager entityManager)
    {
        s_soundBank = soundBank;
        s_entityManager = entityManager;
        foreach (AudioContainer audioContainer in s_audioContainers)
            audioContainer.CreateInstanceEntites(s_entityManager);

        s_counter++;
        if (s_counter > 1)
            Debug.LogWarning("AudioService has been initialized for " + s_counter + " times. Is this intended?");
        else
            Debug.Log("AudioService initialized!");
    }

    public static void RegisterAudioContainer(AudioContainer audioContainer) { s_audioContainers.Add(audioContainer); }

    public static Entity RegisterAudioUser(AudioUser audioUser)
    {
        Entity audioUserEntity = GameObjectEntity.AddToEntityManager(s_entityManager, audioUser.gameObject);
        s_entityManager.AddComponentData(audioUserEntity, new Position(audioUser.transform.position));
        s_entityManager.AddComponentData(audioUserEntity, new CopyTransformFromGameObject());
        return audioUserEntity;
    }

    public static void DeRegisterAudioUser(AudioUser audioUser) { s_entityManager.DestroyEntity(audioUser.GameEntity); }

    public static void ResetAudioSource(AudioSource audioSource)
    {
        audioSource.clip = null;
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0;
        audioSource.timeSamples = 0;
        audioSource.dopplerLevel = 0;
        audioSource.minDistance = 1;
        audioSource.maxDistance = 20;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
    }

    public static AudioClip GetAudioClip(int index) { return s_soundBank.Clips[index]; }
    public static float GetClipLength(int index) { return s_soundBank.Lengths[index]; }

    public static Entity Play(AudioUser audioUser)
    {
        Entity instanceEntity = Entity.Null;
        Entity[] instanceEntities = audioUser.AudioContainer.GetInstanceEntities;
        int instanceIndex = -1;
        int voiceCount = audioUser.AudioContainer.VoiceCount;

        for (int i = 0; i < instanceEntities.Length; i++)
        {
            if (!s_entityManager.HasComponent<InstanceClaimed>(instanceEntities[i]))
            {
                instanceEntity = instanceEntities[i];
                instanceIndex = i;
                break;
            }
        }

        if (instanceEntity == Entity.Null || instanceIndex < 0)
        {
#if UNITY_EDITOR
            Debug.Log("You're trying to play " + audioUser.gameObject.name + ", but it's exceeding the instance limit: " + audioUser.AudioContainer.InstanceLimit);
#endif
            return Entity.Null;
        }

        s_entityManager.SetComponentData(instanceEntity, new InstanceHandle(audioUser.GameEntity, voiceCount));
        Entity[] voiceEntities = audioUser.AudioContainer.GetVoiceEntities;

        for (int i = 0; i < voiceCount; i++)
        {
            Entity voiceEntity = voiceEntities[instanceIndex * voiceCount + i];
            if (s_entityManager.HasComponent<RealVoice>(voiceEntity) || s_entityManager.HasComponent<VirtualVoice>(voiceEntity))
            {
                Debug.LogWarning("You're trying to play " + audioUser.gameObject.name + ", which is already playing."); //get rid of this when blending mode is implemented.
                continue;
            }

            s_entityManager.AddComponentData(voiceEntity, new RealVoiceRequest(voiceEntity));
            s_entityManager.AddComponentData(voiceEntity, new AudioProperty_AudioClipID(voiceEntity, audioUser.GetClipID(i)));

            var blend = audioUser.GetSpatialBlend(i);
            if (blend != 0)
                s_entityManager.AddComponentData(voiceEntity, new AudioProperty_SpatialBlend(voiceEntity, blend));

            var loop = audioUser.GetLoop(i);
            if (loop)
                s_entityManager.AddComponentData(voiceEntity, new AudioProperty_Loop(voiceEntity));
        }

        s_entityManager.AddComponentData(instanceEntity, new InstanceClaimed(0, 0));

        return instanceEntity;
    }

    public static void Stop(AudioUser audioUser)
    {
        Entity instanceEntity = Entity.Null;
        Entity[] instanceEntities = audioUser.AudioContainer.GetInstanceEntities;
        int instanceIndex = -1;
        Entity[] voiceEntities = audioUser.AudioContainer.GetVoiceEntities;
        int voiceCount = audioUser.AudioContainer.VoiceCount;

        for (int i = 0; i < instanceEntities.Length; i++)
        {
            if (s_entityManager.HasComponent<InstanceClaimed>(instanceEntities[i]))
            {
                instanceEntity = instanceEntities[i];
                instanceIndex = i;
            }
            else
                continue;

            for (int j = 0; j < voiceCount; j++)
            {
                Entity voiceEntity = voiceEntities[instanceIndex * voiceCount + j];
                if (!s_entityManager.HasComponent<VirtualVoice>(voiceEntity) && !s_entityManager.HasComponent<RealVoice>(voiceEntity))
                {
                    Debug.LogWarning("You're trying to stop " + audioUser.gameObject.name + ", which is not playing."); //get rid of this when blending mode is implemented.
                    continue;
                }
                s_entityManager.AddSharedComponentData(voiceEntity, new StopRequest());
            }
        }
    }

    public static void Mute(AudioUser audioUser)
    {
        Entity instanceEntity = Entity.Null;
        Entity[] instanceEntities = audioUser.OccupiedInstances.ToArray();
        int instanceIndex = -1;
        Entity[] voiceEntities = audioUser.AudioContainer.GetVoiceEntities;
        int voiceCount = audioUser.AudioContainer.VoiceCount;

        for (int i = 0; i < instanceEntities.Length; i++)
        {
            if (s_entityManager.HasComponent<InstanceClaimed>(instanceEntities[i]) && !s_entityManager.HasComponent<InstanceMuted>(instanceEntities[i]))
            {
                instanceEntity = instanceEntities[i];
                instanceIndex = audioUser.AudioContainer.InstanceIndex[instanceEntity];
            }
            else
                continue;

            for (int j = 0; j < voiceCount; j++)
            {
                Entity voiceEntity = voiceEntities[instanceIndex * voiceCount + j];
                if (s_entityManager.HasComponent<VirtualVoice>(voiceEntity) && !s_entityManager.HasComponent<RealVoiceRequest>(voiceEntity))
                {
                    Debug.LogWarning("You're trying to mute " + audioUser.gameObject.name + ", which is already muted."); //get rid of this when blending mode is implemented.
                    continue;
                }
                if (!s_entityManager.HasComponent<VirtualVoice>(voiceEntity) && !s_entityManager.HasComponent<RealVoice>(voiceEntity))
                {
                    Debug.LogWarning("You're trying to mute " + audioUser.gameObject.name + ", which is not playing."); //get rid of this when blending mode is implemented.
                    continue;
                }
                s_entityManager.AddSharedComponentData(voiceEntity, new MuteRequest());
            }

        }
    }

    public static void Unmute(AudioUser audioUser)
    {
        Entity instanceEntity = Entity.Null;
        Entity[] instanceEntities = audioUser.OccupiedInstances.ToArray();
        int instanceIndex = -1;
        Entity[] voiceEntities = audioUser.AudioContainer.GetVoiceEntities;
        int voiceCount = audioUser.AudioContainer.VoiceCount;

        for (int i = 0; i < instanceEntities.Length; i++)
        {
            if (s_entityManager.HasComponent<InstanceClaimed>(instanceEntities[i]) && s_entityManager.HasComponent<InstanceMuted>(instanceEntities[i]))
            {
                instanceEntity = instanceEntities[i];
                instanceIndex = audioUser.AudioContainer.InstanceIndex[instanceEntity];
            }
            else
                continue;

            for (int j = 0; j < voiceCount; j++)
            {
                Entity voiceEntity = voiceEntities[instanceIndex * voiceCount + j];
                if (s_entityManager.HasComponent<VirtualVoice>(voiceEntity) && !s_entityManager.HasComponent<RealVoiceRequest>(voiceEntity))
                    s_entityManager.AddComponentData(voiceEntity, new RealVoiceRequest(voiceEntity));
                else
                {
                    Debug.LogWarning("You're trying to unmute " + audioUser.gameObject.name + ", which is either not playing, not muted, or already trying to play."); //get rid of this when blending mode is implemented.
                    continue;
                }
            }
        }
    }

}
