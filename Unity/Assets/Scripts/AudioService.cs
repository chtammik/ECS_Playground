using UnityEngine;
using System.Collections;
using Unity.Entities;

public sealed class AudioService
{
    public static void ResetAudioSource(AudioSource audioSource)
    {
        audioSource.clip = null;
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    public static void Play(EntityCommandBuffer commandBuffer, Entity entity)
    {
        commandBuffer.AddComponent(entity, new AudioPlayRequest(entity));
    }

    public static void Play(EntityCommandBuffer commandBuffer, GameObject gameObject)
    {

        //commandBuffer.AddComponent(entity, new AudioPlayRequest(entity));
    }
}
