using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Physics.Systems;
using UnityEngine;
using Math = Unity.Physics.Math;
using Unity.Physics.Aspects;
using Unity.Transforms;


[RequireMatchingQueriesForUpdate]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(PresentationSystemGroup))]
[BurstCompile]
public partial struct LateFrameSystem : ISystem
{

    
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // TODO(DOTS-6141): This expression can't currently be inlined into the IJobEntity initializer
       
       // state.Dependency = new ApplyAgentFixY
        //{
           
        //}.Schedule(state.Dependency);
    }

    public partial struct ApplyAgentFixY : IJobEntity
    {
       

        [BurstCompile]
        public void Execute(Entity _entity,
            ref LocalTransform localTra,in AgentConfiguration ac)
        {
            localTra.Position = new float3(localTra.Position.x, 1, localTra.Position.z);
            Debug.Log("fixed position");
        }
    }
}
