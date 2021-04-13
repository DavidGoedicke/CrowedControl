using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;




public class InertiaLock : MonoBehaviour
{

    public bool LockX = false;
    public bool LockY = false;
    public bool LockZ = false;


    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {


        if (dstManager.HasComponent<PhysicsMass>(entity)){
            var mass = dstManager.GetComponentData<PhysicsMass>(entity);
            mass.InverseInertia[0] = LockX? 0 : mass.InverseInertia[0];
            mass.InverseInertia[1] = LockY? 0 : mass.InverseInertia[1];
            mass.InverseInertia[2] = LockZ? 0 : mass.InverseInertia[2];
        } 
}
}
