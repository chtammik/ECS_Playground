using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using System.Collections.Generic;
using System;

[UpdateBefore(typeof(AssignAudioSourceSystem.AssignSourceIDBarrier))]
public class AudioStopVirtualSystem : ComponentSystem
{
    struct ToStopVirtualGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        [ReadOnly] public SharedComponentDataArray<StopRequest> StopRequests;
        [ReadOnly] public ComponentDataArray<RealVoiceRequest> RealVoiceRequests;
        [ReadOnly] public SubtractiveComponent<RealVoice> No_RealVoice;
        [ReadOnly] public ComponentDataArray<VirtualVoice> VirtualVoices;
        [ReadOnly] public ComponentDataArray<VoiceHandle> VoiceHandles;
    }
    [Inject] ToStopVirtualGroup _toStopVirtualGroup;

    struct ToStopMutedGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        [ReadOnly] public SharedComponentDataArray<StopRequest> StopRequests;
        [ReadOnly] public SubtractiveComponent<RealVoiceRequest> No_RealVoiceRequest;
        [ReadOnly] public SubtractiveComponent<RealVoice> No_RealVoice;
        [ReadOnly] public ComponentDataArray<VirtualVoice> VirtualVoices;
        [ReadOnly] public ComponentDataArray<VoiceHandle> VoiceHandles;
    }
    [Inject] ToStopMutedGroup _toStopMutedGroup;

    struct VirtualDonePlayingGroup
    {
        public readonly int Length;
        public EntityArray Entities;
        [ReadOnly] public ComponentDataArray<VoiceHandle> VoiceHandles;
        [ReadOnly] public SubtractiveComponent<StopRequest> No_StopRequest;
        [ReadOnly] public SubtractiveComponent<RealVoice> No_RealVoice;
        [ReadOnly] public SubtractiveComponent<AudioProperty_Loop> No_Loop;
        [ReadOnly] public ComponentDataArray<VirtualVoice> VirtualVoices;
        [ReadOnly] public ComponentDataArray<DSPTimeOnPlay> TimeOnPlay;
        [ReadOnly] public ComponentDataArray<AudioProperty_AudioClipID> AudioClipID;
    }
    [Inject] VirtualDonePlayingGroup _virtualDonePlayingGroup;

    [Inject] ComponentDataFromEntity<InstanceClaimed> _instanceClaimed;
    [Inject] ComponentDataFromEntity<InstanceHandle> _instanceHandle;

    protected override void OnUpdate()
    {
        Dictionary<Entity, Tuple<int, int>> cachedCount = new Dictionary<Entity, Tuple<int, int>>();

        for (int i = 0; i < _toStopVirtualGroup.Length; i++)
        {
            Entity voiceEntity = _toStopVirtualGroup.VirtualVoices[i].VoiceEntity;
            PostUpdateCommands.RemoveComponent<StopRequest>(voiceEntity);
            PostUpdateCommands.RemoveComponent<VirtualVoice>(voiceEntity);
            PostUpdateCommands.RemoveComponent<RealVoiceRequest>(voiceEntity);

            Entity instanceEntity = _toStopVirtualGroup.VoiceHandles[i].InstanceEntity;
            int previousPlayingCount = _instanceClaimed[instanceEntity].PlayingVoiceCount;
            int previousVirtualCount = _instanceClaimed[instanceEntity].VirtualVoiceCount;
            int totalVoiceCount = _instanceHandle[instanceEntity].VoiceCount;
            if (totalVoiceCount == 1)
            {
                PostUpdateCommands.RemoveComponent<InstanceClaimed>(instanceEntity);
                PostUpdateCommands.AddComponent(instanceEntity, new AudioMessage_InstanceStopped(instanceEntity));
            }
            else
            {
                if (cachedCount.TryGetValue(instanceEntity, out Tuple<int, int> voiceInfo))
                    cachedCount[instanceEntity] = new Tuple<int, int>(voiceInfo.Item1 - 1, voiceInfo.Item2 - 1);
                else
                    cachedCount.Add(instanceEntity, new Tuple<int, int>(previousPlayingCount - 1, previousVirtualCount - 1));
            }
        }

        for (int i = 0; i < _toStopMutedGroup.Length; i++)
        {
            Entity voiceEntity = _toStopMutedGroup.VirtualVoices[i].VoiceEntity;
            PostUpdateCommands.RemoveComponent<StopRequest>(voiceEntity);
            PostUpdateCommands.RemoveComponent<VirtualVoice>(voiceEntity);

            Entity instanceEntity = _toStopMutedGroup.VoiceHandles[i].InstanceEntity;
            int previousPlayingCount = _instanceClaimed[instanceEntity].PlayingVoiceCount;
            int previousVirtualCount = _instanceClaimed[instanceEntity].VirtualVoiceCount;
            int totalVoiceCount = _instanceHandle[instanceEntity].VoiceCount;
            if (totalVoiceCount == 1)
            {
                PostUpdateCommands.RemoveComponent<InstanceClaimed>(instanceEntity);
                PostUpdateCommands.AddComponent(instanceEntity, new AudioMessage_InstanceStopped(instanceEntity));
            }
            else
            {
                if (cachedCount.TryGetValue(instanceEntity, out Tuple<int, int> voiceInfo))
                    cachedCount[instanceEntity] = new Tuple<int, int>(voiceInfo.Item1 - 1, voiceInfo.Item2 - 1);
                else
                    cachedCount.Add(instanceEntity, new Tuple<int, int>(previousPlayingCount - 1, previousVirtualCount - 1));
            }
        }

        foreach (KeyValuePair<Entity, Tuple<int, int>> pair in cachedCount)
        {
            Entity instanceEntity = pair.Key;
            int newPlayingCount = pair.Value.Item1;
            int newVirtualCount = pair.Value.Item2;

            if (newPlayingCount == 0)
            {
                PostUpdateCommands.RemoveComponent<InstanceClaimed>(instanceEntity);
                PostUpdateCommands.AddComponent(instanceEntity, new AudioMessage_InstanceStopped(instanceEntity));
            }
        }

        for (int i = 0; i < _virtualDonePlayingGroup.Length; i++)
        {
            if (AudioSettings.dspTime - _virtualDonePlayingGroup.TimeOnPlay[i].Time > AudioService.GetClipLength(_virtualDonePlayingGroup.AudioClipID[i].ID)) //TODO: checking this every frame is not ideal, any better way to do this?
                PostUpdateCommands.AddSharedComponent(_virtualDonePlayingGroup.VirtualVoices[i].VoiceEntity, new StopRequest());
        }
    }
}

