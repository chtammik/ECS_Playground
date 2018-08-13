using Unity.Entities;
using Unity.Mathematics;

public struct AudioSourceID : IComponentData
{
    public Entity GameEntity;
    public Entity SourceEntity;

    public AudioSourceID(Entity carrierEntity, Entity handleEntity)
    {
        GameEntity = carrierEntity;
        SourceEntity = handleEntity; //THIS LINE! TOOK ME FOREVER! GOODBYE DICTIONARY!
    }
}

public struct AudioPlayRequest : IComponentData
{
    public Entity Entity;

    public AudioPlayRequest(Entity entity)
    {
        Entity = entity;
    }
}
public struct AudioStopRequest : ISharedComponentData { }
public struct AudioMuteRequest : ISharedComponentData { }
public struct AudioReadyToPlay : ISharedComponentData { }
public struct AudioPlaying : ISharedComponentData { }

public struct AudioPlayingVirtually : IComponentData
{
    public Entity Entity;

    public AudioPlayingVirtually(Entity entity)
    {
        Entity = entity;
    }
}

public struct AudioProperty_StartTime : IComponentData
{
    public Entity Entity;
    public double Time;

    public AudioProperty_StartTime(Entity entity, double time)
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

public struct AudioSourceHandle : IComponentData
{
    public Entity GameEntity;
    public Entity SourceEntity;

    public AudioSourceHandle(Entity gameEntity, Entity sourceEntity)
    {
        GameEntity = gameEntity;
        SourceEntity = sourceEntity;
    }
}

public struct AudioSourceClaimed : ISharedComponentData { }

