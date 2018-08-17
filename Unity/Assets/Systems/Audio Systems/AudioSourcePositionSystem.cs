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

    [Inject] ComponentDataFromEntity<Position> _sourcePosition;
    [Inject] ComponentDataFromEntity<Position> _gameEntityPosition;

    protected override void OnUpdate()
    {
        for (int i = 0; i < _realVoiceGroup.Length; i++)
        {
            _sourcePosition[_realVoiceGroup.RealVoices[i].SourceEntity] = _gameEntityPosition[_realVoiceGroup.VoiceHandles[i].GameEntity];
        }
    }
}

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
