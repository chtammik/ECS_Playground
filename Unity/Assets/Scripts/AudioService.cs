using UnityEngine;
using System.Collections;
using Unity.Entities;

public sealed class AudioService
{
    static AudioClipList s_clipList;

    public AudioService(AudioClipList audioClipList)
    {
        s_clipList = audioClipList;
    }

    public static void ResetAudioSource(AudioSource audioSource)
    {
        audioSource.clip = null;
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    public static AudioClip GetAudioClip(int index)
    {
        return s_clipList.Clips[index];
    }

    public static float GetClipLength(int index)
    {
        return s_clipList.Lengths[index];
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
