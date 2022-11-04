//#define DEBUGDIRECT
//	#define DEBUGDIRTEXT
//#define DEBUGMEMEORY

#define DEBUGWALL

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


[BurstCompile]
public partial struct AgentSystem : ISystem
{
    public struct BoidJobResults
    {
        public float3 avoid;
        public float3 algin;
        public float3 group;
    }


    public struct GateJobResults
    {
        public float3 Direction;
    }

    public struct WalljobResult
    {
        public float3 Avoid;
        public float Timmer;
    }

    struct ComponentDataHandles
    {
        public ComponentLookup<WalkingTag> c_walkingTagGroup;
        public ComponentLookup<AgentConfiguration> c_agentConfigurationGroup;
        public ComponentLookup<Translation> c_agentTranslation;
        public ComponentLookup<Rotation> c_agentRotation;

        public ComponentDataHandles(ref SystemState state)
        {
            c_walkingTagGroup = state.GetComponentLookup<WalkingTag>(true);
            c_agentConfigurationGroup = state.GetComponentLookup<AgentConfiguration>(true);
            c_agentTranslation = state.GetComponentLookup<Translation>(false);
            c_agentRotation = state.GetComponentLookup<Rotation>(false);
        }

        public void Update(ref SystemState state)
        {
            c_walkingTagGroup.Update(ref state);
            c_agentConfigurationGroup.Update(ref state);
            c_agentTranslation.Update(ref state);
            c_agentRotation.Update(ref state);
        }
    }


    private EntityQuery m_WalkingAgents;
    ComponentDataHandles m_Handles;
    private const float aviodWeight = 0.16f;
    private const float alignWeight = 0.1f;
    private const float groupingWeight = 0.15f;
    private const float gateWeight = 0.5f;
    private const float wallWeight = 0f;//.25f;

    public void OnCreate(ref SystemState state)
    {
        m_Handles = new ComponentDataHandles(ref state);

        var builder = new EntityQueryBuilder(Allocator.Temp);

        builder.WithNone<WasBornTag, ArrivedTag>();
        builder.WithAll<WalkingTag, Translation, Rotation>();
        m_WalkingAgents = state.GetEntityQuery(builder);


        Debug.Log("AgentSystem Created");
    }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        m_Handles.Update(ref state);
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var (wbt, dir, entity) in SystemAPI.Query<WasBornTag, RefRO<Rotation>>().WithEntityAccess())
        {
            ecb.RemoveComponent<WasBornTag>(entity);
            ecb.AddComponent<WalkingTag>(entity);
            ecb.AddComponent(entity, new WallAvoidVector { Value = float2.zero });
            ecb.AddComponent(entity,
                new ApplyImpulse { Direction = math.forward(dir.ValueRO.Value) });
        }


        var agentCount = m_WalkingAgents.CalculateEntityCount();

        if (agentCount == 0)
        {
            // We dont need to spawn a bunch of jobs if nothing needs to be done.
            Debug.Log("Skipping since there are no agents. probably the first frame. ");
            return;
        }

        float deltaTime = SystemAPI.Time.DeltaTime;

      

#if DEBUGMEMEORY
        Debug.Log("Will Allocate memory for Agent system");
#endif
        NativeArray<Translation> positionMemory = m_WalkingAgents.ToComponentDataArray<Translation>(Allocator.TempJob);
        NativeArray<Rotation> directionMemory = m_WalkingAgents.ToComponentDataArray<Rotation>(Allocator.TempJob);
        NativeParallelHashMap<Entity, BoidJobResults> boidResults =
            new NativeParallelHashMap<Entity, BoidJobResults>(agentCount, Allocator.TempJob);
#if DEBUGMEMEORY
        Debug.Log("Finished memory for Agent system");
#endif
        // Schedule the job. Source generation creates and passes the query implicitly.
        var boidjob = new boidJob()
        {
            positionMemory = positionMemory.AsReadOnly(),
            directionMemory = directionMemory.AsReadOnly(),
            results = boidResults.AsParallelWriter()
        };
        var boidJobHandle = boidjob.Schedule(state.Dependency);
#if DEBUGMEMEORY
        Debug.Log("Scheduled boid job");
#endif
        positionMemory.Dispose(boidJobHandle);
        directionMemory.Dispose(boidJobHandle);
      
#if DEBUGMEMEORY
        Debug.Log("Scheduling Dispose of memoery");
