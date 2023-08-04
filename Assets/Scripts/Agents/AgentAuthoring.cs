using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Entities.Hybrid.Baking;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Physics.Authoring;
using Random = Unity.Mathematics.Random;


public struct WallAvoidVector : IComponentData
{
    public float2 Value;
}

public struct WasBornTag : IComponentData
{
}

public struct WalkingTag : IComponentData
{
}

public struct ArrivedTag : IComponentData
{
}

public struct StartGateEntity : IComponentData
{
    public Entity Value;
}

//public struct AgentPosition : IComponentData {  public float3 Value; }
//public struct AgentDirection : IComponentData { public float3 Value; }
public struct ApplyImpulse : IComponentData
{
    public float3 Direction;
}


public struct BoidJobResults: IComponentData
{
    public float3 avoid;
    public float3 algin;
    public float3 group;
    
    public float3 avoid_OPP;
    public float3 algin_OPP;
    public float3 group_OPP;
   
}
public struct GateJobResults: IComponentData
{
    public float3 Direction;
   
}


public struct AgentConfiguration : IComponentData
{
    public float Speed;
    public GateNums TargetGate;
    public float ViewingDistance;
    public CollisionFilter ViewingFilter;
    public float Lazyness;
}

public struct AgentLazyness :  IComponentData
{
    public float currentLazyness;
    public bool active;
}


public class AgentAuthoring : MonoBehaviour
{

    public static float MaxGroupingRadius = 10f;
    [Header("Agent MaxSpeed")]
    // Fields are used to populate Entity data
    public float speed = 3.0f;

    [Header("Agent Target Gate")] public GateNums SelectTargetGate = GateNums.A;

    [Header("Viewing Distance")] public float ViewingDistance = 600;

    [Header("TimeBetweenUpdates")] public float Lazyness = 0.1f;
    public static CollisionFilter ViewingFilter = new CollisionFilter
    {
        BelongsTo = 1u <<2,
        CollidesWith = 3u,
        GroupIndex = 0
    }; // TODO: This is by handAnd should be selected or automated

    public class AgentBaker : Baker<AgentAuthoring>
    {
        Random rand =  Random.CreateFromIndex(0);
        public override void Bake(AgentAuthoring m)
        {
           // Debug.Log("Bake one unit");
            // This simple baker adds just one component to the entity.
            Entity entity = GetEntity(m, TransformUsageFlags.Dynamic);
            AddComponent(entity,new AgentConfiguration
            {
                Speed = m.speed,
                TargetGate = m.SelectTargetGate,
                ViewingDistance = m.ViewingDistance,
                ViewingFilter = ViewingFilter,
                Lazyness = m.Lazyness +rand.NextFloat(-0.1f,0.1f)
            });
            
            AddComponent(entity,new WasBornTag());
            AddComponent(entity,new AgentLazyness{currentLazyness= 0});
        }
    }
     
   
    

    
    
}