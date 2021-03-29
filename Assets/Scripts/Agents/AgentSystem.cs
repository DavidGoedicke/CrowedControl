using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;





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
            .ForEach((Entity entity, int entityInQueryIndex, ref AgentPosition pos, ref AgentDirection vel) =>
            {
                pos = new AgentPosition
                {
                    Value = new float3(temp.NextFloat(-25, 25), 1, temp.NextFloat(-50, 50))
                };
                vel = new AgentDirection
                {
                    Value = temp.NextFloat3Direction()
                    
                };
                vel.Value.y = 0;
                commandBuffer.RemoveComponent<WasBornTag>(entityInQueryIndex,entity);
                commandBuffer.AddComponent<WalkingTag>(entityInQueryIndex, entity);
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
                    .ForEach((int entityInQueryIndex, in AgentPosition pos,in AgentDirection dir) =>
                    {
                        positionMemory[entityInQueryIndex] = pos.Value;
                        directionMemory[entityInQueryIndex] = dir.Value;
                    })
                    .ScheduleParallel(Dependency);
 
        float deltaTime = Time.DeltaTime;
        var steerJob = Entities
            .WithName("Steer")
            .WithReadOnly(positionMemory)
            .WithReadOnly(directionMemory)
            .WithAll<WalkingTag>()
            .ForEach((ref AgentPosition pos, ref AgentDirection vel) =>//in AgentInfo agent
            {
                        float minRaduis = 0.75f;
                float aviodWeight = 0.7f;

                        float maxAlignRadius = 4.5f;
                float alignWeight = 0.21f;

                        float maxGroupingRadius = 18.0f;
                float groupingWeight = 0.18f;

                uint avoidCount = 0;
                float3 avoidVector = float3.zero;
                uint alignCount = 0;
                float3 alignDirection = float3.zero;
                uint groupingCount = 0;
                float3 groupingCenter = float3.zero;
                for (int i = 0; i < positionMemory.Length; i++) {
                    float distance = math.lengthsq(pos.Value - positionMemory[i]);
                    if (distance <= 0) { continue; } //cheap way of skipping our own 
                    else if(distance>=0 && distance < minRaduis)
                    {
                        avoidVector += pos.Value - positionMemory[i];
                        avoidCount++;
                    }
                    else if (distance > minRaduis && distance < maxGroupingRadius)
                    {
                        if(distance> maxAlignRadius)
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

                

                vel = new AgentDirection
                {
                    Value = math.normalizesafe((
                        (math.normalizesafe(avoidVector) * aviodWeight +
                        math.normalizesafe(alignDirection)*alignWeight+
                        math.normalizesafe(groupingCenter-pos.Value)*groupingWeight)

                        * deltaTime) + ((1- deltaTime) * vel.Value)) //ToDo: Need to adjust this to react to delta time (i.e. how fast can they rotate
                };
                vel.Value.y = 0;

                pos = new AgentPosition
                {
                    Value = pos.Value + vel.Value * deltaTime
                };
            })
           .ScheduleParallel(initialPosArrayJobHandle);

        positionMemory.Dispose(steerJob);
        directionMemory.Dispose(steerJob);
        var movingJob = Entities
            .WithName("Moving")
            .WithAll<WalkingTag>()
            .ForEach((ref LocalToWorld localToWorld, in AgentPosition pos,in AgentDirection dir) =>//in AgentInfo agent
            {
                localToWorld = new LocalToWorld
                {
                    Value = float4x4.TRS(
                                new float3(pos.Value) ,
                                quaternion.LookRotationSafe(dir.Value, math.up()),
                                new float3(1.0f, 1.0f, 1.0f))
                };
            })
            .ScheduleParallel(steerJob);

        Dependency = movingJob;


    }
}