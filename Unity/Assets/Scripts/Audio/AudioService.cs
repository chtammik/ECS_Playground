using UnityEngine;
using System.Collections;
using Unity.Entities;
using System.Threading;
using Unity.Transforms;

public sealed class AudioService
{
    static int s_counter;
    static AudioClipList s_clipList;
    static EntityManager s_entityManager;

    public static void Initialize(AudioClipList audioClipList, EntityManager entityManager)
    {
        s_clipList = audioClipList;
        s_entityManager = entityManager;
        s_counter++;
        if (s_counter > 1)
            Debug.LogWarning("AudioService has been initialized for " + s_counter + " times. Is this intended?");
        else
            Debug.Log("AudioService initialized!");
    }

    public static Entity CreateVoiceEntity(Entity gameEntity)
    {
        Entity voiceEntity = s_entityManager.CreateEntity(typeof(VoiceHandle));
        s_entityManager.SetComponentData(voiceEntity, new VoiceHandle(gameEntity, voiceEntity));
        return voiceEntity;
    }
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
    }

    public static AudioClip GetAudioClip(int index) { return s_clipList.Clips[index]; }
    public static float GetClipLength(int index) { return s_clipList.Lengths[index]; }

    public static void Play(AudioOwner audioOwner)
    {
        for (int i = 0; i < audioOwner.VoiceEntities.Length; i++)
        {
            Entity voiceEntity = audioOwner.VoiceEntities[i];
            if (s_entityManager.HasComponent<RealVoice>(voiceEntity) || s_entityManager.HasComponent<VirtualVoice>(voiceEntity))
            {
                Debug.LogWarning("You're trying to play " + audioOwner.gameObject.name + ", which is already playing.");
                return;
            }

            s_entityManager.AddComponentData(voiceEntity, new AudioPlayRequest(voiceEntity));
            s_entityManager.AddComponentData(voiceEntity, new AudioProperty_AudioClipID(voiceEntity, audioOwner.GetClipID(i)));

            var blend = audioOwner.GetSpatialBlend(i);
            if (blend != 0)
                s_entityManager.AddComponentData(voiceEntity, new AudioProperty_SpatialBlend(voiceEntity, blend));

            var loop = audioOwner.GetLoop(i);
            if (loop)
                s_entityManager.AddComponentData(voiceEntity, new AudioProperty_Loop(voiceEntity));
        }
    }

    public static void Stop(AudioOwner audioOwner)
    {
        for (int i = 0; i < audioOwner.VoiceEntities.Length; i++)
        {
            Entity voiceEntity = audioOwner.VoiceEntities[i];
            if (!s_entityManager.HasComponent<VirtualVoice>(voiceEntity) && !s_entityManager.HasComponent<RealVoice>(voiceEntity))
            {
                Debug.LogWarning("You're trying to stop " + audioOwner.gameObject.name + ", which is not playing.");
                return;
            }
            s_entityManager.AddSharedComponentData(audioOwner.VoiceEntities[i], new AudioStopRequest());
        }
    }

    public static void Mute(AudioOwner audioOwner)
    {
        for (int i = 0; i < audioOwner.VoiceEntities.Length; i++)
        {
            Entity voiceEntity = audioOwner.VoiceEntities[i];
            if (s_entityManager.HasComponent<VirtualVoice>(voiceEntity) && !s_entityManager.HasComponent<AudioPlayRequest>(voiceEntity))
            {
                Debug.LogWarning("You're trying to mute " + audioOwner.gameObject.name + ", which is already muted.");
                return;
            }
                s_entityManager.AddSharedComponentData(audioOwner.VoiceEntities[i], new AudioMuteRequest());
        }
    }

    public static void Unmute(AudioOwner audioOwner)
    {
        for (int i = 0; i < audioOwner.VoiceEntities.Length; i++)
        {
            Entity voiceEntity = audioOwner.VoiceEntities[i];
            if (s_entityManager.HasComponent<VirtualVoice>(voiceEntity) && !s_entityManager.HasComponent<AudioPlayRequest>(voiceEntity))
                s_entityManager.AddComponentData(voiceEntity, new AudioPlayRequest(voiceEntity));
            else
            {
                Debug.LogWarning("You're trying to unmute " + audioOwner.gameObject.name + ", which is either not playing, not muted, or already trying to play.");
                return;
            }
        }
    }

}
