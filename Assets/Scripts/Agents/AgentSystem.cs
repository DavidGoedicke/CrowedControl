using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Systems;




public partial class AgentSystem_IJobChunk : SystemBase
{
    
    
    BeginInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;
    protected override void OnCreate()
    {
        
        
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();

    }

    EntityQueryDesc ActiveAgentQuery = new EntityQueryDesc
    {
        None = new ComponentType[]
           {
               ComponentType.ReadOnly<WasBornTag>(),
               ComponentType.ReadOnly<ArrivedTag>()
           },
        All = new ComponentType[]
           {
               
               ComponentType.ReadOnly<WalkingTag>()
           }
    };

    protected override void OnUpdate()
    {

        var commandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        var temp = new Unity.Mathematics.Random(1);
        Entities
            .WithName("Initialize")
            .WithAll<WasBornTag>()
            .WithBurst(FloatMode.Default, FloatPrecision.Standard, true)
            .ForEach((Entity entity, int entityInQueryIndex,ref AgentPosition pos, ref AgentDirection vel,ref PhysicsMass mass) =>
            {
                pos = new AgentPosition
                {
                    Value = new float3(temp.NextFloat(-10, 10), 1, temp.NextFloat(-10, 10))
                };
                vel = new AgentDirection
                {
                    Value = temp.NextFloat3Direction()
                    
                };
                vel.Value.y = 0;
                commandBuffer.RemoveComponent<WasBornTag>(entityInQueryIndex,entity);
                commandBuffer.AddComponent<WalkingTag>(entityInQueryIndex, entity);
                mass.InverseInertia[0] = 0;
                mass.InverseInertia[2] = 0;


            })
            .ScheduleParallel();
            m_EntityCommandBufferSystem.AddJobHandleForProducer(Dependency);

        EntityQuery m_AgentQuery = GetEntityQuery(ActiveAgentQuery);
        var agentCount = m_AgentQuery.CalculateEntityCount();
       
        if (agentCount == 0) { return; }
       
        var positionMemory = new NativeArray<float3>(agentCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        var directionMemory = new NativeArray<float3>(agentCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
        
        var initialPosArrayJobHandle = Entities
                    .WithAll<WalkingTag>()
                    .WithName("InitPosMemorySeparationJob")
                    .ForEach((int entityInQueryIndex, in LocalToWorld pos) =>
                    {
                        
                        positionMemory[entityInQueryIndex] = pos.Position;
                        directionMemory[entityInQueryIndex] = pos.Forward;// pos.Forward;
                    })
                    .ScheduleParallel(Dependency);
       
        
        float deltaTime = Time.DeltaTime;
        var steerJob = Entities
            .WithName("Steer")
            .WithReadOnly(positionMemory)
            .WithReadOnly(directionMemory)
            .WithAll<WalkingTag>()
            .ForEach((ref ApplyImpulse impulse, in LocalToWorld pos) =>//in AgentInfo agent
            {
                float minRaduis = 1.5f;
                float aviodWeight = 0.9f;

                float maxAlignRadius = 25f;
                float alignWeight = 0.8f;

                float maxGroupingRadius = 100.0f;
                float groupingWeight = 0.2f;

                uint avoidCount = 0;
                float3 avoidVector = float3.zero;
                uint alignCount = 0;
                float3 alignDirection = float3.zero;
                uint groupingCount = 0;
                float3 groupingCenter = float3.zero;
                for (int i = 0; i < positionMemory.Length; i++) {
                    float distance = math.length(pos.Position - positionMemory[i]);
                    if (distance <= 0) { continue; } //cheap way of skipping our own 
                    else if (distance >= 0 && distance < minRaduis)
                    {
                        avoidVector += pos.Position - positionMemory[i];
                        avoidCount++;
                    }
                    else if (distance > minRaduis && distance < maxGroupingRadius)
                    {
                        if (distance < maxAlignRadius)
                        {
                            alignDirection += directionMemory[i];
                            alignCount++;
                        }
                        groupingCenter += positionMemory[i];
                        groupingCount++;
                    }
                }
                groupingCenter /= groupingCount;
                alignDirection /= alignCount;
                avoidVector /= avoidCount;


                float3 newValue = math.normalizesafe(
                    (math.normalizesafe(avoidVector) * aviodWeight +
                    math.normalizesafe(alignDirection) * alignWeight +
                    math.normalizesafe(groupingCenter - pos.Position) * groupingWeight));
                newValue.y = 0;

                impulse = new ApplyImpulse
                {
                    Direction = newValue
                };
               
               
            })
           .ScheduleParallel(initialPosArrayJobHandle);

        positionMemory.Dispose(steerJob);
        directionMemory.Dispose(steerJob);
        
       
        Dependency = steerJob;


    }
}