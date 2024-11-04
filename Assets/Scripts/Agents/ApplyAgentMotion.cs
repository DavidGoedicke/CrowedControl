//#define DEBUGMOTION
//#define WITHROTATION

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

/// <summary> Our simple system. </summary>
[RequireMatchingQueriesForUpdate]
//[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(AgentSystem))]


//[BurstCompile]
public partial struct ApplyAgentMotion : ISystem {
    //[BurstCompile]
    public void OnCreate(ref SystemState state) {
    }

    //[BurstCompile]
    public void OnDestroy(ref SystemState state) {
    }

    // [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        // var tmp = GameController.Singelton.SimParams.MotionIsRunning;
        state.Dependency = new ApplyAgentImpulseJob {
            DeltaTime = SystemAPI.Time.DeltaTime
        }.Schedule(state.Dependency);
    }


    [BurstCompile]
    public partial struct ApplyAgentImpulseJob : IJobEntity {
        public float DeltaTime;

        
        public void Execute(Entity _entity,
            RigidBodyAspect rigidBodyAspect,
            in ApplyImpulse _applyImpulseOnKeyData,
            in AgentConfiguration ac) {
            //Debug.Log("Befor: " + rigidBodyAspect.LinearVelocity+"Time:"+DeltaTime );

        if (math.length(_applyImpulseOnKeyData.Direction) > 0.0f) {
                // rigidBodyAspect.ApplyLinearImpulseWorldSpace(math.normalizesafe(_applyImpulseOnKeyData.Direction) * DeltaTime);
               // Debug.Log(rigidBodyAspect.Mass);
                rigidBodyAspect.ApplyLinearImpulseWorldSpace(new float3(0.00000001f, 0, 0));
            }
            //Debug.Log("After: "+rigidBodyAspect.LinearVelocity);
        }
    }
}


//if (math.length(rigidBodyAspect.LinearVelocity) > ac.Speed) {
//      float3 temp = rigidBodyAspect.LinearVelocity;
//        temp = math.normalizesafe(temp) * ac.Speed;
//        rigidBodyAspect.LinearVelocity = temp;
//     }