// #define DEBUGDIRECT
//	#define DEBUGDIRTEXT

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

    //  FloorVectorManager floorVectorManager;


    private EntityQuery m_WalkingAgents;
    ComponentDataHandles m_Handles;

    public void OnCreate(ref SystemState state)
    {
        m_Handles = new ComponentDataHandles(ref state);

        var builder = new EntityQueryBuilder(Allocator.Temp);
        builder.WithAny<WalkingTag>();
        builder.WithNone<WasBornTag, ArrivedTag>();
        m_WalkingAgents = state.GetEntityQuery(builder);

        //floorVectorManager.Init(
        //     World.GetOrCreateSystem<BuildPhysicsWorld>(),
        //     World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>()
        // );
    }


    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        m_Handles.Update(ref state);
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        var temp = new Unity.Mathematics.Random();
        foreach (var (wbt, pos, entity) in SystemAPI.Query<WasBornTag, LocalToWorld>().WithEntityAccess())
        {
            ecb.RemoveComponent<WasBornTag>(entity);
            ecb.AddComponent<WalkingTag>(entity);
            ecb.AddComponent<ApplyImpulse>(entity,
                new ApplyImpulse { Direction = pos.Forward });
        }


        var agentCount = m_WalkingAgents.CalculateEntityCount();

        if (agentCount == 0)
        {
            // We dont need to spawn a bunch of jobs if nothing needs to be done.
            return;
        }

        float deltaTime = SystemAPI.Time.DeltaTime;

        var wallDirection =
            new NativeArray<float2>(agentCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
/*
        Entities
            .WithoutBurst()
            .WithAll<WalkingTag>()
            .WithName("WallAsignandCalc")
            .ForEach((int entityInQueryIndex, in LocalToWorld pos) =>
            {
                wallDirection[entityInQueryIndex] =
                //    floorVectorManager.WallVector(new float2(pos.Position.x, pos.Position.z));
            })
            .Run();
*/
        NativeArray<Translation> positionMemory = m_WalkingAgents.ToComponentDataArray<Translation>(Allocator.Temp);
        NativeArray<Rotation> directionMemory = m_WalkingAgents.ToComponentDataArray<Rotation>(Allocator.Temp);


        var steerJob1 = Entities
            .WithName("Steer")
            .WithReadOnly(positionMemory)
            .WithReadOnly(directionMemory)
            .WithAll<WalkingTag>()
            .ForEach((ref ApplyImpulse impulse, in LocalToWorld pos, in int entityInQueryIndex) => //in AgentInfo agent
            {
            })
            .ScheduleParallel(initialPosArrayJobHandle); //getGateDirection

        positionMemory.Dispose(steerJob1);
        directionMemory.Dispose(steerJob1);
        wallDirection.Dispose(steerJob1);
        var steerJob2 = Entities
            .WithName("SteerToGate")
            .WithAll<WalkingTag, TargetGateEntity>()
            .ForEach(
                (ref ApplyImpulse impulse, in LocalToWorld IsPos,
                    in TargetGatePosition TargetPos) => //in AgentInfo agent
                {
                    float3 direction = TargetPos.value - IsPos.Position;
                    float3 newDirection;
                    if (math.length(impulse.Direction) > 0.01f)
                    {
                        newDirection = (impulse.Direction * 0.3f) + (math.normalizesafe(direction) * 0.7f);
                    }
                    else
                    {
                        newDirection = direction;
                    }

                    impulse = new ApplyImpulse
                    {
                        Direction = math.normalizesafe(newDirection)
                    };
                }).ScheduleParallel(steerJob1);


        state.Dependency = steerJob2;
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
    public partial struct Steeringjob : IJobEntity
    {
        [BurstCompile]
        public void Execute([ChunkIndexInQuery] int chunkIndex, Entity entity, in Translation positionMemory,
            in Rotation directionMemory)
        {
            const float minRaduis = 3f;
            const float aviodWeight = 0.16f;

            const float maxAlignRadius = 15f;
            const float alignWeight = 0.1f;

            const float maxGroupingRadius = 100.0f;
            const float groupingWeight = 0.15f;

            uint avoidCount = 0;
            float2 avoidVector = float2.zero;
            uint alignCount = 0;
            float2 alignDirection = float2.zero;
            uint groupingCount = 0;
            float2 groupingCenter = float2.zero;
            for (int i = 0; i < positionMemory.Length; i++)
            {
                float distance = math.length(pos.Position.xz - positionMemory[i]);
                if (distance <= 0)
                {
                    continue;
                } //cheap way of skipping our own


                #region AreTheyInFronOfMe

                bool inFront = AngleDiff(pos.Forward,
                    math.normalizesafe(positionMemory[i].ExtendTo3(pos.Position.y) - pos.Position));

                #endregion


                #region DirectionAllignment

                bool inLine = AngleDiff(pos.Forward, math.normalizesafe(directionMemory[i].ExtendTo3()));

                #endregion

                float avoidAdjust = distance.MapRange(0, minRaduis, 2, 0.0001f);
#if DEBUGDIRECT
					Color c = Color.red;
					if (inLine && !inFront) {
						c = Color.green;
					} else if (inLine && inFront) {
						c = Color.cyan;
						
					} else if (! inLine && inFront) {
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
                        avoidVector += ((pos.Position.xz - positionMemory[i]) * avoidAdjust);
                    }
                    else
                    {
                        avoidVector +=
                            ((pos.Position.xz -
                              positionMemory
                                  [i])); //if we go into the opposite direction it doesnt matter as much when they come close
                    }

                    avoidCount++;
                }

                if (distance < maxAlignRadius)
                {
                    if (inLine)
                    {
                        alignDirection += directionMemory[i];
                        alignCount++;
                    }
                }

                if (distance < maxGroupingRadius)
                {
                    if (inLine && inFront)
                    {
                        // Only if they go in the same direction do we want to go with them 
                        groupingCenter += positionMemory[i];
                        groupingCount++;
                    }
                }
            }

            groupingCenter /= groupingCount;
            alignDirection /= alignCount;
            avoidVector /= avoidCount;
#if DEBUGDIRTEXT
				Debug.Log(groupingCount.ToString() + " " + alignCount.ToString() + " " + avoidCount.ToString());
#endif
            float2 newValue = math.normalizesafe(
                (math.normalizesafe(avoidVector) * aviodWeight +
                 math.normalizesafe(alignDirection) * alignWeight +
                 math.normalizesafe(groupingCenter - pos.Position.xz) * groupingWeight));

#if DEBUGDIRECT
				if (groupingCount > 0) {
					Debug.DrawRay(pos.Position, (groupingCenter.ExtendTo3(pos.Position.y) - pos.Position) * groupingWeight, Color.black) ;
				}
				
				if (alignCount > 0) {
					Debug.DrawRay(pos.Position, alignDirection.ExtendTo3() * 10 * alignWeight, Color.green);
				}
				if (avoidCount > 0) {
					Debug.DrawRay(pos.Position, avoidVector.ExtendTo3() * 10 * aviodWeight, Color.red);
				}
				Debug.DrawRay(pos.Position, newValue.ExtendTo3() * 10 * aviodWeight, Color.white);

#endif

#if DEBUGWALL
            Debug.DrawRay(pos.Position, newValue.ExtendTo3(), Color.green);
            Debug.DrawRay(pos.Position, wallDirection[entityInQueryIndex].ExtendTo3(), Color.white);
            Debug.DrawRay(pos.Position, (newValue * 0.5f + wallDirection[entityInQueryIndex] * 0.5f).ExtendTo3(),
                Color.red);

#endif
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
                    Direction = newValue.ExtendTo3()
                };
            }
        }
    }
}