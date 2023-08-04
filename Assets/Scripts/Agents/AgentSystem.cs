//#define DEBUGDIRECT
//	#define DEBUGDIRTEXT
//#define DEBUGMEMEORY
//#define WITHROTATION
#define DEBUGWALL

using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics.Aspects;


[BurstCompile]
public partial struct AgentSystem : ISystem
{
    public struct WalljobResult
    {
        public float3 Avoid;
        public float Timmer;
    }

    struct ComponentDataHandles
    {
        public ComponentLookup<WalkingTag> c_walkingTagGroup;
        public ComponentLookup<AgentConfiguration> c_agentConfigurationGroup;
        public ComponentLookup<LocalTransform> c_agentLocalTransform;

        public ComponentDataHandles(ref SystemState state)
        {
            c_walkingTagGroup = state.GetComponentLookup<WalkingTag>(true);
            c_agentConfigurationGroup = state.GetComponentLookup<AgentConfiguration>(true);
            c_agentLocalTransform = state.GetComponentLookup<LocalTransform>(false);
        }

        public void Update(ref SystemState state)
        {
            c_walkingTagGroup.Update(ref state);
            c_agentConfigurationGroup.Update(ref state);
            c_agentLocalTransform.Update(ref state);
        }
    }


    private EntityQuery m_WalkingAgents;
    ComponentDataHandles m_Handles;


    public void OnCreate(ref SystemState state)
    {
        m_Handles = new ComponentDataHandles(ref state);

        var builder = new EntityQueryBuilder(Allocator.Temp);

        builder.WithNone<WasBornTag, ArrivedTag>();
        builder.WithAll<PhysicsVelocity, LocalTransform, WalkingTag>();
        m_WalkingAgents = state.GetEntityQuery(builder);
    }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float deltaTime = SystemAPI.Time.DeltaTime;


        m_Handles.Update(ref state);
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (wbt, tra, entity) in SystemAPI.Query<WasBornTag, RefRO<LocalTransform>>().WithEntityAccess())
        {
            ecb.RemoveComponent<WasBornTag>(entity);
            ecb.AddComponent<WalkingTag>(entity);
            ecb.AddComponent(entity, new WallAvoidVector {Value = float2.zero});
            ecb.AddComponent(entity, new GateJobResults {Direction = float3.zero});
            ecb.AddComponent(entity,
                new ApplyImpulse {Direction = tra.ValueRO.Forward()*0.01f});
            ecb.AddComponent(entity, new BoidJobResults());
        }


        int agentCount = m_WalkingAgents.CalculateEntityCount();

        if (agentCount == 0)
        {
            // We dont need to spawn a bunch of jobs if nothing needs to be done.
            Debug.Log("Skipping since there are no agents. probably the first frame. ");
            return;
        }


        var updateLazynessJob = new updateLazynessJob()
        {
            deltaTime = deltaTime
        };
        var updateLazynessJobHandle = updateLazynessJob.Schedule(state.Dependency);


        //  NativeParallelHashMap<Entity, LocalTransform> localTransforMap =
        //      new NativeParallelHashMap<Entity, LocalTransform>(agentCount, Allocator.TempJob);


        //ToDo: THis should really use `ToComponentDataListAsync` I couldnt get it to work.
        //  var allocDepend = JobHandle.CombineDependencies(updateAllocJobHandlePos , updateAllocJobHandleVel);

        NativeArray<LocalTransform> localTransformMemory =
            m_WalkingAgents.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
        NativeArray<PhysicsVelocity> localVelocityMemory =
            m_WalkingAgents.ToComponentDataArray<PhysicsVelocity>(Allocator.TempJob);

        // Schedule the job. Source generation creates and passes the query implicitly.
        var boidjob = new boidJob()
        {
            transformMemory = localTransformMemory.AsReadOnly(),
            velocityMemory = localVelocityMemory.AsReadOnly(),
        };


        var boidJobHandle = boidjob.Schedule(updateLazynessJobHandle);
        localVelocityMemory.Dispose(boidJobHandle);
        localTransformMemory.Dispose(boidJobHandle);

        var gateJob = new gateJob();
        var gateJobHandle = gateJob.Schedule(state.Dependency);

        var impulsejob = new impulseApplyJob();

        state.Dependency = impulsejob.Schedule(JobHandle.CombineDependencies(boidJobHandle, gateJobHandle));


        state.CompleteDependency();
        #if DEBUGDIRECT
        if (true) //ToDo DrawDebugRay
        {
            const float ext = 10.0f;
            //SystemAPI.GetComponent<>()
            AgentDebugData[] OutPutData = new AgentDebugData[agentCount*2];
            int i = 0;
            foreach (var (transform, bjr, vel) in SystemAPI
                .Query<RefRO<LocalTransform>, RefRO<BoidJobResults>, RefRO<PhysicsVelocity>>())
            {
                OutPutData[i] = new AgentDebugData()
                {
                    start = transform.ValueRO.Position,
                    dir = new float3[]
                    {
                        vel.ValueRO.Linear * 2,
                        bjr.ValueRO.avoid * ext * SimVal.aviodWeight,
                        bjr.ValueRO.algin * ext * SimVal.alignWeight, 
                        bjr.ValueRO.group * ext * SimVal.groupingWeight,
                        bjr.ValueRO.avoid_OPP * ext * SimVal.aviodWeight_OPP,
                        bjr.ValueRO.algin_OPP * ext * SimVal.alignWeight_OPP,
                        bjr.ValueRO.group_OPP * ext * SimVal.groupingWeight_OPP
                    },
                    col = new float3[]
                    {
                        DbgLin.col[DbgTpe.SELF], DbgLin.col[DbgTpe.Avoid], DbgLin.col[DbgTpe.Align],
                        DbgLin.col[DbgTpe.Group], DbgLin.col[DbgTpe.Avoid_OPP], DbgLin.col[DbgTpe.Align_OPP],
                        DbgLin.col[DbgTpe.Group_OPP]
                    },
                    circlesize = new float[] {SimVal.minRaduis, SimVal.maxAlignRadius, SimVal.maxGroupingRadius},
                    circleCol = new float3[]
                        {DbgLin.col[DbgTpe.Avoid], DbgLin.col[DbgTpe.Align], DbgLin.col[DbgTpe.Group]}
                };
                i++;
            }
            
            foreach (var (transform, gate) in SystemAPI
                .Query<RefRO<LocalTransform>, 
                RefRO<TargetGateEntity>>())
            {
                OutPutData[i] = new AgentDebugData()
                {
                    start = transform.ValueRO.Position,
                    dir = new float3[]
                    {
                        gate.ValueRO.pos-transform.ValueRO.Position
                    },
                    col = new float3[]
                    {
                        new float3 (0.8f,0.8f,0f)
                    },
                    circlesize = new float[] {},
                    circleCol = new float3[] {}
                };
                i++;
            }


             AgentAlgorythmDebug.renderData = OutPutData;
            AgentAlgorythmDebug.newData = true;
           
        }
