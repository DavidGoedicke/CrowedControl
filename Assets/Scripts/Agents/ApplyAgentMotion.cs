using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Entities;
using Unity.Mathematics;

/// <summary> Our simple system. </summary>
[UpdateAfter(typeof(Unity.Physics.Systems.EndFramePhysicsSystem))]
[UpdateAfter(typeof(AgentSystem_IJobChunk))]
public class ApplyImpulseOnKeySystem : SystemBase
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
            in ApplyImpulse _applyImpulseOnKeyData) =>
        {
            
                int rigidbodyIndex = physicsWorld.GetRigidBodyIndex(_entity);

                /// Apply a linear impulse to the entity.
                PhysicsComponentExtensions.ApplyLinearImpulse(ref _physicsVelocity, _physicsMass, _applyImpulseOnKeyData.Direction * 0.5f);

        }).Run();
    }
}

