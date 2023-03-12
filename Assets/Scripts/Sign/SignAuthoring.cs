using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;



[DisallowMultipleComponent]
public class SignAuthoring : MonoBehaviour
{
    public GateNums[] PublicSignNumber;
    public bool StartActive;

}
public class SignAuthoringBaker : Baker<SignAuthoring>
{
/// <param name="conversionSystem">Used for more advanced conversion features. Not used here.</param>
    public override void Bake(SignAuthoring newSign)
    {
      
        if (newSign.StartActive)
        {
            foreach (GateNums temp in newSign.PublicSignNumber)
            {
                AddComponent(new ActiveSign() { value = temp });
            }

        }
       
    }

}
