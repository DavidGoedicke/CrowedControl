//#define DEBUGMOTION
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary> Our simple system. </summary>

[UpdateAfter(typeof(AgentSystem))]
public partial class ApplyAgentMotion : SystemBase
{
    

    protected override void OnUpdate()
    {
        /// Get the physics world.
        // You don't need to do this if you're going to use ComponentExtensions.ApplyLinearImpulse, but I have kept the code in to show you how to do it.
        PhysicsWorld physicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>().PhysicsWorld;

        Entities.WithoutBurst().ForEach(
        (
            Entity _entity,
            ref PhysicsVelocity _physicsVelocity,
            ref PhysicsMass _physicsMass,
            in ApplyImpulse _applyImpulseOnKeyData,
            in LocalToWorld ltw,
            in AgentConfiguration ac) =>
        {


            int rigidbodyIndex = physicsWorld.GetRigidBodyIndex(_entity);

            /// Apply a linear impulse to the entity.

             PhysicsComponentExtensions.ApplyLinearImpulse(ref _physicsVelocity, _physicsMass, _applyImpulseOnKeyData.Direction * 0.5f);
            if (math.length(_physicsVelocity.Linear) > ac.Speed)
            {
                float3 temp = _physicsVelocity.Linear;
                temp = math.normalize(temp) * ac.Speed;
                _physicsVelocity.Linear = temp;
            }

            float2 A = math.normalize(new float2(ltw.Forward.x, ltw.Forward.z));
            float2 B = math.normalize(new float2(_applyImpulseOnKeyData.Direction.x, _applyImpulseOnKeyData.Direction.z));
          

            float dotProduct = math.dot(A, B);
            float angleDiff = math.acos(dotProduct);
            angleDiff = (math.cross(ltw.Forward, _applyImpulseOnKeyData.Direction)).y < 0 ? -angleDiff : angleDiff;
#if DEBUGMOTION
            //Debug.Log(math.degrees(angleDiff) + "<=Angle : lanrg =>"+"  DotProduct =>"+A.ToString()+B.ToString()+ _applyImpulseOnKeyData.Direction.x.ToString());
            Debug.DrawRay(ltw.Position , new Vector3(A.x,0,A.y) , Color.green);
            Debug.DrawRay(ltw.Position, new Vector3(B.x,0, B.y), Color.red);
            Debug.DrawRay(ltw.Position, ltw.Right *angleDiff*10, Color.cyan);
            //Debug.Log(math.degrees(angleDiff));
#endif

            if (angleDiff >=-math.PI && angleDiff <= math.PI)
            {
                float multiplyer = 0.025f;
                float rotationSpeed = math.length(_physicsVelocity.Angular);
            PhysicsComponentExtensions.ApplyAngularImpulse(ref _physicsVelocity, _physicsMass, new float3(0, angleDiff* math.clamp(rotationSpeed.MapRange(0f,1f, multiplyer, 0f),multiplyer,0f), 0));
            }

        }).Run();

    }
}

