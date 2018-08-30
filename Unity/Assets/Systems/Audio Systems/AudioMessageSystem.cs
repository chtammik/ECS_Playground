using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

[UpdateAfter(typeof(AudioPlaySystem))]
public class AudioMessageSystem : ComponentSystem
{
    public static Dictionary<Entity, AudioContainer> AudioContainerFindBook { get; private set; }
    public static void AddAudioContainer(Entity instanceEntity, AudioContainer audioContainer) { AudioContainerFindBook.Add(instanceEntity, audioContainer); }

    protected override void OnCreateManager(int capacity)
    {
        AudioContainerFindBook = new Dictionary<Entity, AudioContainer>();
    }

    struct AudioPlayedGroup
    {
        public readonly int Length;
        [ReadOnly] public ComponentDataArray<AudioMessage_InstancePlayed> Played;
    }
    [Inject] AudioPlayedGroup _audioPlayedGroup;

    struct AudioStoppedGroup
    {
        public readonly int Length;
        [ReadOnly] public ComponentDataArray<AudioMessage_InstanceStopped> Stopped;
    }
    [Inject] AudioStoppedGroup _audioStoppedGroup;

    struct AudioMutedGroup
    {
        public readonly int Length;
        [ReadOnly] public ComponentDataArray<AudioMessage_InstanceMuted> Muted;
    }
    [Inject] AudioMutedGroup _audioMutedGroup;

    struct AudioUnmutedGroup
    {
        public readonly int Length;
        [ReadOnly] public ComponentDataArray<AudioMessage_InstanceUnmuted> Unmuted;
    }
    [Inject] AudioUnmutedGroup _audioUnmutedGroup;

    protected override void OnUpdate()
    {
        for (int i = 0; i < _audioPlayedGroup.Length; i++)
        {
            Entity instanceEntity = _audioPlayedGroup.Played[i].InstanceEntity;
            AudioContainer audioContainer = AudioContainerFindBook[instanceEntity];
            audioContainer.OnPlayed(instanceEntity);
            PostUpdateCommands.RemoveComponent<AudioMessage_InstancePlayed>(instanceEntity);
        }

        for (int i = 0; i < _audioStoppedGroup.Length; i++)
        {
            Entity instanceEntity = _audioStoppedGroup.Stopped[i].InstanceEntity;
            AudioContainer audioContainer = AudioContainerFindBook[instanceEntity];
            audioContainer.OnStopped(instanceEntity);
            PostUpdateCommands.RemoveComponent<AudioMessage_InstanceStopped>(instanceEntity);
        }

        for (int i = 0; i < _audioMutedGroup.Length; i++)
        {
            Entity instanceEntity = _audioMutedGroup.Muted[i].InstanceEntity;   
            AudioContainer audioContainer = AudioContainerFindBook[instanceEntity];
            audioContainer.OnMuted();
            PostUpdateCommands.RemoveComponent<AudioMessage_InstanceMuted>(instanceEntity);
        }

        for (int i = 0; i < _audioUnmutedGroup.Length; i++)
        {
            Entity instanceEntity = _audioUnmutedGroup.Unmuted[i].InstanceEntity;
            AudioContainer audioContainer = AudioContainerFindBook[instanceEntity];
            audioContainer.OnUnmuted();
            PostUpdateCommands.RemoveComponent<AudioMessage_InstanceUnmuted>(instanceEntity);
        }
    }
}

//public class AudioMessageSystem : ComponentSystem
//{
//    public static Dictionary<Entity, AudioOwner> AudioOwnerFindBook { get; private set; }
//    public static Dictionary<Entity, AudioInstance> AudioInstanceFindBook { get; private set; }

//    public static void AddNewAudioOwner(Entity voiceEntity, AudioOwner audioOwner) { AudioOwnerFindBook.Add(voiceEntity, audioOwner); }
//    public static void AddNewAudioInstance(Entity voiceEntity, AudioInstance audioInstance) { AudioInstanceFindBook.Add(voiceEntity, audioInstance); }

//    protected override void OnCreateManager(int capacity)
//    {
//        AudioOwnerFindBook = new Dictionary<Entity, AudioOwner>();
//        AudioInstanceFindBook = new Dictionary<Entity, AudioInstance>();
//    }

