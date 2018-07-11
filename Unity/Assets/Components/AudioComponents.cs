using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;

public enum PlayType { NeedSource, ReadyToPlay, Play, Mute, Stop }
public enum VoiceStatusType { Real, Virtual, Zero }
public enum PriorityType { Low, Medium, High }
public struct AudioSourceID : IComponentData
{
    public Entity EntityID;
    public int ASID;
    public VoiceStatusType VoiceStatus;
    public PriorityType Priority;
    public PlayType PlayStatus;

    public AudioSourceID(Entity entity, int id, PriorityType priority, PlayType status)
    {
        EntityID = entity;
        ASID = id;
        Priority = priority;
        PlayStatus = status;

        if (status == PlayType.Mute || status == PlayType.NeedSource)
            VoiceStatus = VoiceStatusType.Virtual;
        else if (status == PlayType.Stop)
            VoiceStatus = VoiceStatusType.Zero;
        else
            VoiceStatus = VoiceStatusType.Real;
    }
}

public struct AudioClipID : IComponentData
{
    public int ACID;

    public AudioClipID(int value)
    {
        ACID = value;
    }
}

public struct StopSoundRequest : IComponentData { }
public struct MuteSoundRequest : IComponentData { }

public struct AudioProperty: IComponentData
{
    public double StartTime;

    public AudioProperty(double startTime)
    {
        StartTime = startTime;
    }
}

