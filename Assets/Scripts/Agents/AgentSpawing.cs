using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if USERBURST
using Unity.Burst;
#endif
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;
using UnityEditor;
using Material = UnityEngine.Material;


[UpdateBefore(typeof(AgentSystem))]
public partial class AgentSpawing : SystemBase {
    public Texture2D AgentSprite;
    
    private EntityQuery m_WalkingAgents;
    private EntityQuery m_ActiveGatesCount;
    private Random generator;
    protected override void OnCreate() {
        Debug.Log("Created Spawning System");
        
        AgentArchetypeManager.InitializeArchetype(EntityManager);
        


        var builder = new EntityQueryBuilder(Allocator.Temp);
        builder.WithNone<ReadyToSpawn, ArrivedTag>();
        builder.WithAll<WalkingTag, LocalToWorld>();
        m_WalkingAgents = GetEntityQuery(builder);

        var builder2 = new EntityQueryBuilder(Allocator.Temp);
        builder2.WithAll<ActiveGate>();
        m_ActiveGatesCount = GetEntityQuery(builder2);

        generator = new Random((uint)DateTime.UtcNow.Millisecond);
        RequireForUpdate<AgentPrefab>();
        Debug.Log("AgentSpawning has started!");
      

       
    }

    protected override void OnDestroy() {
    }
#if USERBURST
    [BurstCompile]
#endif
    public partial struct UpdateGateTimeJob : IJobEntity {
        public float DeltaTime;

        public void Execute(ref GateSpawnDelay gsDelay) {
            gsDelay.Value += DeltaTime;
        }
    }

