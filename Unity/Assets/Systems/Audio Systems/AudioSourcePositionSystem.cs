using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

//Non-Job version
public class AudioSourcePositionSystem : ComponentSystem
{
    struct RealVoiceGroup
    {
        public readonly int Length;
        [ReadOnly] public ComponentDataArray<RealVoice> RealVoices;
        [ReadOnly] public ComponentDataArray<VoiceHandle> VoiceHandles;
    }
    [Inject] RealVoiceGroup _realVoiceGroup;

    [Inject] ComponentDataFromEntity<Position> _position;
    [Inject] ComponentDataFromEntity<InstanceHandle> _instanceHandle;

    protected override void OnUpdate()
    {
        for (int i = 0; i < _realVoiceGroup.Length; i++)
        {
            Entity gameEntity = _instanceHandle[_realVoiceGroup.VoiceHandles[i].InstanceEntity].GameEntity;
            _position[_realVoiceGroup.RealVoices[i].SourceEntity] = _position[gameEntity];
        }
    }
}

//twice slower
//public class AudioSourcePositionSystem : JobComponentSystem
//{
//    [BurstCompile]
//    struct AudioSourcePositionJob : IJobProcessComponentData<RealVoice, VoiceHandle>
//    {
//        [ReadOnly] public ComponentDataFromEntity<InstanceHandle> InstanceHandle;
//        public ComponentDataFromEntity<Position> Position;

//        public void Execute([ReadOnly]ref RealVoice realVoice, [ReadOnly]ref VoiceHandle voiceHandle)
//        {
//            Entity gameEntity = InstanceHandle[voiceHandle.InstanceEntity].GameEntity;
//            Position[realVoice.SourceEntity] = Position[gameEntity];
//        }
//    }

//    [Inject] ComponentDataFromEntity<InstanceHandle> _instanceHandle;
//    [Inject] ComponentDataFromEntity<Position> _position;

//    protected override JobHandle OnUpdate(JobHandle inputDeps)
//    {
//        return new AudioSourcePositionJob
//        {
//            InstanceHandle = _instanceHandle,
//            Position = _position
//        }.Schedule(this, inputDeps);
//    }
//}

//IJob version, actually way slower.
//public class AudioSourcePositionSystem : JobComponentSystem
//{
//    struct VoiceGroup
//    {
//        public readonly int Length;
//        [ReadOnly] public ComponentDataArray<Position> Positions;
//        [ReadOnly] public ComponentDataArray<Voice> Voices;
//    }
//    [Inject] VoiceGroup voiceGroup;

//    [Inject] ComponentDataFromEntity<Position> sourcePosition;

//    struct AudioSourcePositionJob : IJob
//    {
//        [WriteOnly] public Position SourcePosition;
//        [ReadOnly] public Position ASIDPosition;

//        public void Execute()
//        {
//            SourcePosition = ASIDPosition;
//        }
//    }

//    protected override JobHandle OnUpdate(JobHandle inputDeps)
//    {
//        NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(voiceGroup.Length, Allocator.Temp);

//        for (int i = 0; i < voiceGroup.Length; i++)
//        {
//            var audioSourcePositionJob = new AudioSourcePositionJob
//            {
//                SourcePosition = sourcePosition[voiceGroup.Voices[i].SourceEntity],
//                ASIDPosition = voiceGroup.Positions[i]
//            };
//            jobHandles[i] = audioSourcePositionJob.Schedule(inputDeps);
//        }

//        JobHandle combinedDeps = JobHandle.CombineDependencies(jobHandles);
//        jobHandles.Dispose();

//        return combinedDeps;
//    }
//}