//Job version, not working correctly and it's actually slow.
//[UpdateBefore(typeof(AssignAudioSourceSystem.AssignSourceIDBarrier))]
//public class AudioStopVirtualSystem : JobComponentSystem
//{
//    NativeArray<JobHandle> _jobHandles;

//    public class StopVirtualBarrier : BarrierSystem { }

//    //stop virtual voices that are still trying to get a RealVoice.
//    [RequireComponentTag(typeof(StopRequest), typeof(RealVoiceRequest))]
//    [RequireSubtractiveComponent(typeof(RealVoice))]
//    struct StopVirtualJob : IJobProcessComponentData<VirtualVoice, VoiceHandle>
//    {
//        public EntityCommandBuffer.Concurrent CommandBuffer;
//        [ReadOnly] public ComponentDataFromEntity<InstanceClaimed> InstanceClaimed;

//        public void Execute([ReadOnly]ref VirtualVoice virtualVoice, [ReadOnly]ref VoiceHandle voiceHandle)
//        {
//            Entity voiceEntity = virtualVoice.VoiceEntity;
//            CommandBuffer.RemoveComponent<StopRequest>(voiceEntity);
//            CommandBuffer.RemoveComponent<VirtualVoice>(voiceEntity);
//            CommandBuffer.RemoveComponent<RealVoiceRequest>(voiceEntity);

//            Entity instanceEntity = voiceHandle.InstanceEntity;
//            int previousPlayingCount = InstanceClaimed[instanceEntity].PlayingVoiceCount;
//            int previousVirtualCount = InstanceClaimed[instanceEntity].VirtualVoiceCount;
//            if (previousPlayingCount - 1 == 0)
//            {
//                CommandBuffer.RemoveComponent<InstanceClaimed>(instanceEntity);
//                CommandBuffer.AddComponent(instanceEntity, new AudioMessage_InstanceStopped(instanceEntity));
//            }
//            else
//                CommandBuffer.SetComponent(instanceEntity, new InstanceClaimed(previousPlayingCount - 1, previousVirtualCount - 1));
//        }
//    }

