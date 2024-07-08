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


[UpdateBefore(typeof(AgentSystem))]
public partial class AgentSpawing : SystemBase
{
    struct ComponentDataHandles
    {
        public ComponentLookup<ActiveGate> c_ActiveGateGroup;
        public ComponentLookup<AgentConfiguration> c_agentConfigurationGroup;
        public ComponentLookup<LocalTransform> c_Translate;
        public ComponentLookup<GateSpawnDelay> c_SpawnDelay;

        public ComponentDataHandles(ref SystemState state)
        {
            c_ActiveGateGroup = state.GetComponentLookup<ActiveGate>(true);
            c_agentConfigurationGroup = state.GetComponentLookup<AgentConfiguration>(true);
            c_Translate = state.GetComponentLookup<LocalTransform>(false);
            
            c_SpawnDelay = state.GetComponentLookup<GateSpawnDelay>(false);
        }

        public void Update(ref SystemState state)
        {
            c_ActiveGateGroup.Update(ref state);
            c_agentConfigurationGroup.Update(ref state);
            c_Translate.Update(ref state);
           
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
        builder.WithNone<ReadyToSpawn, ArrivedTag>();
        builder.WithAll<WalkingTag, LocalTransform>();
        m_WalkingAgents = GetEntityQuery(builder);

        var builder2 = new EntityQueryBuilder(Allocator.Temp);

        builder2.WithAll<ActiveGate>();

        m_ActiveGatesCount = GetEntityQuery(builder2);
        generator = new Random((uint)DateTime.Now.Second);
        RequireForUpdate<AgentPrefab>();
    }

    protected override void OnDestroy()
    {
    }

    protected override void OnUpdate() {

        
        //  m_Handles.Update(ref state);
        float deltaTime = SystemAPI.Time.DeltaTime;
        Entities
            
            .WithName("UpdateGateTime")
            .WithAll<ActiveGate>()
            .ForEach((ref GateSpawnDelay gsDelay) => { gsDelay.Value += deltaTime; }).ScheduleParallel();

       // Debug.Log("just updated time for the gates respawn");
        int agentCount = m_WalkingAgents.CalculateEntityCount();
        int gateCount = m_ActiveGatesCount.CalculateEntityCount();
        var _agentPrefab = SystemAPI.GetSingleton<AgentPrefab>();
        int targetAgentCount = SimVal.MaxAgents;

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
        

        NativeArray<GateNums> _nativeAvalibleTargets =
            new NativeArray<GateNums>(avalibleTargets.ToArray(), Allocator.Temp);

        Entities
            .WithReadOnly(_nativeAvalibleTargets)
            .WithoutBurst()
            .WithName("SpawnMissing")
            .ForEach((ref GateSpawnDelay spawnDealy, in LocalTransform  trans,in Entity gateEntity, in ActiveGate gate) =>
            {
                if (spawnDealy.Value > SimVal.SpawnDelay)
                {
                    spawnDealy.Value = 0;

                    GateNums targetGate = GateNums.NONE;
                    int attempts = 0;
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
                    
                    ecb.AddComponent(e, new AgentConfiguration
                    {
                        Speed = 4,
                        TargetGate = targetGate,
                        ViewingDistance = 75,
                        ViewingFilter = AgentAuthoring.ViewingFilter
                    });

                    quaternion q1 = math.mul(trans.Rotation,
                        quaternion.Euler(0, generator.NextFloat(-math.PI / 4, math.PI / 4), 0));


                    quaternion q2 = math.mul(trans.Rotation,
                        quaternion.Euler(0, generator.NextFloat(-math.PI / 4, math.PI / 4), 0));

                    float3 tempPos=  (math.forward(q2) * 10) + trans.Position + new float3(0, 2, 0);
                   
                    Debug.Log("Spawing at:"+tempPos);
                    ecb.SetComponent(e, new LocalTransform
                    {
                        Position = tempPos,
                        Rotation = q1
                    });
                    
                    ecb.AddComponent(e, new StartGateEntity
                    {
                        Value = gateEntity
                    });
                    
                    InitAgent(ref ecb, e);
                }
            }).Run();
        
        Entities
            .WithoutBurst()
            .WithName("UpdatedSpanwed")
            .WithAll<ReadyToSpawn>()
            .ForEach(ref Entity e) =>
        {
            // add all the reuqired components including physics objects.  the idea is that any agent needs to go through this.
        }


        private static void InitAgent(ref EntityCommandBuffer ecb, Entity e) {
        
                   ecb.AddComponent<WalkingTag>(e);
                   ecb.AddComponent(e, new WallAvoidVector {Value = float2.zero});
                   ecb.AddComponent(e, new GateJobResults {Direction = float3.zero});
                   ecb.AddComponent(e, new ApplyImpulse {Direction =new float3(1,0,1)});// tra.ValueRO.Forward()
                   ecb.AddComponent(e, new BoidJobResults());
                   ecb.AddComponent(e,new AgentLazyness{currentLazyness= 0});
                    
                    
                   
                    ecb.AddComponent(e, new URPMaterialPropertyBaseColor
                    {
                        Value = GateColor.val[targetGate]
                    });

                  
                   
                  
                    
                    // Ensure that PhysicsMass and PhysicsVelocity components are properly initialized
                    ecb.SetComponent(e, new PhysicsMass
                    {
                        InverseMass = 1.0f,
                        InverseInertia = new float3(1, 1, 1) // Ensure proper inertia values
                        
                    });
                    
                    ecb.SetComponent(e, new PhysicsVelocity
                    {
                        Linear = float3.zero,
                        Angular = float3.zero
                        
                    });
                    
                    ecb.SetComponent(e, new PhysicsDamping
                    {
                        Linear = 0.01f,
                        Angular = 0.05f
                        
                    });
    }
}