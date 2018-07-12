using Unity.Entities;

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

public struct PlaySoundRequest : IComponentData
{
    public int AudioClipID;

    public PlaySoundRequest(int value)
    {
        AudioClipID = value;
    }
}

public struct StopSoundRequest : IComponentData { }
public struct MuteSoundRequest : IComponentData { }

public struct AudioProperty : IComponentData
{
    public double StartTime;
    public int AudioClipID;

    public AudioProperty(double startTime, int clipID)
    {
        StartTime = startTime;
        AudioClipID = clipID;
    }
}

