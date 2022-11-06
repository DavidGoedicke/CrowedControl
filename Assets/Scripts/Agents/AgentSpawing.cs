using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;


[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class AgentSpawing : SystemBase
{
    struct ComponentDataHandles
    {
        public ComponentLookup<ActiveGate> c_ActiveGateGroup;
        public ComponentLookup<AgentConfiguration> c_agentConfigurationGroup;
        public ComponentLookup<Translation> c_Translate;
        public ComponentLookup<Rotation> c_Rotate;
        public ComponentLookup<GateSpawnDelay> c_SpawnDelay;

        public ComponentDataHandles(ref SystemState state)
        {
            c_ActiveGateGroup = state.GetComponentLookup<ActiveGate>(true);
            c_agentConfigurationGroup = state.GetComponentLookup<AgentConfiguration>(true);
            c_Translate = state.GetComponentLookup<Translation>(true);
            c_Rotate = state.GetComponentLookup<Rotation>(true);
            c_SpawnDelay = state.GetComponentLookup<GateSpawnDelay>(false);
        }

        public void Update(ref SystemState state)
        {
            c_ActiveGateGroup.Update(ref state);
            c_agentConfigurationGroup.Update(ref state);
            c_Translate.Update(ref state);
            c_Rotate.Update(ref state);
            c_SpawnDelay.Update(ref state);
        }
    }

    ComponentDataHandles m_Handles;
    private EntityQuery m_WalkingAgents;
    private EntityQuery m_ActiveGatesCount;
    private Random generator;

    protected override void OnCreate()
    {
        Debug.Log("Created Spawning System");
        var builder = new EntityQueryBuilder(Allocator.Temp);
        builder.WithNone<WasBornTag, ArrivedTag>();
        builder.WithAll<WalkingTag, Translation, Rotation>();
        m_WalkingAgents = GetEntityQuery(builder);

        var builder2 = new EntityQueryBuilder(Allocator.Temp);

        builder2.WithAll<ActiveGate>();

        m_ActiveGatesCount = GetEntityQuery(builder2);
        generator = new Random((uint)DateTime.Now.Second);
    }

    protected override void OnDestroy()
    {
    }

    protected override void OnUpdate()
    {
        //  m_Handles.Update(ref state);
        float deltaTime = SystemAPI.Time.DeltaTime;
        Entities
            .WithName("UpdateGateTime")
            .WithAll<ActiveGate>()
            .ForEach((ref GateSpawnDelay gsDelay) => { gsDelay.Value += deltaTime; }).ScheduleParallel();

        Debug.Log("just updated time for the gates respawn");
        int agentCount = m_WalkingAgents.CalculateEntityCount();
        int gateCount = m_ActiveGatesCount.CalculateEntityCount();
        var _agentPrefab = SystemAPI.GetSingleton<AgentPrefab>();
        int targetAgentCount = FixedGameValues.MaxAgents;

        int diff = targetAgentCount - agentCount;

        if (diff <= 0)
        {
            return;
        }

        int spawnPerGate = (int)math.floor(diff / gateCount);
        if (spawnPerGate > 4) spawnPerGate = 4;

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(World.Unmanaged);
        List<GateNums> avalibleTargets = new List<GateNums>();

        Entities
            .WithName("FindActiveGates").WithoutBurst()
            .ForEach((ref ActiveGate AG) =>
            {
                if (!avalibleTargets.Contains(AG.value))
                {
                    avalibleTargets.Add(AG.value);
                }
            }).Run();

        //Debug.Log("Needtospawn: " + diff);
        

        NativeArray<GateNums> _nativeAvalibleTargets =
            new NativeArray<GateNums>(avalibleTargets.ToArray(), Allocator.Temp);

        Entities
            .WithReadOnly(_nativeAvalibleTargets)
            .WithoutBurst()
            .WithName("SpawnMissing")
            .ForEach((ref GateSpawnDelay spawnDealy, in Entity gateEntity, in ActiveGate gate,
                in Translation pos, in Rotation rot) =>
            {
                if (spawnDealy.Value > FixedGameValues.SpawnDelay)
                {
                    spawnDealy.Value = 0;

                    GateNums targetGate = GateNums.NONE;
                    int attempts = 0;
                    Debug.Log(gate.value);
                    while (attempts < 100)
                    {
                        var tmp = generator.NextInt(0, _nativeAvalibleTargets.Length);
                        if (_nativeAvalibleTargets[tmp] != gate.value)
                        {
                            targetGate = _nativeAvalibleTargets[tmp];
                            break;
                        }

                        attempts++;
                    }


                    var e = ecb.Instantiate(_agentPrefab.Value);

                    quaternion q1 = math.mul(rot.Value,
                        quaternion.Euler(0, generator.NextFloat(-math.PI / 4, math.PI / 4), 0));


                    quaternion q2 = math.mul(rot.Value,
                        quaternion.Euler(0, generator.NextFloat(-math.PI / 4, math.PI / 4), 0));

                    ecb.SetComponent(e, new Rotation { Value = q1 });

                    ecb.AddComponent(e, new URPMaterialPropertyBaseColor
                    {
                        Value = GateColor.val[targetGate]
                    });

                    ecb.SetComponent(e, new Translation { Value = (math.forward(q2) * 10) + pos.Value });
                    ecb.AddComponent(e, new StartGateEntity
                    {
                        Value = gateEntity
                    });
                    ecb.AddComponent<WasBornTag>(e);

                    ecb.AddComponent(e, new AgentConfiguration
                    {
                        Speed = 4,
                        TargetGate = targetGate,
                        ViewingDistance = 200,
                        ViewingFilter = new CollisionFilter
                        {
                            BelongsTo = 1u << 2,
                            CollidesWith = 3u,
                            GroupIndex = 0
                        }
                    });
                }
            }).Run();
    }
}