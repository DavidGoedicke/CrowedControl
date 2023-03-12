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

}


public class InertiaLockBaker : Baker<InertiaLock>
{
    public override void Bake(InertiaLock authoring)
    {
        float3 tmp = new float3(1, 1, 1);
        tmp[0] = authoring.LockX ? 0 : tmp[0];
        tmp[1] = authoring.LockY ? 0 : tmp[1];
        tmp[2] = authoring.LockZ ? 0 : tmp[2];
       
AddComponent<PhysicsMass>(new PhysicsMass(){ InverseInertia = tmp});
 }
}
