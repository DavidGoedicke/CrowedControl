//#define DEBUGRAY

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(PhysicsSystemGroup))]
[BurstCompile]
public partial struct AgentRayCast : ISystem
{
    [WithNone(typeof(TargetGateEntity))]
    [BurstCompile]
    public partial struct CastRayJob : IJobEntity
    {
        public PhysicsWorldSingleton World;
        public NativeParallelHashMap<Entity, GateNums> SignAndGateMemory;
        public EntityCommandBuffer.ParallelWriter Ecb;

        [BurstCompile]
        public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, in Translation localPosition,
            in AgentConfiguration agentConfiguration, in Rotation localRotation)
        {
            float3 rayStart = localPosition.Value;
            float3 rayEnd = math.forward(localRotation.Value) * agentConfiguration.ViewingDistance
                            + rayStart;


#if DEBUGRAY
                    Debug.DrawRay(rayStart, rayEnd - rayStart);
#endif

Debug.Log("Ray Legnth"+math.length(rayEnd-rayStart)+"  "+rayStart.ToString());
            var raycastInput = new RaycastInput
            {
                Start = rayStart,
                End = rayEnd,
                Filter = agentConfiguration.ViewingFilter
            };

            var hit = World.CastRay(raycastInput, out var rayResult);

            if (hit)
            {
                if (SignAndGateMemory.ContainsKey(rayResult.Entity))
                {
#if DEBUGRAY
                            Debug.DrawRay(rayResult.Position, Vector3.up, Color.green, 5f);
#endif
                    if (SignAndGateMemory[rayResult.Entity] == agentConfiguration.TargetGate)
                    {
                        Ecb.AddComponent<TargetGateEntity>(chunkIndex, entity,
                            new TargetGateEntity { value = rayResult.Entity, pos = rayResult.Position });
                    }
                }
                else
                {
#if DEBUGRAY
                            Debug.DrawRay(rayStart + (math.forward(localRotation.Value) * agentConfiguration.ViewingDistance) * rayResult.Fraction, Vector3.up, Color.red, 5f);
#endif
                }
            }
        }
    }

     
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp);
        builder.WithAny<ActiveGate>();
        GateQuery = state.GetEntityQuery(builder);

        
        builder = new EntityQueryBuilder(Allocator.Temp);
        builder.WithAny<ActiveSign>();
        SignQuery = state.GetEntityQuery(builder);

        ActiveGateHandle = state.GetComponentTypeHandle<ActiveGate>();
        ActiveSignHandle = state.GetComponentTypeHandle<ActiveSign>();

        if (!SystemAPI.TryGetSingleton(out ecbSingleton))
        {
            tryagain = true;
            Debug.Log("Agent raycast could not grab the BeginSimulationEntityCommandBufferSystem singelton");
        }
    }

    private bool tryagain;
    BeginSimulationEntityCommandBufferSystem.Singleton ecbSingleton ;
    
    
    
    
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    private EntityQuery GateQuery;
    private EntityQuery SignQuery;
    private ComponentTypeHandle<ActiveGate> ActiveGateHandle;
    private ComponentTypeHandle<ActiveSign> ActiveSignHandle;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {

        if (tryagain)
        {
            tryagain = !SystemAPI.TryGetSingleton(out ecbSingleton);
            return;
        }
        ActiveGateHandle.Update(ref state);
        ActiveSignHandle.Update(ref state);


        var gateCount = GateQuery.CalculateEntityCount();
        var TargetCount = gateCount + SignQuery.CalculateEntityCount();

        var SignAndGateMemory = new NativeParallelHashMap<Entity, GateNums>(TargetCount, Allocator.TempJob);


        foreach (var (gate, entity) in
                 SystemAPI.Query<RefRO<ActiveGate>>().WithEntityAccess())
        {
            SignAndGateMemory[entity] = gate.ValueRO.value;
        }

        foreach (var (sign, entity) in
                 SystemAPI.Query<RefRO<ActiveSign>>().WithEntityAccess())
        {
            SignAndGateMemory[entity] = sign.ValueRO.value;
        }

        
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        // Version that schedules a job


        if (SystemAPI.TryGetSingleton(out PhysicsWorldSingleton physicsWorldSingleton)){
            // var world = physicsWorldSingleton.CollisionWorld;

            state.Dependency = new CastRayJob
            {
                World = physicsWorldSingleton,
                SignAndGateMemory = SignAndGateMemory,
                Ecb = ecb.AsParallelWriter()
            }.Schedule(state.Dependency);
        }else
        {
            Debug.Log("Failed to get physics world. Maybe there are more then one hmm...");
        }
//ToDo ECB should be scheduled?? 
    }
}