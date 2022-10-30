/// For Referencew https://github.com/Unity-Technologies/EntityComponentSystemSamples/blob/f9043577ddc0d8667b6b4649a71454741075f4d1/UnityPhysicsSamples/Assets/Demos/2.%20Setup/2d.%20Events/Scripts/TriggerGravityFactorAuthoring.cs

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;




[DisallowMultipleComponent]
public class GateAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{


    public GateNums PublicGateNumber;
    public bool StartActive;
 /// <param name="conversionSystem">Used for more advanced conversion features. Not used here.</param>
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        // Here we add all of the components needed to start the guard off in the "Patrol" state
        // i.e. We add TargetPosition, and don't add IdleTimer or IsChasing tag
        dstManager.AddComponents(entity, new ComponentTypeSet(
            new ComponentType[] {
                typeof(GateNumber)
            }));
        if(StartActive){
            dstManager.AddComponents(entity, new ComponentTypeSet(
           new ComponentType[] {
                typeof(ActiveGate)
           }));
        }

        dstManager.SetComponentData(entity, new GateNumber { value = PublicGateNumber });

    }
}