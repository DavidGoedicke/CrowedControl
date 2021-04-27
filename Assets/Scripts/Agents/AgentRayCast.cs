#define DEBUG

using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Collections;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(BuildPhysicsWorld)), UpdateBefore(typeof(StepPhysicsWorld))]
public class AgentRayCast : SystemBase
{
    private BuildPhysicsWorld m_BuildPhysicsWorldSystem;
    private EndSimulationEntityCommandBufferSystem m_EndSimulationEcbSystem;

    protected override void OnCreate()
    {
        m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
        m_EndSimulationEcbSystem = World
           .GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        RequireForUpdate(GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(AgentConfiguration) }
        }));
    }

    EntityQueryDesc ActiveTargetQuery = new EntityQueryDesc
    {
        None = new ComponentType[]
          {
          },
   

        Any = new ComponentType[]
          {

               ComponentType.ReadOnly<ActiveGate>(),
               ComponentType.ReadOnly<ActiveSign>()
          }
    };


    protected override void OnUpdate()
    {

        Dependency = m_BuildPhysicsWorldSystem.GetOutputDependency();

        PhysicsWorld world = m_BuildPhysicsWorldSystem.PhysicsWorld;
        var collisionWorld = world.CollisionWorld;

        var ecb = m_EndSimulationEcbSystem.CreateCommandBuffer();


        EntityQuery m_TargetQuery = GetEntityQuery(ActiveTargetQuery);
        var TargetCount = m_TargetQuery.CalculateEntityCount();

        if (TargetCount == 0) { return; }
        Debug.Log("Target Count"  + TargetCount.ToString());

        var SignMemory2 = new NativeHashMap<int, GateNums>(TargetCount, Allocator.TempJob);
        Entities
            .WithoutBurst()
                   .WithAny<ActiveGate,ActiveSign>()
                   .WithName("AssociatingGateToNum")
                   .ForEach((in Entity en ,in GateNumber gn) =>
                   {
                       
                       SignMemory2[en.GetHashCode()] = gn.value;
                       
                    })
                   .Run();


        Entities
                .WithName("RayCastJob")
                .WithNone<TargetGateEntity>()
                .WithoutBurst() // change back
                .WithReadOnly(collisionWorld)
                .WithReadOnly(SignMemory2)
                .ForEach((
                    Entity entity, in Translation localPosition,
                    in Rotation localRotation,
                    in AgentConfiguration agentConfiguration) =>
                {
                    // create a raycast from the suspension point on the chassis
                    float3 rayStart = localPosition.Value;
                    float3 rayEnd = math.forward(localRotation.Value) * agentConfiguration.ViewingDistance
                    + rayStart;

#if DEBUG
                    
                    Debug.DrawRay(rayStart, rayEnd - rayStart);
#endif



                    var raycastInput = new RaycastInput
                    {
                        Start = rayStart,
                        End = rayEnd,
                        Filter = agentConfiguration.ViewingFilter
                    };

                    var hit = world.CastRay(raycastInput, out var rayResult);

                    if (hit && SignMemory2[rayResult.Entity.GetHashCode()] == agentConfiguration.TargetGate)
                    {

                       Debug.DrawRay(rayStart + (math.forward(localRotation.Value) * agentConfiguration.ViewingDistance) * rayResult.Fraction, Vector3.up, Color.red,5f);

                       // Debug.Log(rayResult.Entity.Index+"  "+ entity.Index );
                       // ecb.DestroyEntity(entity);
                        ecb.AddComponent(entity, new TargetGateEntity { value = rayResult.Entity });
                      
                    }


                }).Run();

        combineJo
        SignMemory2.Dispose(this.Dependency);
        m_EndSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);
       
    }

}