   /*
#if USERBURST
    [BurstCompile]
#endif
    public partial struct SpawnAgentJob : IJobEntity {
        public EntityCommandBuffer ECB;
        public Entity AgentPrefab;
        public float DeltaTime;
        public GateNums[] AvailableTargets;
        public Random Generator;

        public void Execute(ref GateSpawnDelay spawnDelay, in LocalTransform trans, in Entity gateEntity,
            in ActiveGate gate) {
            if (spawnDelay.Value > SimVal.SpawnDelay) {
                spawnDelay.Value = 0;

                // Select a target gate
                GateNums targetGate = AvailableTargets[Generator.NextInt(AvailableTargets.Length)];

                // Create and set up the agent entity
                var e = ECB.Instantiate(AgentPrefab);
                

                // Randomize position and rotation
                quaternion randomRotation = math.mul(trans.Rotation, quaternion.Euler(0, Generator.NextFloat(-math.PI / 4, math.PI / 4), 0));
                float3 spawnPosition = (math.forward(randomRotation) * 10) + trans.Position + new float3(0, 2, 0);

                // Apply components
                ECB.SetComponent(e, new LocalTransform { Position = spawnPosition, Rotation = randomRotation });
                ECB.AddComponent(e, new StartGateEntity { Value = gateEntity });
                ECB.AddComponent(e, new AgentConfiguration
                {
                    Speed = 4,
                    TargetGate = targetGate,
                    ViewingDistance = 75,
                    ViewingFilter = AgentAuthoring.ViewingFilter
                });
                ECB.AddComponent(e, new WallAvoidVector { Value = float2.zero });
                ECB.AddComponent(e, new ApplyImpulse { Direction = new float3(1, 0, 1) });
                ECB.AddComponent(e, new URPMaterialPropertyBaseColor { Value = new float4(1, 0, 0, 1) });  // Set color as desired

                // Initialize physics components
                ECB.SetComponent(e, new PhysicsMass { InverseMass = 1.0f, InverseInertia = new float3(1, 1, 1) });
                ECB.SetComponent(e, new PhysicsVelocity { Linear = float3.zero, Angular = float3.zero });
                ECB.SetComponent(e, new PhysicsDamping { Linear = 0.01f, Angular = 0.05f });
            }
        }
    }
*/
    protected override void OnUpdate() {
        float deltaTime = SystemAPI.Time.DeltaTime;
        //Debug.Log($"I am supposed to spawn some agents I think.");

        
        // Schedule the gate delay update job
        var gateTimeJob = new UpdateGateTimeJob { DeltaTime = deltaTime };
        Dependency = gateTimeJob.ScheduleParallel(Dependency);

        // Calculate agent counts and available gates
        int agentCount = m_WalkingAgents.CalculateEntityCount();
        int gateCount = m_ActiveGatesCount.CalculateEntityCount();
        int targetAgentCount = SimVal.MaxAgents;
        int diff = targetAgentCount - agentCount;
        Debug.Log($"I am supposed to spawn {diff} Agents");
        if (diff <= 0 && gateCount==0) {
            return; // No agents need to be spawned
        }
        

     

        var avalibleGates = new HashSet<GateNums>();

        // Query active gates and collect their values into the HashSet
        Entities.WithAll<ActiveGate>()
            .ForEach((in ActiveGate gate) => { avalibleGates.Add(gate.value); }).WithoutBurst().Run();

        // Prepare spawn configurations
        List<SpawnConfig> spawnConfigs = PrepareSpawnConfigs(diff, avalibleGates);
       // Entity prefab = SystemAPI.GetSingleton<AgentPrefab>().Value;
       AgentSprite = Resources.Load<Texture2D>("Textures/AgentSprite");  // Loads from Assets/Resources/AgentSprite.png
       
       if (AgentSprite == null)
       {
           Debug.LogError("Sprite not found in Resources folder!");
           return;
       }
       var quadMesh = Resources.Load<Mesh>("Quad"); // Ensure this exists in Assets/Resources/QuadMesh.asset
       if (quadMesh == null)
       {
           Debug.LogError("Quad mesh not found in Resources folder!");
           return;
       }
       
        // Spawn agents synchronously using the archetype
        foreach (var config in spawnConfigs) {

            //var agent = EntityManager.CreateEntity(AgentArchetypeManager.AgentArchetype);
            //var agent = EntityManager.Instantiate(prefab);
            Entity agent = EntityManager.CreateEntity();

            float3 scale = new float3(1f, 1f, 1f);  // Adjust scale if needed
            float4x4 worldTransform = float4x4.TRS(config.Position, config.Rotation, scale);
            EntityManager.AddComponentData(agent, new LocalToWorld { Value = worldTransform });
            
            
           // EntityManager.AddComponentData(agent, new LocalTransform {
       //         Position = config.Position,
        //        Rotation = config.Rotation
       //     });
            EntityManager.AddComponentData(agent, new AgentConfiguration {
                Speed = 4,
                TargetGate = config.TargetGate,
                ViewingDistance = 75,
                ViewingFilter = AgentAuthoring.ViewingFilter
            });
            EntityManager.AddComponentData(agent, new WalkingTag());
            EntityManager.AddComponentData(agent, new StartGateEntity { Value = Entity.Null });
            EntityManager.AddComponentData(agent, new WallAvoidVector { Value = float2.zero });
            EntityManager.AddComponentData(agent, new ApplyImpulse { Direction = new float3(1, 0, 1) });
            EntityManager.AddComponentData(agent,
                new PhysicsMass { InverseMass = 1.0f, InverseInertia = new float3(1, 1, 1) });
            EntityManager.AddComponentData(agent, new PhysicsVelocity { Linear = float3.zero, Angular = float3.zero });
            EntityManager.AddComponentData(agent, new PhysicsDamping { Linear = 0.01f, Angular = 0.05f });
            EntityManager.AddComponentData(agent, new AgentLazyness { active = true });
            EntityManager.AddComponentData(agent, new GateJobResults { Direction = float3.zero });
            EntityManager.AddComponentData(agent,
                new BoidJobResults()
                    { avoid = float3.zero, algin = float3.zero, avoid_OPP = float3.zero, algin_OPP = float3.zero });
            EntityManager.AddComponentData(agent, new URPMaterialPropertyBaseColor{Value = new float4(1,1,1,1)});
            Debug.Log(agent.ToString());
            Debug.Log($"I added a Agent at {config.Position}");

            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.mainTexture = AgentSprite;
            material.SetFloat("_Surface", 0.5f); // 1 = Transparent, 0 = Opaque
            material.SetFloat("_AlphaClip", 0.5f); // Enables alpha clipping
            material.color = Color.white; // Ensure it’s a visible color

            var renderMeshDescription = new RenderMeshDescription(
                shadowCastingMode: UnityEngine.Rendering.ShadowCastingMode.Off,
                receiveShadows: false);

            
            Debug.Log($"check the mesh {quadMesh } : {quadMesh.vertexCount}");
            // Create a RenderMeshArray
            var renderMeshArray = new RenderMeshArray(new[] { material }, new Mesh[] { quadMesh },
                new MaterialMeshIndex[]{new MaterialMeshIndex(){MaterialIndex = 0,MeshIndex = 0,SubMeshIndex = 0}});

            RenderMeshUtility.AddComponents(agent, EntityManager, renderMeshDescription, renderMeshArray,MaterialMeshInfo.FromRenderMeshArrayIndices(0,0));
        }
        
    }
    private Mesh GenerateQuadMesh()
    {
        var mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(0.5f, 0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0)
        };
        mesh.uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };
        mesh.triangles = new int[] { 0, 1, 2, 2, 3, 0 };
        mesh.RecalculateNormals();
        return mesh;
    }
    private List<SpawnConfig> PrepareSpawnConfigs(int count, HashSet<GateNums> avalibleGates)
    {
        var spawnConfigs = new List<SpawnConfig>();
        List<GateNums> availableTargets = avalibleGates.ToList();
        
        // Generate random configurations
        for (int i = 0; i < count; i++)
        {
            GateNums targetGate = availableTargets[generator.NextInt(availableTargets.Count)];
            quaternion randomRotation = quaternion.Euler(0, generator.NextFloat(-math.PI / 4, math.PI / 4), 0);
            //float3 randomPosition = new float3(generator.NextFloat(-10f, 10f), 1, generator.NextFloat(-10f, 10f));
            float3 randomPosition = new float3(generator.NextFloat(-1, 1f), 1, generator.NextFloat(-1f, 1f));

            spawnConfigs.Add(new SpawnConfig
            {
                TargetGate = targetGate,
                Position = randomPosition,
                Rotation = randomRotation,
                //GateEntity = Entity.Null // Update if gate-specific info is needed
            });
        }

        return spawnConfigs;
    }
    private struct SpawnConfig
    {
        public GateNums TargetGate;
        public float3 Position;
        public quaternion Rotation;
       // public Entity GateEntity;
    }

}