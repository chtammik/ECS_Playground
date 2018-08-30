using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[UpdateBefore(typeof(CopyAudioPropertiesSystem.CopyAudioPropertiesBarrier))]
public class AssignAudioSourceSystem : JobComponentSystem
{
    public class AssignSourceIDBarrier : BarrierSystem { }

    struct PlayRequestGroup //all entities that need to play a sound.
    {
        public readonly int Length;
        [ReadOnly] public ComponentDataArray<RealVoiceRequest> RealVoiceRequests;
        [ReadOnly] public SubtractiveComponent<RealVoice> No_RealVoice;
    }
    [Inject] PlayRequestGroup _playRequestGroup;

    struct SourceHandleGroup //all vacant AudioSources
    {
        public readonly int Length;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> SourceHandles;
        [ReadOnly] public SubtractiveComponent<ClaimedByVoice> No_Claimed;
    }
    [Inject] SourceHandleGroup _sourceHandleGroup;

    struct AssignSourceIDJob : IJobParallelFor
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        [ReadOnly] public ComponentDataArray<RealVoiceRequest> PlayRequests;
        [ReadOnly] public ComponentDataArray<AudioSourceHandle> SourceHandles;

        public void Execute(int index)
        {
            //a Voice is hooked with a new AudioSource.
            CommandBuffer.AddComponent(PlayRequests[index].VoiceEntity, new RealVoice(PlayRequests[index].VoiceEntity, SourceHandles[index].SourceEntity));
            CommandBuffer.SetComponent(SourceHandles[index].SourceEntity, new AudioSourceHandle(SourceHandles[index].SourceEntity));

            //the vacant AudioSource is now claimed.
            CommandBuffer.AddComponent(SourceHandles[index].SourceEntity, new ClaimedByVoice(PlayRequests[index].VoiceEntity));

            //the voice entity is no longer requiring to obtain a voice.
            CommandBuffer.RemoveComponent<RealVoiceRequest>(PlayRequests[index].VoiceEntity);
        }
    }

    [Inject] AssignSourceIDBarrier _assignSourceIDBarrier;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        int jobAmount = math.min(_playRequestGroup.Length, _sourceHandleGroup.Length); //always try to meet all the demands with all the available sources.
        var assignSourceIDJob = new AssignSourceIDJob
        {
            CommandBuffer = _assignSourceIDBarrier.CreateCommandBuffer(),
            PlayRequests = _playRequestGroup.RealVoiceRequests,
            SourceHandles = _sourceHandleGroup.SourceHandles
        };
        JobHandle assignSourceIDJH = assignSourceIDJob.Schedule(jobAmount, 64, inputDeps);

        return assignSourceIDJH;
    }
}

//public class AudioPoolSystem : ComponentSystem
//{
//    struct PlayRequestGroup
//    {
//        public readonly int Length;
//        public EntityArray Entities;
//        [ReadOnly] public ComponentDataArray<PlaySoundRequest> PlayRequests;
//        public SubtractiveComponent<AudioSourceID> Voices;
//    }

//    [Inject] PlayRequestGroup playRequestGroup;

//    struct SourceHandleGroup
//    {
//        public readonly int Length;
//        public EntityArray Entities;
//        [ReadOnly] public ComponentDataArray<AudioSourceHandle> SourceHandles;
//        public SubtractiveComponent<AudioSourceClaimed> Claimeds;
//    }

//    [Inject] SourceHandleGroup sourceHandleGroup;

//    protected override void OnUpdate()
//    {
//        int HandleAmount = sourceHandleGroup.Length;
//        for (int i = 0; i < playRequestGroup.Length; i++)
//        {
//            if (HandleAmount > 0)
//            {
//                int HandleIndex = HandleAmount - 1;
//                Entity entity = playRequestGroup.Entities[i];
//                PostUpdateCommands.AddComponent(entity, new AudioSourceID(entity, sourceHandleGroup.SourceHandles[HandleIndex].Voice));
//                PostUpdateCommands.RemoveComponent<PlaySoundRequest>(entity);
//                PostUpdateCommands.AddComponent(entity, new ReadyToPlay());
//                PostUpdateCommands.AddComponent(sourceHandleGroup.Entities[HandleIndex], new AudioSourceClaimed());
//                --HandleAmount;
//            }
//        }
//    }
//}