//    //stop virtual voices that are not trying to get RealVoice.
//    [RequireComponentTag(typeof(StopRequest))]
//    [RequireSubtractiveComponent(typeof(RealVoice), typeof(RealVoiceRequest))]
//    struct StopMutedJob : IJobProcessComponentData<VirtualVoice, VoiceHandle>
//    {
//        public EntityCommandBuffer.Concurrent CommandBuffer;
//        [ReadOnly] public ComponentDataFromEntity<InstanceClaimed> InstanceClaimed;

//        public void Execute([ReadOnly]ref VirtualVoice virtualVoice, [ReadOnly]ref VoiceHandle voiceHandle)
//        {
//            Entity voiceEntity = virtualVoice.VoiceEntity;
//            CommandBuffer.RemoveComponent<StopRequest>(voiceEntity);
//            CommandBuffer.RemoveComponent<VirtualVoice>(voiceEntity);

//            Entity instanceEntity = voiceHandle.InstanceEntity;
//            int previousPlayingCount = InstanceClaimed[instanceEntity].PlayingVoiceCount;
//            int previousVirtualCount = InstanceClaimed[instanceEntity].VirtualVoiceCount;
//            if (previousPlayingCount - 1 == 0)
//            {
//                CommandBuffer.RemoveComponent<InstanceClaimed>(instanceEntity);
//                CommandBuffer.AddComponent(instanceEntity, new AudioMessage_InstanceStopped(instanceEntity));
//            }
//            else
//                CommandBuffer.SetComponent(instanceEntity, new InstanceClaimed(previousPlayingCount - 1, previousVirtualCount - 1));
//        }
//    }

//    //add a StopRequest to virtual voices that have done playing.
//    [RequireComponentTag(typeof(VoiceHandle))]
//    [RequireSubtractiveComponent(typeof(RealVoice), typeof(StopRequest), typeof(AudioProperty_Loop))]
//    struct StopVirtualDonePlayingJob : IJobProcessComponentData<VirtualVoice, DSPTimeOnPlay, AudioProperty_AudioClipID>
//    {
//        public EntityCommandBuffer.Concurrent CommandBuffer;

//        public void Execute([ReadOnly]ref VirtualVoice audioPlayingVirtually, [ReadOnly]ref DSPTimeOnPlay timeOnPlay, [ReadOnly]ref AudioProperty_AudioClipID audioClip)
//        {
//            if (AudioSettings.dspTime - timeOnPlay.Time > AudioService.GetClipLength(audioClip.ID))
//                CommandBuffer.AddSharedComponent(audioPlayingVirtually.VoiceEntity, new StopRequest());
//        }
//    }

//    protected override void OnStartRunning()
//    {
//        _jobHandles = new NativeArray<JobHandle>(3, Allocator.Persistent);
//    }

//    protected override void OnStopRunning()
//    {
//        _jobHandles.Dispose();
//    }

//    [Inject] StopVirtualBarrier _stopVirtualBarrier;
//    [Inject] ComponentDataFromEntity<InstanceClaimed> _instanceClaimed;

//    protected override JobHandle OnUpdate(JobHandle inputDeps)
//    {
//        var stopVirtualJob = new StopVirtualJob { CommandBuffer = _stopVirtualBarrier.CreateCommandBuffer(), InstanceClaimed = _instanceClaimed };
//        var stopMutedJob = new StopMutedJob { CommandBuffer = _stopVirtualBarrier.CreateCommandBuffer(), InstanceClaimed = _instanceClaimed };
//        var stopVirtualDonePlayingJob = new StopVirtualDonePlayingJob { CommandBuffer = _stopVirtualBarrier.CreateCommandBuffer() };

//        _jobHandles[0] = stopVirtualJob.Schedule(this, inputDeps);
//        _jobHandles[1] = stopMutedJob.Schedule(this, inputDeps);
//        _jobHandles[2] = stopVirtualDonePlayingJob.Schedule(this, 64, inputDeps);

//        return JobHandle.CombineDependencies(_jobHandles);
//    }
//}
