/// For Referencew https://github.com/Unity-Technologies/EntityComponentSystemSamples/blob/f9043577ddc0d8667b6b4649a71454741075f4d1/UnityPhysicsSamples/Assets/Demos/2.%20Setup/2d.%20Events/Scripts/TriggerGravityFactorAuthoring.cs

using UnityEngine;
using Unity.Entities;




public class GateAuthoring : MonoBehaviour
{
    
    public GateNums PublicGateNumber;
    
    class GateAuthoringBaker : Baker<GateAuthoring>
    {
        public override void Bake(GateAuthoring authoring)
        {
            AddComponent(new ActiveGate
            {
                value = authoring.PublicGateNumber
            });


        }
    }
    
    
}

