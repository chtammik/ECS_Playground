using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateBefore(typeof(AssignAudioSourceIDSystem.AssignSourceIDBarrier))]
public class AudioMuteSystem : ComponentSystem
{
    struct AudioSourceGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        [ReadOnly] public ComponentArray<AudioSource> AudioSources;
        [ReadOnly] public SharedComponentDataArray<AudioMuteRequest> VirtualizeRequests;
        [ReadOnly] public SharedComponentDataArray<AudioPlaying> Playing;
        [ReadOnly] public ComponentDataArray<ClaimedByVoice> Claimed;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> Handles;
    }
    [Inject] AudioSourceGroup _audioSourceGroup;

    [Inject] ComponentDataFromEntity<AudioPlayRequest> _playRequest;
    [Inject] ComponentDataFromEntity<AudioProperty_AudioClipID> _audioClip;
    [Inject] ComponentDataFromEntity<AudioProperty_SpatialBlend> _audioSpatialBlend;
    [Inject] ComponentDataFromEntity<DSPTimeOnPlay> _timeOnPlay;
    [Inject] ComponentDataFromEntity<AudioProperty_Loop> _audioLoops;

    protected override void OnUpdate()
    {
        for (int i = 0; i < _audioSourceGroup.Length; i++)
        {
            Entity sourceEntity = _audioSourceGroup.Entities[i];
            Entity voiceEntity = _audioSourceGroup.Claimed[i].VocieEntity;
            AudioSource audioSource = _audioSourceGroup.AudioSources[i];
            audioSource.Stop();
            AudioService.ResetAudioSource(audioSource);
            PostUpdateCommands.RemoveComponent<AudioMuteRequest>(sourceEntity);
            PostUpdateCommands.RemoveComponent<AudioPlaying>(sourceEntity);
            PostUpdateCommands.RemoveComponent<ClaimedByVoice>(sourceEntity);

            if (_playRequest.Exists(sourceEntity))
                PostUpdateCommands.RemoveComponent<AudioPlayRequest>(sourceEntity);

            if (_timeOnPlay.Exists(sourceEntity))
            {
                PostUpdateCommands.AddComponent(voiceEntity, new DSPTimeOnPlay(voiceEntity, _timeOnPlay[sourceEntity].Time));
                PostUpdateCommands.RemoveComponent<DSPTimeOnPlay>(sourceEntity);
            }

            //copy Audio Properties back to voice handles for the later devirtualizing.
            #region Copy Properties
            if (_audioClip.Exists(sourceEntity))
            {
                PostUpdateCommands.AddComponent(voiceEntity, new AudioProperty_AudioClipID(voiceEntity, _audioClip[sourceEntity].ID));
                PostUpdateCommands.RemoveComponent<AudioProperty_AudioClipID>(sourceEntity);
            }

            if (_audioSpatialBlend.Exists(sourceEntity))
            {
                PostUpdateCommands.AddComponent(voiceEntity, new AudioProperty_SpatialBlend(voiceEntity, _audioSpatialBlend[sourceEntity].Blend));
                PostUpdateCommands.RemoveComponent<AudioProperty_SpatialBlend>(sourceEntity);
            }

            if (_audioLoops.Exists(sourceEntity))
            {
                PostUpdateCommands.AddComponent(voiceEntity, new AudioProperty_Loop(voiceEntity));
                PostUpdateCommands.RemoveComponent<AudioProperty_Loop>(sourceEntity);
            }

            //...
            #endregion

            PostUpdateCommands.AddComponent(voiceEntity, new VirtualVoice(voiceEntity));
            PostUpdateCommands.AddComponent(voiceEntity, new AudioMessage_Muted(voiceEntity));
        }
    }
}