#endif
        NativeParallelHashMap<Entity, GateJobResults> gateResult =
            new NativeParallelHashMap<Entity, GateJobResults>(agentCount, Allocator.TempJob);

#if DEBUGMEMEORY
        Debug.Log("Finished allocation of GateJobresult hasmap");
#endif

        var gateJob = new gateJob()
        {
            results = gateResult.AsParallelWriter()
        };
        var gateJobHandle = gateJob.Schedule(state.Dependency);
#if DEBUGMEMEORY
        Debug.Log("Scheduled gate job");
#endif
        var impulsejob = new impulseApplyJob()
        {
            GateJobResults = gateResult.AsReadOnly(),
            BoidJobResult = boidResults.AsReadOnly()
        };

        state.Dependency = impulsejob.Schedule(JobHandle.CombineDependencies(boidJobHandle, gateJobHandle));
#if DEBUGMEMEORY
        Debug.Log("Scheduled Impulse job");
#endif
        gateResult.Dispose(state.Dependency);
        boidResults.Dispose(state.Dependency);
#if DEBUGMEMEORY
        Debug.Log("Disposing result Arrays");
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
        else
        {
            return false;
        }
    }

    [WithAll(typeof(WalkingTag))]
    [BurstCompile]
    public partial struct impulseApplyJob : IJobEntity // it might be better to just do this in main thread... 

    {
        public NativeParallelHashMap<Entity, GateJobResults>.ReadOnly GateJobResults;
        public NativeParallelHashMap<Entity, BoidJobResults>.ReadOnly BoidJobResult;

        [BurstCompile]
        public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, ref ApplyImpulse impulse,
            in WallAvoidVector wav
#if DEBUGDIRECT
            , in Translation pos)
#else
        )
