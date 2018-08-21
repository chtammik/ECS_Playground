using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Unity.Entities;

public enum VoiceState { Real, Virtual, Stopped }
public enum InstanceState { PartiallyMuted, FullyMuted, FullyPlaying, Stopped }

[Serializable]
public class AudioInstance
{
    public Entity[] VoiceEntities { get; private set; }
    public int[] VoiceEntityIndex { get; private set; }
    public Dictionary<int, VoiceState> VoicePlaybackStatus { get; set; }
    public bool PlayedMessageSent { get; private set; }

    public delegate void PlaybackMessage();
    public event PlaybackMessage OnInstancePlayed;
    public event PlaybackMessage OnInstanceStopped;
    public event PlaybackMessage OnInstanceMuted;
    public event PlaybackMessage OnInstanceUnmuted;

    public void CreateVoiceEntities(AudioOwner audioOwner)
    {
        VoiceEntities = new Entity[audioOwner.AudioContainer.VoiceCount];
        VoiceEntityIndex = new int[audioOwner.AudioContainer.VoiceCount];
        VoicePlaybackStatus = new Dictionary<int, VoiceState>(audioOwner.AudioContainer.VoiceCount);

        for (int i = 0; i < VoiceEntities.Length; i++)
        {
            VoiceEntities[i] = AudioService.CreateVoiceEntity(audioOwner.GameEntity);
            VoiceEntityIndex[i] = VoiceEntities[i].Index;
            AudioMessageSystem.AddNewAudioOwner(VoiceEntities[i], audioOwner);
            AudioMessageSystem.AddNewAudioInstance(VoiceEntities[i], this);
        }
    }

    public InstanceState GetInstanceState()
    {
        if (VoiceEntities.Length == 1)
        {
            switch (VoicePlaybackStatus[VoiceEntityIndex[0]])
            {
                case VoiceState.Real:
                    return InstanceState.PartiallyMuted;
                case VoiceState.Virtual:
                    return InstanceState.FullyMuted;
                case VoiceState.Stopped:
                    return InstanceState.Stopped;
                default:
                    Debug.LogError("The Voice Entity " + VoiceEntityIndex[0] + "'s playback status is showing null.");
                    return InstanceState.Stopped;
            }
        }
        else
        {
            if (!VoicePlaybackStatus.ContainsValue(VoiceState.Real) && !VoicePlaybackStatus.ContainsValue(VoiceState.Stopped))
                return InstanceState.FullyMuted;
            else if (!VoicePlaybackStatus.ContainsValue(VoiceState.Virtual) && !VoicePlaybackStatus.ContainsValue(VoiceState.Stopped))
                return InstanceState.FullyPlaying;
            else if (VoicePlaybackStatus.ContainsValue(VoiceState.Real) && VoicePlaybackStatus.ContainsValue(VoiceState.Virtual))
                return InstanceState.PartiallyMuted;
            else //it needs all voices to be stopped for the instance to be considered stopped.
                return InstanceState.Stopped;
        }
    }

    public void BroadcastPlaybackStatus(PlaybackMessageType playback)
    {
        switch (playback)
        {
            case PlaybackMessageType.Played:
                if (!PlayedMessageSent) //so that it's only called when the first voice gets played.
                {
                    OnInstancePlayed();
                    PlayedMessageSent = true;
                }
                return;

            case PlaybackMessageType.Stopped:
                OnInstanceStopped();
                PlayedMessageSent = false;
                return;

            case PlaybackMessageType.Muted:
                OnInstanceMuted();
                return;

            case PlaybackMessageType.Unmuted:
                OnInstanceUnmuted();
                return;
        }
    }
}