//    struct AudioPlayedGroup
//    {
//        public readonly int Length;
//        [ReadOnly] public ComponentDataArray<AudioMessage_VoicePlayed> Played;
//    }
//    [Inject] AudioPlayedGroup _audioPlayedGroup;

//    struct AudioStoppedGroup
//    {
//        public readonly int Length;
//        [ReadOnly] public ComponentDataArray<AudioMessage_VoiceStopped> Stopped;
//    }
//    [Inject] AudioStoppedGroup _audioStoppedGroup;

//    struct AudioMutedGroup
//    {
//        public readonly int Length;
//        [ReadOnly] public ComponentDataArray<AudioMessage_VoiceMuted> Muted;
//    }
//    [Inject] AudioMutedGroup _audioMutedGroup;

//    struct AudioUnmutedGroup
//    {
//        public readonly int Length;
//        [ReadOnly] public ComponentDataArray<AudioMessage_VoiceUnmuted> Unmuted;
//    }
//    [Inject] AudioUnmutedGroup _audioUnmutedGroup;

//    protected override void OnUpdate()
//    {
//        for (int i = 0; i < _audioPlayedGroup.Length; i++)
//        {
//            Entity voiceEntity = _audioPlayedGroup.Played[i].VoiceEntity;
//            AudioOwner audioOwner = AudioOwnerFindBook[voiceEntity];
//            AudioInstance instance = AudioInstanceFindBook[voiceEntity];

//            instance.VoicePlaybackStatus[voiceEntity.Index] = VoiceState.Real;
//            audioOwner.InstanceOccupation[instance] = instance.GetInstanceState();
//            instance.BroadcastPlaybackStatus(PlaybackMessageType.Played);

//            PostUpdateCommands.RemoveComponent<AudioMessage_VoicePlayed>(voiceEntity);
//        }

//        for (int i = 0; i < _audioStoppedGroup.Length; i++)
//        {
//            Entity voiceEntity = _audioStoppedGroup.Stopped[i].VoiceEntity;
//            AudioOwner audioOwner = AudioOwnerFindBook[voiceEntity];
//            AudioInstance instance = AudioInstanceFindBook[voiceEntity];

//            instance.VoicePlaybackStatus[voiceEntity.Index] = VoiceState.Stopped;
//            InstanceState instanceState = instance.GetInstanceState();
//            audioOwner.InstanceOccupation[instance] = instanceState;
//            if (instanceState == InstanceState.Stopped)
//                instance.BroadcastPlaybackStatus(PlaybackMessageType.Stopped);

//            PostUpdateCommands.RemoveComponent<AudioMessage_VoiceStopped>(voiceEntity);
//        }

//        for (int i = 0; i < _audioMutedGroup.Length; i++)
//        {
//            Entity voiceEntity = _audioMutedGroup.Muted[i].VoiceEntity;
//            AudioOwner audioOwner = AudioOwnerFindBook[voiceEntity];
//            AudioInstance instance = AudioInstanceFindBook[voiceEntity];

//            instance.VoicePlaybackStatus[voiceEntity.Index] = VoiceState.Virtual;
//            InstanceState instanceState = instance.GetInstanceState();
//            audioOwner.InstanceOccupation[instance] = instanceState;
//            if (instanceState == InstanceState.FullyMuted)
//                instance.BroadcastPlaybackStatus(PlaybackMessageType.Muted);

//            PostUpdateCommands.RemoveComponent<AudioMessage_VoiceMuted>(voiceEntity);
//        }

//        for (int i = 0; i < _audioUnmutedGroup.Length; i++)
//        {
//            Entity voiceEntity = _audioUnmutedGroup.Unmuted[i].VoiceEntity;
//            AudioOwner audioOwner = AudioOwnerFindBook[voiceEntity];
//            AudioInstance instance = AudioInstanceFindBook[voiceEntity];

//            instance.VoicePlaybackStatus[voiceEntity.Index] = VoiceState.Real;
//            InstanceState instanceState = instance.GetInstanceState();
//            audioOwner.InstanceOccupation[instance] = instanceState;
//            if (instanceState == InstanceState.FullyPlaying)
//                instance.BroadcastPlaybackStatus(PlaybackMessageType.Unmuted);

//            PostUpdateCommands.RemoveComponent<AudioMessage_VoiceUnmuted>(voiceEntity);
//        }
//    }
//}
