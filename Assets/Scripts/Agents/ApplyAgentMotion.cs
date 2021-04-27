using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

/// <summary> Our simple system. </summary>

[UpdateAfter(typeof(AgentSystem_IJobChunk))]
public class ApplyAgentMotion : SystemBase
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

             quaternion TargertOrientation = quaternion.LookRotationSafe(_applyImpulseOnKeyData.Direction, math.up());
           quaternion angularchange = math.mul(TargertOrientation, math.inverse(quaternion.LookRotationSafe(ltw.Forward,math.up())));
            PhysicsComponentExtensions.ApplyAngularImpulse(ref _physicsVelocity, _physicsMass, new float3 (0,angularchange.value.y * 0.05f, 0));

        }).Run();
    }
}