#endif
        {
            float3 newValue = float3.zero;
            float3 GateDirection = float3.zero;
            if (GateJobResults.ContainsKey(entity))
            {
                GateDirection = GateJobResults[entity].Direction;
            }

            newValue = (BoidJobResult[entity].avoid * aviodWeight) +
                       (BoidJobResult[entity].algin * alignWeight) +
                       (BoidJobResult[entity].group * groupingWeight) +
                       (GateDirection * gateWeight);
            if (math.length(wav.Value) > 0)
            {
                
                float3 WallAvoidVector =  math.rotate(quaternion.AxisAngle(new float3(0, 1, 0), math.PI / 2) , wav.Value.ExtendTo3());

                WallAvoidVector =  AngleDiff(newValue, WallAvoidVector) ?  WallAvoidVector : -WallAvoidVector;
#if DEBUGDIRECT
                    Debug.DrawRay(pos.Value,WallAvoidVector*10,Color.magenta);
#endif
                    newValue += WallAvoidVector * 0.1f;
                    newValue += wav.Value.ExtendTo3() * 0.05f;
            }




#if DEBUGDIRECT
            Debug.Log(BoidJobResult[entity].avoid.ToString()+"     "+ BoidJobResult[entity].algin.ToString()+"    "+BoidJobResult[entity].group.ToString());
            Debug.DrawRay(pos.Value, (BoidJobResult[entity].group) * groupingWeight, Color.black);

            Debug.DrawRay(pos.Value, BoidJobResult[entity].algin * 10 * alignWeight, Color.green);

            Debug.DrawRay(pos.Value, BoidJobResult[entity].avoid * 10 * aviodWeight, Color.red);


#endif

            impulse.Direction = newValue;
           
        }
    }

    [WithAll(typeof(WalkingTag))]
    [BurstCompile]
    public partial struct gateJob : IJobEntity // it might be better to just do this in main thread... 

    {
        public NativeParallelHashMap<Entity, GateJobResults>.ParallelWriter results;

        [BurstCompile]
        public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, in Translation pos,
            in TargetGateEntity TargetPos)
        {
            results.TryAdd(entity, new GateJobResults()
            {
                Direction = (TargetPos.pos - pos.Value)
            });
        }
    }


    [WithAll(typeof(WalkingTag))]
    [BurstCompile]
    public partial struct boidJob : IJobEntity
    {
        public NativeArray<Translation>.ReadOnly positionMemory;
        public NativeArray<Rotation>.ReadOnly directionMemory;
        public NativeParallelHashMap<Entity, BoidJobResults>.ParallelWriter results;

        [BurstCompile]
        public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, in Translation pos,
            in Rotation dir)
        {
            const float minRaduis = 3f;
            const float maxAlignRadius = 15f;
            const float maxGroupingRadius = 100.0f;

            uint avoidCount = 0;
            float3 avoidVector = float3.zero;
            uint alignCount = 0;
            float3 alignDirection = float3.zero;
            uint groupingCount = 0;
            float3 groupingCenter = float3.zero;
            for (int i = 0; i < positionMemory.Length; i++)
            {
                float distance = math.length(pos.Value - positionMemory[i].Value);
                if (distance <= 0)
                {
                    continue;
                } //cheap way of skipping our own


                #region AreTheyInFronOfMe

                float3 forwardVector = math.rotate(dir.Value, new float3(0f, 0f, 1f));
                bool inFront = AngleDiff(forwardVector,
                    math.normalizesafe(positionMemory[i].Value - pos.Value));

                #endregion


                #region DirectionAllignment

                float3 other_forwardVector = math.rotate(directionMemory[i].Value, new float3(0f, 0f, 1f));
                bool inLine = AngleDiff(forwardVector, other_forwardVector);

                #endregion

                float avoidAdjust = distance.MapRange(0, minRaduis, 2, 0.0001f);
#if DEBUGDIRECT
                Color c = Color.red;
                if (inLine && !inFront)
                {
                    c = Color.green;
                }
                else if (inLine && inFront)
                {
                    c = Color.cyan;
                }
                else if (!inLine && inFront)
                {
                    c = Color.magenta;
                }
                //Debug.DrawLine(pos.Position, positionMemory[i].ExtendTo3(pos.Position.y), c);
                //if(avoidAdjust>0) Debug.DrawRay(pos.Position, new float3(0, avoidAdjust * 100, 0), Color.white);
#endif

                if (distance <= minRaduis)
                {
#if DEBUGDIRTEXT
						Debug.Log(distance.ToString() + "  " + avoidAdjust.ToString());
#endif
                    if (inLine)
                    {
                        avoidVector += (pos.Value - positionMemory[i].Value) * avoidAdjust;
                    }
                    else
                    {
                        avoidVector += pos.Value - positionMemory[i].Value;
                        //if we go into the opposite direction it doesnt matter as much when they come close
                    }

                    avoidCount++;
                }

                if (distance < maxAlignRadius)
                {
                    if (inLine)
                    {
                        alignDirection += other_forwardVector; //directionMemory[i];
                        alignCount++;
                    }
                }

                if (distance < maxGroupingRadius)
                {
                    if (inLine && inFront)
                    {
                        // Only if they go in the same direction do we want to go with them 
                        groupingCenter += positionMemory[i].Value;
                        groupingCount++;
                    }
                }
            }

            groupingCenter /= groupingCount;
            alignDirection /= alignCount;
            avoidVector /= avoidCount;
            BoidJobResults tmp = new BoidJobResults()
                {
                    avoid = math.normalizesafe(avoidVector),
                    algin = math.normalizesafe(alignDirection),
                    group = math.normalizesafe(groupingCenter - pos.Value)
                }
                ;
            if (!results.TryAdd(entity, tmp))
            {
                Debug.Log("Failed to add somedata to re reulsts ");
            }

#if DEBUGDIRTEXT
				Debug.Log(groupingCount.ToString() + " " + alignCount.ToString() + " " + avoidCount.ToString());
#endif
            /*
            float3 newValue = math.normalizesafe(
                (math.normalizesafe(avoidVector) * aviodWeight +
                 math.normalizesafe(alignDirection) * alignWeight +
                 math.normalizesafe(groupingCenter - pos.Value) * groupingWeight));
                 */


#if DEBUGWALL
            // Debug.DrawRay(pos.Value, newValue., Color.green);
            //   Debug.DrawRay(pos.Value, wallDirection[entityInQueryIndex].ExtendTo3(), Color.white);
            //   Debug.DrawRay(pos.Value, (newValue * 0.5f + wallDirection[entityInQueryIndex] * 0.5f).ExtendTo3(),
            //  Color.red);

#endif
            /*
            impulse = new ApplyImpulse
            {
                Direction = newValue
            };
            */
            /*
             // I still need to fix the wall stuff later
            if (math.length(wallDirection[entityInQueryIndex]) > 0)
            {
                impulse = new ApplyImpulse
                {
                    Direction = (newValue * 0.3f + wallDirection[entityInQueryIndex] * 0.7f).ExtendTo3()
                };
            }
            else
            {
                impulse = new ApplyImpulse
                {
                    Direction = newValue;
                };
            }*/
        }
    }
}