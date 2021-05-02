//#define DEBUGTRIGGER
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;


[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class GateCollisionHandle : SystemBase
{


    private BuildPhysicsWorld m_BuildPhysicsWorld;
    private StepPhysicsWorld m_StepPhysicsWorld;
    private EntityQuery m_ActiveGates;
    private EndSimulationEntityCommandBufferSystem m_CmdBuffer;


    protected override void OnCreate()
    {
        m_BuildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        m_StepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        m_CmdBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        m_ActiveGates = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[]
          {
                typeof(ActiveGate)
          }
        });
    }
#if DEBUGTRIGGER
    [BurstCompile]
#endif
    struct TriggerGateJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<ActiveGate> ActiveGatesGroup;
        public ComponentDataFromEntity<WalkingTag> WalkingTagGroup;
        public ComponentDataFromEntity<GateNumber> GateNumberGroup;
        public ComponentDataFromEntity<AgentConfiguration> AgentConfigurationGroup;
        public EntityCommandBuffer m_CmdBuffer;

        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.EntityA;
            Entity entityB = triggerEvent.EntityB;

            bool isBodyATrigger = ActiveGatesGroup.HasComponent(entityA);
            bool isBodyBTrigger = ActiveGatesGroup.HasComponent(entityB);

            // Ignoring Triggers overlapping other Triggers
            if (isBodyATrigger && isBodyBTrigger)
                return;

            bool isBodyADynamic = WalkingTagGroup.HasComponent(entityA);
            bool isBodyBDynamic = WalkingTagGroup.HasComponent(entityB);

            // Ignoring overlapping static bodies
            if ((isBodyATrigger && !isBodyBDynamic) ||
                (isBodyBTrigger && !isBodyADynamic))
                return;
            
            var AgentEntity = isBodyATrigger ? entityB : entityA;
            var GateEntity = isBodyATrigger ? entityA : entityB;
#if DEBUGTRIGGER
            Debug.Log("Trtying to delet a thing");
#endif
            if (GateNumberGroup.HasComponent(GateEntity))
            {
                GateNumber GNcomp = GateNumberGroup[GateEntity];
                AgentConfiguration ACcomp = AgentConfigurationGroup[AgentEntity];
               
                if (GNcomp.value==ACcomp.TargetGate) { 
                m_CmdBuffer.DestroyEntity(AgentEntity);
                }
            }
            // Look here https://github.com/Unity-Technologies/EntityComponentSystemSamples/blob/f9043577ddc0d8667b6b4649a71454741075f4d1/UnityPhysicsSamples/Assets/Demos/2.%20Setup/2d.%20Events/Scripts/TriggerGravityFactorAuthoring.cs



        }
    }

    protected override void OnUpdate()
    {
        if (m_ActiveGates.CalculateEntityCount() == 0)
        {
            return;
        }
        var ecb = m_CmdBuffer.CreateCommandBuffer();
        Dependency = new TriggerGateJob
        {
            ActiveGatesGroup = GetComponentDataFromEntity<ActiveGate>(true),
            WalkingTagGroup = GetComponentDataFromEntity<WalkingTag>(),
            GateNumberGroup = GetComponentDataFromEntity<GateNumber>(),
            AgentConfigurationGroup = GetComponentDataFromEntity<AgentConfiguration>(),
            m_CmdBuffer = ecb,
        }.Schedule(m_StepPhysicsWorld.Simulation,
            ref m_BuildPhysicsWorld.PhysicsWorld, Dependency);
        Dependency.Complete();
        m_CmdBuffer.AddJobHandleForProducer(this.Dependency);
    }

}
