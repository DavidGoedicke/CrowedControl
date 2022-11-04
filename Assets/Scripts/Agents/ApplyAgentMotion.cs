//#define DEBUGMOTION

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

[BurstCompile]
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

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // TODO(DOTS-6141): This expression can't currently be inlined into the IJobEntity initializer
        float dt = SystemAPI.Time.DeltaTime;
        state.Dependency = new ApplyAgentImpulseJob
        {
            DeltaTime = dt,
        }.Schedule(state.Dependency);
    }


    [BurstCompile]
    public partial struct ApplyAgentImpulseJob : IJobEntity
    {
        public float DeltaTime;

        [BurstCompile]
        public void Execute(Entity _entity,
            ref RigidBodyAspect rigidBodyAspect,
            in ApplyImpulse _applyImpulseOnKeyData,
            in AgentConfiguration ac)
        {
            /// Apply a linear impulse to the entity.
          
            if (math.length(_applyImpulseOnKeyData.Direction) <= 0.0f) // if we are alone sometimes there is no directio
            {
                rigidBodyAspect.ApplyLinearImpulseWorldSpace(math.forward(rigidBodyAspect.Rotation) *DeltaTime  );
            }
            else
            {

                rigidBodyAspect.ApplyLinearImpulseWorldSpace(_applyImpulseOnKeyData.Direction * 10 * DeltaTime);
            }

            if (math.length(rigidBodyAspect.LinearVelocity) > ac.Speed)
            {
                float3 temp = rigidBodyAspect.LinearVelocity;
                temp = math.normalize(temp) * ac.Speed;
                rigidBodyAspect.LinearVelocity = temp;
                
            }
            
            
         //  Debug.Log("Agent"+_entity.Index.ToString()+ " speed: "+math.length(rigidBodyAspect.LinearVelocity).ToString()+"   "+DeltaTime.ToString()+" Direction:"+math.length(_applyImpulseOnKeyData.Direction).ToString());
            var fwd = math.forward( rigidBodyAspect.Rotation);

            float2 A = math.normalize(fwd.xz);
            float2 B = math.normalize(_applyImpulseOnKeyData.Direction.xz);


            float dotProduct = math.dot(A, B);
            float angleDiff = math.acos(dotProduct);
            angleDiff = (math.cross(fwd, _applyImpulseOnKeyData.Direction)).y < 0 ? -angleDiff : angleDiff;

#if DEBUGMOTION
            //Debug.Log(math.degrees(angleDiff) + "<=Angle : lanrg =>"+"  DotProduct =>"+A.ToString()+B.ToString()+ _applyImpulseOnKeyData.Direction.x.ToString());
            Debug.DrawRay(rigidBodyAspect.Position , new Vector3(A.x,0,A.y) , Color.green);
            Debug.DrawRay(rigidBodyAspect.Position, new Vector3(B.x,0, B.y), Color.red);
          //  Debug.DrawRay(rigidBodyAspect.Position, rigidBodyAspect.Right *angleDiff*10, Color.cyan);
            //Debug.Log(math.degrees(angleDiff));
#endif

          //  if (angleDiff >= -math.PI && angleDiff <= math.PI)
         //   {
                float multiplyer = 0.025f;
                float rotationSpeed = math.length(rigidBodyAspect.AngularVelocityLocalSpace);

                rigidBodyAspect.ApplyAngularImpulseLocalSpace(new float3(0,
                    angleDiff * math.clamp(rotationSpeed.MapRange(0f, 1f, multiplyer, 0f), multiplyer, 0f), 0));
         //   }
        }
    }
}