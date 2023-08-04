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
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(PhysicsSystemGroup))]
//[BurstCompile]
public partial struct ApplyAgentMotion : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

   // [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // TODO(DOTS-6141): This expression can't currently be inlined into the IJobEntity initializer
        var tmp = GameController.Singelton.SimParams.MotionIsRunning;
        float dt = SystemAPI.Time.DeltaTime;
        state.Dependency = new ApplyAgentImpulseJob
        {
            running=tmp,
            DeltaTime = dt
        }.Schedule(state.Dependency);
    }


    [BurstCompile]
    public partial struct ApplyAgentImpulseJob : IJobEntity
    {
        public float DeltaTime;
        public bool running;

        [BurstCompile]
        public void Execute(Entity _entity,
            RigidBodyAspect rigidBodyAspect,
            in ApplyImpulse _applyImpulseOnKeyData,
            in AgentConfiguration ac)
        {
            if (running)
            {
                if (math.length(_applyImpulseOnKeyData.Direction) <=
                    0.0f) // if we are alone sometimes there is no directio
                {
                    //  Debug.DrawRay(rigidBodyAspect.Position,math.forward(rigidBodyAspect.Rotation)*50,Color.magenta,10);
                    // rigidBodyAspect.ApplyLinearImpulseWorldSpace(math.forward(rigidBodyAspect.Rotation) * DeltaTime);
                }
                else
                {
                    
                    if (! math.isnan(_applyImpulseOnKeyData.Direction.x) && 
                        ! math.isnan(_applyImpulseOnKeyData.Direction.y) &&
                        ! math.isnan(_applyImpulseOnKeyData.Direction.z) )
                    {
                         rigidBodyAspect.ApplyLinearImpulseWorldSpace(_applyImpulseOnKeyData.Direction  * DeltaTime);
                    }
                    else
                    {
                        
                    }
                }

                if (math.length(rigidBodyAspect.LinearVelocity) > ac.Speed)
                {
                    float3 temp = rigidBodyAspect.LinearVelocity;
                    temp = math.normalize(temp) * ac.Speed;
                    rigidBodyAspect.LinearVelocity = temp;
                }

/*

                float dotProduct = math.dot(math.normalize(rigidBodyAspect.WorldFromBody.Forward().xz),
                    math.normalize(rigidBodyAspect.LinearVelocity.xz));
                float angleDiff = math.acos(dotProduct);
                angleDiff = math.cross(rigidBodyAspect.WorldFromBody.Forward(), rigidBodyAspect.LinearVelocity).y < 0
                    ? -angleDiff
                    : angleDiff;
                Debug.Log(angleDiff);
                //rigidBodyAspect.Apply
                // rigidBodyAspect.WorldFromBody.RotateY(angleDiff);
                // rigidBodyAspect.Rotation = math.mul(rigidBodyAspect.Rotation,
                //      quaternion.AxisAngle(new float3(0, 1, 0), angleDiff));
                if (!math.isnan(angleDiff))
                {
                    if (rigidBodyAspect.AngularVelocityWorldSpace.y > 0 && angleDiff < 0)
                    {
                        rigidBodyAspect.ApplyAngularImpulseWorldSpace(new float3(0, -0.5f, 0));
                    }


                    else if (rigidBodyAspect.AngularVelocityWorldSpace.y < 0 && angleDiff > 0)
                    {

                        rigidBodyAspect.ApplyAngularImpulseWorldSpace(new float3(0, 0.5f, 0));
                    }
                }
                */
            }




    
            else
            {
               
                rigidBodyAspect.LinearVelocity=float3.zero;
            }

        }
    }
}