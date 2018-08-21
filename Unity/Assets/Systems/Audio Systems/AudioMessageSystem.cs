using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

public class AudioMessageSystem : ComponentSystem
{
    public static Dictionary<Entity, AudioOwner> AudioOwnerFindBook { get; private set; }
    public static Dictionary<Entity, AudioInstance> AudioInstanceFindBook { get; private set; }

    public static void AddNewAudioOwner(Entity voiceEntity, AudioOwner audioOwner) { AudioOwnerFindBook.Add(voiceEntity, audioOwner); }
    public static void AddNewAudioInstance(Entity voiceEntity, AudioInstance audioInstance) { AudioInstanceFindBook.Add(voiceEntity, audioInstance); }

    protected override void OnCreateManager(int capacity)
    {
        AudioOwnerFindBook = new Dictionary<Entity, AudioOwner>();
        AudioInstanceFindBook = new Dictionary<Entity, AudioInstance>();
    }

    struct AudioPlayedGroup
    {
        public readonly int Length;
        [ReadOnly] public ComponentDataArray<AudioMessage_Played> Played;
    }
    [Inject] AudioPlayedGroup _audioPlayedGroup;

    struct AudioStoppedGroup
    {
        public readonly int Length;
        [ReadOnly] public ComponentDataArray<AudioMessage_Stopped> Stopped;
    }
    [Inject] AudioStoppedGroup _audioStoppedGroup;

    struct AudioMutedGroup
    {
        public readonly int Length;
        [ReadOnly] public ComponentDataArray<AudioMessage_Muted> Muted;
    }
    [Inject] AudioMutedGroup _audioMutedGroup;

    struct AudioUnmutedGroup
    {
        public readonly int Length;
        [ReadOnly] public ComponentDataArray<AudioMessage_Unmuted> Unmuted;
    }
    [Inject] AudioUnmutedGroup _audioUnmutedGroup;

    protected override void OnUpdate()
    {
        for (int i = 0; i < _audioPlayedGroup.Length; i++)
        {
            Entity voiceEntity = _audioPlayedGroup.Played[i].VoiceEntity;
            AudioOwner audioOwner = AudioOwnerFindBook[voiceEntity];
            AudioInstance instance = AudioInstanceFindBook[voiceEntity];

            instance.VoicePlaybackStatus[voiceEntity.Index] = VoiceState.Real;
            audioOwner.InstanceOccupation[instance] = instance.GetInstanceState();
            instance.BroadcastPlaybackStatus(PlaybackMessageType.Played);

            PostUpdateCommands.RemoveComponent<AudioMessage_Played>(voiceEntity);
        }

        for (int i = 0; i < _audioStoppedGroup.Length; i++)
        {
            Entity voiceEntity = _audioStoppedGroup.Stopped[i].VoiceEntity;
            AudioOwner audioOwner = AudioOwnerFindBook[voiceEntity];
            AudioInstance instance = AudioInstanceFindBook[voiceEntity];

            instance.VoicePlaybackStatus[voiceEntity.Index] = VoiceState.Stopped;
            InstanceState instanceState = instance.GetInstanceState();
            audioOwner.InstanceOccupation[instance] = instanceState;
            if (instanceState == InstanceState.Stopped)
                instance.BroadcastPlaybackStatus(PlaybackMessageType.Stopped);

            PostUpdateCommands.RemoveComponent<AudioMessage_Stopped>(voiceEntity);
        }

        for (int i = 0; i < _audioMutedGroup.Length; i++)
        {
            Entity voiceEntity = _audioMutedGroup.Muted[i].VoiceEntity;
            AudioOwner audioOwner = AudioOwnerFindBook[voiceEntity];
            AudioInstance instance = AudioInstanceFindBook[voiceEntity];

            instance.VoicePlaybackStatus[voiceEntity.Index] = VoiceState.Virtual;
            InstanceState instanceState = instance.GetInstanceState();
            audioOwner.InstanceOccupation[instance] = instanceState;
            if (instanceState == InstanceState.FullyMuted)
                instance.BroadcastPlaybackStatus(PlaybackMessageType.Muted);

            PostUpdateCommands.RemoveComponent<AudioMessage_Muted>(voiceEntity);
        }

        for (int i = 0; i < _audioUnmutedGroup.Length; i++)
        {
            Entity voiceEntity = _audioUnmutedGroup.Unmuted[i].VoiceEntity;
            AudioOwner audioOwner = AudioOwnerFindBook[voiceEntity];
            AudioInstance instance = AudioInstanceFindBook[voiceEntity];

            instance.VoicePlaybackStatus[voiceEntity.Index] = VoiceState.Real;
            InstanceState instanceState = instance.GetInstanceState();
            audioOwner.InstanceOccupation[instance] = instanceState;
            if (instanceState == InstanceState.FullyPlaying)
                instance.BroadcastPlaybackStatus(PlaybackMessageType.Unmuted);

            PostUpdateCommands.RemoveComponent<AudioMessage_Unmuted>(voiceEntity);
        }
    }


}
