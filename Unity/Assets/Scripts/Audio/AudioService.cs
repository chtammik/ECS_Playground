using UnityEngine;
using System.Collections.Generic;
using Unity.Entities;
using System.Threading;
using Unity.Transforms;

public sealed class AudioService
{
    static int s_counter;
    static AudioClipList s_clipList;
    static EntityManager s_entityManager;
    static HashSet<AudioContainer> s_audioContainers = new HashSet<AudioContainer>();

    public static void Initialize(AudioClipList audioClipList, EntityManager entityManager)
    {
        s_clipList = audioClipList;
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

    public static Entity RegisterAudioOwner(AudioOwner audioOwner)
    {
        Entity audioOwnerEntity = GameObjectEntity.AddToEntityManager(s_entityManager, audioOwner.gameObject);
        s_entityManager.AddComponentData(audioOwnerEntity, new Position(audioOwner.transform.position));
        s_entityManager.AddComponentData(audioOwnerEntity, new CopyTransformFromGameObject());
        return audioOwnerEntity;
    }

    public static void DeRegisterAudioOwner(AudioOwner audioOwner) { s_entityManager.DestroyEntity(audioOwner.GameEntity); }

    public static void ResetAudioSource(AudioSource audioSource)
    {
        audioSource.clip = null;
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0;
        audioSource.timeSamples = 0;
    }

    public static AudioClip GetAudioClip(int index) { return s_clipList.Clips[index]; }
    public static float GetClipLength(int index) { return s_clipList.Lengths[index]; }

    public static void Play(AudioOwner audioOwner)
    {
        Entity instanceEntity = Entity.Null;
        Entity[] instanceEntities = audioOwner.AudioContainer.GetInstanceEntities;
        int instanceIndex = -1;
        int voiceCount = audioOwner.AudioContainer.VoiceCount;

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
            Debug.Log("You're trying to play " + audioOwner.gameObject.name + ", but it's exceeding the instance limit: " + audioOwner.AudioContainer.InstanceLimit);
#endif
            return;
        }

        s_entityManager.SetComponentData(instanceEntity, new InstanceHandle(audioOwner.GameEntity, voiceCount));
        Entity[] voiceEntities = audioOwner.AudioContainer.GetVoiceEntities;

        for (int i = 0; i < voiceCount; i++)
        {
            Entity voiceEntity = voiceEntities[instanceIndex * voiceCount + i];
            if (s_entityManager.HasComponent<RealVoice>(voiceEntity) || s_entityManager.HasComponent<VirtualVoice>(voiceEntity))
            {
                Debug.LogWarning("You're trying to play " + audioOwner.gameObject.name + ", which is already playing."); //get rid of this when blending mode is implemented.
                continue;
            }

            s_entityManager.AddComponentData(voiceEntity, new RealVoiceRequest(voiceEntity));
            s_entityManager.AddComponentData(voiceEntity, new AudioProperty_AudioClipID(voiceEntity, audioOwner.GetClipID(i)));

            var blend = audioOwner.GetSpatialBlend(i);
            if (blend != 0)
                s_entityManager.AddComponentData(voiceEntity, new AudioProperty_SpatialBlend(voiceEntity, blend));

            var loop = audioOwner.GetLoop(i);
            if (loop)
                s_entityManager.AddComponentData(voiceEntity, new AudioProperty_Loop(voiceEntity));
        }

        s_entityManager.AddComponentData(instanceEntity, new InstanceClaimed(0, 0));
    }

    public static void Stop(AudioOwner audioOwner)
    {
        Entity instanceEntity = Entity.Null;
        Entity[] instanceEntities = audioOwner.AudioContainer.GetInstanceEntities;
        int instanceIndex = -1;
        Entity[] voiceEntities = audioOwner.AudioContainer.GetVoiceEntities;
        int voiceCount = audioOwner.AudioContainer.VoiceCount;

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
                    Debug.LogWarning("You're trying to stop " + audioOwner.gameObject.name + ", which is not playing."); //get rid of this when blending mode is implemented.
                    continue;
                }
                s_entityManager.AddSharedComponentData(voiceEntity, new StopRequest());
            }
        }
    }

    public static void Mute(AudioOwner audioOwner)
    {
        Entity instanceEntity = Entity.Null;
        Entity[] instanceEntities = audioOwner.AudioContainer.GetInstanceEntities;
        int instanceIndex = -1;
        Entity[] voiceEntities = audioOwner.AudioContainer.GetVoiceEntities;
        int voiceCount = audioOwner.AudioContainer.VoiceCount;

        for (int i = 0; i < instanceEntities.Length; i++)
        {
            if (s_entityManager.HasComponent<InstanceClaimed>(instanceEntities[i]) && !s_entityManager.HasComponent<InstanceMuted>(instanceEntities[i]))
            {
                instanceEntity = instanceEntities[i];
                instanceIndex = i;
            }
            else
                continue;

            for (int j = 0; j < voiceCount; j++)
            {
                Entity voiceEntity = voiceEntities[instanceIndex * voiceCount + j];
                if (s_entityManager.HasComponent<VirtualVoice>(voiceEntity) && !s_entityManager.HasComponent<RealVoiceRequest>(voiceEntity))
                {
                    Debug.LogWarning("You're trying to mute " + audioOwner.gameObject.name + ", which is already muted."); //get rid of this when blending mode is implemented.
                    continue;
                }
                if (!s_entityManager.HasComponent<VirtualVoice>(voiceEntity) && !s_entityManager.HasComponent<RealVoice>(voiceEntity))
                {
                    Debug.LogWarning("You're trying to mute " + audioOwner.gameObject.name + ", which is not playing."); //get rid of this when blending mode is implemented.
                    continue;
                }
                s_entityManager.AddSharedComponentData(voiceEntity, new MuteRequest());
            }

        }
    }

    public static void Unmute(AudioOwner audioOwner)
    {
        Entity instanceEntity = Entity.Null;
        Entity[] instanceEntities = audioOwner.AudioContainer.GetInstanceEntities;
        int instanceIndex = -1;
        Entity[] voiceEntities = audioOwner.AudioContainer.GetVoiceEntities;
        int voiceCount = audioOwner.AudioContainer.VoiceCount;

        for (int i = 0; i < instanceEntities.Length; i++)
        {
            if (s_entityManager.HasComponent<InstanceClaimed>(instanceEntities[i]) && s_entityManager.HasComponent<InstanceMuted>(instanceEntities[i]))
            {
                instanceEntity = instanceEntities[i];
                instanceIndex = i;
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
                    Debug.LogWarning("You're trying to unmute " + audioOwner.gameObject.name + ", which is either not playing, not muted, or already trying to play."); //get rid of this when blending mode is implemented.
                    continue;
                }
            }
        }
    }

}
