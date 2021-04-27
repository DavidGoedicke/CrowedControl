using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Physics.Authoring;



public struct WasBornTag : IComponentData{ }
public struct WalkingTag : IComponentData{ }
public struct ArrivedTag : IComponentData{ }

//public struct AgentPosition : IComponentData {  public float3 Value; }
//public struct AgentDirection : IComponentData { public float3 Value; }
public struct ApplyImpulse : IComponentData {  public float3 Direction;}

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

    [Header("Agent Target Gate")]
    public GateNums SelectTargetGate = GateNums.A;

    [Header("Viewing Distance")]
    public float ViewingDistance = 300;
    
    public CollisionFilter ViewingFilter = new CollisionFilter {
        BelongsTo = 1u<<3 ,
        CollidesWith= 3u,
        GroupIndex = 0
    }; // TODO: This is by handAnd should be selected or automated
    
}



[UpdateAfter(typeof(EndColliderConversionSystem))]
[UpdateAfter(typeof(PhysicsBodyConversionSystem))]
[DisallowMultipleComponent]
public class AgentConversion : GameObjectConversionSystem
{

    protected override void OnUpdate()
    {
        Entities.ForEach((AgentAuthoring m) =>
        {
            var entity = GetPrimaryEntity(m);

            DstEntityManager.AddComponents(entity, new ComponentTypes(new ComponentType[] {
                typeof(WasBornTag),
                typeof(ApplyImpulse)
        }));


            DstEntityManager.AddComponentData(entity, new AgentConfiguration {
                Speed = m.speed,
                TargetGate = m.SelectTargetGate,
                ViewingDistance = m.ViewingDistance,
                ViewingFilter = m.ViewingFilter
            }
                );
          

        });
    }
}
