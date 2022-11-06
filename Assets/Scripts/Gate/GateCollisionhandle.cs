//#define DEBUGTRIGGER

using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;


[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateAfter(typeof(PhysicsSimulationGroup))]
[UpdateBefore(typeof(ExportPhysicsWorld))]
[BurstCompile]
public partial struct GateCollisionHandle : ISystem
{
    ComponentDataHandles m_Handles;
    private BeginSimulationEntityCommandBufferSystem.Singleton ecbSingleton;
    private bool tryagain;

    struct ComponentDataHandles
    {
        public ComponentLookup<ActiveGate> ActiveGatesGroup;
        public ComponentLookup<WalkingTag> WalkingTagGroup;
        public ComponentLookup<AgentConfiguration> AgentConfigurationGroup;
        


        public ComponentDataHandles(ref SystemState state)
        {
            ActiveGatesGroup = state.GetComponentLookup<ActiveGate>(true);
            WalkingTagGroup = state.GetComponentLookup<WalkingTag>(true);
            AgentConfigurationGroup = state.GetComponentLookup<AgentConfiguration>(true);
        }

        public void Update(ref SystemState state)
        {
            ActiveGatesGroup.Update(ref state);
            WalkingTagGroup.Update(ref state);
            AgentConfigurationGroup.Update(ref state);
        }
    }


    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        m_Handles = new ComponentDataHandles(ref state);


        tryagain  = ! SystemAPI.TryGetSingleton(out ecbSingleton);
    }

    
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        m_Handles.Update(ref state);
        if (tryagain)
        {
            tryagain = !SystemAPI.TryGetSingleton(out ecbSingleton);
            Debug.Log("Was trying to find a singelton");
            return;
        }
        
        

        state.Dependency = new TriggerGateJob
        {
            ActiveGatesGroup = state.GetComponentLookup<ActiveGate>(true),
            WalkingTagGroup = state.GetComponentLookup<WalkingTag>(true),
            AgentConfigurationGroup = state.GetComponentLookup<AgentConfiguration>(true),
            m_CmdBuffer =  ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged)
        }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        
    }

    [BurstCompile]
    struct TriggerGateJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentLookup<ActiveGate> ActiveGatesGroup;
        [ReadOnly] public ComponentLookup<WalkingTag> WalkingTagGroup;
        [ReadOnly] public ComponentLookup<AgentConfiguration> AgentConfigurationGroup;
       
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

            if (ActiveGatesGroup.HasComponent(GateEntity))
            {
                ActiveGate GNcomp = ActiveGatesGroup[GateEntity];
                AgentConfiguration ACcomp = AgentConfigurationGroup[AgentEntity];

                if (GNcomp.value == ACcomp.TargetGate)
                {
                    m_CmdBuffer.DestroyEntity(AgentEntity);
                }
            }
            // Look here https://github.com/Unity-Technologies/EntityComponentSystemSamples/blob/f9043577ddc0d8667b6b4649a71454741075f4d1/UnityPhysicsSamples/Assets/Demos/2.%20Setup/2d.%20Events/Scripts/TriggerGravityFactorAuthoring.cs
        }
    }
}