#endif
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    private static bool AngleDiff(float3 ThisForward, float3 OtherForward, float MaxAngle = math.PI / 2)
    {
        float dotProduct = math.dot(math.normalize(ThisForward.xz), math.normalize(OtherForward.xz));
        float angleDiff = math.acos(dotProduct);
        angleDiff = math.cross(ThisForward, OtherForward).y < 0 ? -angleDiff : angleDiff;

        if (angleDiff < MaxAngle && angleDiff > -MaxAngle)
        {
            return true;
        }

        return false;
    }

    [WithAll(typeof(WalkingTag))]
    [BurstCompile]
    public partial struct impulseApplyJob : IJobEntity // it might be better to just do this in main thread... 

    {
        [BurstCompile]
        public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref ApplyImpulse impulse,
            in WallAvoidVector wav, in BoidJobResults bjr, in GateJobResults gjr, in AgentLazyness lazyness
#if DEBUGDIRECT
            , in LocalTransform pos
#endif
        )
        {
            float3 newValue = float3.zero;
            newValue = (bjr.avoid * SimVal.aviodWeight) +
                       (bjr.algin * SimVal.alignWeight) +
                       (bjr.group * SimVal.groupingWeight) +
                       (bjr.avoid_OPP * SimVal.aviodWeight_OPP) +
                       (bjr.algin_OPP * SimVal.alignWeight_OPP) +
                       (bjr.group_OPP * SimVal.groupingWeight_OPP) +
                       (gjr.Direction * SimVal.gateWeight);

            if (false && math.length(wav.Value) > 0)
            {
                float3 WallAvoidVector = math.rotate(quaternion.AxisAngle(new float3(0, 1, 0), math.PI / 2),
                    wav.Value.ExtendTo3());

                WallAvoidVector = AngleDiff(newValue, WallAvoidVector) ? WallAvoidVector : -WallAvoidVector;
#if DEBUGDIRECT
                Debug.DrawRay(pos.Position, WallAvoidVector * 10, Color.magenta);
#endif
                newValue += WallAvoidVector * 0.1f;
                newValue += wav.Value.ExtendTo3() * 0.05f;
            }

            impulse.Direction = newValue;
        }
    }

    [WithAll(typeof(WalkingTag))]
    [BurstCompile]
    public partial struct updateLazynessJob : IJobEntity // it might be better to just do this in main thread... 
    {
        public float deltaTime;

        [BurstCompile]
        public void Execute(ref AgentLazyness laz, in AgentConfiguration conf)
        {
            laz.currentLazyness -= deltaTime;
            if (laz.currentLazyness <= 0)
            {
                laz.active = true;
                laz.currentLazyness = conf.Lazyness;
            }
            else
            {
                laz.active = false;
            }
        }
    }


    [WithAll(typeof(WalkingTag))]
    [BurstCompile]
    public partial struct gateJob : IJobEntity // it might be better to just do this in main thread... 
    {
        [BurstCompile]
        public void Execute(ref GateJobResults gjr, in LocalTransform tra,
            in TargetGateEntity TargetPos)
        {
            gjr.Direction = math.normalizesafe(TargetPos.pos - tra.Position);
        }
    }


    [WithAll(typeof(WalkingTag))]
    [BurstCompile]
    public partial struct boidJob : IJobEntity
    {
        public NativeArray<LocalTransform>.ReadOnly transformMemory;
        public NativeArray<PhysicsVelocity>.ReadOnly velocityMemory;


        [BurstCompile]
        public void Execute(ref BoidJobResults bjr, [ChunkIndexInQuery] int chunkIndex, in Entity entity,
            in LocalTransform tra, in PhysicsVelocity vel, in AgentLazyness lazyness)
        {
            if (!lazyness.active)
            {
                return;
            }


            uint avoidCount = 0;
            float3 avoidVector = float3.zero;
            uint alignCount = 0;
            float3 alignDirection = float3.zero;
            uint groupingCount = 0;
            float3 groupingCenter = float3.zero;

            uint avoidCount_OPP = 0;
            float3 avoidVector_OPP = float3.zero;
            uint alignCount_OPP = 0;
            float3 alignDirection_OPP = float3.zero;
            uint groupingCount_OPP = 0;
            float3 groupingCenter_OPP = float3.zero;


            for (int i = 0; i < transformMemory.Length; i++)
            {
                float distance = math.length(tra.Position - transformMemory[i].Position);
                if (distance <= 0)
                {
                    continue;
                }


                float3 forwardVector = math.normalizesafe(vel.Linear);


                float3 other_forwardVector = float3.zero;
                if (math.length(velocityMemory[i].Linear) > 0)
                {
                    other_forwardVector = math.normalize(velocityMemory[i].Linear);
                }

                if (AngleDiff(forwardVector, other_forwardVector))
                {
                    if (distance <= SimVal.minRaduis)
                    {
                        avoidVector += tra.Position - transformMemory[i].Position;
                        avoidCount++;
                    }

                    if (distance < SimVal.maxAlignRadius)
                    {
                        alignDirection += other_forwardVector; //directionMemory[i];
                        alignCount++;
                    }

                    if (distance < SimVal.maxGroupingRadius)
                    {
                        groupingCenter += transformMemory[i].Position;
                        groupingCount++;
                    }
                }
                else
                {
                    if (distance <= SimVal.minRaduis)
                    {
                        avoidVector_OPP += tra.Position - transformMemory[i].Position;
                        avoidCount_OPP++;
                    }

                    if (distance < SimVal.maxAlignRadius)
                    {
                        alignDirection_OPP += other_forwardVector; //directionMemory[i];
                        alignCount_OPP++;
                    }

                    if (distance < SimVal.maxGroupingRadius)
                    {
                        groupingCenter_OPP += transformMemory[i].Position;
                        groupingCount_OPP++;
                    }
                }
            }

            avoidVector /= avoidCount;
            alignDirection /= alignCount;
            groupingCenter /= groupingCount;

            avoidVector_OPP /= avoidCount_OPP;
            alignDirection_OPP /= alignCount_OPP;
            groupingCenter_OPP /= groupingCount_OPP;

            bjr.avoid = avoidCount > 0 ? math.normalize(avoidVector) : float3.zero;
            bjr.algin = alignCount > 0 ? math.normalize(alignDirection) : float3.zero;
            bjr.group = groupingCount > 0 ? math.normalize(groupingCenter - tra.Position) : float3.zero;

            bjr.avoid_OPP = avoidCount_OPP > 0 ? math.normalize(avoidVector_OPP) : float3.zero;
            bjr.algin_OPP = alignCount_OPP > 0 ? math.normalize(alignDirection_OPP) : float3.zero;
            bjr.group_OPP = groupingCount_OPP > 0 ? math.normalize(groupingCenter_OPP - tra.Position) : float3.zero;
        }
    }
}