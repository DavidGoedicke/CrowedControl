using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Physics.Authoring;


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
}


[RequireComponent(typeof(PhysicsShapeAuthoring))]
[RequireComponent(typeof(PhysicsBodyAuthoring))]
public class AgentAuthoring : MonoBehaviour
{
    [Header("Agent MaxSpeed")]
    // Fields are used to populate Entity data
    public float speed = 3.0f;

    [Header("Agent Target Gate")] public GateNums SelectTargetGate = GateNums.A;

    [Header("Viewing Distance")] public float ViewingDistance = 300;

    public CollisionFilter ViewingFilter = new CollisionFilter
    {
        BelongsTo = 1u << 2,
        CollidesWith = 3u,
        GroupIndex = 0
    }; // TODO: This is by handAnd should be selected or automated

    public class Baker : Baker<AgentAuthoring>
    {
        public override void Bake(AgentAuthoring m)
        {
            Debug.Log("Bake one unit");
            // This simple baker adds just one component to the entity.
            AddComponent(new AgentConfiguration
            {
                Speed = m.speed,
                TargetGate = m.SelectTargetGate,
                ViewingDistance = m.ViewingDistance,
                ViewingFilter = m.ViewingFilter
            });

            AddComponent(new WasBornTag());
        }
    }
}