using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;

public enum PlayType { NeedSource, ReadyToPlay, Play, Stop }
public enum VoiceStatusType { Real, Virtual }
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
        VoiceStatus = id != -1 ? VoiceStatusType.Real : VoiceStatusType.Virtual;
        Priority = priority;
        PlayStatus = status;
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

public struct AudioProperty: IComponentData
{
    public double StartTime;

    public AudioProperty(double startTime)
    {
        StartTime = startTime;
    }
}

