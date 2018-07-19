using Unity.Entities;
using Unity.Mathematics;

public struct AudioSourceID : IComponentData
{
    public Entity OriginalEntity;
    public Entity HandleEntity;
    public int ASID;

    public AudioSourceID(Entity originalEntity, Entity handleEntity, int audioSourceID)
    {
        OriginalEntity = originalEntity;
        HandleEntity = handleEntity; //THIS LINE! TOOK ME FOREVER! GOODBYE DICTIONARY!
        ASID = audioSourceID;
    }
}

public enum PlaybackStateType { NeedSource, ReadyToPlay, Play, Mute, Stop }
public struct PlaybackState : IComponentData
{
    public PlaybackStateType State;

    public PlaybackState(PlaybackStateType state)
    {
        State = state;
    }
}

public struct PlaySoundRequest : IComponentData
{
    public Entity Entity;

    public PlaySoundRequest(Entity entity)
    {
        Entity = entity;
    }
}
public struct StopSoundRequest : ISharedComponentData { }
public struct MuteSoundRequest : ISharedComponentData { }
public struct ReadyToPlay : ISharedComponentData { }
public struct AudioPlaying : ISharedComponentData { }

public struct StartTime : IComponentData
{
    public double Time;

    public StartTime(double time)
    {
        Time = time;
    }
}

public struct AudioClipID : IComponentData
{
    public int ID;

    public AudioClipID(int id)
    {
        ID = id;
    }
}

public struct AudioProperty_SpatialBlend : IComponentData
{
    public float Blend;

    public AudioProperty_SpatialBlend(float blend)
    {
        Blend = math.clamp(blend, 0, 1);
    }
}

public struct AudioSourceHandle : IComponentData
{
    public Entity Entity;
    public int ASID;

    public AudioSourceHandle(Entity entity, int asid)
    {
        Entity = entity;
        ASID = asid;
    }
}

public struct AudioSourceClaimed : ISharedComponentData { }

//public enum PlayType { NeedSource, ReadyToPlay, Play, Mute, Stop }
//public enum VoiceStatusType { Real, Virtual, Zero }
//public enum PriorityType { Low, Medium, High }
//public struct AudioSourceID : IComponentData
//{
//    public Entity EntityID;
//    public int ASID;
//    public VoiceStatusType VoiceStatus;
//    public PriorityType Priority;
//    public PlayType PlayStatus;

//    public AudioSourceID(Entity entity, int id)
//    {
//        EntityID = entity;
//        ASID = id;
//        Priority = priority;
//        PlayStatus = status;

//        if (status == PlayType.Mute || status == PlayType.NeedSource)
//            VoiceStatus = VoiceStatusType.Virtual;
//        else if (status == PlayType.Stop)
//            VoiceStatus = VoiceStatusType.Zero;
//        else
//            VoiceStatus = VoiceStatusType.Real;
//    }
//}

//public struct PlaySoundRequest : IComponentData
//{
//    public int AudioClipID;

//    public PlaySoundRequest(int value)
//    {
//        AudioClipID = value;
//    }
//}

//public struct StopSoundRequest : IComponentData { }
//public struct MuteSoundRequest : IComponentData { }

//public struct AudioProperty : IComponentData
//{
//    public double StartTime;
//    public int AudioClipID;

//    public AudioProperty(double startTime, int clipID)
//    {
//        StartTime = startTime;
//        AudioClipID = clipID;
//    }
//}

