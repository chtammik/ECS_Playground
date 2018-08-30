using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;

public struct VoiceHandle : IComponentData
{
    public Entity InstanceEntity;
    public Entity VoiceEntity;
    
    public VoiceHandle(Entity instanceEntity, Entity voiceEntity)
    {
        InstanceEntity = instanceEntity;
        VoiceEntity = voiceEntity;
    }
}

public struct RealVoice : IComponentData
{
    public Entity VoiceEntity;
    public Entity SourceEntity;

    public RealVoice(Entity voiceEntity, Entity sourceEntity)
    {
        VoiceEntity = voiceEntity;
        SourceEntity = sourceEntity; //THIS LINE! TOOK ME FOREVER! GOODBYE DICTIONARY!
    }
}

public struct VirtualVoice : IComponentData
{
    public Entity VoiceEntity;

    public VirtualVoice(Entity voiceEntity)
    {
        VoiceEntity = voiceEntity;
    }
}

public struct AudioSourceHandle : IComponentData
{
    public Entity SourceEntity;

    public AudioSourceHandle(Entity sourceEntity)
    {
        SourceEntity = sourceEntity;
    }
}

public struct ClaimedByVoice : IComponentData
{
    public Entity VoiceEntity;

    public ClaimedByVoice(Entity voiceEntity)
    {
        VoiceEntity = voiceEntity;
    }
}

public struct InstanceHandle : IComponentData
{
    public Entity GameEntity;
    public int VoiceCount;

    public InstanceHandle(Entity gameEntity, int voiceCount)
    {
        GameEntity = gameEntity;
        VoiceCount = voiceCount;
    }
}

public struct InstanceClaimed : IComponentData
{
    public int PlayingVoiceCount;
    public int VirtualVoiceCount;

    public InstanceClaimed(int playingVoiceCount, int virtualVoiceCount)
    {
        PlayingVoiceCount = playingVoiceCount;
        VirtualVoiceCount = virtualVoiceCount;
    }
}
public struct InstanceMuted : IComponentData
{
    public Entity InstanceEntity;

    public InstanceMuted(Entity instanceEntity)
    {
        InstanceEntity = instanceEntity;
    }
}

public struct RealVoiceRequest : IComponentData
{
    public Entity VoiceEntity;

    public RealVoiceRequest(Entity voiceEntity)
    {
        VoiceEntity = voiceEntity;
    }
}
public struct StopRequest : ISharedComponentData { }
public struct MuteRequest : ISharedComponentData { }
public struct ReadToPlay : ISharedComponentData { }
public struct Playing : ISharedComponentData { } //different from RealVoice, which is on the voice entity, Playing is on the source entity.

public struct DSPTimeOnPlay : IComponentData
{
    public Entity Entity;
    public double Time;

    public DSPTimeOnPlay(Entity entity, double time)
    {
        Entity = entity;
        Time = time;
    }
}

public struct AudioProperty_AudioClipID : IComponentData
{
    public Entity Entity;
    public int ID;

    public AudioProperty_AudioClipID(Entity entity, int id)
    {
        Entity = entity;
        ID = id;
    }
}

public struct AudioProperty_SpatialBlend : IComponentData
{
    public Entity Entity;
    public float Blend;

    public AudioProperty_SpatialBlend(Entity entity, float blend)
    {
        Entity = entity;
        Blend = math.clamp(blend, 0, 1);
    }
}

public struct AudioProperty_Loop : IComponentData
{
    public Entity Entity;

    public AudioProperty_Loop(Entity entity)
    {
        Entity = entity;
    }
}

public struct AudioMessage_InstancePlayed : IComponentData
{
    public Entity InstanceEntity;

    public AudioMessage_InstancePlayed(Entity instanceEntity)
    {
        InstanceEntity = instanceEntity;
    }
}

public struct AudioMessage_InstanceStopped : IComponentData
{
    public Entity InstanceEntity;

    public AudioMessage_InstanceStopped(Entity instanceEntity)
    {
        InstanceEntity = instanceEntity;
    }
}

public struct AudioMessage_InstanceMuted : IComponentData
{
    public Entity InstanceEntity;

    public AudioMessage_InstanceMuted(Entity instanceEntity)
    {
        InstanceEntity = instanceEntity;
    }
}

public struct AudioMessage_InstanceUnmuted : IComponentData
{
    public Entity InstanceEntity;

    public AudioMessage_InstanceUnmuted(Entity instanceEntity)
    {
        InstanceEntity = instanceEntity;
    }
}




